#pragma once

#include <Cesium3DTilesSelection/IPrepareRendererResources.h>

#include <Oxidize/UnityEngine/GameObject.h>

namespace CesiumForUnity {

class UnityPrepareRendererResources
    : public Cesium3DTilesSelection::IPrepareRendererResources {
public:
  UnityPrepareRendererResources(
      const ::Oxidize::UnityEngine::GameObject& tileset);

  virtual void* prepareInLoadThread(
      const CesiumGltf::Model& model,
      const glm::dmat4& transform) override;

  virtual void* prepareInMainThread(
      Cesium3DTilesSelection::Tile& tile,
      void* pLoadThreadResult) override;

  virtual void free(
      Cesium3DTilesSelection::Tile& tile,
      void* pLoadThreadResult,
      void* pMainThreadResult) noexcept override;

  virtual void* prepareRasterInLoadThread(
      const CesiumGltf::ImageCesium& image,
      const std::any& rendererOptions) override;

  virtual void* prepareRasterInMainThread(
      const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
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
  ::Oxidize::UnityEngine::GameObject _tileset;
};

} // namespace CesiumForUnity
