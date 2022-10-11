using System;
using UnityEngine;
using UnityEditor;
using CesiumForUnity;

public class CesiumIonAssetsWindow : EditorWindow
{

    private void Awake()
    {

    }

    [MenuItem("Cesium/Cesium ion Assets")]
    public static void ShowWindow()
    {
        // Get existing open window or if none, make a new one docked next to Console window.
        Type siblingWindow = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
        CesiumIonAssetsWindow ionAssetsWindow = GetWindow<CesiumIonAssetsWindow>("Cesium ion Assets", new Type[] { siblingWindow });

        //cesiumWindow.titleContent.image; // add cesium icon here
        ionAssetsWindow.Show();
        ionAssetsWindow.Focus();

    }

    void OnGUI()
    {
    }
}
