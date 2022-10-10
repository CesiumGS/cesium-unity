#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

namespace Cesium3DTilesSelection {
class TilesetExternals;
}

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace CesiumForUnityNative {

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

}
