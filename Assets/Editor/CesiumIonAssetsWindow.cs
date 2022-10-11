using System;
using UnityEngine;
using UnityEditor;
using CesiumForUnity;

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

    void Awake()
    {
        if (CesiumIonSession.currentSession == null)
        {
            CesiumIonSession.currentSession = new CesiumIonSession();
        }
    }

    void OnGUI()
    {
        DrawFilterButtons();
        DrawAssetsList();
    }

    void DrawFilterButtons() {
        GUILayout.BeginHorizontal();
        GUILayout.Button("Name");
        GUILayout.Button("Type");
        GUILayout.Button("Date added");
        GUILayout.EndHorizontal();
    }

    void DrawAssetsList()
    {
        Vector2 scrollPosition = Vector2.zero;
        EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.EndScrollView();
    }
}
