using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TestReinterop
{
    static TestReinterop()
    {
        Reinterop.ReinteropInitializer.Initialize();
    }
}
