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

        public static GUIStyle profileLinkStyle = null!;

        public static GUIStyle cesiumButtonStyle = null!;
        public static GUIStyle cesiumButtonDisabledStyle = null!;
        public static GUIStyle refreshButtonStyle = null!;

        public static GUIStyle descriptionHeaderStyle = null!;
        public static GUIStyle descriptionSubheaderStyle = null!;
        public static GUIStyle descriptionCenterTextStyle = null!;

        public static Texture2D cesiumForUnityLogo = null!;
        public static Texture2D quickAddIcon = null!;
        public static Texture2D[] toolbarIcons = null!;
        public static Texture2D refreshIcon = null!;

        private static readonly Color buttonColor = new Color(0.2945f, 0.6317f, 0.7930f);
        private static readonly Color buttonColorLighter = new Color(0.4475f, 0.7544f, 0.8904f);
        private static readonly Color buttonColorDarker = new Color(0.2598f, 0.5785f, 0.7075f);
        private static readonly Color disabledButtonTextColor = new Color(0.7f, 0.7f, 0.7f);

        private static Texture2D buttonTexture = null!;
        private static Texture2D buttonHoverTexture = null!;
        private static Texture2D buttonPressedTexture = null!;

        static CesiumEditorStyle()
        {
            LoadImages();
            LoadStyles();
        }

        private static void LoadImages()
        {
            if (quickAddIcon == null)
            {
                quickAddIcon = LoadIcon("FontAwesome/plus-solid");
            }

            if (toolbarIcons == null)
            {
                toolbarIcons = new Texture2D[Enum.GetNames(typeof(CesiumEditorWindow.ToolbarIndex)).Length];
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Add] = quickAddIcon;
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Upload] = LoadIcon("FontAwesome/cloud-upload-alt-solid");
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Token] = LoadIcon("FontAwesome/key-solid");
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Learn] = LoadIcon("FontAwesome/book-reader-solid");
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.Help] = LoadIcon("FontAwesome/hands-helping-solid");
                toolbarIcons[(int)CesiumEditorWindow.ToolbarIndex.SignOut] = LoadIcon("FontAwesome/sign-out-alt-solid");
            }

            if (refreshIcon == null)
            {
                refreshIcon = LoadIcon("FontAwesome/sync-alt-solid");
            }

            if (buttonTexture == null)
            {
                buttonTexture = new Texture2D(1, 1);
                buttonTexture.SetPixel(0, 0, buttonColor);
                buttonTexture.Apply();
            }

            if (buttonHoverTexture == null)
            {
                buttonHoverTexture = new Texture2D(1, 1);
                buttonHoverTexture.SetPixel(0, 0, buttonColorLighter);
                buttonHoverTexture.Apply();
            }

            if (buttonPressedTexture == null)
            {
                buttonPressedTexture = new Texture2D(1, 1);
                buttonPressedTexture.SetPixel(0, 0, buttonColorDarker);
                buttonPressedTexture.Apply();
            }
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

            profileLinkStyle = new GUIStyle(EditorStyles.label);
            profileLinkStyle.margin = new RectOffset(5, 5, 10, 10);
            profileLinkStyle.hover.textColor = new Color(0.5f, 0.5f, 0.5f);

            cesiumButtonStyle = new GUIStyle();
            cesiumButtonStyle.padding = new RectOffset(25, 25, 10, 10);
            cesiumButtonStyle.margin = new RectOffset(10, 10, 10, 10);
            cesiumButtonStyle.fontStyle = FontStyle.Bold;
            cesiumButtonStyle.alignment = TextAnchor.MiddleCenter;
            cesiumButtonStyle.normal.background = buttonTexture;
            cesiumButtonStyle.normal.textColor = Color.white;
            cesiumButtonStyle.hover.background = buttonHoverTexture;
            cesiumButtonStyle.hover.textColor = Color.white;
            cesiumButtonStyle.active.background = buttonPressedTexture;
            cesiumButtonStyle.active.textColor = Color.white;

            cesiumButtonDisabledStyle = new GUIStyle(cesiumButtonStyle);
            cesiumButtonDisabledStyle.normal.background = Texture2D.grayTexture;
            cesiumButtonDisabledStyle.normal.textColor = disabledButtonTextColor;
            cesiumButtonDisabledStyle.hover.background = Texture2D.grayTexture;
            cesiumButtonDisabledStyle.hover.textColor = disabledButtonTextColor;
            cesiumButtonDisabledStyle.active.background = Texture2D.grayTexture;
            cesiumButtonDisabledStyle.active.textColor = disabledButtonTextColor;

            refreshButtonStyle = new GUIStyle(cesiumButtonStyle);
            refreshButtonStyle.padding = new RectOffset(5, 5, 5, 5);
            refreshButtonStyle.fixedHeight = 32;
            refreshButtonStyle.fixedWidth = 40;

            descriptionHeaderStyle = new GUIStyle(EditorStyles.label);
            descriptionHeaderStyle.fontSize = 18;
            descriptionHeaderStyle.wordWrap = true;

            descriptionSubheaderStyle = new GUIStyle(EditorStyles.label);
            descriptionSubheaderStyle.fontSize = 14;

            descriptionCenterTextStyle = new GUIStyle(EditorStyles.label);
            descriptionCenterTextStyle.alignment = TextAnchor.MiddleCenter;
        }

    }
}
