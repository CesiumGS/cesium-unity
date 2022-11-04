using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Marks that, in the Editor, the owning object should be notified of changes in this
    /// property via its `INotifyOfChanges` interface. The object that owns the property
    /// must be a `MonoBehaviour` and it must implement `INotifyOfChanges`.
    /// </summary>
    public class NotifyOfChangesAttribute : PropertyAttribute
    {
    }
}
