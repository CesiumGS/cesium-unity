using NativeScript;
using UnityEngine;
// using UnityEditor;

namespace CesiumForUnity
{

public class Plugin
{
    static Plugin()
    {
        Debug.Log("BEFORE INIT!!!");
        Bindings.Open(16 * 1024 * 1024);
        Debug.Log("AFTER INIT!!!");
    }

    // [InitializeOnLoadMethod]
    // [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        // This method does nothing, but by calling it we guarantee the static
        // constructor is invoked exactly once.
    }

    // public static void Thingify()
    // {
    //     Debug.Log("BEFORE INIT!!!");
    //     Debug.Log(Application.dataPath);
    //     Debug.Log("Wat");
    //     try
    //     {
    //     Bindings.Open(16 * 1024 * 1024);
    //     } catch (System.Exception e)
    //     {
    //         Debug.Log("Exception: " + e.ToString());
    //     }
    //     Debug.Log("AFTER INIT!!!");
    //     // This method does nothing, but by calling it we guarantee the static
    //     // constructor is invoked exactly once.
    // }
}

}