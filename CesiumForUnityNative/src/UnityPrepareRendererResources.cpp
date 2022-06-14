#include "UnityPrepareRendererResources.h"

#include "Bindings.h"

#include <Cesium3DTilesSelection/GltfContent.h>
#include <Cesium3DTilesSelection/Tile.h>
#include <Cesium3DTilesSelection/TileContentLoadResult.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumGltf/AccessorView.h>

#include <glm/gtc/matrix_inverse.hpp>

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnity;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace CesiumGltf;
using namespace System;

namespace {

template <typename T>
void setTriangles(UnityEngine::Mesh& mesh, const AccessorView<T>& indices) {
  Array1<System::Int32> triangles(indices.size());
  for (int64_t i = 0; i < indices.size(); ++i) {
    triangles[i] = indices[i];
  }
  mesh.SetTriangles(triangles, 0, true, 0);
}

} // namespace

UnityPrepareRendererResources::UnityPrepareRendererResources(
    UnityEngine::GameObject& tileset)
    : _tileset(tileset) {}

void* UnityPrepareRendererResources::prepareInLoadThread(
    const CesiumGltf::Model& model,
    const glm::dmat4& transform) {
  return nullptr;
}

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult) {
  TileContentLoadResult* pContent = tile.getContent();
  if (!pContent) {
    return nullptr;
  }

  if (!pContent->model) {
    return nullptr;
  }

  const QuadtreeTileID* pQ = std::get_if<QuadtreeTileID>(&tile.getTileID());
  if (pQ) {
    if (pQ->level >= 14) {
      UnityEngine::Debug::Log(String(">=14"));
    }
  }

  const Model& model = *pContent->model;

  std::string name = "glTF";
  auto urlIt = model.extras.find("Cesium3DTiles_TileUrl");
  if (urlIt != model.extras.end()) {
    name = urlIt->second.getStringOrDefault("glTF");
  }

  UnityEngine::GameObject modelGameObject(String(name.c_str()));
  modelGameObject.GetTransform().SetParent(this->_tileset.GetTransform());

  glm::dmat4 tileTransform = tile.getTransform();
  tileTransform = GltfContent::applyRtcCenter(model, tileTransform);
  tileTransform = GltfContent::applyGltfUpAxisTransform(model, tileTransform);

  model.forEachPrimitiveInScene(
      -1,
      [&modelGameObject, &tileTransform](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        if (primitive.indices < 0) {
          // TODO: support non-indexed primitives.
          return;
        }

        auto positionAccessorIt = primitive.attributes.find("POSITION");
        if (positionAccessorIt == primitive.attributes.end()) {
          // This primitive doesn't have a POSITION semantic, ignore it.
          return;
        }

        int32_t positionAccessorID = positionAccessorIt->second;
        AccessorView<UnityEngine::Vector3> positionView(
            gltf,
            positionAccessorID);
        if (positionView.status() != AccessorViewStatus::Valid) {
          // TODO: report invalid accessor
          return;
        }

        // TODO: better name (index of mesh and primitive?)
        UnityEngine::GameObject primitiveGameObject(String("Primitive"));
        primitiveGameObject.GetTransform().SetParent(
            modelGameObject.GetTransform());

        // Hard-coded "georeference" to put the Unity origin at a default
        // location in Denver and adjust for Unity left-handed, Y-up
        // convention.
        glm::dvec3 origin = Ellipsoid::WGS84.cartographicToCartesian(
            Cartographic::fromDegrees(-105.25737, 39.736401, 2250.0));
        glm::dmat4 enuToFixed = Transforms::eastNorthUpToFixedFrame(origin);
        glm::dmat4 fixedToEnu = glm::affineInverse(enuToFixed);
        glm::dmat4 swapYandZ(
            glm::dvec4(1.0, 0.0, 0.0, 0.0),
            glm::dvec4(0.0, 0.0, 1.0, 0.0),
            glm::dvec4(0.0, 1.0, 0.0, 0.0),
            glm::dvec4(0.0, 0.0, 0.0, 1.0));

        glm::dmat4 fullTransform =
            swapYandZ * (fixedToEnu * (tileTransform * transform));

        // Unity transforms can't be assigned a simple 4x4 matrix, because
        // that would be too easy. So we need to decompose the matrix into
        // position, rotation, and scale.
        // TODO: the approach here might not be perfect in all cases. See
        // https://pixel.engineer/posts/matrix-decomposition-unity/.
        static_assert(sizeof(UnityEngine::Matrix4x4) == sizeof(glm::mat4));
        UnityEngine::Matrix4x4 unityMatrix(
            UnityEngine::Vector4(
                fullTransform[0].x,
                fullTransform[0].y,
                fullTransform[0].z,
                fullTransform[0].w),
            UnityEngine::Vector4(
                fullTransform[1].x,
                fullTransform[1].y,
                fullTransform[1].z,
                fullTransform[1].w),
            UnityEngine::Vector4(
                fullTransform[2].x,
                fullTransform[2].y,
                fullTransform[2].z,
                fullTransform[2].w),
            UnityEngine::Vector4(
                fullTransform[3].x,
                fullTransform[3].y,
                fullTransform[3].z,
                fullTransform[3].w));

        UnityEngine::Vector3 position = unityMatrix.GetPosition();
        UnityEngine::Quaternion rotation = unityMatrix.GetRotation();
        UnityEngine::Vector3 scale = unityMatrix.GetLossyScale();

        primitiveGameObject.GetTransform().SetPosition(position);
        primitiveGameObject.GetTransform().SetRotation(rotation);
        primitiveGameObject.GetTransform().SetLocalScale(scale);

        UnityEngine::MeshFilter meshFilter =
            primitiveGameObject.AddComponent<UnityEngine::MeshFilter>();
        UnityEngine::MeshRenderer meshRenderer =
            primitiveGameObject.AddComponent<UnityEngine::MeshRenderer>();

        UnityEngine::Mesh unityMesh{};

        Array1<UnityEngine::Vector3> vertices(positionView.size());
        for (int64_t i = 0; i < positionView.size(); ++i) {
          vertices[i] = positionView[i];
        }
        unityMesh.SetVertices(vertices);

        AccessorView<uint8_t> indices8(gltf, primitive.indices);
        if (indices8.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices8);
        }

        AccessorView<uint16_t> indices16(gltf, primitive.indices);
        if (indices16.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices16);
        }

        AccessorView<uint32_t> indices32(gltf, primitive.indices);
        if (indices32.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices32);
        }

        meshFilter.SetMesh(unityMesh);
      });

  return nullptr;
}

void UnityPrepareRendererResources::free(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {}

void* UnityPrepareRendererResources::prepareRasterInLoadThread(
    const CesiumGltf::ImageCesium& image,
    const std::any& rendererOptions) {
  return nullptr;
}

void* UnityPrepareRendererResources::prepareRasterInMainThread(
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult) {
  return nullptr;
}

void UnityPrepareRendererResources::freeRaster(
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {}

void UnityPrepareRendererResources::attachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources,
    const glm::dvec2& translation,
    const glm::dvec2& scale) {}

void UnityPrepareRendererResources::detachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources) noexcept {}
