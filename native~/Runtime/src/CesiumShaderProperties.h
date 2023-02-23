#pragma once
#include <DotNet/System/String.h>

#include <cstdint>
#include <vector>

using namespace DotNet;

namespace CesiumForUnityNative {
class CesiumShaderProperties {
public:
  CesiumShaderProperties();
  const int32_t getBaseColorFactorID() const { return baseColorFactorID; }
  const int32_t getMetallicRoughnessFactorID() const {
    return metallicRoughnessFactorID;
  }
  const int32_t getBaseColorTextureID() const { return baseColorTextureID; }
  const int32_t getBaseColorTextureCoordinateIndexID() const {
    return baseColorTextureCoordinateIndexID;
  }
  const int32_t getMetallicRoughnessTextureID() const {
    return metallicRoughnessTextureID;
  }
  const int32_t getMetallicRoughnessTextureCoordinateIndexID() const {
    return metallicRoughnessTextureCoordinateIndexID;
  }
  const int32_t getNormalMapTextureID() const { return normalMapTextureID; }
  const int32_t getNormalMapTextureCoordinateIndexID() const {
    return normalMapTextureCoordinateIndexID;
  }
  const int32_t getNormalMapScaleID() const { return normalMapScaleID; }
  const int32_t getOcclusionTextureID() const { return occlusionTextureID; }
  const int32_t getOcclusionTextureCoordinateIndexID() const {
    return occlusionTextureCoordinateIndexID;
  }
  const int32_t getOcclusionStrengthID() const { return occlusionStrengthID; }
  const int32_t getEmissiveFactorID() const { return emissiveFactorID; }
  const int32_t getEmissiveTextureID() const { return emissiveTextureID; }
  const int32_t getEmissiveTextureCoordinateIndexID() const {
    return emissiveTextureCoordinateIndexID;
  }

  const int32_t getOverlayTextureCoordinateIndexID(int32_t index) {
    return overlayTextureCoordinateIndexID[index];
  }
  const int32_t getOverlayTextureID(int32_t index) {
    return overlayTextureID[index];
  }
  const int32_t getOverlayTranslationAndScaleID(int32_t index) {
    return overlayTranslationAndScaleID[index];
  }

private:
  int32_t baseColorFactorID;
  int32_t metallicRoughnessFactorID;
  int32_t baseColorTextureID;
  int32_t baseColorTextureCoordinateIndexID;
  int32_t metallicRoughnessTextureID;
  int32_t metallicRoughnessTextureCoordinateIndexID;
  int32_t normalMapTextureID;
  int32_t normalMapTextureCoordinateIndexID;
  int32_t normalMapScaleID;
  int32_t occlusionTextureID;
  int32_t occlusionTextureCoordinateIndexID;
  int32_t occlusionStrengthID;
  int32_t emissiveFactorID;
  int32_t emissiveTextureID;
  int32_t emissiveTextureCoordinateIndexID;

  std::vector<int32_t> overlayTextureCoordinateIndexID;
  std::vector<int32_t> overlayTextureID;
  std::vector<int32_t> overlayTranslationAndScaleID;
};
} // namespace CesiumForUnityNative
