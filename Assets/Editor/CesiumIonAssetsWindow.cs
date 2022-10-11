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
        // Get existing open window or if none, make a new one docked next to Console window.
        Type siblingWindow = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
        currentWindow = GetWindow<CesiumIonAssetsWindow>("Cesium ion Assets", new Type[] { siblingWindow });

        currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
        currentWindow.Show();
        currentWindow.Focus();
    }

    void Awake()
    {
        if (CesiumIonSession.currentSession == null)
        {
            CesiumIonSession.currentSession = new CesiumIonSession();
        }

        if (CesiumEditorStyle.currentStyle == null)
        {
            CesiumEditorStyle.currentStyle = new CesiumEditorStyle();
        }
    }

    void OnGUI()
    {
    }
}
