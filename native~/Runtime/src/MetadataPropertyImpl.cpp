#include "MetadataPropertyImpl.h"

#include "CesiumGltf/PropertyType.h"
#include "CesiumGltf/PropertyTypeTraits.h"
#include "CesiumUtility/JsonValue.h"

#include <DotNet/CesiumForUnity/MetadataProperty.h>
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

DotNet::System::String MetadataPropertyImpl::GetPropertyName(
    const DotNet::CesiumForUnity::MetadataProperty& property) {
  return DotNet::System::String(this->_propertyName);
}

void MetadataPropertyImpl::SetProperty(
    const std::string& propertyName,
    const PropertyType& property,
    ValueType value) {
  this->_propertyName = propertyName;
  this->_propertyType = property;
  this->_value = value;
}

int MetadataPropertyImpl::GetComponentCount(
    const DotNet::CesiumForUnity::MetadataProperty& property) {
  return std::visit(
      [](auto&& arg) { return arg.getComponentCount(); },
      this->_propertyType);
}

int8_t MetadataPropertyImpl::GetInt8(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    int8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
uint8_t MetadataPropertyImpl::GetUInt8(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    uint8_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint8_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
int16_t MetadataPropertyImpl::GetInt16(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    int16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
uint16_t MetadataPropertyImpl::GetUInt16(
    const DotNet::CesiumForUnity::MetadataProperty& value,
    uint16_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint16_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
int32_t MetadataPropertyImpl::GetInt32(
    const DotNet::CesiumForUnity::MetadataProperty& value,
    int32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
uint32_t MetadataPropertyImpl::GetUInt32(
    const DotNet::CesiumForUnity::MetadataProperty& value,
    uint32_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint32_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
int64_t MetadataPropertyImpl::GetInt64(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    int64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToInt<int64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
uint64_t MetadataPropertyImpl::GetUInt64(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    uint64_t defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToUint<uint64_t, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}

float MetadataPropertyImpl::GetFloat32(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    float defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<float, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
double MetadataPropertyImpl::GetFloat64(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    double defaultValue) {
  return std::visit(
      [defaultValue](auto&& arg) {
        return convertToFloat<double, std::decay_t<decltype(arg)>>(
            arg,
            defaultValue);
      },
      _value);
}
bool MetadataPropertyImpl::GetBoolean(
    const DotNet::CesiumForUnity::MetadataProperty& property,
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
      _value);
}

DotNet::System::String MetadataPropertyImpl::GetString(
    const DotNet::CesiumForUnity::MetadataProperty& property,
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
      _value);
}

void MetadataPropertyImpl::GetComponent(
    const DotNet::CesiumForUnity::MetadataProperty& property,
    const DotNet::CesiumForUnity::MetadataProperty& component,
    int index) {
  ValueType value = std::visit(
      [index](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
          if (index >= 0 && index < arg.size()) {
            return static_cast<ValueType>(arg[index]);
          }
        }
        return static_cast<ValueType>(0);
      },
      _value);
  component.NativeImplementation().SetProperty(
      this->_propertyName,
      this->_propertyType,
      value);
}

DotNet::CesiumForUnity::MetadataType MetadataPropertyImpl::GetMetadataType(
    const DotNet::CesiumForUnity::MetadataProperty& property) {
  CesiumGltf::PropertyType type = std::visit(
      [](auto&& arg) {
        using T = std::decay_t<decltype(arg)>;
        return CesiumGltf::TypeToPropertyType<T>::value;
      },
      this->_value);
  return ::GetMetadataType(type);
}

DotNet::CesiumForUnity::MetadataType MetadataPropertyImpl::GetComponentType(
    const DotNet::CesiumForUnity::MetadataProperty& property) {
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
      this->_value);
}

bool MetadataPropertyImpl::IsNormalized(
    const DotNet::CesiumForUnity::MetadataProperty& property) {
  return std::visit(
      [](auto&& arg) { return arg.isNormalized(); },
      this->_propertyType);
}