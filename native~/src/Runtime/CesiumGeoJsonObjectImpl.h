#pragma once

#include "CesiumImpl.h"

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

  void setNativeObject(std::shared_ptr<CesiumVectorData::GeoJsonObject> pObject);

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

  std::shared_ptr<CesiumVectorData::GeoJsonObject> getNativeObject() const {
    return _pObject;
  }

private:
  std::shared_ptr<CesiumVectorData::GeoJsonObject> _pObject;
};

} // namespace CesiumForUnityNative
