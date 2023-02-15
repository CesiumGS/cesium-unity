namespace CesiumForUnity
{
    /// <summary>
    /// An interface for an object with a <see cref="Restart"/> method, allowing the state of the
    /// object to be reinitialized from its serialized properties.
    /// </summary>
    public interface ICesiumRestartable
    {
        /// <summary>
        /// Completely re-initializes the state of this object from its serialized properties. It
        /// is not usually necessary to call directly. It can sometimes be useful after Unity has
        /// modified the private, serializable fields of this instance. For example: after an
        /// undo or redo operation.
        /// </summary>
        void Restart();
    }
}
