#include "TilesetMaterialProperties.h"

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Shader.h>

using namespace DotNet;
using namespace DotNet::UnityEngine;

namespace CesiumForUnityNative {

#pragma region Parameter Names
const DotNet::System::String TilesetMaterialProperties::_baseColorFactorName =
    System::String("_baseColorFactor");
const DotNet::System::String TilesetMaterialProperties::_baseColorTextureName =
    System::String("_baseColorTexture");
const DotNet::System::String
    TilesetMaterialProperties::_baseColorTextureCoordinateIndexName =
        System::String("_baseColorTextureCoordinateIndex");

const DotNet::System::String TilesetMaterialProperties::_normalMapTextureName =
    System::String("_normalMapTexture");
const DotNet::System::String
    TilesetMaterialProperties::_normalMapTextureCoordinateIndexName =
        System::String("_normalMapTextureCoordinateIndex");
const DotNet::System::String TilesetMaterialProperties::_normalMapScaleName =
    System::String("_normalMapScale");

const DotNet::System::String
    TilesetMaterialProperties::_metallicRoughnessFactorName =
        System::String("_metallicRoughnessFactor");
const DotNet::System::String
    TilesetMaterialProperties::_metallicRoughnessTextureName =
        System::String("_metallicRoughnessTexture");
const DotNet::System::String
    TilesetMaterialProperties::_metallicRoughnessTextureCoordinateIndexName =
        System::String("_metallicRoughnessTextureCoordinateIndex");

const DotNet::System::String TilesetMaterialProperties::_occlusionTextureName =
    System::String("_occlusionTexture");
const DotNet::System::String
    TilesetMaterialProperties::_occlusionTextureCoordinateIndexName =
        System::String("_occlusionTextureCoordinateIndex");
const DotNet::System::String TilesetMaterialProperties::_occlusionStrengthName =
    System::String("_occlusionStrength");

const DotNet::System::String TilesetMaterialProperties::_emissiveFactorName =
    System::String("_emissiveFactor");
const DotNet::System::String TilesetMaterialProperties::_emissiveTextureName =
    System::String("_emissiveTexture");
const DotNet::System::String
    TilesetMaterialProperties::_emissiveTextureCoordinateIndexName =
        System::String("_emissiveTextureCoordinateIndex");

const DotNet::System::String
    TilesetMaterialProperties::_baseColorTextureRotationName =
        "_baseColorTextureRotation";
const DotNet::System::String
    TilesetMaterialProperties::_metallicRoughnessTextureRotationName =
        "_metallicRoughnessTextureRotation";
const DotNet::System::String
    TilesetMaterialProperties::_emissiveTextureRotationName =
        "_emissiveTextureRotation";
const DotNet::System::String
    TilesetMaterialProperties::_normalMapTextureRotationName =
        "_normalMapTextureRotation";
const DotNet::System::String
    TilesetMaterialProperties::_occlusionTextureRotationName =
        "_occlusionTextureRotation";

const DotNet::System::String TilesetMaterialProperties::_overlayTexturePrefix =
    System::String("_overlayTexture_");
const DotNet::System::String
    TilesetMaterialProperties::_overlayTextureCoordinateIndexPrefix =
        System::String("_overlayTextureCoordinateIndex_");
const DotNet::System::String
    TilesetMaterialProperties::_overlayTranslationAndScalePrefix =
        System::String("_overlayTranslationAndScale_");
#pragma endregion

TilesetMaterialProperties::TilesetMaterialProperties()
    : _baseColorFactorID(Shader::PropertyToID(_baseColorFactorName)),
      _baseColorTextureID(Shader::PropertyToID(_baseColorTextureName)),
      _baseColorTextureCoordinateIndexID(
          Shader::PropertyToID(_baseColorTextureCoordinateIndexName)),
      _normalMapTextureID(Shader::PropertyToID(_normalMapTextureName)),
      _normalMapTextureCoordinateIndexID(
          Shader::PropertyToID(_normalMapTextureCoordinateIndexName)),
      _normalMapScaleID(Shader::PropertyToID(_normalMapScaleName)),
      _metallicRoughnessFactorID(
          Shader::PropertyToID(_metallicRoughnessFactorName)),
      _metallicRoughnessTextureID(
          Shader::PropertyToID(_metallicRoughnessTextureName)),
      _metallicRoughnessTextureCoordinateIndexID(
          Shader::PropertyToID(_metallicRoughnessTextureCoordinateIndexName)),
      _occlusionTextureID(Shader::PropertyToID(_occlusionTextureName)),
      _occlusionTextureCoordinateIndexID(
          Shader::PropertyToID(_occlusionTextureCoordinateIndexName)),
      _occlusionStrengthID(Shader::PropertyToID(_occlusionStrengthName)),
      _emissiveFactorID(Shader::PropertyToID(_emissiveFactorName)),
      _emissiveTextureID(
          Shader::PropertyToID(System::String(_emissiveTextureName))),
      _emissiveTextureCoordinateIndexID(Shader::PropertyToID(
          System::String(_emissiveTextureCoordinateIndexName))),
      _baseColorTextureRotationID(
          Shader::PropertyToID(_baseColorTextureRotationName)),
      _metallicRoughnessTextureRotationID(
          Shader::PropertyToID(_metallicRoughnessTextureRotationName)),
      _emissiveTextureRotationID(
          Shader::PropertyToID(_emissiveTextureRotationName)),
      _normalMapTextureRotationID(
          Shader::PropertyToID(_normalMapTextureRotationName)),
      _occlusionTextureRotationID(
          Shader::PropertyToID(_occlusionTextureRotationName)),
      _overlayTextureCoordinateIndexIDs(),
      _overlayTextureIDs(),
      _overlayTranslationAndScaleIDs() {}

void TilesetMaterialProperties::updateOverlayParameterIDs(
    const std::vector<std::string>& overlayMaterialKeys) {
  this->_overlayTextureIDs.resize(overlayMaterialKeys.size());
  this->_overlayTextureCoordinateIndexIDs.resize(overlayMaterialKeys.size());
  this->_overlayTranslationAndScaleIDs.resize(overlayMaterialKeys.size());

  for (size_t i = 0; i < overlayMaterialKeys.size(); i++) {
    const DotNet::System::String& key = System::String(overlayMaterialKeys[i]);
    this->_overlayTextureIDs[i] = Shader::PropertyToID(
        System::String::Concat(_overlayTexturePrefix, key));
    this->_overlayTextureCoordinateIndexIDs[i] = Shader::PropertyToID(
        System::String::Concat(_overlayTextureCoordinateIndexPrefix, key));
    this->_overlayTranslationAndScaleIDs[i] = Shader::PropertyToID(
        System::String::Concat(_overlayTranslationAndScalePrefix, key));
  }
}

} // namespace CesiumForUnityNative
