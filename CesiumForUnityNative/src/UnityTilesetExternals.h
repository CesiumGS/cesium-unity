#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

namespace Cesium3DTilesSelection {
class TilesetExternals;
}

namespace UnityEngine {
struct GameObject;
}

namespace CesiumForUnity {

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(UnityEngine::GameObject& tileset);

}
