using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CesiumForUnity
{
    public static class CesiumEditorStyle
    {
        public static GUIStyle toolbarStyle;
        public static GUIStyle toolbarButtonStyle;
        public static GUIStyle toolbarButtonDisabledStyle;

        public static GUIStyle quickAddItemStyle;
        public static GUIStyle quickAddButtonStyle;

        public static GUIStyle cesiumButtonStyle;
        public static GUIStyle cesiumButtonDisabledStyle;
        public static GUIStyle refreshButtonStyle;

        public static GUIStyle headerStyle;
        public static GUIStyle subheaderStyle;

        //public static Texture2D cesiumForUnityLogo;
        public static Texture2D quickAddIcon;
        public static Dictionary<CesiumEditorWindow.ToolbarButton, Texture2D> toolbarIcons;
        public static Texture2D refreshIcon;
        public static Texture2D checkIcon;
        public static Texture2D xIcon;

        private static readonly Color buttonColor = new Color(0.2945f, 0.6317f, 0.7930f);
        private static readonly Color buttonColorLighter = new Color(0.4475f, 0.7544f, 0.8904f);
        private static readonly Color buttonColorDarker = new Color(0.2598f, 0.5785f, 0.7075f);
        private static readonly Color disabledButtonTextColor = new Color(0.7f, 0.7f, 0.7f);

        private static Texture2D buttonTexture;
        private static Texture2D buttonHoverTexture;
        private static Texture2D buttonPressedTexture;

        private static Texture2D LoadIcon(string resourcePath)
        {
            Texture2D icon = (Texture2D)Resources.Load(resourcePath);
            icon.wrapMode = TextureWrapMode.Clamp;

            return icon;
        }

        static CesiumEditorStyle()
        {
            if (quickAddIcon == null)
            {
                quickAddIcon = LoadIcon("FontAwesome/plus-solid");
            }

            quickAddItemStyle = new GUIStyle();
            quickAddItemStyle.margin = new RectOffset(5, 5, 10, 10);

            quickAddButtonStyle = new GUIStyle();
            quickAddButtonStyle.fixedWidth = 16.0f;
            quickAddButtonStyle.fixedHeight = 16.0f;
            quickAddButtonStyle.hover.background = Texture2D.grayTexture;

            if (toolbarIcons == null)
            {
                toolbarIcons = new Dictionary<CesiumEditorWindow.ToolbarButton, Texture2D>();
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.Add,
                    quickAddIcon);
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.Upload,
                    LoadIcon("FontAwesome/cloud-upload-alt-solid"));
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.Token,
                    LoadIcon("FontAwesome/key-solid"));
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.Learn,
                    LoadIcon("FontAwesome/book-reader-solid"));
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.Help,
                    LoadIcon("FontAwesome/hands-helping-solid"));
                toolbarIcons.Add(
                    CesiumEditorWindow.ToolbarButton.SignOut,
                    LoadIcon("FontAwesome/sign-out-alt-solid"));
            }

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

            headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.fontSize = 18;
            headerStyle.wordWrap = true;

            subheaderStyle = new GUIStyle(EditorStyles.label);
            subheaderStyle.fontSize = 14;
            subheaderStyle.margin = new RectOffset(5, 5, 5, 5);

            checkIcon = LoadIcon("FontAwesome/check-solid");
            xIcon = LoadIcon("FontAwesome/times-solid");
        }

    }
}
