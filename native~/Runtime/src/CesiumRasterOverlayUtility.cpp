#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/RasterOverlay.h>
#include <CesiumAsync/IAssetResponse.h>

#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlayLoadFailureDetails.h>
#include <DotNet/System/String.h>

using namespace DotNet;
using namespace Cesium3DTilesSelection;

namespace CesiumForUnityNative {
/*static*/ RasterOverlayOptions CesiumRasterOverlayUtility::GetOverlayOptions(
    const DotNet::CesiumForUnity::CesiumRasterOverlay& overlay) {
  RasterOverlayOptions options{};
  options.maximumScreenSpaceError = overlay.maximumScreenSpaceError();
  options.maximumSimultaneousTileLoads = overlay.maximumSimultaneousTileLoads();
  options.maximumTextureSize = overlay.maximumTextureSize();
  options.subTileCacheBytes = overlay.subTileCacheBytes();
  options.showCreditsOnScreen = overlay.showCreditsOnScreen();
  options.loadErrorCallback =
      [overlay](const RasterOverlayLoadFailureDetails& details) {
        int typeValue = (int)details.type;
        long statusCode = details.pRequest && details.pRequest->response()
                              ? details.pRequest->response()->statusCode()
                              : 0;
        CesiumForUnity::CesiumRasterOverlayLoadFailureDetails unityDetails(
            overlay,
            CesiumForUnity::CesiumRasterOverlayLoadType(typeValue),
            statusCode,
            System::String(details.message));

        CesiumForUnity::CesiumRasterOverlay::
            BroadcastCesiumRasterOverlayLoadFailure(unityDetails);
      };

  return options;
}
} // namespace CesiumForUnityNative
