#include "CesiumGeoJsonFeatureImpl.h"
#include "CesiumGeoJsonObjectImpl.h"
#include "CesiumVectorStyleConversions.h"

#include <CesiumUtility/JsonValue.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonFeature.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonFeatureIdType.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/System/String.h>

#include <rapidjson/document.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/writer.h>

#include <cstdint>
#include <memory>
#include <string>
#include <variant>

using namespace DotNet;
using namespace CesiumVectorData;

namespace CesiumForUnityNative {

namespace {

void writeJsonValue(
    rapidjson::Writer<rapidjson::StringBuffer>& writer,
    const CesiumUtility::JsonValue& value) {
  if (value.isNull()) {
    writer.Null();
  } else if (value.isBool()) {
    writer.Bool(value.getBool());
  } else if (value.isInt64()) {
    writer.Int64(value.getInt64());
  } else if (value.isUint64()) {
    writer.Uint64(value.getUint64());
  } else if (value.isDouble()) {
    writer.Double(value.getDouble());
  } else if (value.isString()) {
    writer.String(value.getString().c_str());
  } else if (value.isArray()) {
    writer.StartArray();
    for (const auto& item : value.getArray()) {
      writeJsonValue(writer, item);
    }
    writer.EndArray();
  } else if (value.isObject()) {
    writer.StartObject();
    for (const auto& [key, val] : value.getObject()) {
      writer.Key(key.c_str());
      writeJsonValue(writer, val);
    }
    writer.EndObject();
  }
}

} // namespace

CesiumGeoJsonFeatureImpl::CesiumGeoJsonFeatureImpl(
    const CesiumForUnity::CesiumGeoJsonFeature& feature)
    : _pFeature(nullptr) {}

CesiumGeoJsonFeatureImpl::~CesiumGeoJsonFeatureImpl() {
  _pFeature = nullptr;
}

void CesiumGeoJsonFeatureImpl::setNativeFeatureInDocument(
    GeoJsonFeature* pFeature) {
  _pFeature = pFeature;
}

std::int32_t CesiumGeoJsonFeatureImpl::GetIdType(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature) {
    return static_cast<std::int32_t>(
        CesiumForUnity::CesiumGeoJsonFeatureIdType::None);
  }

  if (std::holds_alternative<std::monostate>(_pFeature->id)) {
    return static_cast<std::int32_t>(
        CesiumForUnity::CesiumGeoJsonFeatureIdType::None);
  }

  if (std::holds_alternative<std::string>(_pFeature->id)) {
    return static_cast<std::int32_t>(
        CesiumForUnity::CesiumGeoJsonFeatureIdType::String);
  }

  if (std::holds_alternative<std::int64_t>(_pFeature->id)) {
    return static_cast<std::int32_t>(
        CesiumForUnity::CesiumGeoJsonFeatureIdType::Integer);
  }

  return static_cast<std::int32_t>(
      CesiumForUnity::CesiumGeoJsonFeatureIdType::None);
}

System::String CesiumGeoJsonFeatureImpl::GetIdAsString(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature) {
    return System::String("");
  }

  if (auto* pStringId = std::get_if<std::string>(&_pFeature->id)) {
    return System::String(*pStringId);
  }

  if (auto* pIntId = std::get_if<std::int64_t>(&_pFeature->id)) {
    return System::String(std::to_string(*pIntId));
  }

  return System::String("");
}

std::int64_t CesiumGeoJsonFeatureImpl::GetIdAsInteger(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature) {
    return -1;
  }

  if (auto* pIntId = std::get_if<std::int64_t>(&_pFeature->id)) {
    return *pIntId;
  }

  return -1;
}

System::String CesiumGeoJsonFeatureImpl::GetPropertiesAsJson(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature || !_pFeature->properties.has_value()) {
    return System::String("{}");
  }

  rapidjson::StringBuffer buffer;
  rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

  writer.StartObject();
  for (const auto& [key, value] : *_pFeature->properties) {
    writer.Key(key.c_str());
    writeJsonValue(writer, value);
  }
  writer.EndObject();

  return System::String(buffer.GetString());
}

System::String CesiumGeoJsonFeatureImpl::GetStringProperty(
    const CesiumForUnity::CesiumGeoJsonFeature& feature,
    System::String propertyName) {
  if (!_pFeature || !_pFeature->properties.has_value()) {
    return System::String("");
  }

  std::string name = propertyName.ToStlString();
  auto it = _pFeature->properties->find(name);
  if (it == _pFeature->properties->end()) {
    return System::String("");
  }

  if (it->second.isString()) {
    return System::String(it->second.getString());
  }

  return System::String("");
}

double CesiumGeoJsonFeatureImpl::GetNumericProperty(
    const CesiumForUnity::CesiumGeoJsonFeature& feature,
    System::String propertyName) {
  if (!_pFeature || !_pFeature->properties.has_value()) {
    return 0.0;
  }

  std::string name = propertyName.ToStlString();
  auto it = _pFeature->properties->find(name);
  if (it == _pFeature->properties->end()) {
    return 0.0;
  }

  if (it->second.isDouble()) {
    return it->second.getDouble();
  } else if (it->second.isInt64()) {
    return static_cast<double>(it->second.getInt64());
  } else if (it->second.isUint64()) {
    return static_cast<double>(it->second.getUint64());
  }

  return 0.0;
}

bool CesiumGeoJsonFeatureImpl::HasProperty(
    const CesiumForUnity::CesiumGeoJsonFeature& feature,
    System::String propertyName) {
  if (!_pFeature || !_pFeature->properties.has_value()) {
    return false;
  }

  std::string name = propertyName.ToStlString();
  return _pFeature->properties->find(name) != _pFeature->properties->end();
}

CesiumForUnity::CesiumGeoJsonObject CesiumGeoJsonFeatureImpl::GetGeometry(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature || !_pFeature->geometry) {
    return CesiumForUnity::CesiumGeoJsonObject(nullptr);
  }

  CesiumForUnity::CesiumGeoJsonObject result;
  auto pGeomCopy =
      std::make_shared<GeoJsonObject>(*_pFeature->geometry);
  result.NativeImplementation().setNativeObject(std::move(pGeomCopy));
  return result;
}

bool CesiumGeoJsonFeatureImpl::HasStyle(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  return _pFeature != nullptr && _pFeature->style.has_value();
}

CesiumForUnity::CesiumVectorStyle CesiumGeoJsonFeatureImpl::GetStyle(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (_pFeature && _pFeature->style.has_value()) {
    return toUnityStyle(*_pFeature->style);
  }

  // Return default style
  CesiumForUnity::CesiumVectorStyle result;

  result.lineStyle.color.r = 255;
  result.lineStyle.color.g = 255;
  result.lineStyle.color.b = 255;
  result.lineStyle.color.a = 255;
  result.lineStyle.colorMode = CesiumForUnity::CesiumVectorColorMode::Normal;
  result.lineStyle.width = 1.0;
  result.lineStyle.widthMode =
      CesiumForUnity::CesiumVectorLineWidthMode::Pixels;

  result.polygonStyle.fill = true;
  result.polygonStyle.fillStyle.color.r = 255;
  result.polygonStyle.fillStyle.color.g = 255;
  result.polygonStyle.fillStyle.color.b = 255;
  result.polygonStyle.fillStyle.color.a = 255;
  result.polygonStyle.fillStyle.colorMode =
      CesiumForUnity::CesiumVectorColorMode::Normal;
  result.polygonStyle.outline = false;

  return result;
}

void CesiumGeoJsonFeatureImpl::SetStyle(
    const CesiumForUnity::CesiumGeoJsonFeature& feature,
    CesiumForUnity::CesiumVectorStyle style) {
  if (!_pFeature) {
    return;
  }

  _pFeature->style = fromUnityStyle(style);
}

void CesiumGeoJsonFeatureImpl::ClearStyle(
    const CesiumForUnity::CesiumGeoJsonFeature& feature) {
  if (!_pFeature) {
    return;
  }

  _pFeature->style = std::nullopt;
}

} // namespace CesiumForUnityNative
