using System.Collections;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomPropertyDrawer(typeof(NotifyOfChangesAttribute))]
    public class NotifyOfChangesPropertyDrawer : PropertyDrawer
    {
        public NotifyOfChangesPropertyDrawer()
        {
            Debug.Log("Construct!");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                foreach (object target in property.serializedObject.targetObjects)
                {
                    INotifyOfChanges? notify = target as INotifyOfChanges;
                    if (notify == null)
                        continue;

                    MonoBehaviour? mb = target as MonoBehaviour;
                    if (mb == null)
                        continue;

                    mb.StartCoroutine(this.NotifyChanged(notify, property.name));
                }
            }
        }

        private IEnumerator NotifyChanged(INotifyOfChanges receiver, string propertyName)
        {
            Debug.Log("Begin Changed: " + propertyName);
            // Delay one frame before notifying so that the new value can be applied.
            yield return null;

            // Do the actual notification.
            receiver.NotifyPropertyChanged(propertyName);
        }
    }
}
