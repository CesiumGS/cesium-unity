using Reinterop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    public class CesiumCreditComponent
    {
        private string _text;
        private string _link;
        private int _imageId = -1;

        public string text
        {
            get => this._text;
        }

        public string link
        {
            get => this._link;
        }

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

    public class CesiumCredit
    {
        private List<CesiumCreditComponent> _components;

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
    public partial class CesiumCreditSystem : MonoBehaviour
    {
        private List<CesiumCredit> _onScreenCredits;
        private List<CesiumCredit> _popupCredits;

        /// <summary>
        /// The current on-screen credits.
        /// </summary>
        public List<CesiumCredit> onScreenCredits
        {
            get => this._onScreenCredits;
        }

        /// <summary>
        /// The credits to be displayed in the "Data Attribution" panel.
        /// </summary>
        public List<CesiumCredit> popupCredits
        {
            get => this._popupCredits;
        }

        public delegate void CreditsUpdateDelegate(List<CesiumCredit> onScreenCredits, List<CesiumCredit> onPopupCredits);

        public event CreditsUpdateDelegate OnCreditsUpdate;

        private void OnEnable()
        {
            this._onScreenCredits = new List<CesiumCredit>();
            this._popupCredits = new List<CesiumCredit>();
        }

        private partial void Update();

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

        private static CesiumCreditSystem CreateDefaultCreditSystem()
        {
            GameObject creditSystemPrefab = Resources.Load<GameObject>(creditSystemPrefabName);
            GameObject creditSystemGameObject = UnityEngine.Object.Instantiate(creditSystemPrefab);
            creditSystemGameObject.name = defaultName;

            return creditSystemGameObject.GetComponent<CesiumCreditSystem>();
        }

        public static CesiumCreditSystem GetDefaultCreditSystem()
        {
            if (_defaultCreditSystem == null)
            {
                CesiumCreditSystem[] creditSystems = Resources.FindObjectsOfTypeAll<CesiumCreditSystem>();
                for (int i = 0; i < creditSystems.Length; i++)
                {
                    if (creditSystems[i].gameObject.name == defaultName)
                    {
                        UnityLifetime.Destroy(creditSystems[i].gameObject);
                    }
                }

                _defaultCreditSystem = CreateDefaultCreditSystem();
            }

            return _defaultCreditSystem;
        }

        private List<Texture2D> _images = new List<Texture2D>();

        internal List<Texture2D> images
        {
            get => this._images;
        }

        private int _numLoadingImages = 0;

        public bool HasLoadingImages()
        {
            return this._numLoadingImages > 0;
        }

        const string base64Prefix = "data:image/png;base64,";

        internal IEnumerator LoadImage(string url)
        {
            int index = this._images.Count;

            // Initialize a texture of arbitrary size.
            Texture2D texture = new Texture2D(1, 1);

            // Add it early so that when other images are loaded,
            // their ID aligns properly with the current list of images.
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
            for (int i = 0, count = this._images.Count; i < count; i++)
            {
                UnityLifetime.Destroy(this._images[i]);
            }

            this._images.Clear();

            if (_defaultCreditSystem == this)
            {
                _defaultCreditSystem = null;
            }
        }
    }
}