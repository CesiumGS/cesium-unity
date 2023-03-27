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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Manages credits / attribution for <see cref="Cesium3DTileset"/> and <see cref="CesiumRasterOverlay"/>.
    /// </summary>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCreditSystemImpl", "CesiumCreditSystemImpl.h")]
    public partial class CesiumCreditSystem : MonoBehaviour
    {
        private string _onScreenCredits;
        private string _popupCredits;

        public string onScreenCredits
        {
            get => this._onScreenCredits;
            internal set => this._onScreenCredits = value;
        }

        public string popupCredits
        {
            get => this._popupCredits;
            internal set => this._popupCredits = value;
        }

        // The delimiter refers to the string used to separate credit entries
        // when they are presented on-screen.
        private string _defaultDelimiter = " \u2022 ";

        internal string defaultDelimiter
        {
            get => this._defaultDelimiter;
        }

        private Shader _defaultSpriteShader = null!;

        private List<Texture2D> _images = new List<Texture2D>();

        internal List<Texture2D> images
        {
            get => this._images;
        }

        private int _numLoadingImages = 0;

        private static CesiumCreditSystem _defaultCreditSystem;

        public static CesiumCreditSystem GetDefaultCreditSystem()
        {
            if (_defaultCreditSystem == null)
            {
                _defaultCreditSystem = CreateDefaultCreditSystem();
            }

            return _defaultCreditSystem;
        }

        internal static partial CesiumCreditSystem CreateDefaultCreditSystem();

        internal partial void Update();

        public bool HasLoadingImages()
        {
            return this._numLoadingImages > 0;
        }

        const string base64Prefix = "data:image/png;base64,";

        internal IEnumerator LoadImage(string url)
        {
            // Each image is identified by its index.
            int imageId = this._images.Count;

            // Initialize a texture of arbitrary size.
            Texture2D texture = new Texture2D(1, 1);

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
                }

                this._numLoadingImages--;
            }

            this._images.Add(texture);

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                yield break;
            }
#endif
            // Create a TMP_SpriteAsset out of the texture and add it as a fallback
            // for the default sprite asset. The sprite will be accessed when the text
            // searches for its name.
            //string name = "credit-image-" + imageId;
            //TMP_SpriteAsset spriteAsset = CreateSpriteAssetFromTexture(texture, name);
            //TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
        }

        private TMP_SpriteAsset CreateSpriteAssetFromTexture(Texture2D texture, string name)
        {
            texture.wrapMode = TextureWrapMode.Clamp;

            // Convert the image to a sprite asset for TextMeshPro.
            TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            spriteAsset.name = name;
            spriteAsset.spriteSheet = texture;
            spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAsset.name);

            // Make a single sprite with the sprite sheet.
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f);

            // Create a sprite glyph that treats the entire sprite as one glyph.
            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph();
            spriteGlyph.sprite = sprite;
            spriteGlyph.index = 0;
            spriteGlyph.metrics = new GlyphMetrics(
                texture.width,
                texture.height,
                -0.5f,
                texture.height - 0.5f,
                texture.width);
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

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            List<TMP_SpriteAsset> fallbackSpriteAssets =
                TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets;
            int count = fallbackSpriteAssets.Count;
            fallbackSpriteAssets.RemoveRange(count - this._images.Count, this._images.Count);
        }
    }
}