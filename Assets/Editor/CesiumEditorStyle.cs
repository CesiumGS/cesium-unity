using System;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public static class CesiumEditorStyle
    {
        public static GUIStyle toolbarStyle = null!;
        public static GUIStyle toolbarButtonStyle = null!;
        public static GUIStyle toolbarButtonDisabledStyle = null!;

        public static GUIStyle quickAddItemStyle = null!;
        public static GUIStyle quickAddButtonStyle = null!;

        public static GUIStyle cesiumButtonStyle = null!;
        public static GUIStyle refreshButtonStyle = null!;

        public static GUIStyle descriptionHeaderStyle = null!;
        public static GUIStyle descriptionSubheaderStyle = null!;
        public static GUIStyle descriptionCenterTextStyle = null!;

        public static Texture2D cesiumIcon = null!;
        public static Texture2D cesiumForUnityLogo = null!;
        public static Texture2D quickAddIcon = null!;
        public static Texture2D[] toolbarIcons = null!;
        public static Texture2D refreshIcon = null!;

        private static readonly Color buttonColor = new Color(0.2945f, 0.6317f, 0.7930f, 1.0f);
        private static readonly Color buttonColorLighter = new Color(0.4475f, 0.7544f, 0.8904f, 1.0f);
        private static readonly Color buttonColorDarker = new Color(0.2598f, 0.5785f, 0.7075f, 1.0f);

        private static Texture2D buttonTexture = null!;
        private static Texture2D buttonHoverTexture = null!;
        private static Texture2D buttonPressedTexture = null!;

        public static void Reload()
        {
            LoadImages();
            LoadStyles();
        }

        private static void LoadImages()
        {
            cesiumIcon = LoadIcon("Cesium-icon-16x16");
            quickAddIcon = LoadIcon("FontAwesome/plus-solid");

            toolbarIcons = new Texture2D[Enum.GetNames(typeof(CesiumEditorWindow.ToolbarIndex)).Length];
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Add] = quickAddIcon;
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Upload] = LoadIcon("FontAwesome/cloud-upload-alt-solid");
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Token] = LoadIcon("FontAwesome/key-solid");
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Learn] = LoadIcon("FontAwesome/book-reader-solid");
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Help] = LoadIcon("FontAwesome/hands-helping-solid");
            toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.SignOut] = LoadIcon("FontAwesome/sign-out-alt-solid");

            refreshIcon = LoadIcon("FontAwesome/sync-alt-solid");

            buttonTexture = new Texture2D(1, 1);
            buttonHoverTexture = new Texture2D(1, 1);
            buttonPressedTexture = new Texture2D(1, 1);

            buttonTexture.SetPixel(0, 0, buttonColor);
            buttonHoverTexture.SetPixel(0, 0, buttonColorLighter);
            buttonPressedTexture.SetPixel(0, 0, buttonColorDarker);

            buttonTexture.Apply();
            buttonHoverTexture.Apply();
            buttonPressedTexture.Apply();
        }

        private static Texture2D LoadIcon(string resourcePath)
        {
            Texture2D icon = (Texture2D)Resources.Load(resourcePath);
            icon.wrapMode = TextureWrapMode.Clamp;

            return icon;
        }

        private static void LoadStyles()
        {
            toolbarStyle = new GUIStyle();
            toolbarStyle.margin = new RectOffset(5, 5, 5, 5);

            toolbarButtonStyle = new GUIStyle();
            toolbarButtonStyle.imagePosition = ImagePosition.ImageAbove;
            toolbarButtonStyle.padding = new RectOffset(5, 5, 5, 5);
            toolbarButtonStyle.alignment = TextAnchor.MiddleCenter;
            toolbarButtonStyle.normal.textColor = EditorStyles.label.normal.textColor;
            toolbarButtonStyle.hover.textColor = EditorStyles.label.normal.textColor;
            toolbarButtonStyle.hover.background = Texture2D.grayTexture;

            toolbarButtonDisabledStyle = new GUIStyle(toolbarButtonStyle);
            toolbarButtonDisabledStyle.hover.background = null;

            quickAddItemStyle = new GUIStyle();
            quickAddItemStyle.margin = new RectOffset(5, 5, 10, 10);

            quickAddButtonStyle = new GUIStyle();
            quickAddButtonStyle.fixedWidth = 16.0f;
            quickAddButtonStyle.fixedHeight = 16.0f;
            quickAddButtonStyle.hover.background = Texture2D.grayTexture;

            cesiumButtonStyle = new GUIStyle();
            cesiumButtonStyle.padding = new RectOffset(10, 10, 10, 10);
            cesiumButtonStyle.margin = new RectOffset(10, 10, 10, 10);
            cesiumButtonStyle.fontStyle = FontStyle.Bold;
            cesiumButtonStyle.alignment = TextAnchor.MiddleCenter;
            cesiumButtonStyle.normal.background = buttonTexture;
            cesiumButtonStyle.normal.textColor = Color.white;
            cesiumButtonStyle.hover.background = buttonHoverTexture;
            cesiumButtonStyle.hover.textColor = Color.white;
            cesiumButtonStyle.active.background = buttonPressedTexture;
            cesiumButtonStyle.active.textColor = Color.white;

            refreshButtonStyle = new GUIStyle(cesiumButtonStyle);
            refreshButtonStyle.padding = new RectOffset(5, 5, 5, 5);
            refreshButtonStyle.fixedHeight = 32;
            refreshButtonStyle.fixedWidth = 40;

            descriptionHeaderStyle = new GUIStyle(EditorStyles.label);
            descriptionHeaderStyle.fontSize = 18;

            descriptionSubheaderStyle = new GUIStyle(EditorStyles.label);
            descriptionSubheaderStyle.fontSize = 14;

            descriptionCenterTextStyle = new GUIStyle(EditorStyles.label);
            descriptionCenterTextStyle.alignment = TextAnchor.MiddleCenter;
        }

    }
}
