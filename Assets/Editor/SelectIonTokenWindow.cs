using Reinterop;
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
        public static SelectIonTokenWindow currentWindow = null!;

        public static void ShowWindow()
        {
            CesiumEditorStyle.Reload();

            if (currentWindow == null)
            {
                currentWindow = GetWindow<SelectIonTokenWindow>("Select a Cesium ion Token");
                currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        private IonTokenSource _source;
        private string _createdTokenName = "";
        private string[] _existingTokensList = { };
        private int _existingTokenIndex;
        private string _specifiedToken = "";

        private void OnEnable()
        {
            CesiumIonSession.Ion().Resume();
            CesiumEditorStyle.Reload();

            _createdTokenName = IonTokenSelector.GetDefaultNewTokenName();
        }

        public IonTokenSource GetTokenSource()
        {
            return _source;
        }

        public void SetTokenSource(IonTokenSource source)
        {
            _source = source;
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
                DrawCreateTokenOption();
                GUILayout.Space(20);
                DrawUseExistingTokenOption();
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
            DrawSpecifyTokenOption();
            DrawActionButton();
        }

        private void DrawCreateTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(_source == IonTokenSource.Create, "", GUILayout.Width(30)))
            {
                _source = IonTokenSource.Create;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Create a new token", EditorStyles.boldLabel);
            _createdTokenName = EditorGUILayout.TextField("Name:", _createdTokenName);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawUseExistingTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(_source == IonTokenSource.UseExisting, "", GUILayout.Width(30)))
            {
                _source = IonTokenSource.UseExisting;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Use an existing token", EditorStyles.boldLabel);
            _existingTokenIndex = EditorGUILayout.Popup(_existingTokenIndex, _existingTokensList);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawSpecifyTokenOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Toggle(_source == IonTokenSource.Specify, "", GUILayout.Width(30)))
            {
                _source = IonTokenSource.Specify;
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Specify a token", EditorStyles.boldLabel);
            _specifiedToken = EditorGUILayout.TextField("Token:", _specifiedToken);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawActionButton()
        {
            GUILayout.Space(25);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string label = _source == IonTokenSource.Create
                ? "Create New Project Default Token"
                : "Use as Project Default Token";

            if (GUILayout.Button(label,
                     CesiumEditorStyle.cesiumButtonStyle,
                     GUILayout.Width(400)))
            {
                if (_source == IonTokenSource.Create)
                {
                    CreateToken(_createdTokenName);
                }
                else if (_source == IonTokenSource.UseExisting)
                {
                    UseExistingToken(_existingTokenIndex);
                }
                else
                {
                    SpecifyToken(_specifiedToken);
                }
            };

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public partial void RefreshTokens();

        public partial void CreateToken(string name);

        public partial void UseExistingToken(int tokenIndex);

        public partial void SpecifyToken(string token);

        public void SetExistingTokenList(string[] tokens)
        {
            _existingTokensList = tokens;
        }
    }
}
