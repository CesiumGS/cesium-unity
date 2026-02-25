#include "CesiumGeoJsonObjectImpl.h"
#include "CesiumGeoJsonFeatureImpl.h"

#include <CesiumUtility/JsonValue.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonFeature.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObjectType.h>
#include <DotNet/CesiumForUnity/CesiumVectorColorMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineWidthMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonFillStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorStyle.h>
#include <DotNet/CesiumForUnity/CesiumColor32.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>

#include <cstdint>
#include <memory>
#include <string>

using namespace DotNet;
using namespace CesiumVectorData;

namespace CesiumForUnityNative {

namespace {

// Convert CesiumUtility::Color (0-255 components) to CesiumColor32
CesiumForUnity::CesiumColor32 toUnityColor(const CesiumUtility::Color& color) {
  CesiumForUnity::CesiumColor32 result;
  result.r = static_cast<std::uint8_t>(color.r);
  result.g = static_cast<std::uint8_t>(color.g);
  result.b = static_cast<std::uint8_t>(color.b);
  result.a = static_cast<std::uint8_t>(color.a);
  return result;
}

// Convert CesiumColor32 to CesiumUtility::Color
CesiumUtility::Color fromUnityColor(const CesiumForUnity::CesiumColor32& color) {
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

} // namespace

CesiumGeoJsonObjectImpl::CesiumGeoJsonObjectImpl(
    const CesiumForUnity::CesiumGeoJsonObject& object)
    : _pDocument(nullptr), _pObject(nullptr), _pStandaloneObject(nullptr) {}

CesiumGeoJsonObjectImpl::~CesiumGeoJsonObjectImpl() {}

void CesiumGeoJsonObjectImpl::setNativeObject(
    std::shared_ptr<GeoJsonObject> pObject) {
  _pStandaloneObject = std::move(pObject);
  _pObject = _pStandaloneObject.get();
  _pDocument = nullptr;
}

void CesiumGeoJsonObjectImpl::setNativeObjectInDocument(
    std::shared_ptr<GeoJsonDocument> pDocument,
    GeoJsonObject* pObject) {
  _pDocument = std::move(pDocument);
  _pObject = pObject;
  _pStandaloneObject = nullptr;
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

CesiumForUnity::CesiumGeoJsonFeature
CesiumGeoJsonObjectImpl::GetObjectAsFeature(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return CesiumForUnity::CesiumGeoJsonFeature(nullptr);
  }

  GeoJsonFeature* pFeature = _pObject->getIf<GeoJsonFeature>();
  if (!pFeature) {
    return CesiumForUnity::CesiumGeoJsonFeature(nullptr);
  }

  CesiumForUnity::CesiumGeoJsonFeature result;
  result.NativeImplementation().setNativeFeatureInDocument(
      _pDocument,
      pFeature);
  return result;
}

System::Array1<CesiumForUnity::CesiumGeoJsonFeature>
CesiumGeoJsonObjectImpl::GetObjectAsFeatureCollection(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonFeature>(nullptr);
  }

  auto* pFeatureCollection = _pObject->getIf<GeoJsonFeatureCollection>();
  if (!pFeatureCollection) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonFeature>(nullptr);
  }

  std::int32_t count =
      static_cast<std::int32_t>(pFeatureCollection->features.size());
  System::Array1<CesiumForUnity::CesiumGeoJsonFeature> result(count);

  for (std::int32_t i = 0; i < count; ++i) {
    GeoJsonObject& featureObject =
        pFeatureCollection->features[static_cast<size_t>(i)];
    GeoJsonFeature* pFeature = featureObject.getIf<GeoJsonFeature>();

    CesiumForUnity::CesiumGeoJsonFeature feature;
    if (pFeature) {
      feature.NativeImplementation().setNativeFeatureInDocument(
          _pDocument,
          pFeature);
    }
    result.Item(i, feature);
  }

  return result;
}

bool CesiumGeoJsonObjectImpl::HasStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  return _pObject != nullptr && _pObject->getStyle().has_value();
}

CesiumForUnity::CesiumVectorStyle CesiumGeoJsonObjectImpl::GetStyle(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (_pObject && _pObject->getStyle().has_value()) {
    return toUnityStyle(*_pObject->getStyle());
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

void CesiumGeoJsonObjectImpl::DisposeNative(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  _pDocument = nullptr;
  _pObject = nullptr;
  _pStandaloneObject = nullptr;
}

} // namespace CesiumForUnityNative
