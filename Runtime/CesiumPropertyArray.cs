using UnityEngine;
using System;
using Reinterop;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the value type of a metadata value or property, akin to the 
    /// property types in EXT_structural_metadata.
    /// </summary>
    //[ReinteropNativeImplementation("CesiumForUnityNative::CesiumPropertyArrayImpl", "CesiumPropertyArrayImpl.h")]
    public partial class CesiumPropertyArray
    {
        internal CesiumMetadataValue[] values { get; set; }

        internal CesiumPropertyArray() { }

        internal CesiumPropertyArray(CesiumMetadataValue[] values)
        {
            this.values = values;
        }

        /// <summary>
        /// The value type of the elements in the array. Some of these types are
        /// not accessible from Unity, but can be converted to a close-fitting type.
        /// </summary>
        public CesiumMetadataValueType valueType
        {
            get; internal set;
        }

        /// <summary>
        /// Gets the number of elements in the array. Returns 0 if the elements have 
        /// an unknown type.
        /// </summary>
        public Int64 length
        {
            get { return this.values.LongLength; }
        }

        /// <summary>
        /// Retrieves an element from the array as a CesiumMetadataValue. The value
        /// can then be retrieved as a specific C# or Unity type.
        /// 
        /// If the index is out-of-bounds, this returns a bogus CesiumMetadataValue 
        /// of an unknown type.
        /// </summary>
        /// <param name="index">The desired index</param>
        /// <returns>The element as a CesiumMetadataValue.</returns>
        public CesiumMetadataValue this[int index]
        {
            get
            {
                if (index < 0 || index >= this.length)
                {
                    return new CesiumMetadataValue();
                }

                return values[index];
            }
        }

    }
}
