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
        public static GUIStyle quickAddItemLabelStyle = null!;
        public static GUIStyle quickAddButtonStyle = null!;

        public static GUIStyle connectToIonButtonStyle = null!;

        public static Texture2D cesiumIcon = null!;
        public static Texture2D cesiumForUnityLogo = null!;
        public static Texture2D quickAddIcon = null!;
        public static Texture2D[] toolbarIcons = null!;

        private static readonly Color textColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

        private static readonly Color buttonColor = new Color(0.07059f, 0.35686f, 0.59216f, 1.0f);
        private static readonly Color buttonColorLighter = new Color(0.16863f, 0.52941f, 0.76863f, 1.0f);
        private static readonly Color buttonColorDarker = new Color(0.05490f, 0.29412f, 0.45882f, 1.0f);

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
            toolbarButtonStyle.normal.textColor = textColor;
            toolbarButtonStyle.hover.textColor = textColor;
            toolbarButtonStyle.hover.background = Texture2D.grayTexture;

            toolbarButtonDisabledStyle = new GUIStyle(toolbarButtonStyle);
            toolbarButtonDisabledStyle.hover.background = null;

            quickAddItemStyle = new GUIStyle();
            quickAddItemStyle.margin = new RectOffset(5, 5, 10, 10);

            quickAddItemLabelStyle = new GUIStyle();
            quickAddItemLabelStyle.normal.textColor = textColor;
            quickAddItemLabelStyle.wordWrap = true;

            quickAddButtonStyle = new GUIStyle();
            quickAddButtonStyle.fixedWidth = 16.0f;
            quickAddButtonStyle.fixedHeight = 16.0f;
            quickAddButtonStyle.hover.background = Texture2D.grayTexture;

            connectToIonButtonStyle = new GUIStyle();
            connectToIonButtonStyle.fontStyle = FontStyle.Bold;
        }

    }
}
