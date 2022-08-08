#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

namespace Cesium3DTilesSelection {
class TilesetExternals;
}

namespace Oxidize::UnityEngine {
class GameObject;
}

namespace CesiumForUnity {

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const ::Oxidize::UnityEngine::GameObject& tileset);

}
