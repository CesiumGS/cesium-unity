using System.Collections;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomPropertyDrawer(typeof(NotifyOfChangesAttribute))]
    public class NotifyOfChangesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                property.serializedObject.ApplyModifiedProperties();
                foreach (object target in property.serializedObject.targetObjects)
                {
                    INotifyOfChanges? notify = target as INotifyOfChanges;
                    if (notify == null)
                        continue;

                    notify.NotifyPropertyChanged(property);
                }
            }
        }
    }
}
