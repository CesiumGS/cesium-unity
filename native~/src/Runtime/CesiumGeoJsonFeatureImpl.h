#pragma once

#include "CesiumImpl.h"

#include <CesiumVectorData/GeoJsonObject.h>

#include <DotNet/System/String.h>

namespace DotNet::CesiumForUnity {
class CesiumGeoJsonFeature;
class CesiumGeoJsonObject;
struct CesiumVectorStyle;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class CesiumGeoJsonFeatureImpl
    : public CesiumImpl<CesiumGeoJsonFeatureImpl> {
public:
  CesiumGeoJsonFeatureImpl(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);
  ~CesiumGeoJsonFeatureImpl();

  void setNativeFeatureInDocument(
      CesiumVectorData::GeoJsonFeature* pFeature);

  std::int32_t
  GetIdType(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  DotNet::System::String
  GetIdAsString(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  std::int64_t
  GetIdAsInteger(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  DotNet::System::String GetPropertiesAsJson(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  DotNet::System::String GetStringProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature,
      DotNet::System::String propertyName);

  double GetNumericProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature,
      DotNet::System::String propertyName);

  bool HasProperty(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature,
      DotNet::System::String propertyName);

  DotNet::CesiumForUnity::CesiumGeoJsonObject
  GetGeometry(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  bool HasStyle(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  DotNet::CesiumForUnity::CesiumVectorStyle
  GetStyle(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  void SetStyle(
      const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature,
      DotNet::CesiumForUnity::CesiumVectorStyle style);

  void
  ClearStyle(const DotNet::CesiumForUnity::CesiumGeoJsonFeature& feature);

  CesiumVectorData::GeoJsonFeature* getNativeFeature() const {
    return _pFeature;
  }

private:
  CesiumVectorData::GeoJsonFeature* _pFeature;
};

} // namespace CesiumForUnityNative
