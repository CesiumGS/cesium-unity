using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public class CesiumIonTokensWindow : EditorWindow
    {
        public static CesiumIonTokensWindow currentWindow = null!;

        public static void ShowWindow()
        {
            CesiumEditorStyle.Reload();

            if (currentWindow == null)
            {
                currentWindow = GetWindow<CesiumIonTokensWindow>("Select a Cesium ion Token");
                currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        void Awake()
        {
            CesiumIonSession.Ion().Resume();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Cesium for Unity embeds a Cesium ion token in your project in " +
                "order to allow it to access the assets you add to your levels. " +
                "Select the Cesium ion token to use.",
                EditorStyles.wordWrappedLabel
            );

            if (CesiumIonSession.Ion().IsConnected()) 
            {
                EditorGUILayout.LabelField(
                    "Please connect to Cesium ion to select a token from your " +
                    "account or to create a new token.",
                    EditorStyles.wordWrappedLabel
                );
            }

            DrawTokenSourceOptions();
            DrawActionButton();
        }

        enum TokenSource
        {
            Create,
            UseExisting,
            Specify
        }

        private TokenSource source = TokenSource.Specify;

        bool IsCreatingToken()
        {
            return source == TokenSource.Create;
        }

        bool IsUsingExistingToken()
        {
            return source == TokenSource.UseExisting;
        }

        bool IsSpecifyingToken()
        {
            return source == TokenSource.Specify;
        }

        private void DrawTokenSourceOptions()
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if(GUILayout.Toggle(IsCreatingToken(), "", GUILayout.Width(30)))
            {
                source = TokenSource.Create;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Create a new token", EditorStyles.boldLabel);
            //createTokenString = EditorGUILayout.TextField("Name:", createTokenString);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(IsUsingExistingToken(), "", GUILayout.Width(30)))
            {
                source = TokenSource.UseExisting;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Use an existing token", EditorStyles.boldLabel);
            // TODO: EXISTING TOKEN DROPDOWN
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if(GUILayout.Toggle(IsSpecifyingToken(), "", GUILayout.Width(30)))
            {
                source = TokenSource.Specify;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Specify a token", EditorStyles.boldLabel);
            //specifyTokenString = EditorGUILayout.TextField("Token:", specifyTokenString);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawActionButton()
        {
            GUILayout.Space(25);
            GUILayout.Button("Use as Project Default Token", CesiumEditorStyle.cesiumButtonStyle);
        }
    }

}
