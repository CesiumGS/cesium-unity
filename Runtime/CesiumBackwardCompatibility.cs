using UnityEngine;
using System;
using UnityEditorInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    public interface IBackwardCompatibilityComponent<TNew>
    {
        public string UpgradedVersion { get; }
        public void Upgrade(GameObject gameObject, TNew newComponent);
    }

    public static class CesiumBackwardCompatibility<TNew>
        where TNew : MonoBehaviour
    {
#if UNITY_EDITOR
        public static TNew Upgrade<TOld>(TOld oldComponent)
            where TOld : MonoBehaviour, IBackwardCompatibilityComponent<TNew>
        {
            GameObject go = oldComponent.gameObject;

            Debug.Log("Upgrading " + typeof(TNew).Name + " on game object " + go.name + " from Cesium for Unity " + oldComponent.UpgradedVersion + ".");

            TNew newComponent = go.AddComponent<TNew>();

            try
            {
                oldComponent.Upgrade(go, newComponent);

                // Destroy the old component and move the new one where the old
                // one used to be.
                Component[] components = go.GetComponents<Component>();
                int indexOfOriginal = Array.IndexOf(components, oldComponent);
                int indexOfUpgraded = Array.IndexOf(components, newComponent);

                Helpers.Destroy(oldComponent);

                if (indexOfOriginal >= 0 && indexOfUpgraded >= 0)
                {
                    if (indexOfUpgraded > indexOfOriginal)
                        --indexOfUpgraded;
                    while (indexOfUpgraded > indexOfOriginal)
                    {
                        ComponentUtility.MoveComponentUp(newComponent);
                        --indexOfUpgraded;
                    }
                }

                // Unity seems to ignore SetDirty from within OnEnable. So do it later.
                EditorApplication.CallbackFunction markDirty = null;
                markDirty = new EditorApplication.CallbackFunction(() =>
                {
                    EditorUtility.SetDirty(go);
                    EditorApplication.update -= markDirty;
                });

                EditorApplication.update += markDirty;
            }
            catch (Exception e)
            {
                Debug.LogError("Upgrading failed with an exception: " + e.ToString());
            }

            return newComponent;
        }
#endif
    }
}
