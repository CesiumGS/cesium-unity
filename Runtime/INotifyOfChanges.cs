#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    public interface INotifyOfChanges
    {
#if UNITY_EDITOR
        void NotifyPropertyChanged(SerializedProperty property);
#endif
    }
}
