#include "CesiumFeatureImpl.h"

#include "CesiumGltf/PropertyType.h"
#include "CesiumGltf/PropertyTypeTraits.h"
#include "CesiumUtility/JsonValue.h"

#include <DotNet/CesiumForUnity/MetadataType.h>
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
} // namespace

int8_t CesiumFeatureImpl::GetInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
uint8_t CesiumFeatureImpl::GetUInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
int16_t CesiumFeatureImpl::GetInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
uint16_t CesiumFeatureImpl::GetUInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
int32_t CesiumFeatureImpl::GetInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
uint32_t CesiumFeatureImpl::GetUInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
int64_t CesiumFeatureImpl::GetInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
uint64_t CesiumFeatureImpl::GetUInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}

float CesiumFeatureImpl::GetFloat32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    float defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<float, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
double CesiumFeatureImpl::GetFloat64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    double defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<double, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      getValueType(property));
}
bool CesiumFeatureImpl::GetBoolean(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    bool defaultValue) {
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
      getValueType(property));
}

DotNet::System::String CesiumFeatureImpl::GetString(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    const DotNet::System::String& defaultValue) {
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
      getValueType(property));
}

DotNet::CesiumForUnity::MetadataType CesiumFeatureImpl::GetMetadataType(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  CesiumGltf::PropertyType type = std::visit(
      [](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        return CesiumGltf::TypeToPropertyType<T>::value;
      },
      getValueType(property));
  return ::GetMetadataType(type);
}

PropertyType CesiumForUnityNative::CesiumFeatureImpl::getPropertyType(
    const DotNet::System::String& property) {
  auto find = properties.find(property.ToStlString());
  if (find != properties.end()) {
    return find->second.first;
  }
  return PropertyType();
}

ValueType CesiumForUnityNative::CesiumFeatureImpl::getValueType(
    const DotNet::System::String& property) {
  auto find = properties.find(property.ToStlString());
  if (find != properties.end()) {
    return find->second.second;
  }
  return ValueType();
}
