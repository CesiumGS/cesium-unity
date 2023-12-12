using System.Diagnostics;

namespace CesiumForUnity
{
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
}