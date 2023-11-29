using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumIonServerUI
    {
        public static void Selector()
        {
            CesiumIonServer server = CesiumIonServerManager.instance.Current;
            CesiumIonServer newServer = (CesiumIonServer)EditorGUILayout.ObjectField(server, typeof(CesiumIonServer), false);
            if (server != newServer)
            {
                CesiumIonServerManager.instance.Current = newServer;
            }
        }
    }
}
