using Reinterop;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class TokenTroubleshootingDetails
    {
        private string _token;
        public string token
        {
            get => this._token;
            set
            {
                this._token = value;
            }
        }

        private bool _isValid;
        public bool isValid
        {
            get => this._isValid;
            set
            {
                this._isValid = value;
            }
        }

        private bool _allowsAccessToAsset;
        public bool allowsAccessToAsset
        {
            get => this._allowsAccessToAsset;
            set
            {
                this._allowsAccessToAsset = value;
            }
        }

        private bool _associatedWithUserAccount;
        public bool associatedWithUserAccount
        {
            get => this._associatedWithUserAccount;
            set
            {
                this._associatedWithUserAccount = value;
            }
        }

        private bool _loaded;
        public bool loaded
        {
            get => this._loaded;
            set
            {
                this._loaded = value;
            }
        }

        public TokenTroubleshootingDetails() : this("") { }

        public TokenTroubleshootingDetails(string token)
        {
            this._token = token;
            this._isValid = false;
            this._allowsAccessToAsset = false;
            this._associatedWithUserAccount = false;
            this._loaded = false;
        }
    }

    public class AssetTroubleshootingDetails
    {
        private long _assetID;
        public long assetID
        {
            get => this._assetID;
            set
            {
                this._assetID = value;
            }
        }

        private bool _assetExistsInUserAccount;
        public bool assetExistsInUserAccount
        {
            get => this._assetExistsInUserAccount;
            set
            {
                this._assetExistsInUserAccount = value;
            }
        }

        private bool _loaded;
        public bool loaded
        {
            get => this._loaded;
            set
            {
                this._loaded = value;
            }
        }

        public AssetTroubleshootingDetails() : this(0)
        { }

        public AssetTroubleshootingDetails(long assetID)
        {
            this._assetID = assetID;
            this._assetExistsInUserAccount = false;
        }
    }

    [ReinteropNativeImplementation("CesiumForUnityNative::IonTokenTroubleshootingWindowImpl",
        "IonTokenTroubleshootingWindowImpl.h")]
    public partial class IonTokenTroubleshootingWindow : EditorWindow
    {
        private static List<IonTokenTroubleshootingWindow> _existingWindows =
            new List<IonTokenTroubleshootingWindow>();

        private bool _triggeredByError = false;
        private CesiumIonAsset _ionAsset = new CesiumIonAsset();
        private bool _requestedDetails = false;

        public CesiumIonAsset ionAsset
        {
            get => this._ionAsset;
            internal set
            {
                this._ionAsset = value;
                this._assetTokenDetails.token = value.ionAccessToken;
                this._assetDetails.assetID = value.ionAssetID;
            }
        }

        private TokenTroubleshootingDetails _assetTokenDetails;
        public TokenTroubleshootingDetails assetTokenDetails
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

        public static bool HasExistingWindow(CesiumIonAsset ionAsset)
        {
            for (int i = 0; i < IonTokenTroubleshootingWindow._existingWindows.Count; i++)
            {
                IonTokenTroubleshootingWindow existingWindow =
                    IonTokenTroubleshootingWindow._existingWindows[i];
                if (existingWindow._ionAsset == ionAsset)
                {
                    return true;
                }
            }

            return false;
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

        public static void ShowWindow(Cesium3DTileset tileset, bool triggeredByError) {
            ShowWindow(new CesiumIonAsset(tileset), triggeredByError);
        }

        public static void ShowWindow(CesiumRasterOverlay overlay, bool triggeredByError)
        {
            ShowWindow(new CesiumIonAsset(overlay), triggeredByError);
        }

        private static void ShowWindow(CesiumIonAsset ionAsset, bool triggeredByError)
        {
            if (ionAsset.IsNull())
            {
                return;
            }

            // If a window is already open for this object, close it.
            Predicate<CesiumIonAsset> containsSameAsset =
                delegate (CesiumIonAsset windowAsset) {
                    return windowAsset.ionAssetID == ionAsset.ionAssetID;
                };
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
            this._assetDetails = new AssetTroubleshootingDetails();
            this.CreateImplementation();
        }

        private partial void GetTroubleshootingDetails();

        private void Update()
        {
            if (this._ionAsset.IsNull())
            {
                this.Close();
            }

            // This has to be deferred to the Update() loop so that CesiumRuntimeSettings
            // calls AssetDatabase.LoadAssetAtPath during a game loop.
            if (!this._requestedDetails)
            {
                this._requestedDetails = true;
                this.GetTroubleshootingDetails();
            }

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

            GUILayout.Space(5);

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
                GUILayout.Space(5);
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

            GUILayout.FlexibleSpace();

            DrawSolutionPanel();
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
            if (this._assetTokenDetails.loaded)
            {
                DrawConditionCheck("Is a valid Cesium ion Token",
                    this._assetTokenDetails.isValid);
                DrawConditionCheck("Allows access to this asset",
                    this._assetTokenDetails.allowsAccessToAsset);
                DrawConditionCheck("Is associated with your user account",
                    this._assetTokenDetails.associatedWithUserAccount);
            }
            GUILayout.EndVertical();
        }

        private void DrawDefaultAccessTokenTroubleshootPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Project Default Access Token",
                CesiumEditorStyle.subheaderStyle);
            if (this._defaultTokenDetails.loaded)
            {
                DrawConditionCheck("Is a valid Cesium ion Token",
                    this._defaultTokenDetails.isValid);
                DrawConditionCheck("Allows access to this asset",
                    this._defaultTokenDetails.allowsAccessToAsset);
                DrawConditionCheck("Is associated with your user account",
                    this._defaultTokenDetails.associatedWithUserAccount);
            }
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
            bool noAssetTokenRemedies =
                string.IsNullOrEmpty(this._assetTokenDetails.token)
                || !this._assetTokenDetails.allowsAccessToAsset;
            if (noAssetTokenRemedies)
                EditorGUILayout.LabelField("No automatic remedies are possible for Asset ID " +
                    this._ionAsset.ionAssetID + ", because:\n" +
                    " - The current token does not authorize access to the specified asset ID, and\n" +
                    " - The asset ID does not exist in your Cesium ion account.\n" +
                    "\n" +
                    "Please click the button below to open Cesium ion and check:\n" +
                    " - The " + this._ionAsset.type + "'s \"Ion Asset ID\" property is correct.\n" +
                    " - If the asset is from the \"Asset Depot\", verify that it has been added to \"My Assets\".",
                    EditorStyles.wordWrappedLabel);


            if (CanUseDefaultToken())
            {
                DrawUseDefaultTokenButton();
            }

            if (this._isConnected)
            {
                DrawOpenCesiumIonButton();
            }
            else
            {
                DrawConnectToIonButton();
            }

        }

        private bool CanUseDefaultToken()
        {
            TokenTroubleshootingDetails details = this._defaultTokenDetails;
            return !string.IsNullOrEmpty(this._ionAsset.ionAccessToken)
                && details.isValid && details.allowsAccessToAsset;
        }

        private void DrawUseDefaultTokenButton()
        {
            if (GUILayout.Button("Use the project default token for this " + this._ionAsset.type,
                    CesiumEditorStyle.cesiumButtonStyle))
            {
                this._ionAsset.ionAccessToken = "";
                this.Close();
            }
        }

        private void DrawOpenCesiumIonButton()
        {
            if (GUILayout.Button("Open Cesium ion on the Web",
                        CesiumEditorStyle.cesiumButtonStyle))
            {
                Application.OpenURL("https://cesium.com/ion/tokens");
                this.Close();
            }
        }

        private void DrawConnectToIonButton()
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
