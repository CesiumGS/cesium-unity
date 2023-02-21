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

    [ReinteropNativeImplementation(
        "CesiumForUnityNative::IonTokenTroubleshootingWindowImpl",
        "IonTokenTroubleshootingWindowImpl.h",
        staticOnly: true)]
    public partial class IonTokenTroubleshootingWindow : EditorWindow
    {
        private static List<IonTokenTroubleshootingWindow> _existingWindows =
            new List<IonTokenTroubleshootingWindow>();

        private bool _triggeredByError = false;
        private CesiumIonAsset _ionAsset = new CesiumIonAsset();

        public CesiumIonAsset ionAsset
        {
            get => this._ionAsset;
            internal set
            {
                this._ionAsset = value;
                this._assetTokenDetails.token = value.ionAccessToken;
                this._assetDetails.assetID = value.ionAssetID;
                this.GetTroubleshootingDetails();
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

        public static void ShowWindow(Cesium3DTileset tileset, bool triggeredByError)
        {
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
                delegate (CesiumIonAsset windowAsset)
                {
                    return windowAsset.ionAssetID == ionAsset.ionAssetID;
                };
            RemoveWindowWithPredicate(containsSameAsset, true);

            // If this is a tileset, close any existing windows associated with its
            // overlays. Overlays won't appear until the tileset is working anyway.
            Cesium3DTileset tilesetAsset = ionAsset.tileset;
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
            CesiumRasterOverlay overlayAsset = ionAsset.overlay;
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
            window.Focus();

            IonTokenTroubleshootingWindow._existingWindows.Add(window);
        }

        public IonTokenTroubleshootingWindow()
        {
            this._assetTokenDetails = new TokenTroubleshootingDetails();
            this._defaultTokenDetails = new TokenTroubleshootingDetails();
            this._assetDetails = new AssetTroubleshootingDetails();
        }

        private partial void GetTroubleshootingDetails();

        private void OnEnable()
        {
            // This has to be deferred so that CesiumRuntimeSettings
            // calls AssetDatabase.LoadAssetAtPath during a game loop.
            this._defaultTokenDetails.token = CesiumRuntimeSettings.defaultIonAccessToken;
        }

        private void Update()
        {
            // If the asset has been deleted at some point, close the window.
            if (this._ionAsset.IsNull())
            {
                this.Close();
            }
        }

        private void OnDisable()
        {
            Predicate<CesiumIonAsset> containsSameAsset =
                delegate (CesiumIonAsset windowAsset) { return windowAsset == this._ionAsset; };
            RemoveWindowWithPredicate(containsSameAsset, false);
        }

        private bool _isConnectedToIon = false;

        private void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                CesiumIonSession ion = CesiumIonSession.Ion();
                this._isConnectedToIon = ion.IsConnected();
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

            bool hasAssetToken = !string.IsNullOrEmpty(this._ionAsset.ionAccessToken);
            if (hasAssetToken)
            {
                GUILayout.BeginHorizontal();
                DrawAssetAccessTokenTroubleshootDetails();
                DrawDefaultAccessTokenTroubleshootDetails();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                DrawAssetTroubleshootDetails();
            }
            else
            {
                GUILayout.BeginHorizontal();
                DrawDefaultAccessTokenTroubleshootDetails();
                DrawAssetTroubleshootDetails();
                GUILayout.EndHorizontal();
            }

            DrawSolutionPanel();
        }

        private void DrawConditionCheck(string text, bool isChecked)
        {
            GUILayout.BeginHorizontal();
            if (isChecked)
            {
                GUILayout.Box(CesiumEditorStyle.checkIcon, GUILayout.Width(20));
            }
            else
            {
                GUILayout.Box(CesiumEditorStyle.xIcon, GUILayout.Width(20));
            }
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        private void DrawAssetAccessTokenTroubleshootDetails()
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

        private void DrawDefaultAccessTokenTroubleshootDetails()
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

        private void DrawAssetTroubleshootDetails()
        {
            if (!this._isConnectedToIon)
            {
                return;
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Asset", CesiumEditorStyle.subheaderStyle);
            if (this._assetDetails.loaded)
            {
                DrawConditionCheck("Asset ID exists in your user account",
                    this._assetDetails.assetExistsInUserAccount);
            }
            GUILayout.EndVertical();
        }

        private void DrawSolutionPanel()
        {
            if (this.HasNoAutomaticRemedies())
            {
                EditorGUILayout.LabelField(
                    "No automatic remedies are possible for Asset ID " +
                    this._ionAsset.ionAssetID + ", because:\n" +
                    " - The current token does not authorize access to the specified " +
                    "asset ID, and\n" +
                    " - The asset ID does not exist in your Cesium ion account.\n" +
                    "\n" +
                    "Please click the button below to open Cesium ion and check:\n" +
                    " - The " + this._ionAsset.type + "'s \"Ion Asset ID\" property " +
                    "is correct.\n" +
                    " - If the asset is from the \"Asset Depot\", verify that it has " +
                    "been added to \"My Assets\".",
                    EditorStyles.wordWrappedLabel);
            }

            GUILayout.FlexibleSpace();

            if (!this._isConnectedToIon)
            {
                this.DrawConnectToIonButton();
            }

            if (this.CanUseDefaultToken())
            {
                this.DrawUseDefaultTokenButton();
            }

            if (this.CanAuthorizeToken(this._assetTokenDetails))
            {
                this.DrawAuthorizeAssetTokenButton();
            }

            if (this.CanAuthorizeToken(this._defaultTokenDetails))
            {
                this.DrawAuthorizeDefaultTokenButton();
            }

            if (this.CanSelectNewDefaultToken())
            {
                this.DrawSelectNewDefaultTokenButton();
            }

            if (this._isConnectedToIon)
            {
                this.DrawOpenCesiumIonButton();
            }

            GUILayout.FlexibleSpace();
                
            // Force the window to repaint if it is focused. By default,
            // it only repaints sporadically, so the token details will
            // take a long time to visually update.
            if (EditorWindow.focusedWindow == this)
            {
                this.Repaint();
            }
        }

        private bool CanUseDefaultToken()
        {
            TokenTroubleshootingDetails details = this._defaultTokenDetails;
            return !string.IsNullOrEmpty(this._ionAsset.ionAccessToken)
                && details.isValid
                && details.allowsAccessToAsset;
        }

        private void DrawUseDefaultTokenButton()
        {
            if (GUILayout.Button("Use the project default token for this "
                + this._ionAsset.type, CesiumEditorStyle.cesiumButtonStyle))
            {
                this.UseDefaultToken();
                this.Close();
            }
        }

        private void UseDefaultToken()
        {
            IonTokenTroubleshootingWindow.UseDefaultToken(this._ionAsset);
        }

        public static void UseDefaultToken(CesiumIonAsset asset)
        {
            if (asset.IsNull())
            {
                return;
            }

            if (asset.tileset != null)
            {
                Undo.RecordObject(asset.tileset, "Use Default ion Access Token for Tileset");
            }
            else if (asset.overlay != null)
            {
                Undo.RecordObject(asset.overlay, "Use Default ion Access Token for Raster Overlay");
            }

            asset.ionAccessToken = "";
        }

        private partial void AuthorizeToken(string token, bool isDefaultToken);

        private bool CanAuthorizeToken(TokenTroubleshootingDetails tokenDetails)
        {
            AssetTroubleshootingDetails assetDetails = this._assetDetails;
            if (!assetDetails.assetExistsInUserAccount)
            {
                return false;
            }

            return tokenDetails.isValid
                && !tokenDetails.allowsAccessToAsset
                && tokenDetails.associatedWithUserAccount;
        }

        private void DrawAuthorizeAssetTokenButton()
        {
            if (GUILayout.Button(
                "Authorize the " + this._ionAsset.type + "'s token to access this asset",
                CesiumEditorStyle.cesiumButtonStyle))
            {
                this.AuthorizeToken(this._ionAsset.ionAccessToken, false);
                this.Close();
            }
        }

        private void DrawAuthorizeDefaultTokenButton()
        {
            if (GUILayout.Button(
                "Authorize the project's default token to access this asset",
                CesiumEditorStyle.cesiumButtonStyle))
            {
                this.AuthorizeToken(CesiumRuntimeSettings.defaultIonAccessToken, true);
                this.Close();
            }
        }

        private partial void SelectNewDefaultToken();

        private bool CanSelectNewDefaultToken()
        {
            if (!this._isConnectedToIon || !this._assetDetails.assetExistsInUserAccount)
            {
                return false;
            }

            TokenTroubleshootingDetails details = this._defaultTokenDetails;
            return !details.isValid ||
                !(details.allowsAccessToAsset || details.associatedWithUserAccount);
        }

        private void DrawSelectNewDefaultTokenButton()
        {
            if (GUILayout.Button("Select or create a new project default token",
                CesiumEditorStyle.cesiumButtonStyle))
            {
                this.SelectNewDefaultToken();
                this.Close();
            }
        }

        private bool CannotRemedyToken(TokenTroubleshootingDetails details)
        {
            return string.IsNullOrEmpty(details.token) || !details.allowsAccessToAsset;
        }

        private bool HasNoAutomaticRemedies()
        {
            return this.CannotRemedyToken(this._assetTokenDetails)
                && this.CannotRemedyToken(this._defaultTokenDetails)
                && !this._assetDetails.assetExistsInUserAccount;
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
