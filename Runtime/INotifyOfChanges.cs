using UnityEditor;

namespace CesiumForUnity
{
    public interface INotifyOfChanges
    {
        void NotifyPropertyChanged(SerializedProperty property);
    }
}
