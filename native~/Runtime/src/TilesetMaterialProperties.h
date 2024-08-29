#pragma once

#include <DotNet/System/String.h>

#include <cstdint>
#include <optional>
#include <unordered_map>
#include <vector>

namespace CesiumForUnityNative {

class TilesetMaterialProperties {
public:
  TilesetMaterialProperties();

  const int32_t getDoubleSidedEnableID() const noexcept {
    return this->_doubleSidedEnableID;
  }
  const int32_t getCullID() const noexcept { return this->_cullID; }
  const int32_t getBuiltInCullModeID() const noexcept {
    return this->_builtInCullModeID;
  }
  const int32_t getCullModeID() const noexcept { return this->_cullModeID; }

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

  const std::optional<int32_t>
  getOverlayTextureCoordinateIndexID(const std::string& key) const noexcept;
  const std::optional<int32_t>
  getOverlayTextureID(const std::string& key) const noexcept;
  const std::optional<int32_t>
  getOverlayTranslationAndScaleID(const std::string& key) const noexcept;

  void updateOverlayParameterIDs(
      const std::vector<std::string>& overlayMaterialKeys);

private:
  int32_t _doubleSidedEnableID;
  int32_t _cullID;
  int32_t _cullModeID;
  int32_t _builtInCullModeID;

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

  std::unordered_map<std::string, int32_t> _overlayTextureCoordinateIndexIDs;
  std::unordered_map<std::string, int32_t> _overlayTextureIDs;
  std::unordered_map<std::string, int32_t> _overlayTranslationAndScaleIDs;

  static const std::string _doubleSidedEnableName;
  static const std::string _cullName;
  static const std::string _cullModeName;
  static const std::string _builtInCullModeName;

  static const std::string _baseColorFactorName;
  static const std::string _baseColorTextureName;
  static const std::string _baseColorTextureCoordinateIndexName;

  static const std::string _normalMapTextureName;
  static const std::string _normalMapTextureCoordinateIndexName;
  static const std::string _normalMapScaleName;

  static const std::string _metallicRoughnessFactorName;
  static const std::string _metallicRoughnessTextureName;
  static const std::string _metallicRoughnessTextureCoordinateIndexName;

  static const std::string _occlusionTextureName;
  static const std::string _occlusionTextureCoordinateIndexName;
  static const std::string _occlusionStrengthName;

  static const std::string _emissiveFactorName;
  static const std::string _emissiveTextureName;
  static const std::string _emissiveTextureCoordinateIndexName;

  static const std::string _baseColorTextureRotationName;
  static const std::string _metallicRoughnessTextureRotationName;
  static const std::string _emissiveTextureRotationName;
  static const std::string _normalMapTextureRotationName;
  static const std::string _occlusionTextureRotationName;

  static const std::string _overlayTexturePrefix;
  static const std::string _overlayTextureCoordinateIndexPrefix;
  static const std::string _overlayTranslationAndScalePrefix;
};

} // namespace CesiumForUnityNative
