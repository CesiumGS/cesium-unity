#pragma once

#include "CesiumImpl.h"

#include <CesiumVectorData/GeoJsonDocument.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class CesiumGeoJsonDocument;
class CesiumGeoJsonObject;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
template <typename T> class Action1;
} // namespace DotNet::System

namespace CesiumForUnityNative {

class CesiumGeoJsonDocumentImpl : public CesiumImpl<CesiumGeoJsonDocumentImpl> {
public:
  CesiumGeoJsonDocumentImpl(
      const DotNet::CesiumForUnity::CesiumGeoJsonDocument& document);
  ~CesiumGeoJsonDocumentImpl();

  void setNativeDocument(CesiumVectorData::GeoJsonDocument&& document);

  bool IsValid(const DotNet::CesiumForUnity::CesiumGeoJsonDocument& document);

  DotNet::CesiumForUnity::CesiumGeoJsonObject
  GetRootObject(const DotNet::CesiumForUnity::CesiumGeoJsonDocument& document);

  static DotNet::CesiumForUnity::CesiumGeoJsonDocument
  Parse(DotNet::System::String geoJsonString);

  static void LoadFromUrl(
      DotNet::System::String url,
      DotNet::System::Action1<DotNet::CesiumForUnity::CesiumGeoJsonDocument>
          callback);

  static void LoadFromCesiumIon(
      std::int64_t ionAssetId,
      DotNet::System::String ionAccessToken,
      DotNet::System::String ionApiUrl,
      DotNet::System::Action1<DotNet::CesiumForUnity::CesiumGeoJsonDocument>
          callback);

  void DisposeNative(const DotNet::CesiumForUnity::CesiumGeoJsonDocument& document);

  const CesiumVectorData::GeoJsonDocument& getNativeDocument() const {
    return _document;
  }

private:
  CesiumVectorData::GeoJsonDocument _document;
  bool _isValid;
};

} // namespace CesiumForUnityNative
