#include "CesiumGeoJsonFeatureImpl.h"
#include "CesiumGeoJsonObjectImpl.h"

#include <CesiumUtility/JsonValue.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonFeature.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonFeatureIdType.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/CesiumForUnity/CesiumVectorColorMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineWidthMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonFillStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorStyle.h>
#include <DotNet/CesiumForUnity/CesiumColor32.h>
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

CesiumForUnity::CesiumColor32
toUnityColor(const CesiumUtility::Color& color) {
  CesiumForUnity::CesiumColor32 result;
  result.r = static_cast<std::uint8_t>(color.r);
  result.g = static_cast<std::uint8_t>(color.g);
  result.b = static_cast<std::uint8_t>(color.b);
  result.a = static_cast<std::uint8_t>(color.a);
  return result;
}

CesiumUtility::Color
fromUnityColor(const CesiumForUnity::CesiumColor32& color) {
  return CesiumUtility::Color(color.r, color.g, color.b, color.a);
}

CesiumForUnity::CesiumVectorColorMode toUnityColorMode(ColorMode mode) {
  switch (mode) {
  case ColorMode::Random:
    return CesiumForUnity::CesiumVectorColorMode::Random;
  case ColorMode::Normal:
  default:
    return CesiumForUnity::CesiumVectorColorMode::Normal;
  }
}

ColorMode fromUnityColorMode(CesiumForUnity::CesiumVectorColorMode mode) {
  switch (mode) {
  case CesiumForUnity::CesiumVectorColorMode::Random:
    return ColorMode::Random;
  case CesiumForUnity::CesiumVectorColorMode::Normal:
  default:
    return ColorMode::Normal;
  }
}

CesiumForUnity::CesiumVectorLineWidthMode
toUnityLineWidthMode(LineWidthMode mode) {
  switch (mode) {
  case LineWidthMode::Meters:
    return CesiumForUnity::CesiumVectorLineWidthMode::Meters;
  case LineWidthMode::Pixels:
  default:
    return CesiumForUnity::CesiumVectorLineWidthMode::Pixels;
  }
}

LineWidthMode
fromUnityLineWidthMode(CesiumForUnity::CesiumVectorLineWidthMode mode) {
  switch (mode) {
  case CesiumForUnity::CesiumVectorLineWidthMode::Meters:
    return LineWidthMode::Meters;
  case CesiumForUnity::CesiumVectorLineWidthMode::Pixels:
  default:
    return LineWidthMode::Pixels;
  }
}

CesiumForUnity::CesiumVectorStyle toUnityStyle(const VectorStyle& style) {
  CesiumForUnity::CesiumVectorStyle result;

  result.lineStyle.color = toUnityColor(style.line.color);
  result.lineStyle.colorMode = toUnityColorMode(style.line.colorMode);
  result.lineStyle.width = style.line.width;
  result.lineStyle.widthMode = toUnityLineWidthMode(style.line.widthMode);

  result.polygonStyle.fill = style.polygon.fill.has_value();
  if (style.polygon.fill.has_value()) {
    result.polygonStyle.fillStyle.color =
        toUnityColor(style.polygon.fill->color);
    result.polygonStyle.fillStyle.colorMode =
        toUnityColorMode(style.polygon.fill->colorMode);
  }

  result.polygonStyle.outline = style.polygon.outline.has_value();
  if (style.polygon.outline.has_value()) {
    result.polygonStyle.outlineStyle.color =
        toUnityColor(style.polygon.outline->color);
    result.polygonStyle.outlineStyle.colorMode =
        toUnityColorMode(style.polygon.outline->colorMode);
    result.polygonStyle.outlineStyle.width = style.polygon.outline->width;
    result.polygonStyle.outlineStyle.widthMode =
        toUnityLineWidthMode(style.polygon.outline->widthMode);
  }

  return result;
}

VectorStyle fromUnityStyle(const CesiumForUnity::CesiumVectorStyle& style) {
  VectorStyle result;

  result.line.color = fromUnityColor(style.lineStyle.color);
  result.line.colorMode = fromUnityColorMode(style.lineStyle.colorMode);
  result.line.width = style.lineStyle.width;
  result.line.widthMode = fromUnityLineWidthMode(style.lineStyle.widthMode);

  if (style.polygonStyle.fill) {
    ColorStyle fill;
    fill.color = fromUnityColor(style.polygonStyle.fillStyle.color);
    fill.colorMode = fromUnityColorMode(style.polygonStyle.fillStyle.colorMode);
    result.polygon.fill = fill;
  }

  if (style.polygonStyle.outline) {
    LineStyle outline;
    outline.color = fromUnityColor(style.polygonStyle.outlineStyle.color);
    outline.colorMode =
        fromUnityColorMode(style.polygonStyle.outlineStyle.colorMode);
    outline.width = style.polygonStyle.outlineStyle.width;
    outline.widthMode =
        fromUnityLineWidthMode(style.polygonStyle.outlineStyle.widthMode);
    result.polygon.outline = outline;
  }

  return result;
}

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
    : _pDocument(nullptr), _pFeature(nullptr) {}

CesiumGeoJsonFeatureImpl::~CesiumGeoJsonFeatureImpl() {
  _pDocument = nullptr;
  _pFeature = nullptr;
}

void CesiumGeoJsonFeatureImpl::setNativeFeatureInDocument(
    std::shared_ptr<GeoJsonDocument> pDocument,
    GeoJsonFeature* pFeature) {
  _pDocument = std::move(pDocument);
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
    return 0;
  }

  if (auto* pIntId = std::get_if<std::int64_t>(&_pFeature->id)) {
    return *pIntId;
  }

  return 0;
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
