#include "CesiumGeoJsonObjectImpl.h"
#include "CesiumGeoJsonFeatureImpl.h"
#include "CesiumVectorStyleConversions.h"

#include <CesiumUtility/JsonValue.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>
#include <CesiumVectorData/VectorStyle.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonFeature.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonLineString.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObjectType.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonPolygon.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Mathematics/double3.h>

#include <cstdint>
#include <memory>
#include <string>

using namespace DotNet;
using namespace CesiumVectorData;

namespace CesiumForUnityNative {

CesiumGeoJsonObjectImpl::CesiumGeoJsonObjectImpl(
    const CesiumForUnity::CesiumGeoJsonObject& object)
    : _pDocument(nullptr), _pObject(nullptr) {}

CesiumGeoJsonObjectImpl::~CesiumGeoJsonObjectImpl() {
  _pDocument = nullptr;
  _pObject = nullptr;
}

void CesiumGeoJsonObjectImpl::setNativeObjectInDocument(
    std::shared_ptr<GeoJsonDocument> pDocument,
    GeoJsonObject* pObject) {
  _pDocument = std::move(pDocument);
  _pObject = pObject;
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
  result.NativeImplementation().setNativeFeatureInDocument(_pDocument, pFeature);
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
      feature.NativeImplementation().setNativeFeatureInDocument(_pDocument, pFeature);
    }
    result.Item(i, feature);
  }

  return result;
}

Unity::Mathematics::double3 CesiumGeoJsonObjectImpl::GetObjectAsPoint(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return Unity::Mathematics::double3{0.0, 0.0, 0.0};
  }

  auto* pPoint = _pObject->getIf<GeoJsonPoint>();
  if (!pPoint) {
    return Unity::Mathematics::double3{0.0, 0.0, 0.0};
  }

  return Unity::Mathematics::double3{
      pPoint->coordinates.x,
      pPoint->coordinates.y,
      pPoint->coordinates.z};
}

System::Array1<Unity::Mathematics::double3>
CesiumGeoJsonObjectImpl::GetObjectAsMultiPoint(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::Array1<Unity::Mathematics::double3>(nullptr);
  }

  auto* pMultiPoint = _pObject->getIf<GeoJsonMultiPoint>();
  if (!pMultiPoint) {
    return System::Array1<Unity::Mathematics::double3>(nullptr);
  }

  std::int32_t count =
      static_cast<std::int32_t>(pMultiPoint->coordinates.size());
  System::Array1<Unity::Mathematics::double3> result(count);

  for (std::int32_t i = 0; i < count; ++i) {
    const glm::dvec3& coord = pMultiPoint->coordinates[static_cast<size_t>(i)];
    result.Item(i, Unity::Mathematics::double3{coord.x, coord.y, coord.z});
  }

  return result;
}

CesiumForUnity::CesiumGeoJsonLineString
CesiumGeoJsonObjectImpl::GetObjectAsLineString(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return CesiumForUnity::CesiumGeoJsonLineString(nullptr);
  }

  auto* pLineString = _pObject->getIf<GeoJsonLineString>();
  if (!pLineString) {
    return CesiumForUnity::CesiumGeoJsonLineString(nullptr);
  }

  std::int32_t count =
      static_cast<std::int32_t>(pLineString->coordinates.size());
  System::Array1<Unity::Mathematics::double3> points(count);

  for (std::int32_t i = 0; i < count; ++i) {
    const glm::dvec3& coord =
        pLineString->coordinates[static_cast<size_t>(i)];
    points.Item(i, Unity::Mathematics::double3{coord.x, coord.y, coord.z});
  }

  CesiumForUnity::CesiumGeoJsonLineString result;
  result.points(points);
  return result;
}

System::Array1<CesiumForUnity::CesiumGeoJsonLineString>
CesiumGeoJsonObjectImpl::GetObjectAsMultiLineString(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonLineString>(nullptr);
  }

  auto* pMultiLineString = _pObject->getIf<GeoJsonMultiLineString>();
  if (!pMultiLineString) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonLineString>(nullptr);
  }

  std::int32_t lineCount =
      static_cast<std::int32_t>(pMultiLineString->coordinates.size());
  System::Array1<CesiumForUnity::CesiumGeoJsonLineString> result(lineCount);

  for (std::int32_t i = 0; i < lineCount; ++i) {
    const std::vector<glm::dvec3>& line =
        pMultiLineString->coordinates[static_cast<size_t>(i)];
    std::int32_t pointCount = static_cast<std::int32_t>(line.size());

    System::Array1<Unity::Mathematics::double3> points(pointCount);
    for (std::int32_t j = 0; j < pointCount; ++j) {
      const glm::dvec3& coord = line[static_cast<size_t>(j)];
      points.Item(j, Unity::Mathematics::double3{coord.x, coord.y, coord.z});
    }

    CesiumForUnity::CesiumGeoJsonLineString lineString;
    lineString.points(points);
    result.Item(i, lineString);
  }

  return result;
}

CesiumForUnity::CesiumGeoJsonPolygon
CesiumGeoJsonObjectImpl::GetObjectAsPolygon(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return CesiumForUnity::CesiumGeoJsonPolygon(nullptr);
  }

  auto* pPolygon = _pObject->getIf<GeoJsonPolygon>();
  if (!pPolygon) {
    return CesiumForUnity::CesiumGeoJsonPolygon(nullptr);
  }

  std::int32_t ringCount =
      static_cast<std::int32_t>(pPolygon->coordinates.size());
  System::Array1<CesiumForUnity::CesiumGeoJsonLineString> rings(ringCount);

  for (std::int32_t i = 0; i < ringCount; ++i) {
    const std::vector<glm::dvec3>& ring =
        pPolygon->coordinates[static_cast<size_t>(i)];
    std::int32_t pointCount = static_cast<std::int32_t>(ring.size());

    System::Array1<Unity::Mathematics::double3> points(pointCount);
    for (std::int32_t j = 0; j < pointCount; ++j) {
      const glm::dvec3& coord = ring[static_cast<size_t>(j)];
      points.Item(j, Unity::Mathematics::double3{coord.x, coord.y, coord.z});
    }

    CesiumForUnity::CesiumGeoJsonLineString lineString;
    lineString.points(points);
    rings.Item(i, lineString);
  }

  CesiumForUnity::CesiumGeoJsonPolygon result;
  result.rings(rings);
  return result;
}

System::Array1<CesiumForUnity::CesiumGeoJsonPolygon>
CesiumGeoJsonObjectImpl::GetObjectAsMultiPolygon(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonPolygon>(nullptr);
  }

  auto* pMultiPolygon = _pObject->getIf<GeoJsonMultiPolygon>();
  if (!pMultiPolygon) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonPolygon>(nullptr);
  }

  std::int32_t count =
      static_cast<std::int32_t>(pMultiPolygon->coordinates.size());
  System::Array1<CesiumForUnity::CesiumGeoJsonPolygon> result(count);

  for (std::int32_t i = 0; i < count; ++i) {
    const std::vector<std::vector<glm::dvec3>>& polyRings =
        pMultiPolygon->coordinates[static_cast<size_t>(i)];
    std::int32_t ringCount = static_cast<std::int32_t>(polyRings.size());
    System::Array1<CesiumForUnity::CesiumGeoJsonLineString> rings(ringCount);

    for (std::int32_t r = 0; r < ringCount; ++r) {
      const std::vector<glm::dvec3>& ring =
          polyRings[static_cast<size_t>(r)];
      std::int32_t pointCount = static_cast<std::int32_t>(ring.size());

      System::Array1<Unity::Mathematics::double3> points(pointCount);
      for (std::int32_t j = 0; j < pointCount; ++j) {
        const glm::dvec3& coord = ring[static_cast<size_t>(j)];
        points.Item(
            j,
            Unity::Mathematics::double3{coord.x, coord.y, coord.z});
      }

      CesiumForUnity::CesiumGeoJsonLineString lineString;
      lineString.points(points);
      rings.Item(r, lineString);
    }

    CesiumForUnity::CesiumGeoJsonPolygon polygon;
    polygon.rings(rings);
    result.Item(i, polygon);
  }

  return result;
}

System::Array1<CesiumForUnity::CesiumGeoJsonObject>
CesiumGeoJsonObjectImpl::GetObjectAsGeometryCollection(
    const CesiumForUnity::CesiumGeoJsonObject& object) {
  if (!_pObject) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonObject>(nullptr);
  }

  auto* pGeometryCollection = _pObject->getIf<GeoJsonGeometryCollection>();
  if (!pGeometryCollection) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonObject>(nullptr);
  }

  std::int32_t count =
      static_cast<std::int32_t>(pGeometryCollection->geometries.size());
  System::Array1<CesiumForUnity::CesiumGeoJsonObject> result(count);

  for (std::int32_t i = 0; i < count; ++i) {
    GeoJsonObject& geomObject =
        pGeometryCollection->geometries[static_cast<size_t>(i)];

    CesiumForUnity::CesiumGeoJsonObject geom;
    geom.NativeImplementation().setNativeObjectInDocument(
        _pDocument,
        &geomObject);
    result.Item(i, geom);
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

  return toUnityStyle(CesiumVectorData::VectorStyle{});
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

} // namespace CesiumForUnityNative
