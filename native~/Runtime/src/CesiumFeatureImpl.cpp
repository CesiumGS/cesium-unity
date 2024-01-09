#include "CesiumFeatureImpl.h"

#include <CesiumGltf/MetadataConversions.h>
#include <CesiumGltf/PropertyType.h>
#include <CesiumGltf/PropertyTypeTraits.h>
#include <CesiumUtility/JsonValue.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumPropertyArray.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>
#include <glm/common.hpp>

#include <algorithm>

using namespace DotNet;
using namespace DotNet::CesiumForUnity;
using namespace CesiumGltf;

namespace CesiumForUnityNative {

namespace {
::DotNet::CesiumForUnity::MetadataType
getMetadataType(CesiumMetadataValueType valueType) {
  if (valueType.isArray) {
    return DotNet::CesiumForUnity::MetadataType::Array;
  }

  switch (valueType.type) {
  case CesiumMetadataType::Boolean:
    return DotNet::CesiumForUnity::MetadataType::Boolean;
  case CesiumMetadataType::String:
    return DotNet::CesiumForUnity::MetadataType::String;
  case CesiumMetadataType::Scalar:
    break;
  default:
    return DotNet::CesiumForUnity::MetadataType::None;
  }

  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return DotNet::CesiumForUnity::MetadataType::Int8;
  case CesiumMetadataComponentType::Uint8:
    return DotNet::CesiumForUnity::MetadataType::UInt8;
  case CesiumMetadataComponentType::Int16:
    return DotNet::CesiumForUnity::MetadataType::Int16;
  case CesiumMetadataComponentType::Uint16:
    return DotNet::CesiumForUnity::MetadataType::UInt16;
  case CesiumMetadataComponentType::Int32:
    return DotNet::CesiumForUnity::MetadataType::Int32;
  case CesiumMetadataComponentType::Uint32:
    return DotNet::CesiumForUnity::MetadataType::UInt32;
  case CesiumMetadataComponentType::Int64:
    return DotNet::CesiumForUnity::MetadataType::Int64;
  case CesiumMetadataComponentType::Uint64:
    return DotNet::CesiumForUnity::MetadataType::UInt64;
  case CesiumMetadataComponentType::Float32:
    return DotNet::CesiumForUnity::MetadataType::Float;
  case CesiumMetadataComponentType::Float64:
    return DotNet::CesiumForUnity::MetadataType::Double;
  default:
    return DotNet::CesiumForUnity::MetadataType::None;
  }
}

CesiumMetadataValue getComponent(const CesiumMetadataValue& value, int index) {
  CesiumPropertyArray array = value.GetArray();
  if (index >= 0 && index < array.values().Length()) {
    return array.values()[index];
  }
  return CesiumMetadataValue();
}

} // namespace

int8_t CesiumFeatureImpl::GetInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int8_t defaultValue) {
  return getValue(property).GetSByte(defaultValue);
}

uint8_t CesiumFeatureImpl::GetUInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint8_t defaultValue) {
  return getValue(property).GetByte(defaultValue);
}

int16_t CesiumFeatureImpl::GetInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int16_t defaultValue) {
  return getValue(property).GetInt16(defaultValue);
}

uint16_t CesiumFeatureImpl::GetUInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint16_t defaultValue) {
  return getValue(property).GetUInt16(defaultValue);
}

int32_t CesiumFeatureImpl::GetInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int32_t defaultValue) {
  return getValue(property).GetInt32(defaultValue);
}

uint32_t CesiumFeatureImpl::GetUInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint32_t defaultValue) {
  return getValue(property).GetUInt32(defaultValue);
}

int64_t CesiumFeatureImpl::GetInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int64_t defaultValue) {
  return getValue(property).GetInt64(defaultValue);
}

uint64_t CesiumFeatureImpl::GetUInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    uint64_t defaultValue) {
  return getValue(property).GetUInt64(defaultValue);
}

float CesiumFeatureImpl::GetFloat32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    float defaultValue) {
  return getValue(property).GetFloat(defaultValue);
}

double CesiumFeatureImpl::GetFloat64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    double defaultValue) {
  return getValue(property).GetDouble(defaultValue);
}

bool CesiumFeatureImpl::GetBoolean(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    bool defaultValue) {
  return getValue(property).GetBoolean(defaultValue);
}

DotNet::System::String CesiumFeatureImpl::GetString(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    const DotNet::System::String& defaultValue) {
  return getValue(property).GetString(defaultValue);
}

std::int8_t CesiumFeatureImpl::GetComponentInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int8_t defaultValue) {
  return getComponent(getValue(property), index).GetSByte(defaultValue);
}

std::uint8_t CesiumFeatureImpl::GetComponentUInt8(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint8_t defaultValue) {
  return getComponent(getValue(property), index).GetByte(defaultValue);
}

std::int16_t CesiumFeatureImpl::GetComponentInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int16_t defaultValue) {
  return getComponent(getValue(property), index).GetInt16(defaultValue);
}

std::uint16_t CesiumFeatureImpl::GetComponentUInt16(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint16_t defaultValue) {
  return getComponent(getValue(property), index).GetUInt16(defaultValue);
}

std::int32_t CesiumFeatureImpl::GetComponentInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int32_t defaultValue) {
  return getComponent(getValue(property), index).GetInt32(defaultValue);
}

std::uint32_t CesiumFeatureImpl::GetComponentUInt32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint32_t defaultValue) {
  return getComponent(getValue(property), index).GetUInt32(defaultValue);
}

std::int64_t CesiumFeatureImpl::GetComponentInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::int64_t defaultValue) {
  return getComponent(getValue(property), index).GetInt64(defaultValue);
}

std::uint64_t CesiumFeatureImpl::GetComponentUInt64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    std::uint64_t defaultValue) {
  return getComponent(getValue(property), index).GetUInt64(defaultValue);
}

float CesiumFeatureImpl::GetComponentFloat32(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    float defaultValue) {
  return getComponent(getValue(property), index).GetFloat(defaultValue);
}

double CesiumFeatureImpl::GetComponentFloat64(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    double defaultValue) {
  return getComponent(getValue(property), index).GetDouble(defaultValue);
}

bool CesiumFeatureImpl::GetComponentBoolean(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    bool defaultValue) {
  return getComponent(getValue(property), index).GetBoolean(defaultValue);
}

DotNet::System::String CesiumFeatureImpl::GetComponentString(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property,
    int index,
    const DotNet::System::String& defaultValue) {
  return getComponent(getValue(property), index).GetString(defaultValue);
}

int CesiumFeatureImpl::GetComponentCount(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return getPropertyInfo(property).count;
}

DotNet::CesiumForUnity::MetadataType CesiumFeatureImpl::GetComponentType(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  CesiumMetadataValue value = getValue(property);
  if (!value.valueType().isArray) {
    return DotNet::CesiumForUnity::MetadataType::None;
  }

  CesiumPropertyArray propertyArray = value.GetArray();
  return getMetadataType(propertyArray.valueType());
}

bool CesiumFeatureImpl::IsNormalized(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return getPropertyInfo(property).isNormalized;
}

DotNet::CesiumForUnity::MetadataType CesiumFeatureImpl::GetMetadataType(
    const DotNet::CesiumForUnity::CesiumFeature& feature,
    const DotNet::System::String& property) {
  return getMetadataType(getValue(property).valueType());
}

CesiumFeatureImpl::PropertyInfo
CesiumForUnityNative::CesiumFeatureImpl::getPropertyInfo(
    const DotNet::System::String& property) {
  auto find = values.find(property.ToStlString());
  if (find != values.end()) {
    return find->second.first;
  }
  return PropertyInfo();
}

DotNet::CesiumForUnity::CesiumMetadataValue
CesiumForUnityNative::CesiumFeatureImpl::getValue(
    const DotNet::System::String& property) {
  auto find = values.find(property.ToStlString());
  if (find != values.end()) {
    return find->second.second;
  }
  return DotNet::CesiumForUnity::CesiumMetadataValue();
}
} // namespace CesiumForUnityNative
