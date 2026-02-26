#pragma once

#include <CesiumUtility/Color.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/UnityEngine/Color32.h>
#include <DotNet/CesiumForUnity/CesiumVectorColorMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineWidthMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonFillStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorStyle.h>

#include <cstdint>

namespace CesiumForUnityNative {

// Unity -> Native conversions

inline CesiumUtility::Color
fromUnityColor(const DotNet::UnityEngine::Color32& color) {
  return CesiumUtility::Color(color.r, color.g, color.b, color.a);
}

inline CesiumVectorData::ColorMode
fromUnityColorMode(DotNet::CesiumForUnity::CesiumVectorColorMode mode) {
  return static_cast<CesiumVectorData::ColorMode>(
      static_cast<std::uint8_t>(mode));
}

inline CesiumVectorData::LineWidthMode
fromUnityLineWidthMode(DotNet::CesiumForUnity::CesiumVectorLineWidthMode mode) {
  return static_cast<CesiumVectorData::LineWidthMode>(
      static_cast<std::uint8_t>(mode));
}

inline CesiumVectorData::LineStyle
fromUnityLineStyle(const DotNet::CesiumForUnity::CesiumVectorLineStyle& lineStyle) {
  CesiumVectorData::LineStyle native;
  native.color = fromUnityColor(lineStyle.color);
  native.colorMode = fromUnityColorMode(lineStyle.colorMode);
  native.width = lineStyle.width;
  native.widthMode = fromUnityLineWidthMode(lineStyle.widthMode);
  return native;
}

inline CesiumVectorData::ColorStyle
fromUnityFillStyle(const DotNet::CesiumForUnity::CesiumVectorPolygonFillStyle& fillStyle) {
  CesiumVectorData::ColorStyle native;
  native.color = fromUnityColor(fillStyle.color);
  native.colorMode = fromUnityColorMode(fillStyle.colorMode);
  return native;
}

inline CesiumVectorData::VectorStyle
fromUnityStyle(const DotNet::CesiumForUnity::CesiumVectorStyle& style) {
  CesiumVectorData::VectorStyle native;
  native.line = fromUnityLineStyle(style.lineStyle);

  if (style.polygonStyle.fill) {
    native.polygon.fill = fromUnityFillStyle(style.polygonStyle.fillStyle);
  }

  if (style.polygonStyle.outline) {
    native.polygon.outline = fromUnityLineStyle(style.polygonStyle.outlineStyle);
  }

  return native;
}

// Native -> Unity conversions

inline DotNet::UnityEngine::Color32
toUnityColor(const CesiumUtility::Color& color) {
  DotNet::UnityEngine::Color32 result;
  result.r = static_cast<std::uint8_t>(color.r);
  result.g = static_cast<std::uint8_t>(color.g);
  result.b = static_cast<std::uint8_t>(color.b);
  result.a = static_cast<std::uint8_t>(color.a);
  return result;
}

inline DotNet::CesiumForUnity::CesiumVectorColorMode
toUnityColorMode(CesiumVectorData::ColorMode mode) {
  switch (mode) {
  case CesiumVectorData::ColorMode::Random:
    return DotNet::CesiumForUnity::CesiumVectorColorMode::Random;
  case CesiumVectorData::ColorMode::Normal:
  default:
    return DotNet::CesiumForUnity::CesiumVectorColorMode::Normal;
  }
}

inline DotNet::CesiumForUnity::CesiumVectorLineWidthMode
toUnityLineWidthMode(CesiumVectorData::LineWidthMode mode) {
  switch (mode) {
  case CesiumVectorData::LineWidthMode::Meters:
    return DotNet::CesiumForUnity::CesiumVectorLineWidthMode::Meters;
  case CesiumVectorData::LineWidthMode::Pixels:
  default:
    return DotNet::CesiumForUnity::CesiumVectorLineWidthMode::Pixels;
  }
}

inline DotNet::CesiumForUnity::CesiumVectorStyle
toUnityStyle(const CesiumVectorData::VectorStyle& style) {
  DotNet::CesiumForUnity::CesiumVectorStyle result;

  // Line style
  result.lineStyle.color = toUnityColor(style.line.color);
  result.lineStyle.colorMode = toUnityColorMode(style.line.colorMode);
  result.lineStyle.width = style.line.width;
  result.lineStyle.widthMode = toUnityLineWidthMode(style.line.widthMode);

  // Polygon style
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

} // namespace CesiumForUnityNative
