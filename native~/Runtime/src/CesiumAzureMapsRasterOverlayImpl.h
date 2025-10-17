// Copyright 2020-2025 CesiumGS, Inc. and Contributors
#pragma once

#include "CesiumImpl.h"

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumAzureMapsRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class AzureMapsRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumAzureMapsRasterOverlayImpl
    : public CesiumImpl<CesiumAzureMapsRasterOverlayImpl> {
public:
  CesiumAzureMapsRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay);
  ~CesiumAzureMapsRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<CesiumRasterOverlays::AzureMapsRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
