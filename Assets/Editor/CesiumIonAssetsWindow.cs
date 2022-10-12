using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace CesiumForUnity
{
    public class CesiumIonAssetsWindow : EditorWindow
    {
        public static CesiumIonAssetsWindow currentWindow = null!;

        [MenuItem("Cesium/Cesium ion Assets")]
        public static void ShowWindow()
        {
            CesiumEditorStyle.Reload();

            if (currentWindow == null)
            {
                // Get existing open window or if none, make a new one docked next to Console window.
                Type siblingWindow = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
                currentWindow = GetWindow<CesiumIonAssetsWindow>("Cesium ion Assets", new Type[] { siblingWindow });
                currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        private TreeViewState _assetsTreeState;
        private IonAssetsTreeView _assetsTreeView;

        void Awake()
        {
            if (CesiumIonSession.currentSession == null)
            {
                CesiumIonSession.currentSession = new CesiumIonSession();
            }

            _assetsTreeState = new TreeViewState();
            _assetsTreeView = new IonAssetsTreeView(_assetsTreeState);
        }

        void OnGUI()
        {
        }
    }
}
