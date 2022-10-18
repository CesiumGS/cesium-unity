#pragma once

#include <Cesium3DTilesSelection/IPrepareRendererResources.h>

#include <DotNet/UnityEngine/GameObject.h>

namespace CesiumForUnityNative {

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
};

} // namespace CesiumForUnityNative
