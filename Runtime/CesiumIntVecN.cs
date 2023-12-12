using System;

namespace CesiumForUnity
{

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
}
