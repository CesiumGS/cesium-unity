using Reinterop;
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumEditorWindowImpl", "CesiumEditorWindowImpl.h")]
    public partial class CesiumEditorWindow : EditorWindow
    {
        public static CesiumEditorWindow? currentWindow = null;

        [MenuItem("Cesium/Cesium")]
        public static void ShowWindow()
        {
            // If no existing window, make a new one docked next to the Hierarchy window.
            if (currentWindow == null)
            {
                Type siblingWindow = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll");
                currentWindow = GetWindow<CesiumEditorWindow>(new Type[] { siblingWindow });

            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        void OnEnable()
        {
            // Load the icon separately from the other resources.
            Texture2D icon = (Texture2D)Resources.Load("Cesium-64x64");
            icon.wrapMode = TextureWrapMode.Clamp;
            this.titleContent = new GUIContent("Cesium", icon);

            CesiumIonSession.Ion().Resume();
        }

        private bool _isConnected = false;
        private bool _isConnecting = false;
        private bool _isProfileLoaded = false;
        private bool _isLoadingProfile = false;

        void OnGUI()
        {
            // OnGUI is called for multiple events, including the Layout
            // and Repaint events. If the ion connection status changes
            // between these events, the UI will adapt to the change.
            // Unity will log an error because of the different layout.
            // To avoid this, we only check the connection variables when
            // OnGUI is invoked in the Layout event.
            if (Event.current.type == EventType.Layout)
            {
                CesiumIonSession ion = CesiumIonSession.Ion();
                _isConnected = ion.IsConnected();
                _isConnecting = ion.IsConnecting();
                _isProfileLoaded = ion.IsProfileLoaded();
                _isLoadingProfile = ion.IsLoadingProfile();
            }

            this.DrawCesiumToolbar();
            this.DrawQuickAddBasicAssetsPanel();

            GUILayout.Space(10);
            if (_isConnected)
            {
                this.DrawQuickAddIonAssetsPanel();
            }
            else
            {
                this.DrawIonLoginPanel();
            }

            GUILayout.Space(10);
            this.DrawConnectionStatusPanel();

            // Force the window to repaint if the cursor is hovered over it.
            // By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.
            if (EditorWindow.mouseOverWindow == currentWindow)
            {
                this.Repaint();
            }
        }

        private void Update()
        {
            CesiumIonSession.Ion().Tick();
        }

        public enum ToolbarButton
        {
            Add,
            Upload,
            Token,
            Learn,
            Help,
            SignOut,
        }

        void DrawToolbarButton(string name, ToolbarButton button, string tooltip, bool enableForIonOnly, Action func)
        {
            GUIContent content = new GUIContent(name, CesiumEditorStyle.toolbarIcons[button], tooltip);
            GUIStyle style = CesiumEditorStyle.toolbarButtonStyle;

            if (enableForIonOnly && !this._isConnected)
            {
                GUI.color = Color.grey;
                style = CesiumEditorStyle.toolbarButtonDisabledStyle;
            }

            if (GUILayout.Button(content, style))
            {
                if (!enableForIonOnly || this._isConnected)
                {
                    func();
                }
            }

            GUI.color = Color.white;
        }

        void DrawCesiumToolbar()
        {
            GUILayout.BeginHorizontal(CesiumEditorStyle.toolbarStyle);
            this.DrawToolbarButton(
                "Add",
                ToolbarButton.Add,
                "Add a tileset from Cesium ion to this scene",
                true,
                this.AddFromIon);
            this.DrawToolbarButton(
                "Upload",
                ToolbarButton.Upload,
                "Upload a tileset to Cesium ion to process it for efficient streaming to Cesium for Unity",
                true,
                this.UploadToIon);
            this.DrawToolbarButton(
                "Token",
                ToolbarButton.Token,
                "Select or create a token to use to access Cesium ion assets",
                false,
                this.SetToken);
            this.DrawToolbarButton(
                "Learn",
                ToolbarButton.Learn,
                "Open Cesium for Unity tutorials and learning resources",
                false,
                this.OpenDocumentation);
            this.DrawToolbarButton(
                "Help",
                ToolbarButton.Help,
                "Search for existing questions or ask a new question on the Cesium Community Forum",
                false,
                this.OpenSupport);
            this.DrawToolbarButton(
                "Sign Out",
                ToolbarButton.SignOut,
                "Sign out of Cesium ion",
                true,
                this.SignOutOfIon);
            GUILayout.EndHorizontal();
        }

        private enum QuickAddItemType
        {
            BlankTileset,
            IonTileset
        }

        private class QuickAddItem
        {
            public QuickAddItemType type;
            public string name;
            public string tooltip;
            public long tilesetId;
            public long overlayId;

            public QuickAddItem(
                QuickAddItemType type,
                string name,
                string tooltip,
                long tilesetId,
                long overlayId)
            {
                this.type = type;
                this.name = name;
                this.tooltip = tooltip;
                this.tilesetId = tilesetId;
                this.overlayId = overlayId;
            }
        }

        private readonly QuickAddItem[] _basicAssets = new[]
        {
            new QuickAddItem(
                QuickAddItemType.BlankTileset,
                "Blank 3D Tiles Tileset",
                "An empty tileset that can be configured to show Cesium ion assets or tilesets from other sources.",
                -1,
                -1)
        };

        private readonly QuickAddItem[] _ionAssets = new[]
        {
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with Bing Maps satellite imagery.",
                1,
                2),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial with Labels imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps satellite imagery.",
                1,
                3),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Road imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps imagery.",
                1,
                4),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Sentinel-2 imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with high-resolution satellite imagery from the Sentinel-2 project.",
                1,
                3954),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium OSM Buildings",
                "A 3D buildings layer derived from OpenStreetMap covering the entire world.",
                96188,
                -1)
        };

        void DrawQuickAddBasicAssetsPanel()
        {
            GUILayout.Label("Quick Add Basic Assets", EditorStyles.boldLabel);
            GUIContent addButtonContent = new GUIContent(CesiumEditorStyle.quickAddIcon, "Add this item to the level");

            for (int i = 0; i < this._basicAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(
                    new GUIContent(
                        this._basicAssets[i].name,
                        this._basicAssets[i].tooltip),
                    EditorStyles.wordWrappedLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    this.QuickAddAsset(this._basicAssets[i]);
                }
                GUILayout.EndHorizontal();
            }

            // add other options as they come up
        }
        private void QuickAddAsset(QuickAddItem item)
        {
            switch (item.type)
            {
                case QuickAddItemType.IonTileset:
                    AddAssetFromIon(item.name, item.tilesetId, item.overlayId);
                    break;
                case QuickAddItemType.BlankTileset:
                    GameObject blankTileset = new GameObject("Cesium3DTileset");
                    blankTileset.AddComponent<Cesium3DTileset>();
                    Selection.activeGameObject = blankTileset;
                    break;
                default:
                    break;
            }
        }

        private partial void AddAssetFromIon(
            string name,
            long tilesetID,
            long overlayID);

        void DrawQuickAddIonAssetsPanel()
        {
            GUILayout.Label("Quick Add Cesium ion Assets", EditorStyles.boldLabel);
            GUIContent addButtonContent = new GUIContent(
                CesiumEditorStyle.quickAddIcon,
                "Add this item to the level");

            for (int i = 0; i < this._ionAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(new GUIContent(
                    this._ionAssets[i].name,
                    this._ionAssets[i].tooltip),
                    EditorStyles.wordWrappedLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    this.QuickAddAsset(this._ionAssets[i]);
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawIonLoginPanel()
        {
            //GUILayout.Box(cesiumForUnityLogo, GUILayout.Width(500.0f));

            EditorGUILayout.LabelField("Access global high - resolution 3D content, " +
                "including photogrammetry, terrain, imagery, and buildings. " +
                "Bring your own data for tiling, hosting, and streaming to Unity.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect to Cesium ion", CesiumEditorStyle.cesiumButtonStyle))
            {
                CesiumIonSession.Ion().Connect();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawConnectionStatusPanel()
        {
            if (this._isConnecting)
            {
                EditorGUILayout.LabelField(
                    "Waiting for you to sign into Cesium ion with your web browser...",
                    EditorStyles.wordWrappedLabel);

                string authorizeUrl = CesiumIonSession.Ion().GetAuthorizeUrl();
                if (EditorGUILayout.LinkButton("Open web browser again"))
                {
                    Application.OpenURL(authorizeUrl);
                }
                GUILayout.Space(5);
                EditorGUILayout.LabelField(
                    "Or copy the URL below into your web browser",
                    EditorStyles.wordWrappedLabel);

                GUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(
                    authorizeUrl,
                    EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("Copy To Clipboard"))
                {
                    CopyLinkToClipboard(authorizeUrl);
                }
                GUILayout.EndHorizontal();
            }
            else if (this._isProfileLoaded)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                string username = CesiumIonSession.Ion().GetProfileUsername();
                if (EditorGUILayout.LinkButton(
                    new GUIContent(
                        "Connected to Cesium ion as " + username,
                        "Open your Cesium ion account in your browser")))
                {
                    this.VisitIon();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            else if (this._isLoadingProfile)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Loading user information...");
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            else if (this._isConnected)
            {
                CesiumIonSession.Ion().RefreshProfile();
            }
        }

        void AddFromIon()
        {
            CesiumIonAssetsWindow.ShowWindow();
        }

        void UploadToIon()
        {
            Application.OpenURL("https://cesium.com/ion/addasset");
        }

        void SetToken()
        {
            SelectIonTokenWindow.ShowWindow();
        }

        void OpenDocumentation()
        {
            Application.OpenURL("https://cesium.com/docs");
        }

        void OpenSupport()
        {
            Application.OpenURL("https://community.cesium.com/");
        }

        void SignOutOfIon()
        {
            CesiumIonSession.Ion().Disconnect();
        }

        void CopyLinkToClipboard(string link)
        {
            EditorGUIUtility.systemCopyBuffer = link;
        }

        void VisitIon()
        {
            Application.OpenURL("https://cesium.com/ion");
        }
    }

}