#include "TilesetMaterialProperties.h"

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Shader.h>

#include <unordered_set>

using namespace DotNet;
using namespace DotNet::UnityEngine;

namespace CesiumForUnityNative {

#pragma region Parameter Names
const std::string TilesetMaterialProperties::_doubleSidedEnableName =
    "_DoubleSidedEnable";
const std::string TilesetMaterialProperties::_cullName = "_Cull";
const std::string TilesetMaterialProperties::_cullModeName = "_CullMode";
const std::string TilesetMaterialProperties::_builtInCullModeName =
    "_BUILTIN_CullMode";

const std::string TilesetMaterialProperties::_baseColorFactorName =
    "_baseColorFactor";
const std::string TilesetMaterialProperties::_baseColorTextureName =
    "_baseColorTexture";
const std::string
    TilesetMaterialProperties::_baseColorTextureCoordinateIndexName =
        "_baseColorTextureCoordinateIndex";

const std::string TilesetMaterialProperties::_normalMapTextureName =
    "_normalMapTexture";
const std::string
    TilesetMaterialProperties::_normalMapTextureCoordinateIndexName =
        "_normalMapTextureCoordinateIndex";
const std::string TilesetMaterialProperties::_normalMapScaleName =
    "_normalMapScale";

const std::string TilesetMaterialProperties::_metallicRoughnessFactorName =
    "_metallicRoughnessFactor";
const std::string TilesetMaterialProperties::_metallicRoughnessTextureName =
    "_metallicRoughnessTexture";
const std::string
    TilesetMaterialProperties::_metallicRoughnessTextureCoordinateIndexName =
        "_metallicRoughnessTextureCoordinateIndex";

const std::string TilesetMaterialProperties::_occlusionTextureName =
    "_occlusionTexture";
const std::string
    TilesetMaterialProperties::_occlusionTextureCoordinateIndexName =
        "_occlusionTextureCoordinateIndex";
const std::string TilesetMaterialProperties::_occlusionStrengthName =
    "_occlusionStrength";

const std::string TilesetMaterialProperties::_emissiveFactorName =
    "_emissiveFactor";
const std::string TilesetMaterialProperties::_emissiveTextureName =
    "_emissiveTexture";
const std::string
    TilesetMaterialProperties::_emissiveTextureCoordinateIndexName =
        "_emissiveTextureCoordinateIndex";

const std::string TilesetMaterialProperties::_baseColorTextureRotationName =
    "_baseColorTextureRotation";
const std::string
    TilesetMaterialProperties::_metallicRoughnessTextureRotationName =
        "_metallicRoughnessTextureRotation";
const std::string TilesetMaterialProperties::_emissiveTextureRotationName =
    "_emissiveTextureRotation";
const std::string TilesetMaterialProperties::_normalMapTextureRotationName =
    "_normalMapTextureRotation";
const std::string TilesetMaterialProperties::_occlusionTextureRotationName =
    "_occlusionTextureRotation";

const std::string TilesetMaterialProperties::_overlayTexturePrefix =
    "_overlayTexture_";
const std::string
    TilesetMaterialProperties::_overlayTextureCoordinateIndexPrefix =
        "_overlayTextureCoordinateIndex_";
const std::string TilesetMaterialProperties::_overlayTranslationAndScalePrefix =
    "_overlayTranslationAndScale_";

const std::string TilesetMaterialProperties::_computeFlatNormalsName =
    "_computeFlatNormals";
#pragma endregion

TilesetMaterialProperties::TilesetMaterialProperties()
    : _doubleSidedEnableID(
          Shader::PropertyToID(System::String(_doubleSidedEnableName))),
      _cullID(Shader::PropertyToID(System::String(_cullName))),
      _cullModeID(Shader::PropertyToID(System::String(_cullModeName))),
      _builtInCullModeID(
          Shader::PropertyToID(System::String(_builtInCullModeName))),
      _baseColorFactorID(
          Shader::PropertyToID(System::String(_baseColorFactorName))),
      _baseColorTextureID(
          Shader::PropertyToID(System::String(_baseColorTextureName))),
      _baseColorTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_baseColorTextureCoordinateIndexName))),
      _normalMapTextureID(
          Shader::PropertyToID(System::String(_normalMapTextureName))),
      _normalMapTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_normalMapTextureCoordinateIndexName))),
      _normalMapScaleID(
          Shader::PropertyToID(System::String(_normalMapScaleName))),
      _metallicRoughnessFactorID(
          Shader::PropertyToID(System::String(_metallicRoughnessFactorName))),
      _metallicRoughnessTextureID(
          Shader::PropertyToID(System::String(_metallicRoughnessTextureName))),
      _metallicRoughnessTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_metallicRoughnessTextureCoordinateIndexName))),
      _occlusionTextureID(
          Shader::PropertyToID(System::String(_occlusionTextureName))),
      _occlusionTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_occlusionTextureCoordinateIndexName))),
      _occlusionStrengthID(
          Shader::PropertyToID(System::String(_occlusionStrengthName))),
      _emissiveFactorID(
          Shader::PropertyToID(System::String(_emissiveFactorName))),
      _emissiveTextureID(
          Shader::PropertyToID(System::String(_emissiveTextureName))),
      _emissiveTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_emissiveTextureCoordinateIndexName))),
      _baseColorTextureRotationID(
          Shader::PropertyToID(System::String(_baseColorTextureRotationName))),
      _metallicRoughnessTextureRotationID(Shader::PropertyToID(
          System::String(_metallicRoughnessTextureRotationName))),
      _emissiveTextureRotationID(
          Shader::PropertyToID(System::String(_emissiveTextureRotationName))),
      _normalMapTextureRotationID(
          Shader::PropertyToID(System::String(_normalMapTextureRotationName))),
      _occlusionTextureRotationID(
          Shader::PropertyToID(System::String(_occlusionTextureRotationName))),
      _computeFlatNormalsID(
          Shader::PropertyToID(System::String(_computeFlatNormalsName))),
      _overlayTextureCoordinateIndexIDs(),
      _overlayTextureIDs(),
      _overlayTranslationAndScaleIDs() {}

const std::optional<int32_t>
TilesetMaterialProperties::getOverlayTextureCoordinateIndexID(
    const std::string& key) const noexcept {
  auto iter = this->_overlayTextureCoordinateIndexIDs.find(key);
  if (iter == this->_overlayTextureCoordinateIndexIDs.end()) {
    return std::nullopt;
  }
  return this->_overlayTextureCoordinateIndexIDs.at(key);
}

const std::optional<int32_t> TilesetMaterialProperties::getOverlayTextureID(
    const std::string& key) const noexcept {
  auto iter = this->_overlayTextureIDs.find(key);
  if (iter == this->_overlayTextureIDs.end()) {
    return std::nullopt;
  }
  return this->_overlayTextureIDs.at(key);
}

const std::optional<int32_t>
TilesetMaterialProperties::getOverlayTranslationAndScaleID(
    const std::string& key) const noexcept {
  auto iter = this->_overlayTranslationAndScaleIDs.find(key);
  if (iter == this->_overlayTranslationAndScaleIDs.end()) {
    return std::nullopt;
  }
  return this->_overlayTranslationAndScaleIDs.at(key);
}

void TilesetMaterialProperties::updateOverlayParameterIDs(
    const std::vector<std::string>& overlayMaterialKeys) {
  const size_t size = overlayMaterialKeys.size();

  this->_overlayTextureIDs.reserve(size);
  this->_overlayTextureCoordinateIndexIDs.reserve(size);
  this->_overlayTranslationAndScaleIDs.reserve(size);

  System::String texturePrefix(_overlayTexturePrefix);
  System::String textureCoordinateIndexPrefix(
      _overlayTextureCoordinateIndexPrefix);
  System::String translationAndScalePrefix(_overlayTranslationAndScalePrefix);

  std::unordered_set<std::string> uniqueKeys;

  for (size_t i = 0; i < size; i++) {
    const std::string& keyStlString = overlayMaterialKeys[i];
    if (uniqueKeys.find(keyStlString) != uniqueKeys.end()) {
      UnityEngine::Debug::LogWarning(
          System::String("Two or more raster overlays use the same material "
                         "key on the same Cesium3DTileset. This will cause "
                         "unexpected behavior, as only one of them will be "
                         "passed to the tileset's material."));
      continue;
    }
    uniqueKeys.insert(keyStlString);

    const DotNet::System::String key(keyStlString);
    this->_overlayTextureIDs.insert(
        {keyStlString,
         Shader::PropertyToID(System::String::Concat(texturePrefix, key))});
    this->_overlayTextureCoordinateIndexIDs.insert(
        {keyStlString,
         Shader::PropertyToID(
             System::String::Concat(textureCoordinateIndexPrefix, key))});
    this->_overlayTranslationAndScaleIDs.insert(
        {keyStlString,
         Shader::PropertyToID(
             System::String::Concat(translationAndScalePrefix, key))});
  }
}

} // namespace CesiumForUnityNative
