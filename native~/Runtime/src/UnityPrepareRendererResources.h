#pragma once

#include <Cesium3DTilesSelection/IPrepareRendererResources.h>
#include <CesiumShaderProperties.h>

#include <DotNet/UnityEngine/GameObject.h>

namespace CesiumForUnityNative {

/**
 * @brief Information about how a given glTF primitive was converted into
 * Unity MeshData.
 */
struct CesiumPrimitiveInfo {
  /**
   * @brief Whether or not the primitive's mode is set to POINTS.
   * This affects whether or not it can be baked into a physics mesh.
   */
  bool containsPoints = false;

  /**
   * @brief Maps a texture coordinate index i (TEXCOORD_<i>) to the
   * corresponding Unity texture coordinate index.
   */
  std::unordered_map<uint32_t, uint32_t> uvIndexMap{};

  /**
   * @brief Maps an overlay texture coordinate index i (_CESIUMOVERLAY_<i>) to
   * the corresponding Unity texture coordinate index.
   */
  std::unordered_map<uint32_t, uint32_t> rasterOverlayUvIndexMap{};
};

/**
 * @brief The fully loaded game object for this glTF and associated information.
 */
struct CesiumGltfGameObject {
  /**
   * @brief The fully loaded Unity game objet for this glTF.
   */
  std::unique_ptr<::DotNet::UnityEngine::GameObject> pGameObject{};

  /**
   * @brief Information about how glTF mesh primitives were translated to Unity
   * meshes.
   */
  std::vector<CesiumPrimitiveInfo> primitiveInfos{};
};

class UnityPrepareRendererResources
    : public Cesium3DTilesSelection::IPrepareRendererResources {
public:
  UnityPrepareRendererResources(
      const ::DotNet::UnityEngine::GameObject& tileset);

  virtual CesiumAsync::Future<
      Cesium3DTilesSelection::TileLoadResultAndRenderResources>
  prepareInLoadThread(
      const CesiumAsync::AsyncSystem& asyncSystem,
      Cesium3DTilesSelection::TileLoadResult&& tileLoadResult,
      const glm::dmat4& transform,
      const std::any& rendererOptions) override;

  virtual void* prepareInMainThread(
      Cesium3DTilesSelection::Tile& tile,
      void* pLoadThreadResult) override;

  virtual void free(
      Cesium3DTilesSelection::Tile& tile,
      void* pLoadThreadResult,
      void* pMainThreadResult) noexcept override;

  virtual void* prepareRasterInLoadThread(
      CesiumGltf::ImageCesium& image,
      const std::any& rendererOptions) override;

  virtual void* prepareRasterInMainThread(
      Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
      void* pLoadThreadResult) override;

  virtual void freeRaster(
      const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
      void* pLoadThreadResult,
      void* pMainThreadResult) noexcept override;

  virtual void attachRasterInMainThread(
      const Cesium3DTilesSelection::Tile& tile,
      int32_t overlayTextureCoordinateID,
      const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
      void* pMainThreadRendererResources,
      const glm::dvec2& translation,
      const glm::dvec2& scale) override;

  virtual void detachRasterInMainThread(
      const Cesium3DTilesSelection::Tile& tile,
      int32_t overlayTextureCoordinateID,
      const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
      void* pMainThreadRendererResources) noexcept override;

private:
  ::DotNet::UnityEngine::GameObject _tileset;
  CesiumShaderProperties _shaderProperty;
};

} // namespace CesiumForUnityNative
