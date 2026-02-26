#pragma once

#include "CesiumImpl.h"

#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>

#include <DotNet/System/String.h>

#include <memory>

namespace DotNet::Unity::Mathematics {
struct double3;
} // namespace DotNet::Unity::Mathematics

namespace DotNet::CesiumForUnity {
class CesiumGeoJsonFeature;
class CesiumGeoJsonLineString;
class CesiumGeoJsonObject;
class CesiumGeoJsonPolygon;
struct CesiumVectorStyle;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
template <typename T> class Array1;
} // namespace DotNet::System

namespace CesiumForUnityNative {

class CesiumGeoJsonObjectImpl : public CesiumImpl<CesiumGeoJsonObjectImpl> {
public:
  CesiumGeoJsonObjectImpl(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);
  ~CesiumGeoJsonObjectImpl();

  // Set a standalone object (legacy, creates a copy)
  void setNativeObject(std::shared_ptr<CesiumVectorData::GeoJsonObject> pObject);

  // Set an object that references data within a document (no copy, modifications persist)
  void setNativeObjectInDocument(
      std::shared_ptr<CesiumVectorData::GeoJsonDocument> pDocument,
      CesiumVectorData::GeoJsonObject* pObject);

  std::int32_t GetObjectType(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  bool IsValid(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumGeoJsonFeature
  GetObjectAsFeature(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGeoJsonFeature>
  GetObjectAsFeatureCollection(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::Unity::Mathematics::double3
  GetObjectAsPoint(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::Array1<DotNet::Unity::Mathematics::double3>
  GetObjectAsMultiPoint(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumGeoJsonLineString GetObjectAsLineString(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGeoJsonLineString>
  GetObjectAsMultiLineString(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumGeoJsonPolygon
  GetObjectAsPolygon(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGeoJsonPolygon>
  GetObjectAsMultiPolygon(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGeoJsonObject>
  GetObjectAsGeometryCollection(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  bool HasStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumVectorStyle
  GetStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  void SetStyle(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object,
      DotNet::CesiumForUnity::CesiumVectorStyle style);

  void ClearStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  CesiumVectorData::GeoJsonObject* getNativeObject() const {
    return _pObject;
  }

  std::shared_ptr<CesiumVectorData::GeoJsonDocument> getDocument() const {
    return _pDocument;
  }

private:
  // The document that owns this object (keeps it alive)
  std::shared_ptr<CesiumVectorData::GeoJsonDocument> _pDocument;
  // Pointer to the actual object within the document (or standalone copy)
  CesiumVectorData::GeoJsonObject* _pObject;
  // For standalone objects that aren't part of a document
  std::shared_ptr<CesiumVectorData::GeoJsonObject> _pStandaloneObject;
};

} // namespace CesiumForUnityNative
