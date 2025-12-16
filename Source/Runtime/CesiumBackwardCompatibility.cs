using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace CesiumForUnity
{
    public interface IBackwardCompatibilityComponent<TNew>
    {
        public string VersionToBeUpgraded { get; }
        public void Upgrade(GameObject gameObject, TNew newComponent);
    }

    public static class CesiumBackwardCompatibility<TNew>
        where TNew : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Dictionary<MonoBehaviour, MonoBehaviour> _instanceMap = null;

        public static TNew Upgrade<TOld>(TOld oldComponent)
            where TOld : MonoBehaviour, IBackwardCompatibilityComponent<TNew>
        {
            GameObject go = oldComponent.gameObject;

            Debug.Log("Upgrading " + typeof(TNew).Name + " on game object \"" + go.name + "\" from Cesium for Unity " + oldComponent.VersionToBeUpgraded + ".");

            TNew newComponent = go.AddComponent<TNew>();

            try
            {
                oldComponent.Upgrade(go, newComponent);

                if (_instanceMap == null)
                {
                    _instanceMap = new Dictionary<MonoBehaviour, MonoBehaviour>();
                    EditorApplication.update += UpdateScene;
                }

                _instanceMap.Add(oldComponent, newComponent);
            }
            catch (Exception e)
            {
                Debug.LogError("Upgrading failed with an exception: " + e.ToString());
            }

            return newComponent;
        }

        private static void UpdateScene()
        {
            // Search all scenes for instances of the old object (dictionary key), and replace it
            // with the new object (dictionary value).
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                ReplaceReferencesInScene(scene);
            }

            foreach (var kvp in _instanceMap)
            {
                MonoBehaviour oldComponent = kvp.Key;
                MonoBehaviour newComponent = kvp.Value;
                GameObject go = newComponent.gameObject;

                // Unity seems to ignore SetDirty from within OnEnable, so we do it here.
                EditorUtility.SetDirty(go);

                // Destroy the old component and move the new one where the old
                // one used to be.
                Component[] components = go.GetComponents<Component>();
                int indexOfOriginal = Array.IndexOf(components, oldComponent);
                int indexOfUpgraded = Array.IndexOf(components, newComponent);

                UnityEngine.Object.DestroyImmediate(oldComponent, true);

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

            }

            _instanceMap = null;
            EditorApplication.update -= UpdateScene;
        }

        private static void ReplaceReferencesInScene(Scene scene)
        {
            GameObject[] gameObjects = scene.GetRootGameObjects();
            foreach (GameObject go in gameObjects)
            {
                ReplaceReferencesInGameObject(go);
            }
        }

        private static List<Component> _componentList = new List<Component>();

        private static void ReplaceReferencesInGameObject(GameObject go)
        {
            go.GetComponents<Component>(_componentList);
            foreach (Component component in _componentList)
            {
                // Skip the components that are about to be removed.
                if (component is MonoBehaviour m && _instanceMap.ContainsKey(m))
                    continue;

                ReplaceReferencesInComponent(component);
            }

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform childTransform = transform.GetChild(i);
                ReplaceReferencesInGameObject(childTransform.gameObject);
            }
        }

        private static void ReplaceReferencesInComponent(Component component)
        {
            SerializedObject so = new SerializedObject(component);
            SerializedProperty property = so.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ExposedReference)
                {
                    UnityEngine.Object value = property.exposedReferenceValue;

                    MonoBehaviour replacement;
                    if (value is MonoBehaviour m && _instanceMap.TryGetValue(m, out replacement))
                    {
                        Debug.Log("Updating property " + property.propertyPath + " with upgraded component.");
                        property.exposedReferenceValue = replacement;
                    }
                }
                else if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    UnityEngine.Object value = property.objectReferenceValue;

                    MonoBehaviour replacement;
                    if (value is MonoBehaviour m && _instanceMap.TryGetValue(m, out replacement))
                    {
                        Debug.Log("Updating property \"" + property.propertyPath + "\" on component \"" + component.name + "\" with upgraded component.");
                        property.objectReferenceValue = replacement;
                    }
                }
            }
            so.ApplyModifiedProperties();
        }

#endif
    }
}
