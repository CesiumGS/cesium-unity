using Reinterop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Represents an HTML element, either text or an image, that may also be hyperlinked.
    /// This abstraction allows HTML parts of a credit to be converted to the desired UI elements
    /// by a UI implementation.
    /// </summary>
    internal class CesiumCreditComponent
    {
        private string _text;
        private string _link;
        private int _imageId = -1;

        /// <summary>
        /// The text of this credit component. May be empty.
        /// </summary>
        public string text
        {
            get => this._text;
        }

        /// <summary>
        /// The link used by this credit component. May be empty. 
        /// If the UI representation of this component is clicked, it should open this link.
        /// </summary>
        public string link
        {
            get => this._link;
        }

        /// <summary>
        /// The ID of the image represented by this credit component.
        /// </summary>
        /// <remarks>
        /// This ID corresponds to the index of the image in <see cref="CesiumCreditSystem.images"/>.
        /// If it is -1, this component does not contain an image.
        /// </remarks>
        public int imageId
        {
            get => this._imageId;
        }

        public CesiumCreditComponent(string text, string link, int imageId)
        {
            this._text = text;
            this._link = link;
            this._imageId = imageId;
        }
    }

    /// <summary>
    /// Represents an HTML credit from a tileset or raster overlay. This abstracts an HTML string
    /// into multiple <see cref="CesiumCreditComponent"/>s for UI rendering.
    /// </summary>
    internal class CesiumCredit
    {
        private List<CesiumCreditComponent> _components;

        /// <summary>
        /// The <see cref="CesiumCreditComponent"/>s that make up this credit.
        /// </summary>
        public List<CesiumCreditComponent> components
        {
            get => this._components;
        }

        public CesiumCredit() : this(new List<CesiumCreditComponent>())
        { }

        public CesiumCredit(List<CesiumCreditComponent> components)
        {
            this._components = components;
        }
    }

    /// <summary>
    /// Manages credits / attribution for <see cref="Cesium3DTileset"/> and <see cref="CesiumRasterOverlay"/>.
    /// </summary>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCreditSystemImpl", "CesiumCreditSystemImpl.h")]
    [AddComponentMenu("Cesium/Cesium Credit System")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumCreditSystem : MonoBehaviour
    {
        private List<CesiumCredit> _onScreenCredits;
        private List<CesiumCredit> _popupCredits;

        /// <summary>
        /// The current on-screen credits, represented as <see cref="CesiumCredit"/>s.
        /// </summary>
        internal List<CesiumCredit> onScreenCredits
        {
            get => this._onScreenCredits;
        }

        /// <summary>
        /// The credits to be displayed in the "Data Attribution" panel,
        /// represented as <see cref="CesiumCredit"/>s.
        /// </summary>
        internal List<CesiumCredit> popupCredits
        {
            get => this._popupCredits;
        }

        private List<Texture2D> _images;

        /// <summary>
        /// The images loaded by this credit system.
        /// </summary>
        internal List<Texture2D> images
        {
            get => this._images;
        }

        /// <summary>
        /// Encapsulates a method that receives the on-screen and popup credits in <see cref="CesiumCredit"/>
        /// form. This can be used to create UI components for the credits after they have been updated.
        /// </summary>
        /// <param name="onScreenCredits">The on-screen credits.</param>
        /// <param name="onPopupCredits">The popup credits.</param>
        internal delegate void CreditsUpdateDelegate(List<CesiumCredit> onScreenCredits, List<CesiumCredit> onPopupCredits);

        /// <summary>
        /// An event that is raised when credits have been updated by the credit system. This is
        /// only raised when the credits in view have actually changed.
        /// </summary>
        /// <remarks>
        /// If the credit system is loading any images for its credits, the update will not be broadcasted
        /// until all image loads are complete.
        /// </remarks>
        internal event CreditsUpdateDelegate OnCreditsUpdate;

        private void OnEnable()
        {
            this._onScreenCredits = new List<CesiumCredit>();
            this._popupCredits = new List<CesiumCredit>();
            this._images = new List<Texture2D>();

            Cesium3DTileset.OnSetShowCreditsOnScreen += this.ForceUpdateCredits;
            SceneManager.sceneUnloaded += this.OnSceneUnloaded;
#if UNITY_EDITOR
            EditorSceneManager.sceneClosing += HandleClosingScene;
#endif
        }

        private void Update()
        {
            this.UpdateCredits(false);
        }

        /// <summary>
        /// Forces the credits to update, bypassing any performance optimizations in play.
        /// This ensures the credit system accounts for changes to the credits, e.g. if 
        /// <see cref="Cesium3DTileset.showCreditsOnScreen"/> is changed on a tileset.</param>
        /// </summary>
        private void ForceUpdateCredits()
        {
            this.UpdateCredits(true);
        }

        /// <summary>
        /// Updates the underlying native credit system.
        /// </summary>
        /// <param name="forceUpdate">Whether to force the credit system to update.</param>
        private partial void UpdateCredits(bool forceUpdate);

        internal void BroadcastCreditsUpdate()
        {
            if (this.OnCreditsUpdate != null)
            {
                this.OnCreditsUpdate(this._onScreenCredits, this._popupCredits);
            }
        }

        const string defaultName = "CesiumCreditSystemDefault";
        const string creditSystemPrefabName = "CesiumCreditSystem";

        private static CesiumCreditSystem _defaultCreditSystem;

        /// <summary>
        /// Creates an instance of the default credit system prefab.
        /// </summary>
        /// <remarks>
        /// This prefab comes with a <see cref="CesiumCreditSystemUI"/> component so that 
        /// the credits UI is automatically added to the editor and scene. However, 
        /// the CesiumCreditSystem class is uncoupled from any UI implementation itself.
        /// </remarks>
        /// <returns>The new CesiumCreditSystem instance.</returns>
        private static CesiumCreditSystem CreateDefaultCreditSystem()
        {
            GameObject creditSystemPrefab = Resources.Load<GameObject>(creditSystemPrefabName);
            GameObject creditSystemGameObject = UnityEngine.Object.Instantiate(creditSystemPrefab);
            creditSystemGameObject.name = defaultName;
            creditSystemGameObject.hideFlags = HideFlags.HideAndDontSave;

            return creditSystemGameObject.GetComponent<CesiumCreditSystem>();
        }

        /// <summary>
        /// Gets the default credit system, or creates a new default credit system instance if none exist.
        /// </summary>
        /// <returns>The default CesiumCreditSystem instance.</returns>
        internal static CesiumCreditSystem GetDefaultCreditSystem()
        {
            if (_defaultCreditSystem == null)
            {
                CesiumCreditSystem[] creditSystems = Resources.FindObjectsOfTypeAll<CesiumCreditSystem>();
                for (int i = 0; i < creditSystems.Length; i++)
                {
                    if (creditSystems[i].gameObject.name == defaultName)
                    {
                        _defaultCreditSystem = creditSystems[i];
                        break;
                    }
                }
            }

            if (_defaultCreditSystem == null)
            {
                _defaultCreditSystem = CreateDefaultCreditSystem();
            }

            return _defaultCreditSystem;
        }

        private int _numLoadingImages = 0;

        internal bool HasLoadingImages()
        {
            return this._numLoadingImages > 0;
        }

        const string base64Prefix = "data:image/png;base64,";

        internal IEnumerator LoadImage(string url)
        {
            int index = this._images.Count;

            // Initialize a texture of arbitrary size as a placeholder,
            // so that when other images are loaded, their IDs align properly
            // with the current list of images.
            Texture2D texture = new Texture2D(1, 1);
            this._images.Add(texture);

            if (url.LastIndexOf(base64Prefix, base64Prefix.Length) == 0)
            {
                // Load an image from a string that contains the
                // "data:image/png;base64," prefix
                string byteString = url.Substring(base64Prefix.Length);
                byte[] bytes = Convert.FromBase64String(byteString);
                if (!texture.LoadImage(bytes))
                {
                    Debug.Log("Could not parse image from base64 string.");
                }
            }
            else
            {
                // Load an image from a URL.
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                this._numLoadingImages++;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                    Texture2D placeholderTexture = this._images[index];
                    this._images[index] = texture;
                    UnityLifetime.Destroy(placeholderTexture);
                }

                this._numLoadingImages--;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
        }

        private void OnDestroy()
        {
            Cesium3DTileset.OnSetShowCreditsOnScreen -= this.ForceUpdateCredits;

            for (int i = 0, count = this._images.Count; i < count; i++)
            {
                if (this._images != null)
                {
                    UnityLifetime.Destroy(this._images[i]);
                }
            }

            this._images.Clear();

            if (_defaultCreditSystem == this)
            {
                _defaultCreditSystem = null;
            }
        }

        /// <summary>
        /// This handles the destruction of the credit system whenever the application is quit
        /// from a built executable or from play mode.
        /// </summary>
        private void OnApplicationQuit()
        {
            UnityLifetime.Destroy(this.gameObject);
        }

        /// <summary>
        /// This handles the destruction of the credit system whenever a scene is unloaded at runtime.
        /// </summary>
        /// <param name="scene">The scene being unloaded.</param>
        private void OnSceneUnloaded(Scene scene)
        {
            SceneManager.sceneUnloaded -= this.OnSceneUnloaded;
            if (this != null && this.gameObject != null)
                UnityLifetime.Destroy(this.gameObject);
        }

#if UNITY_EDITOR
        /// <summary>
        /// This handles the destruction of the credit system between scene switches in the Unity Editor.
        /// Without this, the credit system will live between instances and won't properly render the 
        /// current scene's credits.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="removingScene">Whether or not the closing scene is also being removed.</param>
        private static void HandleClosingScene(Scene scene, bool removingScene)
        {
            if (_defaultCreditSystem != null)
            {
                UnityLifetime.Destroy(_defaultCreditSystem.gameObject);
            }
        }
#endif
    }
}