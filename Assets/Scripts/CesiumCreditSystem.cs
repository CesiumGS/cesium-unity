using Reinterop;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.TextCore;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCreditSystemImpl", "CesiumCreditSystemImpl.h")]
    public partial class CesiumCreditSystem : MonoBehaviour, IPointerClickHandler
    {
        private GameObject _popupGameObject = null!;
        private TextMeshProUGUI _popupText = null!;
        private TextMeshProUGUI _onScreenText = null!;

        private string _popupTextContent = "";
        private string _onScreenTextContent = "";

        private Shader _defaultSpriteShader = null!;

        private int _numImages = 0;
        public int numberOfImages
        {
            get => this._numImages;
        }

        private int _numLoadingImages = 0;

        private void Awake()
        {
            GameObject canvasGameObject = gameObject.transform.GetChild(0).gameObject;

            _popupGameObject = canvasGameObject.transform.Find("Popup").gameObject;
            _popupGameObject.SetActive(false);
            GameObject popupTextGameObject = _popupGameObject.transform.GetChild(0).gameObject;
            _popupText = popupTextGameObject.GetComponent<TextMeshProUGUI>();

            GameObject onScreenGameObject = canvasGameObject.transform.Find("OnScreen").gameObject;
            GameObject onScreenTextGameObject = onScreenGameObject.transform.GetChild(0).gameObject;
            _onScreenText = onScreenTextGameObject.GetComponent<TextMeshProUGUI>();

            _popupTextContent = "";
            _onScreenTextContent = "";

            // If no event system exists, create one.
            if (EventSystem.current == null)
            {
                GameObject eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();

                #if ENABLE_INPUT_SYSTEM
                eventSystemGameObject.AddComponent<InputSystemUIInputModule>();
                #elif ENABLE_LEGACY_INPUT_MANAGER
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
                #endif
            }

            _defaultSpriteShader = Shader.Find("TextMeshPro/Sprite");
            _numImages = 0;
            _numLoadingImages = 0;
        }

        private partial void Update();
        private partial void OnApplicationQuit();

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex;
            if (_popupGameObject.activeSelf) {
                linkIndex = TMP_TextUtilities.FindIntersectingLink(_popupText, eventData.position, null);
                if (linkIndex != -1)
                {
                    TMP_LinkInfo linkInfo = _popupText.textInfo.linkInfo[linkIndex];
                    Application.OpenURL(linkInfo.GetLinkID());
                    return;
                }
            }

            linkIndex = TMP_TextUtilities.FindIntersectingLink(_onScreenText, eventData.position, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = _onScreenText.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();
                if (linkId == "popup")
                {
                    _popupGameObject.SetActive(!_popupGameObject.activeSelf);
                }
                else
                {
                    Application.OpenURL(linkId);
                }
            }
        }

        private void RefreshCreditsText() {
            if (_numLoadingImages == 0) {
                _popupText.text = _popupTextContent;
                _onScreenText.text = _onScreenTextContent;
            }
        }

        public void SetCreditsText(string popupCredits, string onScreenCredits)
        {
            _popupTextContent = popupCredits;
            _onScreenTextContent = onScreenCredits;

            RefreshCreditsText();
        }

        const string base64Prefix = "data:image/png;base64,";

        public IEnumerator LoadImage(string url)
        {
            // Each image is identified by its index.
            int imageId = _numImages;
            _numImages++; 

            // Initialize a texture of arbitrary size.
            Texture2D texture = new Texture2D(1, 1);

            if (url.LastIndexOf(base64Prefix, base64Prefix.Length) == 0)
            {
                // Load an image from a string that contains the "data:image/png;base64," prefix
                string byteString = url.Substring(base64Prefix.Length);
                byte[] bytes = Convert.FromBase64String(byteString);
                if (!texture.LoadImage(bytes)) {
                    Debug.Log("Could not parse image from base64 string.");
                }
            }
            else
            {
                // Load an image from a URL.
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                _numLoadingImages++;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                }

                _numLoadingImages--;
            }

            // Create a TMP_SpriteAsset out of the texture and add it as a fallback
            // for the default sprite asset. The sprite will be accessed when the text
            // searches for its name.
            string name = "credit-image-" + imageId;
            TMP_SpriteAsset spriteAsset = CreateSpriteAssetFromTexture(texture, name);
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);

            RefreshCreditsText();
        }

        private TMP_SpriteAsset CreateSpriteAssetFromTexture(Texture2D texture, string name)
        {
            // Convert the image to a sprite asset for TextMeshPro.
            TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            spriteAsset.name = name;
            spriteAsset.spriteSheet = texture;
            spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAsset.name);

            // Make a single sprite with the sprite sheet.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

            // Create a sprite glyph that treats the entire sprite as one glyph.
            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph();
            spriteGlyph.sprite = sprite;
            spriteGlyph.index = 0;
            spriteGlyph.metrics = new GlyphMetrics(texture.width, texture.height, -0.5f, texture.height - 0.5f, texture.width);
            spriteGlyph.glyphRect = new GlyphRect(sprite.rect);
            spriteGlyph.scale = 1.0f;
            spriteAsset.spriteGlyphTable.Add(spriteGlyph);

            // Create a sprite character, which represents the sprite as a basic element of text.
            TMP_SpriteCharacter spriteCharacter = new TMP_SpriteCharacter(0xFFFE, spriteGlyph);
            spriteCharacter.name = name;

            spriteAsset.spriteCharacterTable.Add(spriteCharacter);
            spriteAsset.UpdateLookupTables();

            // Create a new default material for this asset.
            Material material = new Material(_defaultSpriteShader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);
            spriteAsset.material = material;

            return spriteAsset;
        }

        public void ClearLoadedImages()
        {
            List<TMP_SpriteAsset> fallbackSpriteAssets = TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets;
            int count = fallbackSpriteAssets.Count;
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.RemoveRange(count - _numImages, _numImages);
        }
    }
}