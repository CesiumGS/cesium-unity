#pragma once

#include "CesiumImpl.h"

#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>

#include <DotNet/System/String.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class CesiumGeoJsonObject;
struct CesiumVectorStyle;
} // namespace DotNet::CesiumForUnity

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

  std::int32_t
  GetChildCount(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumGeoJsonObject
  GetChild(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object, std::int32_t index);

  bool HasStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::CesiumForUnity::CesiumVectorStyle
  GetStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  void SetStyle(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object,
      DotNet::CesiumForUnity::CesiumVectorStyle style);

  void ClearStyle(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::String
  GetFeatureIdString(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  std::int64_t
  GetFeatureIdInt(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  bool HasFeatureId(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  bool HasFeatureIdString(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::String GetPropertiesAsJson(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  DotNet::System::String GetStringProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object,
      DotNet::System::String propertyName);

  double GetNumericProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object,
      DotNet::System::String propertyName);

  bool HasProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonObject& object,
      DotNet::System::String propertyName);

  void DisposeNative(const DotNet::CesiumForUnity::CesiumGeoJsonObject& object);

  CesiumVectorData::GeoJsonObject* getNativeObject() const {
    return _pObject;
  }

  std::shared_ptr<CesiumVectorData::GeoJsonDocument> getDocument() const {
    return _pDocument;
  }

  // Set a direct pointer to a feature within a document
  void setNativeFeatureInDocument(
      std::shared_ptr<CesiumVectorData::GeoJsonDocument> pDocument,
      CesiumVectorData::GeoJsonFeature* pFeature);

private:
  // The document that owns this object (keeps it alive)
  std::shared_ptr<CesiumVectorData::GeoJsonDocument> _pDocument;
  // Pointer to the actual object within the document (or standalone copy)
  CesiumVectorData::GeoJsonObject* _pObject;
  // Direct pointer to a feature (for child features in collections)
  CesiumVectorData::GeoJsonFeature* _pFeature;
  // For standalone objects that aren't part of a document
  std::shared_ptr<CesiumVectorData::GeoJsonObject> _pStandaloneObject;
};

} // namespace CesiumForUnityNative
