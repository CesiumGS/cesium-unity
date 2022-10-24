using System;
using Reinterop;

namespace CesiumForUnity
{
    public enum MetadataType
    {
        None,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        Boolean,
        String,
        Array
    }
    [ReinteropNativeImplementation("CesiumForUnityNative::MetadataPropertyImpl", "MetadataPropertyImpl.h")]
    public partial class MetadataProperty
    {
        public partial string GetPropertyName();
        public partial bool IsNormalized();
        public partial int GetComponentCount();
        public partial void GetComponent(MetadataProperty property, int index);
        public partial MetadataType GetComponentType();
        public partial MetadataType GetMetadataType();
        public partial sbyte GetInt8(sbyte defaultValue);
        public partial byte GetUInt8(byte defaultValue);
        public partial Int16 GetInt16(Int16 defaultValue);
        public partial UInt16 GetUInt16(UInt16 defaultValue);
        public partial Int32 GetInt32(Int32 defaultValue);
        public partial UInt32 GetUInt32(UInt32 defaultValue);
        public partial Int64 GetInt64(Int64 defaultValue);
        public partial UInt64 GetUInt64(UInt64 defaultValue);
        public partial float GetFloat32(float defaultValue);
        public partial double GetFloat64(double defaultValue);
        public partial Boolean GetBoolean(Boolean defaultValue);
        public partial String GetString(String defaultValue);

    }
}
