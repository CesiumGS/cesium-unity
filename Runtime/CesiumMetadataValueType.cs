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

    #region Internal VecN type definitions

    /// <summary>
    /// Represents a vec2 with signed integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer vec2 types, including i8vec2,
    /// i16vec2, i32vec2, and i64vec2. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntVec2
    {
        public CesiumMetadataComponentType componentType { get; }

        public Int64 x { get; }
        public Int64 y { get; }

        public CesiumIntVec2(SByte x, SByte y)
        {
            this.componentType = CesiumMetadataComponentType.Int8;
            this.x = x;
            this.y = y;
        }

        public CesiumIntVec2(Int16 x, Int16 y)
        {
            this.componentType = CesiumMetadataComponentType.Int16;
            this.x = x;
            this.y = y;
        }

        public CesiumIntVec2(Int32 x, Int32 y)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.x = x;
            this.y = y;
        }

        public CesiumIntVec2(Int64 x, Int64 y)
        {
            this.componentType = CesiumMetadataComponentType.Int64;
            this.x = x;
            this.y = y;
        }

        public Int64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a vec3 with signed integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer vec3 types, including i8vec3,
    /// i16vec3, i32vec3, and i64vec3. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntVec3
    {
        public CesiumMetadataComponentType componentType { get; }

        public Int64 x { get; }
        public Int64 y { get; }
        public Int64 z { get; }

        public CesiumIntVec3(SByte x, SByte y, SByte z)
        {
            this.componentType = CesiumMetadataComponentType.Int8;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumIntVec3(Int16 x, Int16 y, Int16 z)
        {
            this.componentType = CesiumMetadataComponentType.Int16;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumIntVec3(Int32 x, Int32 y, Int32 z)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumIntVec3(Int64 x, Int64 y, Int64 z)
        {
            this.componentType = CesiumMetadataComponentType.Int64;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Int64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a vec4 with signed integer components. This preserves the accuracy of the integer values,
    /// which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer vec4 types, including i8vec4,
    /// i16vec4, i32vec4, and i64vec4. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntVec4
    {
        public CesiumMetadataComponentType componentType { get; }

        public Int64 x { get; }
        public Int64 y { get; }
        public Int64 z { get; }
        public Int64 w { get; }

        public CesiumIntVec4(SByte x, SByte y, SByte z, SByte w)
        {
            this.componentType = CesiumMetadataComponentType.Int8;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public CesiumIntVec4(Int16 x, Int16 y, Int16 z, Int16 w)
        {
            this.componentType = CesiumMetadataComponentType.Int16;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public CesiumIntVec4(Int32 x, Int32 y, Int32 z, Int32 w)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public CesiumIntVec4(Int64 x, Int64 y, Int64 z, Int64 w)
        {
            this.componentType = CesiumMetadataComponentType.Int64;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Int64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a vec2 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer vec2 types, including u8vec2,
    /// u16vec2, u32vec2, and u64vec2. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntVec2
    {
        public CesiumMetadataComponentType componentType { get; }

        public UInt64 x { get; }
        public UInt64 y { get; }

        public CesiumUIntVec2(Byte x, Byte y)
        {
            this.componentType = CesiumMetadataComponentType.Uint8;
            this.x = x;
            this.y = y;
        }

        public CesiumUIntVec2(UInt16 x, UInt16 y)
        {
            this.componentType = CesiumMetadataComponentType.Uint16;
            this.x = x;
            this.y = y;
        }

        public CesiumUIntVec2(UInt32 x, UInt32 y)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.x = x;
            this.y = y;
        }

        public CesiumUIntVec2(UInt64 x, UInt64 y)
        {
            this.componentType = CesiumMetadataComponentType.Uint64;
            this.x = x;
            this.y = y;
        }

        public UInt64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a vec3 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer vec3 types, including u8vec3,
    /// u16vec3, u32vec3, and u64vec3. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntVec3
    {
        public CesiumMetadataComponentType componentType { get; }

        public UInt64 x { get; }
        public UInt64 y { get; }
        public UInt64 z { get; }

        public CesiumUIntVec3(Byte x, Byte y, Byte z)
        {
            this.componentType = CesiumMetadataComponentType.Uint8;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumUIntVec3(UInt16 x, UInt16 y, UInt16 z)
        {
            this.componentType = CesiumMetadataComponentType.Uint16;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumUIntVec3(UInt32 x, UInt32 y, UInt32 z)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CesiumUIntVec3(UInt64 x, UInt64 y, UInt64 z)
        {
            this.componentType = CesiumMetadataComponentType.Uint64;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public UInt64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a vec4 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer vec4 types, including u8vec4,
    /// u16vec4, u32vec4, and u64vec4. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntVec4
    {
        public CesiumMetadataComponentType componentType { get; }

        public UInt64 x { get; }
        public UInt64 y { get; }
        public UInt64 z { get; }
        public UInt64 w { get; }

        public CesiumUIntVec4(Byte x, Byte y, Byte z, Byte w)
        {
            this.componentType = CesiumMetadataComponentType.Uint8;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public CesiumUIntVec4(UInt16 x, UInt16 y, UInt16 z, UInt16 w)
        {
            this.componentType = CesiumMetadataComponentType.Uint16;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public CesiumUIntVec4(UInt32 x, UInt32 y, UInt32 z, UInt32 w)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public CesiumUIntVec4(UInt64 x, UInt64 y, UInt64 z, UInt64 w)
        {
            this.componentType = CesiumMetadataComponentType.Uint64;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public UInt64 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
    #endregion

    #region Internal MatN type definitions

    /// <summary>
    /// Represents a mat2x2 with signed integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat2x2,
    /// i16mat2x2, i32mat2x2, and i64mat2x2. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat2x2
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumIntVec2[] value { get; }

        public CesiumIntMat2x2(CesiumIntVec2 v0, CesiumIntVec2 v1)
        {
            Debug.Assert(v0.componentType == v1.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumIntVec2[] { v0, v1 };
        }

        public CesiumIntVec2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat3x3 with signed integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat3x3,
    /// i16mat3x3, i32mat3x3, and i64mat3x3. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat3x3
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumIntVec3[] value { get; }

        public CesiumIntMat3x3(CesiumIntVec3 v0, CesiumIntVec3 v1, CesiumIntVec3 v2)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumIntVec3[] { v0, v1, v2 };
        }

        public CesiumIntVec3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    case 2:
                        return this.value[2];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat4x4 with signed integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat4x4,
    /// i16mat4x4, i32mat4x4, and i64mat4x4. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat4x4
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumIntVec4[] value { get; }

        public CesiumIntMat4x4(CesiumIntVec4 v0, CesiumIntVec4 v1, CesiumIntVec4 v2, CesiumIntVec4 v3)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType && v2.componentType == v3.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumIntVec4[] { v0, v1, v2, v3 };
        }

        public CesiumIntVec4 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    case 2:
                        return this.value[2];
                    case 3:
                        return this.value[3];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat2x2 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer mat types, including u8mat2x2,
    /// u16mat2x2, u32mat2x2, and u64mat2x2. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntMat2x2
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumUIntVec2[] value { get; }

        public CesiumUIntMat2x2(CesiumUIntVec2 v0, CesiumUIntVec2 v1)
        {
            Debug.Assert(v0.componentType == v1.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumUIntVec2[] { v0, v1 };
        }

        public CesiumUIntVec2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat3x3 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer mat types, including u8mat3x3,
    /// u16mat3x3, u32mat3x3, and u64mat3x3. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntMat3x3
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumUIntVec3[] value { get; }

        public CesiumUIntMat3x3(CesiumUIntVec3 v0, CesiumUIntVec3 v1, CesiumUIntVec3 v2)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumUIntVec3[] { v0, v1, v2 };
        }

        public CesiumUIntVec3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    case 2:
                        return this.value[2];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat4x4 with unsigned integer components. This preserves the accuracy of the integer
    /// values, which could otherwise lose precision if represented as doubles.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all unsigned integer mat types, including u8mat4x4,
    /// u16mat4x4, u32mat4x4, and u64mat4x4. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumUIntMat4x4
    {
        public CesiumMetadataComponentType componentType { get; }

        private CesiumUIntVec4[] value { get; }

        public CesiumUIntMat4x4(CesiumUIntVec4 v0, CesiumUIntVec4 v1, CesiumUIntVec4 v2, CesiumUIntVec4 v3)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType && v2.componentType == v3.componentType);
            this.componentType = v0.componentType;
            this.value = new CesiumUIntVec4[] { v0, v1, v2, v3 };
        }

        public CesiumUIntVec4 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.value[0];
                    case 1:
                        return this.value[1];
                    case 2:
                        return this.value[2];
                    case 3:
                        return this.value[3];
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
    #endregion

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
                return new CesiumMetadataValueType(array.valueType.type, array.valueType.componentType, true);
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
                case CesiumUIntVec2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec2, (inObject as CesiumUIntVec2?).Value.componentType, false);
                case CesiumUIntVec3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec3, (inObject as CesiumUIntVec3?).Value.componentType, false);
                case CesiumUIntVec4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Vec4, (inObject as CesiumUIntVec4?).Value.componentType, false);
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
                case CesiumUIntMat2x2:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat2, (inObject as CesiumUIntMat2x2?).Value.componentType, false);
                case CesiumUIntMat3x3:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat3, (inObject as CesiumUIntMat3x3?).Value.componentType, false);
                case CesiumUIntMat4x4:
                    return new CesiumMetadataValueType(CesiumMetadataType.Mat4, (inObject as CesiumUIntMat4x4?).Value.componentType, false);
                case System.String:
                    return new CesiumMetadataValueType(CesiumMetadataType.String, CesiumMetadataComponentType.None, false);
                default:
                    return new CesiumMetadataValueType();
            }
        }
        #endregion
    }
}
