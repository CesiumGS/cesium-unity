#include "CesiumMetadataValueImpl.h"

#include <CesiumGltf/MetadataConversions.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/System/Object.h>

#include <string_view>

using namespace CesiumGltf;
using namespace DotNet;
using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {

namespace {

CesiumMetadataValueImpl::ValueType getNativeBooleanValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value) {
  std::optional<bool> maybeBoolean =
      CesiumForUnity::CesiumMetadataValue::GetObjectAsBoolean(
          value.valueImpl());
  if (maybeBoolean) {
    return *maybeBoolean;
  }

  return std::monostate();
}

template <typename T>
CesiumMetadataValueImpl::ValueType
getNativeScalarValue(const DotNet::System::Object& object) {
  std::optional<T> maybeValue;
  if constexpr (std::is_same_v<T, int8_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsSByte(object);
  } else if constexpr (std::is_same_v<T, uint8_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsByte(object);
  } else if constexpr (std::is_same_v<T, int16_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt16(object);
  } else if constexpr (std::is_same_v<T, uint16_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt16(object);
  } else if constexpr (std::is_same_v<T, int32_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt32(object);
  } else if constexpr (std::is_same_v<T, uint32_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt32(object);
  } else if constexpr (std::is_same_v<T, int64_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt64(object);
  } else if constexpr (std::is_same_v<T, uint64_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt64(object);
  } else if constexpr (std::is_same_v<T, float>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat(object);
  } else if constexpr (std::is_same_v<T, double>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble(object);
  }

  if (maybeValue) {
    return *maybeValue;
  }

  return std::monostate();
}

CesiumMetadataValueImpl::ValueType
getNativeScalarValue(const DotNet::CesiumForUnity::CesiumMetadataValue& value) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Scalar);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeScalarValue<int8_t>(value.valueImpl());
  case CesiumMetadataComponentType::Uint8:
    return getNativeScalarValue<uint8_t>(value.valueImpl());
  case CesiumMetadataComponentType::Int16:
    return getNativeScalarValue<int16_t>(value.valueImpl());
  case CesiumMetadataComponentType::Uint16:
    return getNativeScalarValue<uint16_t>(value.valueImpl());
  case CesiumMetadataComponentType::Int32:
    return getNativeScalarValue<int32_t>(value.valueImpl());
  case CesiumMetadataComponentType::Uint32:
    return getNativeScalarValue<uint32_t>(value.valueImpl());
  case CesiumMetadataComponentType::Int64:
    return getNativeScalarValue<int64_t>(value.valueImpl());
  case CesiumMetadataComponentType::Uint64:
    return getNativeScalarValue<uint64_t>(value.valueImpl());
  case CesiumMetadataComponentType::Float32:
    return getNativeScalarValue<float>(value.valueImpl());
  case CesiumMetadataComponentType::Float64:
    return getNativeScalarValue<double>(value.valueImpl());
  default:
    return std::monostate();
  }
}

CesiumMetadataValueImpl::ValueType
getNativeStringValue(const DotNet::CesiumForUnity::CesiumMetadataValue& value) {
  DotNet::System::String string =
      CesiumForUnity::CesiumMetadataValue::GetObjectAsString(value.valueImpl());
  if (string == nullptr) {
    return std::monostate();
  }

  return string.ToStlString();
}

} // namespace

/*static*/ CesiumMetadataValueImpl::ValueType
CesiumMetadataValueImpl::getNativeValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value) {
  CesiumForUnity::CesiumMetadataValueType valueType = value.valueType();
  switch (valueType.type) {
  case CesiumForUnity::CesiumMetadataType::Boolean:
    return getNativeBooleanValue(value);
  case CesiumForUnity::CesiumMetadataType::Scalar:
    return getNativeScalarValue(value);
  case CesiumForUnity::CesiumMetadataType::String:
    return getNativeStringValue(value);
  default:
    return std::monostate();
  }
}

/*static*/ bool CesiumMetadataValueImpl::ConvertToBoolean(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    bool defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> bool {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<bool, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ int8_t CesiumMetadataValueImpl::ConvertToSByte(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int8_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> int8_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<int8_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ uint8_t CesiumMetadataValueImpl::ConvertToByte(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint8_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> uint8_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<uint8_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ int16_t CesiumMetadataValueImpl::ConvertToInt16(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int16_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> int16_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<int16_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ uint16_t CesiumMetadataValueImpl::ConvertToUInt16(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint16_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> uint16_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<uint16_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ int32_t CesiumMetadataValueImpl::ConvertToInt32(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int32_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> int32_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<int32_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ uint32_t CesiumMetadataValueImpl::ConvertToUInt32(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint32_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> uint32_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<uint32_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}
/*static*/ int64_t CesiumMetadataValueImpl::ConvertToInt64(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int64_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> int64_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<int64_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ uint64_t CesiumMetadataValueImpl::ConvertToUInt64(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint64_t defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> uint64_t {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          return MetadataConversions<uint64_t, decltype(trueValue)>::convert(
                     trueValue)
              .value_or(defaultValue);
        }
      },
      nativeValue);
}

/*static*/ DotNet::System::String CesiumMetadataValueImpl::ConvertToString(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    DotNet::System::String defaultValue) {
  ValueType nativeValue = getNativeValue(value);
  return std::visit(
      [&defaultValue](auto trueValue) -> System::String {
        if constexpr (std::is_same_v<decltype(trueValue), std::monostate>) {
          return defaultValue;
        } else {
          auto maybeString =
              MetadataConversions<std::string, decltype(trueValue)>::convert(
                  trueValue);
          return maybeString ? System::String(*maybeString) : defaultValue;
        }
      },
      nativeValue);
}
} // namespace CesiumForUnityNative
