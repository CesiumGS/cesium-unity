#include "CesiumFeatureImpl.h"

#include "CesiumGltf/PropertyType.h"
#include "CesiumGltf/PropertyTypeTraits.h"
#include "CesiumUtility/JsonValue.h"

#include <DotNet/System/String.h>
#include <glm/common.hpp>

#include <algorithm>

using namespace CesiumForUnityNative;

namespace {

::DotNet::CesiumForUnity::MetadataType
GetMetadataType(CesiumGltf::PropertyType type) {
  switch (type) {
  case CesiumGltf::PropertyType::Int8:
    return DotNet::CesiumForUnity::MetadataType::Int8;
  case CesiumGltf::PropertyType::Uint8:
    return DotNet::CesiumForUnity::MetadataType::UInt8;
  case CesiumGltf::PropertyType::Int16:
    return DotNet::CesiumForUnity::MetadataType::Int16;
  case CesiumGltf::PropertyType::Uint16:
    return DotNet::CesiumForUnity::MetadataType::UInt16;
  case CesiumGltf::PropertyType::Int32:
    return DotNet::CesiumForUnity::MetadataType::Int32;
  case CesiumGltf::PropertyType::Uint32:
    return DotNet::CesiumForUnity::MetadataType::UInt32;
  case CesiumGltf::PropertyType::Int64:
    return DotNet::CesiumForUnity::MetadataType::Int64;
  case CesiumGltf::PropertyType::Uint64:
    return DotNet::CesiumForUnity::MetadataType::UInt64;
  case CesiumGltf::PropertyType::Float32:
    return DotNet::CesiumForUnity::MetadataType::Float;
  case CesiumGltf::PropertyType::Float64:
    return DotNet::CesiumForUnity::MetadataType::Double;
  case CesiumGltf::PropertyType::Boolean:
    return DotNet::CesiumForUnity::MetadataType::Boolean;
  case CesiumGltf::PropertyType::String:
    return DotNet::CesiumForUnity::MetadataType::String;
  default:
    return DotNet::CesiumForUnity::MetadataType::None;
  }
}

template <typename TTo, typename TFrom>
static TTo convertToFloat(TFrom from, TTo defaultValue) {
  if constexpr (CesiumGltf::IsMetadataFloating<TFrom>::value) {
    if (from > std::numeric_limits<TTo>::max() ||
        from < std::numeric_limits<TTo>::lowest()) {
      return defaultValue;
    }
    return static_cast<TTo>(from);
  } else if constexpr (CesiumGltf::IsMetadataInteger<TFrom>::value) {
    return static_cast<TTo>(from);
  } else if constexpr (CesiumGltf::IsMetadataBoolean<TFrom>::value) {
    return from ? 1.0f : 0.0f;
  } else if constexpr (CesiumGltf::IsMetadataString<TFrom>::value) {
    std::string temp(from);
    char* pLastUsed;
    float parsedValue = std::strtof(temp.c_str(), &pLastUsed);
    if (pLastUsed == temp.c_str() + temp.size()) {
      // Successfully parsed the entire string as a float.
      return parsedValue;
    }
  }
  return defaultValue;
}

template <typename TTo, typename TFrom>
static TTo convertToInt(TFrom arg, TTo defaultValue) {
  if constexpr (CesiumGltf::IsMetadataNumeric<TFrom>::value) {
    return CesiumUtility::losslessNarrowOrDefault(arg, defaultValue);
  } else if constexpr (CesiumGltf::IsMetadataFloating<TFrom>::value) {
    if (double(std::numeric_limits<TTo>::max()) < arg ||
        double(std::numeric_limits<TTo>::lowest()) > arg) {
      // Floating-point number is outside the range of this integer type.
      return defaultValue;
    }
    return static_cast<TTo>(arg);
  } else if constexpr (CesiumGltf::IsMetadataString<TFrom>::value) {
    std::string temp(arg);
    char* pLastUsed;
    int64_t parsedValue = std::strtoll(temp.c_str(), &pLastUsed, 10);
    if (pLastUsed == temp.c_str() + temp.size()) {
      // Successfully parsed the entire string as an integer of this type.
      return CesiumUtility::losslessNarrowOrDefault(parsedValue, defaultValue);
    }

    // Failed to parse as an integer. Maybe we can parse as a double and
    // truncate it?
    double parsedDouble = std::strtod(temp.c_str(), &pLastUsed);
    if (pLastUsed == temp.c_str() + temp.size()) {
      // Successfully parsed the entire string as a double.
      // Convert it to an integer if we can.
      double truncated = glm::trunc(parsedDouble);

      int64_t asInteger = static_cast<int64_t>(truncated);
      double roundTrip = static_cast<double>(asInteger);
      if (roundTrip == truncated) {
        return CesiumUtility::losslessNarrowOrDefault(asInteger, defaultValue);
      }
    }
  }
  return defaultValue;
}

template <typename TTo, typename TFrom>
static TTo convertToUint(TFrom arg, TTo defaultValue) {
  if constexpr (CesiumGltf::IsMetadataNumeric<TFrom>::value) {
    return CesiumUtility::losslessNarrowOrDefault(arg, defaultValue);
  } else if constexpr (CesiumGltf::IsMetadataFloating<TFrom>::value) {
    if (double(std::numeric_limits<TTo>::max()) < arg ||
        double(std::numeric_limits<TTo>::lowest()) > arg) {
      // Floating-point number is outside the range of this integer type.
      return defaultValue;
    }
    return static_cast<TTo>(arg);
  } else if constexpr (CesiumGltf::IsMetadataString<TFrom>::value) {
    std::string temp(arg);

    char* pLastUsed;
    uint64_t parsedValue = std::strtoull(temp.c_str(), &pLastUsed, 10);
    if (pLastUsed == temp.c_str() + temp.size()) {
      // Successfully parsed the entire string as an integer of this type.
      return CesiumUtility::losslessNarrowOrDefault(parsedValue, defaultValue);
    }

    // Failed to parse as an integer. Maybe we can parse as a double and
    // truncate it?
    double parsedDouble = std::strtod(temp.c_str(), &pLastUsed);
    if (pLastUsed == temp.c_str() + temp.size()) {
      // Successfully parsed the entire string as a double.
      // Convert it to an integer if we can.
      double truncated = glm::trunc(parsedDouble);

      uint64_t asInteger = static_cast<uint64_t>(truncated);
      double roundTrip = static_cast<double>(asInteger);
      if (roundTrip == truncated) {
        return CesiumUtility::losslessNarrowOrDefault(asInteger, defaultValue);
      }
    }

    return defaultValue;
  } else {
    return defaultValue;
  }
}

int8_t GetInt8(const ValueType& value, int8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
uint8_t GetUInt8(const ValueType& value, uint8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
int16_t GetInt16(const ValueType& value, int16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
uint16_t GetUInt16(const ValueType& value, uint16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
int32_t GetInt32(const ValueType& value, int32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
uint32_t GetUInt32(const ValueType& value, uint32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
int64_t GetInt64(const ValueType& value, int64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
uint64_t GetUInt64(const ValueType& value, uint64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}

float GetFloat32(const ValueType& value, float defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<float, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}
double GetFloat64(const ValueType& value, double defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<double, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      value);
}

bool GetBoolean(const ValueType& value, bool defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        if constexpr (CesiumGltf::IsMetadataBoolean<T>::value) {
          return arg;
        } else if constexpr (CesiumGltf::IsMetadataString<T>::value) {
          std::string str(arg);
          std::transform(str.begin(), str.end(), str.begin(), [](auto c) {
            return std::tolower(c);
          });
          if (str == "1" || str == "true" || str == "yes") {
            return true;
          } else if (str == "0" || str == "false" || str == "no") {
            return false;
          }
        } else if constexpr (CesiumGltf::IsMetadataNumeric<T>::value) {
          return arg != static_cast<T>(0);
        }
        return defaultValue;
      },
      value);
}

DotNet::System::String
GetString(const ValueType& value, const DotNet::System::String& defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        if constexpr (CesiumGltf::IsMetadataNumeric<T>::value) {
          return DotNet::System::String(std::to_string(arg));
        } else if constexpr (CesiumGltf::IsMetadataBoolean<T>::value) {
          return arg ? DotNet::System::String("true")
                     : DotNet::System::String("false");
        } else if constexpr (CesiumGltf::IsMetadataString<T>::value) {
          return DotNet::System::String(std::string(arg));
        } else {
          return defaultValue;
        }
      },
      value);
}

ValueType GetComponent(const ValueType& value, int index) {
  return std::visit(
      [index](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
          if (index >= 0 && index < arg.size()) {
            return static_cast<ValueType>(arg[index]);
          }
        }
        return static_cast<ValueType>(0);
      },
      value);
}

} // namespace

int8_t CesiumFeatureImpl::GetInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int8_t defaultValue) {
  return ::GetInt8(GetValueType(property), defaultValue);
}

uint8_t CesiumFeatureImpl::GetUInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint8_t defaultValue) {
  return ::GetUInt8(GetValueType(property), defaultValue);
}

int16_t CesiumFeatureImpl::GetInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int16_t defaultValue) {
  return ::GetInt16(GetValueType(property), defaultValue);
}

uint16_t CesiumFeatureImpl::GetUInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint16_t defaultValue) {
  return ::GetUInt16(GetValueType(property), defaultValue);
}

int32_t CesiumFeatureImpl::GetInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int32_t defaultValue) {
  return ::GetInt32(GetValueType(property), defaultValue);
}

uint32_t CesiumFeatureImpl::GetUInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint32_t defaultValue) {
  return ::GetUInt32(GetValueType(property), defaultValue);
}

int64_t CesiumFeatureImpl::GetInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int64_t defaultValue) {
  return ::GetInt64(GetValueType(property), defaultValue);
}

uint64_t CesiumFeatureImpl::GetUInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint64_t defaultValue) {
  return ::GetUInt64(GetValueType(property), defaultValue);
}

float CesiumFeatureImpl::GetFloat32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    float defaultValue) {
  return ::GetFloat32(GetValueType(property), defaultValue);
}

double CesiumFeatureImpl::GetFloat64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    double defaultValue) {
  return ::GetFloat64(GetValueType(property), defaultValue);
}

bool CesiumFeatureImpl::GetBoolean(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    bool defaultValue) {
  return ::GetBoolean(GetValueType(property), defaultValue);
}

DotNet::System::String CesiumFeatureImpl::GetString(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    const DotNet::System::String& defaultValue) {
  return ::GetString(GetValueType(property), defaultValue);
}

std::int8_t CesiumFeatureImpl::GetComponentInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int8_t defaultValue) {
  return ::GetInt8(GetComponent(GetValueType(property), index), defaultValue);
}

std::uint8_t CesiumFeatureImpl::GetComponentUInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint8_t defaultValue) {
  return ::GetUInt8(GetComponent(GetValueType(property), index), defaultValue);
}

std::int16_t CesiumFeatureImpl::GetComponentInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int16_t defaultValue) {
  return ::GetInt16(GetComponent(GetValueType(property), index), defaultValue);
}

std::uint16_t CesiumFeatureImpl::GetComponentUInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint16_t defaultValue) {
  return ::GetUInt16(GetComponent(GetValueType(property), index), defaultValue);
}

std::int32_t CesiumFeatureImpl::GetComponentInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int32_t defaultValue) {
  return ::GetInt32(GetComponent(GetValueType(property), index), defaultValue);
}

std::uint32_t CesiumFeatureImpl::GetComponentUInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint32_t defaultValue) {
  return ::GetUInt32(GetComponent(GetValueType(property), index), defaultValue);
}

std::int64_t CesiumFeatureImpl::GetComponentInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int64_t defaultValue) {
  return ::GetInt64(GetComponent(GetValueType(property), index), defaultValue);
}

std::uint64_t CesiumFeatureImpl::GetComponentUInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint64_t defaultValue) {
  return ::GetUInt64(GetComponent(GetValueType(property), index), defaultValue);
}

float CesiumFeatureImpl::GetComponentFloat32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    float defaultValue) {
  return ::GetFloat32(
      GetComponent(GetValueType(property), index),
      defaultValue);
}

double CesiumFeatureImpl::GetComponentFloat64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    double defaultValue) {
  return ::GetFloat64(
      GetComponent(GetValueType(property), index),
      defaultValue);
}

bool CesiumFeatureImpl::GetComponentBoolean(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    bool defaultValue) {
  return ::GetBoolean(
      GetComponent(GetValueType(property), index),
      defaultValue);
}

DotNet::System::String CesiumFeatureImpl::GetComponentString(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    const DotNet::System::String& defaultValue) {
  return ::GetString(GetComponent(GetValueType(property), index), defaultValue);
}

int CesiumFeatureImpl::GetComponentCount(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return std::visit(
      [](auto&& arg) { return arg.getComponentCount(); },
      GetPropertyType(property));
}

DotNet::CesiumForUnity::MetadataType CesiumFeatureImpl::GetComponentType(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return std::visit(
      [](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
          if (arg.size() > 0) {
            CesiumGltf::PropertyType type = std::visit(
                [](auto&& arg2) {
                  using T = std::decay_t<decltype(arg2)>;
                  return CesiumGltf::TypeToPropertyType<T>::value;
                },
                static_cast<ValueType>(arg[0]));
            return ::GetMetadataType(type);
          }
        }
        return DotNet::CesiumForUnity::MetadataType::None;
      },
      GetValueType(property));
}

bool CesiumFeatureImpl::IsNormalized(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return std::visit(
      [](auto&& arg) { return arg.isNormalized(); },
      GetPropertyType(property));
}

DotNet::CesiumForUnity::MetadataType CesiumFeatureImpl::GetMetadataType(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  CesiumGltf::PropertyType type = std::visit(
      [](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        return CesiumGltf::TypeToPropertyType<T>::value;
      },
      GetValueType(property));
  return ::GetMetadataType(type);
}

PropertyType CesiumForUnityNative::CesiumFeatureImpl::GetPropertyType(
    const DotNet::System::String& property) {
  auto find = properties.find(property.ToStlString());
  if (find != properties.end()) {
    return find->second.first;
  }
  return PropertyType();
}

ValueType CesiumForUnityNative::CesiumFeatureImpl::GetValueType(
    const DotNet::System::String& property) {
  auto find = properties.find(property.ToStlString());
  if (find != properties.end()) {
    return find->second.second;
  }
  return ValueType();
}
