using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the type of a property in EXT_structural_metadata. 
    /// </summary>
    public enum CesiumMetadataType
    {
        Invalid = 0,
        Scalar,
        Vec2,
        Vec3,
        Vec4,
        Mat2,
        Mat3,
        Mat4,
        Boolean,
        Enum,
        String
    }

    /// <summary>
    /// Identifies the component type of a property in EXT_structural_metadata. 
    /// Only applicable if the property has a Scalar, VecN, or MatN type.
    /// </summary>
    public enum CesiumMetadataComponentType
    {
        None = 0,
        Int8,
        Uint8,
        Int16,
        Uint16,
        Int32,
        Uint32,
        Int64,
        Uint64,
        Float32,
        Float64
    }

    /// <summary>
    /// Represents the value type of a metadata value or property, akin to the 
    /// property types in EXT_structural_metadata.
    /// </summary>
    public struct CesiumMetadataValueType
    {
        /// <summary>
        /// The type of the metadata property or value.
        /// </summary>
        public CesiumMetadataType type;
        
        /// <summary>
        /// The component of the metadata property or value. Only applies when 
        /// the type is a Scalar, VecN, or MatN type.
        /// </summary>
        public CesiumMetadataComponentType componentType;
        
        /// <summary>
        /// Whether or not this represents an array containing elements of the 
        /// specified types.
        /// </summary>
        public bool isArray;

        /// <summary>
        /// Constructs a metadata value type from the given parameters.
        /// </summary>
        /// <param name="type">The type of the metadata property or value.</param>
        /// <param name="componentType"> The component type of the metadata property or value.</param>
        /// <param name="isArray">Whether or not the property or value is an array.</param>
        public CesiumMetadataValueType(
            CesiumMetadataType type, CesiumMetadataComponentType componentType, bool isArray)
        {
            this.type = type;
            this.componentType = componentType;
            this.isArray = isArray;
        }
    }
}
