#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

namespace Cesium3DTilesSelection {
class TilesetExternals;
}

namespace DotNet::UnityEngine {
class GameObject;
}

namespace CesiumForUnity {

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const ::DotNet::UnityEngine::GameObject& tileset);

}
