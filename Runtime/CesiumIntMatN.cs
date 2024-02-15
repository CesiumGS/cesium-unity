using System.Diagnostics;
using System;
using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a mat2x2 with signed integer components. This preserves the exact type
    /// of the integer components so they can be properly converted (or not converted)
    /// to other types.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat2x2,
    /// i16mat2x2, i32mat2x2, and i64mat2x2. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat2x2
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumIntVec2 c0
        {
            get; private set;
        }
        public CesiumIntVec2 c1
        {
            get; private set;
        }

        public CesiumIntMat2x2(CesiumIntVec2 v0, CesiumIntVec2 v1)
        {
            Debug.Assert(v0.componentType == v1.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
        }
        public CesiumIntMat2x2(int2 v0, int2 v1)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.c0 = new CesiumIntVec2(v0);
            this.c1 = new CesiumIntVec2(v1);
        }

        public CesiumIntVec2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.c0;
                    case 1:
                        return this.c1;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat3x3 with signed integer components. This preserves the exact type
    /// of the integer components so they can be properly converted (or not converted)
    /// to other types.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat3x3,
    /// i16mat3x3, i32mat3x3, and i64mat3x3. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat3x3
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumIntVec3 c0
        {
            get; private set;
        }
        public CesiumIntVec3 c1
        {
            get; private set;
        }
        public CesiumIntVec3 c2
        {
            get; private set;
        }

        public CesiumIntMat3x3(CesiumIntVec3 v0, CesiumIntVec3 v1, CesiumIntVec3 v2)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
            this.c2 = v2;
        }

        public CesiumIntMat3x3(int3 v0, int3 v1, int3 v2)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.c0 = new CesiumIntVec3(v0);
            this.c1 = new CesiumIntVec3(v1);
            this.c2 = new CesiumIntVec3(v2);
        }

        public CesiumIntVec3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.c0;
                    case 1:
                        return this.c1;
                    case 2:
                        return this.c2;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    /// Represents a mat4x4 with signed integer components. This preserves the exact type
    /// of the integer components so they can be properly converted (or not converted)
    /// to other types.
    /// </summary>
    /// <remarks>
    /// Internally, this is used to store all signed integer mat types, including i8mat4x4,
    /// i16mat4x4, i32mat4x4, and i64mat4x4. The intended type is conveyed through CesiumMetadataComponentType.
    /// </remarks>
    internal struct CesiumIntMat4x4
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumIntVec4 c0
        {
            get; private set;
        }
        public CesiumIntVec4 c1
        {
            get; private set;
        }
        public CesiumIntVec4 c2
        {
            get; private set;
        }
        public CesiumIntVec4 c3
        {
            get; private set;
        }

        public CesiumIntMat4x4(CesiumIntVec4 v0, CesiumIntVec4 v1, CesiumIntVec4 v2, CesiumIntVec4 v3)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType && v2.componentType == v3.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
            this.c2 = v2;
            this.c3 = v3;
        }

        public CesiumIntMat4x4(int4 v0, int4 v1, int4 v2, int4 v3)
        {
            this.componentType = CesiumMetadataComponentType.Int32;
            this.c0 = new CesiumIntVec4(v0);
            this.c1 = new CesiumIntVec4(v1);
            this.c2 = new CesiumIntVec4(v2);
            this.c3 = new CesiumIntVec4(v3);
        }

        public CesiumIntVec4 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.c0;
                    case 1:
                        return this.c1;
                    case 2:
                        return this.c2;
                    case 3:
                        return this.c3;
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
    internal struct CesiumUintMat2x2
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumUintVec2 c0
        {
            get; private set;
        }
        public CesiumUintVec2 c1
        {
            get; private set;
        }

        public CesiumUintMat2x2(CesiumUintVec2 v0, CesiumUintVec2 v1)
        {
            Debug.Assert(v0.componentType == v1.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
        }

        public CesiumUintMat2x2(uint2 v0, uint2 v1)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.c0 = new CesiumUintVec2(v0);
            this.c1 = new CesiumUintVec2(v1);
        }

        public CesiumUintVec2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return c0;
                    case 1:
                        return c1;
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
    internal struct CesiumUintMat3x3
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumUintVec3 c0
        {
            get; private set;
        }
        public CesiumUintVec3 c1
        {
            get; private set;
        }
        public CesiumUintVec3 c2
        {
            get; private set;
        }

        public CesiumUintMat3x3(CesiumUintVec3 v0, CesiumUintVec3 v1, CesiumUintVec3 v2)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
            this.c2 = v2;
        }

        public CesiumUintMat3x3(uint3 v0, uint3 v1, uint3 v2)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.c0 = new CesiumUintVec3(v0);
            this.c1 = new CesiumUintVec3(v1);
            this.c2 = new CesiumUintVec3(v2);
        }

        public CesiumUintVec3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.c0;
                    case 1:
                        return this.c1;
                    case 2:
                        return this.c2;
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
    internal struct CesiumUintMat4x4
    {
        public CesiumMetadataComponentType componentType
        {
            get; private set;
        }

        // Column-major order
        public CesiumUintVec4 c0
        {
            get; private set;
        }
        public CesiumUintVec4 c1
        {
            get; private set;
        }
        public CesiumUintVec4 c2
        {
            get; private set;
        }
        public CesiumUintVec4 c3
        {
            get; private set;
        }

        public CesiumUintMat4x4(CesiumUintVec4 v0, CesiumUintVec4 v1, CesiumUintVec4 v2, CesiumUintVec4 v3)
        {
            Debug.Assert(v0.componentType == v1.componentType && v1.componentType == v2.componentType && v2.componentType == v3.componentType);
            this.componentType = v0.componentType;
            this.c0 = v0;
            this.c1 = v1;
            this.c2 = v2;
            this.c3 = v3;
        }

        public CesiumUintMat4x4(uint4 v0, uint4 v1, uint4 v2, uint4 v3)
        {
            this.componentType = CesiumMetadataComponentType.Uint32;
            this.c0 = new CesiumUintVec4(v0);
            this.c1 = new CesiumUintVec4(v1);
            this.c2 = new CesiumUintVec4(v2);
            this.c3 = new CesiumUintVec4(v3);
        }

        public CesiumUintVec4 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.c0;
                    case 1:
                        return this.c1;
                    case 2:
                        return this.c2;
                    case 3:
                        return this.c3;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
}