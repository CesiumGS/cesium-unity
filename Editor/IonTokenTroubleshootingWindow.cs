using Reinterop;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class TokenTroubleshootingDetails
    {
        public string token;
        public bool isValid;
        public bool allowsAccessToAsset;
        public bool associatedWithUserAccount;

        public TokenTroubleshootingDetails() : this("") { }

        public TokenTroubleshootingDetails(string token)
        {
            this.token = token;
            this.isValid = false;
            this.allowsAccessToAsset = false;
            this.associatedWithUserAccount = false;
        }
    }

    public class AssetTroubleshootingDetails
    {
        public CesiumIonAsset asset;
        public bool assetIsInUserAssets;

        public AssetTroubleshootingDetails() : this(new CesiumIonAsset())
        { }

        public AssetTroubleshootingDetails(CesiumIonAsset asset)
        {
            this.asset = asset;
            this.assetIsInUserAssets = false;
        }
    }

    [ReinteropNativeImplementation("CesiumForUnityNative::IonTokenTroubleshootingWindowImpl",
        "IonTokenTroubleshootingWindowImpl.h")]
    public partial class IonTokenTroubleshootingWindow : EditorWindow
    {
        private static List<IonTokenTroubleshootingWindow> _existingWindows;

        private bool _triggeredByError = false;
        private CesiumIonAsset _ionAsset = new CesiumIonAsset();

        public CesiumIonAsset ionAsset
        {
            get => this._ionAsset;
            internal set
            {
                this._ionAsset = value;
                this.assetTokenDetails.token = value.ionAccessToken;
            }
        }

        private TokenTroubleshootingDetails? _assetTokenDetails;
        public TokenTroubleshootingDetails? assetTokenDetails
        {
            get => this._assetTokenDetails;
        }

        private TokenTroubleshootingDetails _defaultTokenDetails;
        public TokenTroubleshootingDetails defaultTokenDetails
        {
            get => this._defaultTokenDetails;
        }

        private AssetTroubleshootingDetails _assetDetails;
        public AssetTroubleshootingDetails assetDetails
        {
            get => this._assetDetails;
        }

        private static void RemoveWindowWithPredicate(
            Predicate<CesiumIonAsset> pred, bool closeWindow)
        {
            for (int i = 0; i < IonTokenTroubleshootingWindow._existingWindows.Count; i++)
            {
                IonTokenTroubleshootingWindow existingWindow =
                    IonTokenTroubleshootingWindow._existingWindows[i];
                if (pred(existingWindow._ionAsset))
                {
                    IonTokenTroubleshootingWindow._existingWindows.RemoveAt(i);
                    if (closeWindow)
                    {
                        existingWindow.Close();
                    }
                    return;
                }
            }
        }

        public static void CreateWindow(CesiumIonAsset ionAsset, bool triggeredByError)
        {
            if (ionAsset.IsNull())
            {
                return;
            }

            // If a window is already open for this object, close it.
            Predicate<CesiumIonAsset> containsSameAsset =
                delegate (CesiumIonAsset windowAsset) { return windowAsset == ionAsset; };
            RemoveWindowWithPredicate(containsSameAsset, true);

            // If this is a tileset, close any existing windows associated with its
            // overlays. Overlays won't appear until the tileset is working anyway.
            Cesium3DTileset? tilesetAsset = ionAsset.tileset;
            if (tilesetAsset != null)
            {
                CesiumRasterOverlay[] rasterOverlays =
                    tilesetAsset.gameObject.GetComponents<CesiumRasterOverlay>();

                for (int i = 0; i < rasterOverlays.Length; i++)
                {
                    CesiumRasterOverlay overlay = rasterOverlays[i];
                    Predicate<CesiumIonAsset> containsOverlay
                        = delegate (CesiumIonAsset windowAsset)
                        {
                            return windowAsset.overlay == overlay;
                        };
                    RemoveWindowWithPredicate(containsOverlay, true);
                };
            }

            // If this is a raster overlay and this panel is already open for its attached
            // tileset, don't open the panel for the overlay for the same reason as above.
            CesiumRasterOverlay? overlayAsset = ionAsset.overlay;
            if (overlayAsset != null)
            {
                Cesium3DTileset tileset
                    = overlayAsset.gameObject.GetComponent<Cesium3DTileset>();
                if (tileset != null)
                {
                    for (int i = 0;
                        i < IonTokenTroubleshootingWindow._existingWindows.Count; i++)
                    {
                        IonTokenTroubleshootingWindow existingWindow =
                            IonTokenTroubleshootingWindow._existingWindows[i];
                        if (existingWindow._ionAsset.tileset == tileset)
                        {
                            return;
                        }
                    }
                }
            }

            IonTokenTroubleshootingWindow window =
                EditorWindow.CreateInstance<IonTokenTroubleshootingWindow>();
            window.ionAsset = ionAsset;
            window._triggeredByError = triggeredByError;

            window.GetTroubleshootingDetails();

            window.titleContent =
                new GUIContent(ionAsset.objectName + ": Cesium ion Token Troubleshooting");
            window.Show();

            IonTokenTroubleshootingWindow._existingWindows.Add(window);
        }

        public IonTokenTroubleshootingWindow()
        {
            this._assetTokenDetails = new TokenTroubleshootingDetails();
            this._defaultTokenDetails =
                new TokenTroubleshootingDetails(CesiumRuntimeSettings.defaultIonAccessToken);
            this.CreateImplementation();
        }

        private partial void GetTroubleshootingDetails();

        private void Update()
        {
            CesiumIonSession.Ion().Tick();
        }

        private void OnDisable()
        {
            Predicate<CesiumIonAsset> containsSameAsset =
                delegate (CesiumIonAsset windowAsset) { return windowAsset == this._ionAsset; };
            RemoveWindowWithPredicate(containsSameAsset, false);
        }

        private bool _isConnected = false;

        private void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                CesiumIonSession ion = CesiumIonSession.Ion();
                this._isConnected = ion.IsConnected();
            }

            if (!this._ionAsset.IsUsingCesiumIon())
            {
                EditorGUILayout.LabelField(
                    "This object is not configured to connect to Cesium ion.",
                    EditorStyles.wordWrappedLabel);
                return;
            }

            if (this._triggeredByError)
            {
                string descriptor = this._ionAsset.type + " " + this._ionAsset.objectName +
                    " (" + this._ionAsset.componentType + ")";
                EditorGUILayout.LabelField(descriptor + " tried to access Cesium ion for " +
                    "asset ID " + this._ionAsset.ionAssetID + ", but it didn't work, " +
                    "probably due to a problem with the access token. This panel will " +
                    "help you fix it!", EditorStyles.wordWrappedLabel);
            }

            bool hasCustomToken = !string.IsNullOrEmpty(this._ionAsset.ionAccessToken);

            if (hasCustomToken)
            {
                // Troubleshoot the specified access token.
                GUILayout.BeginHorizontal();
                DrawAssetAccessTokenTroubleshootPanel();
                DrawDefaultAccessTokenTroubleshootPanel();
                GUILayout.EndHorizontal();
                DrawAssetTroubleshootPanel();
            }
            else
            {
                GUILayout.BeginHorizontal();
                DrawDefaultAccessTokenTroubleshootPanel();
                DrawAssetTroubleshootPanel();
                GUILayout.EndHorizontal();
            }
        }

        private void DrawConditionCheck(string text, bool isChecked)
        {
            GUILayout.BeginHorizontal();
            if (isChecked)
            {
                GUILayout.Box(CesiumEditorStyle.checkIcon);
            }
            else
            {
                GUILayout.Box(CesiumEditorStyle.xIcon);
            }
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        private void DrawAssetAccessTokenTroubleshootPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("This " + this._ionAsset.type + "'s Access Token",
                CesiumEditorStyle.subheaderStyle);
            DrawConditionCheck("Is a valid Cesium ion Token",
                this._assetTokenDetails.isValid);
            DrawConditionCheck("Allows access to this asset",
                this._assetTokenDetails.allowsAccessToAsset);
            DrawConditionCheck("Is associated with your user account",
                this._assetTokenDetails.associatedWithUserAccount);
            GUILayout.EndVertical();
        }

        private void DrawDefaultAccessTokenTroubleshootPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Project Default Access Token",
                CesiumEditorStyle.subheaderStyle);
            DrawConditionCheck("Is a valid Cesium ion Token",
                this._defaultTokenDetails.isValid);
            DrawConditionCheck("Allows access to this asset",
                this._defaultTokenDetails.allowsAccessToAsset);
            DrawConditionCheck("Is associated with your user account",
                this._defaultTokenDetails.associatedWithUserAccount);
            GUILayout.EndVertical();
        }

        private void DrawAssetTroubleshootPanel()
        {
            if (!this._isConnected)
            {
                return;
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Asset", CesiumEditorStyle.subheaderStyle);
            DrawConditionCheck("Asset ID exists in your user account",
                false);
            GUILayout.EndVertical();
        }

        private void DrawSolutionPanel()
        {
            if (!this._assetTokenDetails.allowsAccessToAsset)
                EditorGUILayout.LabelField("No automatic remedies are possible for Asset ID " +
                    this._ionAsset.ionAssetID + ", because:\n" +
                    " - The current token does not authorize access to the specified asset ID, and\n" +
                    " - The asset ID does not exist in your Cesium ion account.\n" +
                    "\n" +
                    "Please click the button below to open Cesium ion and check:\n" +
                    " - The " + this._ionAsset.type + "'s \"Ion Asset ID\" property is correct.\n" +
                    " - If the asset is from the \"Asset Depot\", verify that it has been added to \"My Assets\".",
                    EditorStyles.wordWrappedLabel);

            if (this._isConnected)
            {
                DrawOpenCesiumIonButton();
            }
            else
            {
                DrawConnectToIonButton();
            }

        }

        public void DrawUseDefaultTokenButton()
        {
            if (GUILayout.Button("Use the project default token for this " + this._ionAsset.type,
                    CesiumEditorStyle.cesiumButtonStyle))
            {
                this._ionAsset.ionAccessToken = "";
                this.Close();
            }
        }

        public void DrawOpenCesiumIonButton()
        {
            if (GUILayout.Button("Open Cesium ion on the Web",
                        CesiumEditorStyle.cesiumButtonStyle))
            {
                Application.OpenURL("https://cesium.com/ion/tokens");
                this.Close();
            }
        }

        public void DrawConnectToIonButton()
        {
            if (GUILayout.Button("Connect to Cesium ion",
                        CesiumEditorStyle.cesiumButtonStyle))
            {
                CesiumEditorWindow.ShowWindow();
                CesiumIonSession.Ion().Connect();
                this.Close();
            }
        }
    }
}
