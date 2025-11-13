using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CesiumForUnity;

public class AppDomainTest : MonoBehaviour
{
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }
    
    // void OnEnable()
    // {
    //     Debug.Log($"Adding event handlers");
    //     AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    //     AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    // }
    //
    // void OnDisable()
    // {
    //     Debug.Log("Removing event handlers");
    //     AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    //     AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
    // }

    private static void OnBeforeAssemblyReload()
    {
        Debug.Log("BEFORE assembly reload");

        var tilesets = Object.FindObjectsByType<Cesium3DTileset>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {tilesets.Length} tilesets");
        
        foreach (var ts in tilesets)
        {
            ts.WaitUntilIdle();
        }
        
        Debug.Log("Done");
    }

    private static void OnAfterAssemblyReload()
    {
        Debug.Log("After assembly reload");
    }
}
