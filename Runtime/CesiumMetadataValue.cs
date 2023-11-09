using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the value type of a metadata value or property, akin to the 
    /// property types in EXT_structural_metadata.
    /// </summary>
    public class CesiumMetadataValue<T>
    {
        public T value { get; set; }

        public CesiumMetadataValue(T value) { this.value = value; }
    }
}
