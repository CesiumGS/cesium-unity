#include "CesiumGeoJsonObjectImpl.h"

#include <CesiumUtility/JsonValue.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObjectType.h>
#include <DotNet/CesiumForUnity/CesiumVectorColorMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineWidthMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonFillStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorStyle.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Color32.h>

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

// Convert CesiumUtility::Color (0-255 components) to Unity Color32
UnityEngine::Color32 toUnityColor(const CesiumUtility::Color& color) {
  UnityEngine::Color32 result;
  result.r = static_cast<std::uint8_t>(color.r);
  result.g = static_cast<std::uint8_t>(color.g);
  result.b = static_cast<std::uint8_t>(color.b);
  result.a = static_cast<std::uint8_t>(color.a);
  return result;
}

// Convert Unity Color32 to CesiumUtility::Color
CesiumUtility::Color fromUnityColor(const UnityEngine::Color32& color) {
  return CesiumUtility::Color(color.r, color.g, color.b, color.a);
}

// Convert native ColorMode to Unity CesiumVectorColorMode
CesiumForUnity::CesiumVectorColorMode toUnityColorMode(ColorMode mode) {
  switch (mode) {
  case ColorMode::Random:
    return CesiumForUnity::CesiumVectorColorMode::Random;
  case ColorMode::Normal:
  default:
    return CesiumForUnity::CesiumVectorColorMode::Normal;
  }
}

// Convert Unity CesiumVectorColorMode to native ColorMode
ColorMode fromUnityColorMode(CesiumForUnity::CesiumVectorColorMode mode) {
  switch (mode) {
  case CesiumForUnity::CesiumVectorColorMode::Random:
    return ColorMode::Random;
  case CesiumForUnity::CesiumVectorColorMode::Normal:
  default:
    return ColorMode::Normal;
  }
}

// Convert native LineWidthMode to Unity CesiumVectorLineWidthMode
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

// Convert Unity CesiumVectorLineWidthMode to native LineWidthMode
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

// Convert native VectorStyle to Unity CesiumVectorStyle
CesiumForUnity::CesiumVectorStyle toUnityStyle(const VectorStyle& style) {
  CesiumForUnity::CesiumVectorStyle result;

  // Line style
  result.lineStyle.color = toUnityColor(style.line.color);
  result.lineStyle.colorMode = toUnityColorMode(style.line.colorMode);
  result.lineStyle.width = style.line.width;
  result.lineStyle.widthMode = toUnityLineWidthMode(style.line.widthMode);

  // Polygon style
  result.polygonStyle.fill = style.polygon.fill.has_value();
  if (style.polygon.fill.has_value()) {
    result.polygonStyle.fillStyle.color = toUnityColor(style.polygon.fill->color);
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

// Convert Unity CesiumVectorStyle to native VectorStyle
VectorStyle fromUnityStyle(const CesiumForUnity::CesiumVectorStyle& style) {
  VectorStyle result;

  // Line style
  result.line.color = fromUnityColor(style.lineStyle.color);
  result.line.colorMode = fromUnityColorMode(style.lineStyle.colorMode);
  result.line.width = style.lineStyle.width;
  result.line.widthMode = fromUnityLineWidthMode(style.lineStyle.widthMode);

  // Polygon style
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

// Helper to write JsonValue to rapidjson writer
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

CesiumGeoJsonObjectImpl::CesiumGeoJsonObjectImpl(
    const CesiumForUnity::CesiumGeoJsonObject& object)
    : _pObject(nullptr) {}

CesiumGeoJsonObjectImpl::~CesiumGeoJsonObjectImpl() {}

void CesiumGeoJsonObjectImpl::setNativeObject(
    std::shared_ptr<GeoJsonObject> pObject) {
  _pObject = std::move(pObject);
}

std::int32_t CesiumGeoJsonObjectImpl::GetObjectType(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return 0;
  }

  return static_cast<std::int32_t>(_pObject->getType());
}

bool CesiumGeoJsonObjectImpl::IsValid(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  return _pObject != nullptr;
}

std::int32_t CesiumGeoJsonObjectImpl::GetChildCount(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return 0;
  }

  if (auto* pFeatureCollection =
          _pObject->getIf<GeoJsonFeatureCollection>()) {
    return static_cast<std::int32_t>(pFeatureCollection->features.size());
  } else if (auto* pGeometryCollection =
                 _pObject->getIf<GeoJsonGeometryCollection>()) {
    return static_cast<std::int32_t>(pGeometryCollection->geometries.size());
  } else if (auto* pFeature = _pObject->getIf<GeoJsonFeature>()) {
    if (pFeature->geometry) {
      return 1;
    }
    return 0;
  }

  return 0;
}

CesiumForUnity::CesiumGeoJsonObject CesiumGeoJsonObjectImpl::GetChild(
    const CesiumForUnity::CesiumGeoJsonObject& object,
    std::int32_t index) {
  if (!_pObject) {
    return CesiumForUnity::CesiumGeoJsonObject(nullptr);
  }

  std::shared_ptr<GeoJsonObject> pChild;

  if (auto* pFeatureCollection =
          _pObject->getIf<GeoJsonFeatureCollection>()) {
    if (index >= 0 &&
        static_cast<size_t>(index) < pFeatureCollection->features.size()) {
      pChild = std::make_shared<GeoJsonObject>(
          pFeatureCollection->features[static_cast<size_t>(index)]);
    }
  } else if (auto* pGeometryCollection =
                 _pObject->getIf<GeoJsonGeometryCollection>()) {
    if (index >= 0 &&
        static_cast<size_t>(index) < pGeometryCollection->geometries.size()) {
      pChild = std::make_shared<GeoJsonObject>(
          pGeometryCollection->geometries[static_cast<size_t>(index)]);
    }
  } else if (auto* pFeature = _pObject->getIf<GeoJsonFeature>()) {
    if (index == 0 && pFeature->geometry) {
      pChild = std::make_shared<GeoJsonObject>(*pFeature->geometry);
    }
  }

  if (!pChild) {
    return CesiumForUnity::CesiumGeoJsonObject(nullptr);
  }

  CesiumForUnity::CesiumGeoJsonObject result;
  result.NativeImplementation().setNativeObject(std::move(pChild));
  return result;
}

bool CesiumGeoJsonObjectImpl::HasStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  return _pObject != nullptr && _pObject->getStyle().has_value();
}

CesiumForUnity::CesiumVectorStyle CesiumGeoJsonObjectImpl::GetStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject || !_pObject->getStyle().has_value()) {
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

  return toUnityStyle(*_pObject->getStyle());
}

void CesiumGeoJsonObjectImpl::SetStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object,
    CesiumForUnity::CesiumVectorStyle style) {
  if (!_pObject) {
    return;
  }

  _pObject->getStyle() = fromUnityStyle(style);
}

void CesiumGeoJsonObjectImpl::ClearStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return;
  }

  _pObject->getStyle() = std::nullopt;
}

System::String CesiumGeoJsonObjectImpl::GetFeatureIdString(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::String("");
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature) {
    return System::String("");
  }

  // Check if id is set (not monostate)
  if (std::holds_alternative<std::monostate>(pFeature->id)) {
    return System::String("");
  }

  if (auto* pStringId = std::get_if<std::string>(&pFeature->id)) {
    return System::String(*pStringId);
  }

  // If it's an int64, convert to string
  if (auto* pIntId = std::get_if<std::int64_t>(&pFeature->id)) {
    return System::String(std::to_string(*pIntId));
  }

  return System::String("");
}

std::int64_t CesiumGeoJsonObjectImpl::GetFeatureIdInt(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return 0;
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature) {
    return 0;
  }

  if (auto* pIntId = std::get_if<std::int64_t>(&pFeature->id)) {
    return *pIntId;
  }

  return 0;
}

bool CesiumGeoJsonObjectImpl::HasFeatureId(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return false;
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature) {
    return false;
  }

  return !std::holds_alternative<std::monostate>(pFeature->id);
}

bool CesiumGeoJsonObjectImpl::HasFeatureIdString(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return false;
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature) {
    return false;
  }

  return std::holds_alternative<std::string>(pFeature->id);
}

System::String CesiumGeoJsonObjectImpl::GetPropertiesAsJson(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::String("{}");
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature || !pFeature->properties.has_value()) {
    return System::String("{}");
  }

  rapidjson::StringBuffer buffer;
  rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

  writer.StartObject();
  for (const auto& [key, value] : *pFeature->properties) {
    writer.Key(key.c_str());
    writeJsonValue(writer, value);
  }
  writer.EndObject();

  return System::String(buffer.GetString());
}

System::String CesiumGeoJsonObjectImpl::GetStringProperty(
    const CesiumForUnity::CesiumGeoJsonObject& object,
    System::String propertyName) {
  if (!_pObject) {
    return System::String("");
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature || !pFeature->properties.has_value()) {
    return System::String("");
  }

  std::string name = propertyName.ToStlString();
  auto it = pFeature->properties->find(name);
  if (it == pFeature->properties->end()) {
    return System::String("");
  }

  if (it->second.isString()) {
    return System::String(it->second.getString());
  }

  return System::String("");
}

double CesiumGeoJsonObjectImpl::GetNumericProperty(
    const CesiumForUnity::CesiumGeoJsonObject& object,
    System::String propertyName) {
  if (!_pObject) {
    return 0.0;
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature || !pFeature->properties.has_value()) {
    return 0.0;
  }

  std::string name = propertyName.ToStlString();
  auto it = pFeature->properties->find(name);
  if (it == pFeature->properties->end()) {
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

bool CesiumGeoJsonObjectImpl::HasProperty(
    const CesiumForUnity::CesiumGeoJsonObject& object,
    System::String propertyName) {
  if (!_pObject) {
    return false;
  }

  auto* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature || !pFeature->properties.has_value()) {
    return false;
  }

  std::string name = propertyName.ToStlString();
  return pFeature->properties->find(name) != pFeature->properties->end();
}

void CesiumGeoJsonObjectImpl::DisposeNative(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  _pObject = nullptr;
}

} // namespace CesiumForUnityNative
