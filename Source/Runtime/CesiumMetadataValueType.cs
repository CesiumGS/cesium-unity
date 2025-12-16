using UnityEngine;
using Unity.Mathematics;
using System;

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
        String,
        Boolean,
        Enum
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
        #region Fields
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
        #endregion

        #region Constructor
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
        #endregion

        #region Static methods
        public static CesiumMetadataValueType GetValueType(System.Object inObject)
        {
            if (inObject == null)
            {
                return new CesiumMetadataValueType();
            }

            if (inObject.GetType() == typeof(CesiumPropertyArray))
            {
                CesiumPropertyArray array = inObject as CesiumPropertyArray;
                return new CesiumMetadataValueType(array.elementValueType.type, array.elementValueType.componentType, true);
            }

            switch (inObject)
            {
                case System.Boolean:
                    return new CesiumMetadataValueType(CesiumMetadataType.Boolean, CesiumMetadataComponentType.None, false);
                case System.SByte:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Int8, false);
                case Byte:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Uint8, false);
                case Int16:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Int16, false);
                case UInt16:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Uint16, false);
                case Int32:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Int32, false);
                case UInt32:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Uint32, false);
                case Int64:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Int64, false);
                case UInt64:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Uint64, false);
                case float:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Float32, false);
                case double:
                    return new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Float64, false);
                case int2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, CesiumMetadataComponentType.Int32, false);
                case int3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, CesiumMetadataComponentType.Int32, false);
                case int4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, CesiumMetadataComponentType.Int32, false);
                case uint2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, CesiumMetadataComponentType.Uint32, false);
                case uint3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, CesiumMetadataComponentType.Uint32, false);
                case uint4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, CesiumMetadataComponentType.Uint32, false);
                case float2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, CesiumMetadataComponentType.Float32, false);
                case float3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, CesiumMetadataComponentType.Float32, false);
                case float4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, CesiumMetadataComponentType.Float32, false);
                case double2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, CesiumMetadataComponentType.Float64, false);
                case double3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, CesiumMetadataComponentType.Float64, false);
                case double4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, CesiumMetadataComponentType.Float64, false);
                case int2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, CesiumMetadataComponentType.Int32, false);
                case int3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, CesiumMetadataComponentType.Int32, false);
                case int4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, CesiumMetadataComponentType.Int32, false);
                case CesiumIntVec2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, (inObject as CesiumIntVec2?).Value.componentType, false);
                case CesiumIntVec3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, (inObject as CesiumIntVec3?).Value.componentType, false);
                case CesiumIntVec4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, (inObject as CesiumIntVec4?).Value.componentType, false);
                case CesiumUintVec2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, (inObject as CesiumUintVec2?).Value.componentType, false);
                case CesiumUintVec3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, (inObject as CesiumUintVec3?).Value.componentType, false);
                case CesiumUintVec4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, (inObject as CesiumUintVec4?).Value.componentType, false);
                case uint2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, CesiumMetadataComponentType.Uint32, false);
                case uint3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, CesiumMetadataComponentType.Uint32, false);
                case uint4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, CesiumMetadataComponentType.Uint32, false);
                case float2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, CesiumMetadataComponentType.Float32, false);
                case float3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, CesiumMetadataComponentType.Float32, false);
                case float4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, CesiumMetadataComponentType.Float32, false);
                case double2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, CesiumMetadataComponentType.Float64, false);
                case double3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, CesiumMetadataComponentType.Float64, false);
                case double4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, CesiumMetadataComponentType.Float64, false);
                case CesiumIntMat2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, (inObject as CesiumIntMat2x2?).Value.componentType, false);
                case CesiumIntMat3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, (inObject as CesiumIntMat3x3?).Value.componentType, false);
                case CesiumIntMat4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, (inObject as CesiumIntMat4x4?).Value.componentType, false);
                case CesiumUintMat2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, (inObject as CesiumUintMat2x2?).Value.componentType, false);
                case CesiumUintMat3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, (inObject as CesiumUintMat3x3?).Value.componentType, false);
                case CesiumUintMat4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, (inObject as CesiumUintMat4x4?).Value.componentType, false);
                case System.String:
                    return new CesiumMetadataValueType(CesiumMetadataType.String, CesiumMetadataComponentType.None, false);
                default:
                    return new CesiumMetadataValueType();
            }
        }
        #endregion
    }
}
