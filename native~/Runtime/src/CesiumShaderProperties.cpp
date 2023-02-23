#include <CesiumShaderProperties.h>

#include <DotNet/UnityEngine/Shader.h>

using namespace DotNet::UnityEngine;

namespace CesiumForUnityNative {

CesiumShaderProperties::CesiumShaderProperties() {
  baseColorFactorID = Shader::PropertyToID(System::String("_baseColorFactor"));
  metallicRoughnessFactorID =
      Shader::PropertyToID(System::String("_metallicRoughnessFactor"));
  baseColorTextureID =
      Shader::PropertyToID(System::String("_baseColorTexture"));
  baseColorTextureCoordinateIndexID =
      Shader::PropertyToID(System::String("_baseColorTextureCoordinateIndex"));
  metallicRoughnessTextureID =
      Shader::PropertyToID(System::String("_metallicRoughnessTexture"));
  metallicRoughnessTextureCoordinateIndexID = Shader::PropertyToID(
      System::String("_metallicRoughnessTextureCoordinateIndex"));
  normalMapTextureID =
      Shader::PropertyToID(System::String("_normalMapTexture"));
  normalMapTextureCoordinateIndexID =
      Shader::PropertyToID(System::String("_normalMapTextureCoordinateIndex"));
  normalMapScaleID = Shader::PropertyToID(System::String("_normalMapScale"));
  occlusionTextureID =
      Shader::PropertyToID(System::String("_occlusionTexture"));
  occlusionTextureCoordinateIndexID =
      Shader::PropertyToID(System::String("_occlusionTextureCoordinateIndex"));
  occlusionStrengthID =
      Shader::PropertyToID(System::String("_occlusionStrength"));
  emissiveFactorID = Shader::PropertyToID(System::String("_emissiveFactor"));
  emissiveTextureID = Shader::PropertyToID(System::String("_emissiveTexture"));
  emissiveTextureCoordinateIndexID =
      Shader::PropertyToID(System::String("_emissiveTextureCoordinateIndex"));

  overlayTextureCoordinateIndexID = {
      Shader::PropertyToID(System::String("_overlay0TextureCoordinateIndex")),
      Shader::PropertyToID(System::String("_overlay1TextureCoordinateIndex")),
      Shader::PropertyToID(System::String("_overlay2TextureCoordinateIndex")),
      Shader::PropertyToID(System::String("_overlay3TextureCoordinateIndex"))};

  overlayTextureID = {
      Shader::PropertyToID(System::String("_overlay0Texture")),
      Shader::PropertyToID(System::String("_overlay1Texture")),
      Shader::PropertyToID(System::String("_overlay2Texture")),
      Shader::PropertyToID(System::String("_overlay3Texture"))};

  overlayTranslationAndScaleID = {
      Shader::PropertyToID(System::String("_overlay0TranslationAndScale")),
      Shader::PropertyToID(System::String("_overlay1TranslationAndScale")),
      Shader::PropertyToID(System::String("_overlay2TranslationAndScale")),
      Shader::PropertyToID(System::String("_overlay3TranslationAndScale"))};
}
} // namespace CesiumForUnityNative
