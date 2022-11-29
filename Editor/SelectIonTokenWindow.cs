using Reinterop;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public enum IonTokenSource
    {
        Create,
        UseExisting,
        Specify
    }

    [ReinteropNativeImplementation("CesiumForUnityNative::SelectIonTokenWindowImpl", "SelectIonTokenWindowImpl.h")]
    public partial class SelectIonTokenWindow : EditorWindow
    {
        public static SelectIonTokenWindow currentWindow = null;

        public static void ShowWindow()
        {
            if (currentWindow == null)
            {
                currentWindow = GetWindow<SelectIonTokenWindow>("Select Cesium ion Token");
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        private IonTokenSource _source;
        private string _createdTokenName = "";
        private List<string> _existingTokenList = new List<string>();
        private string[] _existingTokenArray = { };
        private int _existingTokenIndex;
        private string _specifiedToken = "";

        public static string GetDefaultNewTokenName()
        {
            return Application.productName + " (Created by Cesium For Unity)";
        }

        private void OnEnable()
        {
            CesiumIonSession.Ion().Resume();
            CesiumIonSession.OnTokensUpdated += this.RefreshTokens;

            this._createdTokenName = GetDefaultNewTokenName();
            this._specifiedToken = CesiumRuntimeSettings.defaultIonAccessToken;
            this._source = CesiumIonSession.Ion().IsConnected()
                ? IonTokenSource.Create
                : IonTokenSource.Specify;

            CesiumIonSession.Ion().RefreshTokens();
        }

        private void OnDisable()
        {
            CesiumIonSession.OnTokensUpdated -= this.RefreshTokens;
        }

        public IonTokenSource tokenSource
        {
            get => this._source;
            set
            {
                this._source = value;
            }
        }

        public string createdTokenName
        {
            get => this._createdTokenName;
            set
            {
                this._createdTokenName = value;
            }
        }

        public int selectedExistingTokenIndex
        {
            get => this._existingTokenIndex;
            set
            {
                this._existingTokenIndex = value;
            }
        }

        public string specifiedToken
        {
            get => this._specifiedToken;
            set
            {
                this._specifiedToken = value;
            }
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
                GUILayout.Space(20);
                this.DrawCreateTokenOption();
                GUILayout.Space(20);
                this.DrawUseExistingTokenOption();
            }
            else
            {
                EditorGUILayout.LabelField(
                 "Please connect to Cesium ion to select a token from your " +
                 "account or to create a new token.",
                 EditorStyles.wordWrappedLabel
             );
            }

            GUILayout.Space(20);
            this.DrawSpecifyTokenOption();
            this.DrawActionButton();
        }

        private void DrawCreateTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(this._source == IonTokenSource.Create, "", GUILayout.Width(30)))
            {
                this._source = IonTokenSource.Create;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Create a new token", EditorStyles.boldLabel);
            this._createdTokenName = EditorGUILayout.TextField("Name:", this._createdTokenName);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawUseExistingTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(this._source == IonTokenSource.UseExisting, "", GUILayout.Width(30)))
            {
                this._source = IonTokenSource.UseExisting;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Use an existing token", EditorStyles.boldLabel);
            this._existingTokenIndex = EditorGUILayout.Popup(
                this._existingTokenIndex,
                this._existingTokenArray
            );
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawSpecifyTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(this._source == IonTokenSource.Specify, "", GUILayout.Width(30)))
            {
                this._source = IonTokenSource.Specify;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Specify a token", EditorStyles.boldLabel);
            this._specifiedToken = EditorGUILayout.TextField("Token:", this._specifiedToken);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawActionButton()
        {
            GUILayout.Space(25);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string label = this._source == IonTokenSource.Create
                ? "Create New Project Default Token"
                : "Use as Project Default Token";

            if (GUILayout.Button(label, CesiumEditorStyle.cesiumButtonStyle, GUILayout.Width(400)))
            {
                if (this._source == IonTokenSource.Create)
                {
                    this.CreateToken();
                }
                else if (this._source == IonTokenSource.UseExisting)
                {
                    this.UseExistingToken();
                }
                else
                {
                    this.SpecifyToken();
                }
            };

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public partial void RefreshTokens();

        private partial void CreateToken();
        private partial void UseExistingToken();
        private partial void SpecifyToken();

        public List<string> GetExistingTokenList()
        {
            return this._existingTokenList;
        }

        public void RefreshExistingTokenList()
        {
            this._existingTokenArray = this._existingTokenList.ToArray();
            this.Repaint();
        }
    }
}
