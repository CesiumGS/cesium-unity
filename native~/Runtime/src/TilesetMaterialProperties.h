#pragma once

#include <DotNet/System/String.h>

#include <cassert>
#include <cstdint>
#include <vector>

namespace CesiumForUnityNative {

class TilesetMaterialProperties {
public:
  TilesetMaterialProperties();

  const int32_t getBaseColorFactorID() const noexcept {
    return this->_baseColorFactorID;
  }
  const int32_t getBaseColorTextureID() const noexcept {
    return this->_baseColorTextureID;
  }
  const int32_t getBaseColorTextureCoordinateIndexID() const noexcept {
    return this->_baseColorTextureCoordinateIndexID;
  }

  const int32_t getNormalMapTextureID() const noexcept {
    return this->_normalMapTextureID;
  }
  const int32_t getNormalMapTextureCoordinateIndexID() const noexcept {
    return this->_normalMapTextureCoordinateIndexID;
  }
  const int32_t getNormalMapScaleID() const noexcept {
    return this->_normalMapScaleID;
  }

  const int32_t getMetallicRoughnessFactorID() const noexcept {
    return this->_metallicRoughnessFactorID;
  }
  const int32_t getMetallicRoughnessTextureID() const noexcept {
    return this->_metallicRoughnessTextureID;
  }
  const int32_t getMetallicRoughnessTextureCoordinateIndexID() const noexcept {
    return this->_metallicRoughnessTextureCoordinateIndexID;
  }

  const int32_t getOcclusionTextureID() const noexcept {
    return this->_occlusionTextureID;
  }
  const int32_t getOcclusionTextureCoordinateIndexID() const noexcept {
    return this->_occlusionTextureCoordinateIndexID;
  }
  const int32_t getOcclusionStrengthID() const noexcept {
    return this->_occlusionStrengthID;
  }

  const int32_t getEmissiveFactorID() const { return this->_emissiveFactorID; }
  const int32_t getEmissiveTextureID() const noexcept {
    return this->_emissiveTextureID;
  }
  const int32_t getEmissiveTextureCoordinateIndexID() const noexcept {
    return this->_emissiveTextureCoordinateIndexID;
  }

  const int32_t getBaseColorTextureRotationID() const noexcept {
    return this->_baseColorTextureRotationID;
  }
  const int32_t getNormalMapTextureRotationID() const noexcept {
    return this->_normalMapTextureRotationID;
  }
  const int32_t getMetallicRoughnessTextureRotationID() const noexcept {
    return this->_metallicRoughnessTextureRotationID;
  }
  const int32_t getEmissiveTextureRotationID() const noexcept {
    return this->_emissiveTextureRotationID;
  }
  const int32_t getOcclusionTextureRotationID() const noexcept {
    return this->_occlusionTextureRotationID;
  }

  const int32_t getOverlayTextureCoordinateIndexID(int32_t index) {
    return this->_overlayTextureCoordinateIndexIDs[index];
  }
  const int32_t getOverlayTextureID(int32_t index) {
    return this->_overlayTextureIDs[index];
  }
  const int32_t getOverlayTranslationAndScaleID(int32_t index) {
    return this->_overlayTranslationAndScaleIDs[index];
  }

  void updateOverlayParameterIDs(
      const std::vector<std::string>& overlayMaterialKeys);

private:
  int32_t _baseColorFactorID;
  int32_t _baseColorTextureID;
  int32_t _baseColorTextureCoordinateIndexID;

  int32_t _normalMapTextureID;
  int32_t _normalMapTextureCoordinateIndexID;
  int32_t _normalMapScaleID;

  int32_t _metallicRoughnessFactorID;
  int32_t _metallicRoughnessTextureID;
  int32_t _metallicRoughnessTextureCoordinateIndexID;

  int32_t _occlusionTextureID;
  int32_t _occlusionTextureCoordinateIndexID;
  int32_t _occlusionStrengthID;

  int32_t _emissiveFactorID;
  int32_t _emissiveTextureID;
  int32_t _emissiveTextureCoordinateIndexID;

  int32_t _baseColorTextureRotationID;
  int32_t _metallicRoughnessTextureRotationID;
  int32_t _emissiveTextureRotationID;
  int32_t _normalMapTextureRotationID;
  int32_t _occlusionTextureRotationID;

  std::vector<int32_t> _overlayTextureCoordinateIndexIDs;
  std::vector<int32_t> _overlayTextureIDs;
  std::vector<int32_t> _overlayTranslationAndScaleIDs;

  static const DotNet::System::String _baseColorFactorName;
  static const DotNet::System::String _baseColorTextureName;
  static const DotNet::System::String _baseColorTextureCoordinateIndexName;

  static const DotNet::System::String _normalMapTextureName;
  static const DotNet::System::String _normalMapTextureCoordinateIndexName;
  static const DotNet::System::String _normalMapScaleName;

  static const DotNet::System::String _metallicRoughnessFactorName;
  static const DotNet::System::String _metallicRoughnessTextureName;
  static const DotNet::System::String
      _metallicRoughnessTextureCoordinateIndexName;

  static const DotNet::System::String _occlusionTextureName;
  static const DotNet::System::String _occlusionTextureCoordinateIndexName;
  static const DotNet::System::String _occlusionStrengthName;

  static const DotNet::System::String _emissiveFactorName;
  static const DotNet::System::String _emissiveTextureName;
  static const DotNet::System::String _emissiveTextureCoordinateIndexName;

  static const DotNet::System::String _baseColorTextureRotationName;
  static const DotNet::System::String _metallicRoughnessTextureRotationName;
  static const DotNet::System::String _emissiveTextureRotationName;
  static const DotNet::System::String _normalMapTextureRotationName;
  static const DotNet::System::String _occlusionTextureRotationName;

  static const DotNet::System::String _overlayTexturePrefix;
  static const DotNet::System::String _overlayTextureCoordinateIndexPrefix;
  static const DotNet::System::String _overlayTranslationAndScalePrefix;
};

} // namespace CesiumForUnityNative
