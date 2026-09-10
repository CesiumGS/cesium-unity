#include "UnityPrepareRendererResources.h"

#include "CesiumFeaturesMetadataUtility.h"
#include "TextureLoader.h"
#include "TilesetMaterialProperties.h"
#include "UnityLifetime.h"
#include "UnityTransforms.h"

#include <Cesium3DTilesSelection/Tile.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeometry/Transforms.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionExtMeshFeatures.h>
#include <CesiumGltf/ExtensionKhrGaussianSplatting.h>
#include <CesiumGltf/ExtensionKhrMaterialsUnlit.h>
#include <CesiumGltf/ExtensionKhrTextureTransform.h>
#include <CesiumGltf/ExtensionModelExtStructuralMetadata.h>
#include <CesiumGltf/KhrTextureTransform.h>
#include <CesiumGltfContent/GltfUtilities.h>
#include <CesiumGltfReader/GltfReader.h>
#include <CesiumImage/ImageDecoder.h>
#include <CesiumUtility/ScopeGuard.h>

#include <DotNet/CesiumForUnity/Cesium3DTileInfo.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdAttribute.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdSet.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/CesiumForUnity/CesiumMetadata.h>
#include <DotNet/CesiumForUnity/CesiumModelMetadata.h>
#include <DotNet/CesiumForUnity/CesiumObjectPool1.h>
#include <DotNet/CesiumForUnity/CesiumObjectPools.h>
#include <DotNet/CesiumForUnity/CesiumPointCloudRenderer.h>
#include <DotNet/CesiumForUnity/CesiumPrimitiveFeatures.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTable.h>
#include <DotNet/CesiumForUnity/Helpers.h>
#include <DotNet/Gsplat/GsplatAsset.h>
#include <DotNet/Gsplat/GsplatRenderer.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/Object.h>
#include <DotNet/Unity/Collections/Allocator.h>
#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/Unity/Collections/NativeArrayOptions.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/FilterMode.h>
#include <DotNet/UnityEngine/HideFlags.h>
#include <DotNet/UnityEngine/Material.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/MeshCollider.h>
#include <DotNet/UnityEngine/MeshData.h>
#include <DotNet/UnityEngine/MeshDataArray.h>
#include <DotNet/UnityEngine/MeshFilter.h>
#include <DotNet/UnityEngine/MeshRenderer.h>
#include <DotNet/UnityEngine/MeshTopology.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Rendering/CullMode.h>
#include <DotNet/UnityEngine/Rendering/IndexFormat.h>
#include <DotNet/UnityEngine/Rendering/MeshUpdateFlags.h>
#include <DotNet/UnityEngine/Rendering/SubMeshDescriptor.h>
#include <DotNet/UnityEngine/Rendering/VertexAttributeDescriptor.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/ScriptableObject.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/TextureWrapMode.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector2.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <DotNet/UnityEngine/Vector4.h>
#include <fmt/format.h>
#include <glm/gtc/matrix_inverse.hpp>
#include <glm/gtc/quaternion.hpp>

#include <algorithm>
#include <array>
#include <cstddef>
#include <unordered_map>
#include <variant>

using namespace CesiumRasterOverlays;
using namespace CesiumGltfContent;
using namespace Cesium3DTilesSelection;
using namespace CesiumForUnityNative;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace CesiumGltf;
using namespace CesiumImage;
using namespace CesiumUtility;
using namespace DotNet;

static const CesiumGltf::MaterialPBRMetallicRoughness
    defaultPbrMetallicRoughness;

namespace {

template <typename TIndex>
std::vector<TIndex> generateIndices(const int32_t count) {
  std::vector<TIndex> syntheticIndexBuffer(count);
  for (int64_t i = 0; i < count; ++i) {
    syntheticIndexBuffer[i] = static_cast<TIndex>(i);
  }
  return syntheticIndexBuffer;
}

template <typename TIndex>
void computeFlatNormals(
    uint8_t* pWritePos,
    size_t stride,
    TIndex* indices,
    int32_t indexCount,
    const AccessorView<UnityEngine::Vector3>& positionView) {

  for (int i = 0; i < indexCount; i += 3) {

    TIndex i0 = indices[i];
    TIndex i1 = indices[i + 1];
    TIndex i2 = indices[i + 2];

    const glm::vec3& v0 =
        *reinterpret_cast<const glm::vec3*>(&positionView[i0]);
    const glm::vec3& v1 =
        *reinterpret_cast<const glm::vec3*>(&positionView[i1]);
    const glm::vec3& v2 =
        *reinterpret_cast<const glm::vec3*>(&positionView[i2]);

    glm::vec3 normal = glm::normalize(glm::cross(v1 - v0, v2 - v0));
    for (int j = 0; j < 3; j++) {
      *reinterpret_cast<glm::vec3*>(pWritePos) = normal;
      pWritePos += stride;
    }
  }
}

/**
 * @brief The result after populating Unity mesh data with loaded glTF content.
 */
struct MeshDataResult {
  UnityEngine::MeshDataArray meshDataArray;
  System::Array1<DotNet::Gsplat::GsplatAssetUncompressed> splatDataArray;
  std::vector<CesiumPrimitiveInfo> primitiveInfos;
};

template <typename TIndex> struct CopyVertexColors {
  uint8_t* pWritePos;
  size_t stride;
  size_t vertexCount;
  bool duplicateVertices;
  TIndex* indices;

  struct Color32 {
    uint8_t r;
    uint8_t g;
    uint8_t b;
    uint8_t a;
  };

  bool operator()(AccessorView<std::nullptr_t>&& invalidView) { return false; }

  template <typename TColorView> bool operator()(TColorView&& colorView) {
    if (colorView.status() != AccessorViewStatus::Valid) {
      return false;
    }

    bool success = true;
    if (duplicateVertices) {
      for (size_t i = 0; success && i < vertexCount; ++i) {
        TIndex vertexIndex = indices[i];
        if (vertexIndex < 0 || vertexIndex >= colorView.size()) {
          success = false;
        } else {
          Color32& packedColor = *reinterpret_cast<Color32*>(pWritePos);
          success = CopyVertexColors::convertColor(
              colorView[vertexIndex],
              packedColor);
          pWritePos += stride;
        }
      }
    } else {
      for (size_t i = 0; success && i < vertexCount; ++i) {
        if (i >= colorView.size()) {
          success = false;
        } else {
          Color32& packedColor = *reinterpret_cast<Color32*>(pWritePos);
          success = CopyVertexColors::convertColor(colorView[i], packedColor);
          pWritePos += stride;
        }
      }
    }

    return success;
  }

  bool packColorChannel(uint8_t c, uint8_t& result) {
    result = c;
    return true;
  }

  bool packColorChannel(uint16_t c, uint8_t& result) {
    result = static_cast<uint8_t>(c >> 8);
    return true;
  }

  bool packColorChannel(float c, uint8_t& result) {
    result = static_cast<uint8_t>(static_cast<uint32_t>(255.0f * c) & 255);
    return true;
  }

  template <typename T> bool packColorChannel(T c, uint8_t& result) {
    // Invalid accessor type.
    return false;
  }

  template <typename TChannel>
  bool
  convertColor(const AccessorTypes::VEC3<TChannel>& color, Color32& result) {
    result.a = 255;
    return packColorChannel(color.value[0], result.r) &&
           packColorChannel(color.value[1], result.g) &&
           packColorChannel(color.value[2], result.b);
  }

  template <typename TChannel>
  bool
  convertColor(const AccessorTypes::VEC4<TChannel>& color, Color32& result) {
    return packColorChannel(color.value[0], result.r) &&
           packColorChannel(color.value[1], result.g) &&
           packColorChannel(color.value[2], result.b) &&
           packColorChannel(color.value[3], result.a);
  }

  template <typename T> bool convertColor(T color, Color32& result) {
    // Not an accessor.
    return false;
  }
};

bool validateVertexColors(
    const Model& model,
    uint32_t accessorId,
    size_t vertexCount) {
  if (accessorId >= model.accessors.size()) {
    return false;
  }

  const Accessor& colorAccessor = model.accessors[accessorId];
  if (colorAccessor.type != Accessor::Type::VEC3 &&
      colorAccessor.type != Accessor::Type::VEC4) {
    return false;
  }

  if (colorAccessor.componentType != Accessor::ComponentType::UNSIGNED_BYTE &&
      colorAccessor.componentType != Accessor::ComponentType::UNSIGNED_SHORT &&
      colorAccessor.componentType != Accessor::ComponentType::FLOAT) {
    return false;
  }

  if (colorAccessor.count < vertexCount) {
    return false;
  }

  return true;
}

void generateMipMaps(
    Model* pModel,
    const std::optional<TextureInfo>& textureInfo) {
  if (textureInfo) {
    Texture* pTexture = Model::getSafe(&pModel->textures, textureInfo->index);
    if (pTexture) {
      Image* pImage = Model::getSafe(&pModel->images, pTexture->source);
      const Sampler* pSampler =
          Model::getSafe(&pModel->samplers, pTexture->sampler);
      if (pImage && pSampler) {
        // We currently do not support shared resources, so if this image is
        // associated with a depot, unshare it. This is necessary to avoid a
        // race condition where multiple threads attempt to generate mipmaps for
        // the same shared image simultaneously.
        if (pImage->pAsset && pImage->pAsset->getDepot()) {
          // Copy the asset.
          pImage->pAsset.emplace(*pImage->pAsset);
        }

        switch (pSampler->minFilter.value_or(
            CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_LINEAR)) {
        case CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_LINEAR:
        case CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_NEAREST:
        case CesiumGltf::Sampler::MinFilter::NEAREST_MIPMAP_LINEAR:
        case CesiumGltf::Sampler::MinFilter::NEAREST_MIPMAP_NEAREST:
          CesiumImage::ImageDecoder::generateMipMaps(*pImage->pAsset);
        }
      }
    }
  }
}

void generateMipMapsForPrimitive(
    Model* pModel,
    const MeshPrimitive& primitive) {
  const Material* pMaterial =
      Model::getSafe(&pModel->materials, primitive.material);
  if (pMaterial) {
    if (pMaterial->pbrMetallicRoughness) {
      generateMipMaps(
          pModel,
          pMaterial->pbrMetallicRoughness->baseColorTexture);
      generateMipMaps(
          pModel,
          pMaterial->pbrMetallicRoughness->metallicRoughnessTexture);
    }
    generateMipMaps(pModel, pMaterial->normalTexture);
    generateMipMaps(pModel, pMaterial->occlusionTexture);
    generateMipMaps(pModel, pMaterial->emissiveTexture);
  }
}

template <typename TIndex, class TIndexAccessor>
void loadPrimitive(
    UnityEngine::MeshData meshData,
    CesiumPrimitiveInfo& primitiveInfo,
    const CreateModelOptions& options,
    const Model& gltf,
    const Node& node,
    const Mesh& mesh,
    const MeshPrimitive& primitive,
    const glm::dmat4& transform,
    const TIndexAccessor& indicesView,
    UnityEngine::Rendering::IndexFormat indexFormat,
    const AccessorView<UnityEngine::Vector3>& positionView) {
  using namespace DotNet::UnityEngine;
  using namespace DotNet::UnityEngine::Rendering;
  using namespace DotNet::Unity::Collections;
  using namespace DotNet::Unity::Collections::LowLevel::Unsafe;

  CESIUM_TRACE("Cesium::loadPrimitive<T>");
  int32_t indexCount = static_cast<int32_t>(indicesView.size());
  switch (primitive.mode) {
  case MeshPrimitive::Mode::POINTS:
    break;
  case MeshPrimitive::Mode::LINES:
    if (indexCount < 2) {
      return;
    }
    break;
  case MeshPrimitive::Mode::TRIANGLES:
    if (indexCount < 3) {
      return;
    }
    break;
  default:
    // The options set in Cesium3DTilesetImpl::LoadTileset should ensure we only
    // receive supported primitive modes.
    CESIUM_ASSERT(false);
    return;
  }

  const CesiumGltf::Material* pMaterial =
      Model::getSafe(&gltf.materials, primitive.material);

  primitiveInfo.isUnlit =
      options.ignoreKhrMaterialUnlit
          ? false
          : pMaterial && pMaterial->hasExtension<ExtensionKhrMaterialsUnlit>();

  bool hasNormals = false;
  bool computeFlatNormals = false;
  auto normalAccessorIt = primitive.attributes.find("NORMAL");
  AccessorView<UnityEngine::Vector3> normalView;
  if (normalAccessorIt != primitive.attributes.end()) {
    normalView =
        AccessorView<UnityEngine::Vector3>(gltf, normalAccessorIt->second);
    hasNormals = normalView.status() == AccessorViewStatus::Valid;
  } else if (
      !primitiveInfo.isUnlit &&
      primitive.mode == MeshPrimitive::Mode::TRIANGLES) {
    computeFlatNormals = hasNormals = true;
  }

  // Check if we need to upgrade to a large index type to accommodate the
  // larger number of vertices we need for flat normals.
  if (computeFlatNormals && indexFormat == IndexFormat::UInt16 &&
      indexCount >= std::numeric_limits<uint16_t>::max()) {
    loadPrimitive<uint32_t>(
        meshData,
        primitiveInfo,
        options,
        gltf,
        node,
        mesh,
        primitive,
        transform,
        indicesView,
        IndexFormat::UInt32,
        positionView);
    return;
  }

  meshData.SetIndexBufferParams(indexCount, indexFormat);
  const Unity::Collections::NativeArray1<TIndex>& dest =
      meshData.GetIndexData<TIndex>();
  TIndex* indices = static_cast<TIndex*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(dest));

  for (int64_t i = 0; i < indicesView.size(); ++i) {
    indices[i] = indicesView[i];
  }

  // Max attribute count supported by Unity, see VertexAttribute.
  const int MAX_ATTRIBUTES = 14;
  VertexAttributeDescriptor descriptor[MAX_ATTRIBUTES];

  // Interleave all attributes into single stream.
  std::int32_t numberOfAttributes = 0;
  std::int32_t streamIndex = 0;

  assert(numberOfAttributes < MAX_ATTRIBUTES);
  descriptor[numberOfAttributes].attribute = VertexAttribute::Position;
  descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
  descriptor[numberOfAttributes].dimension = 3;
  descriptor[numberOfAttributes].stream = streamIndex;
  ++numberOfAttributes;

  // Add the NORMAL attribute, if it exists.
  if (hasNormals) {
    assert(numberOfAttributes < MAX_ATTRIBUTES);
    descriptor[numberOfAttributes].attribute = VertexAttribute::Normal;
    descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
    descriptor[numberOfAttributes].dimension = 3;
    descriptor[numberOfAttributes].stream = streamIndex;
    ++numberOfAttributes;
  }

  // Add the COLOR_0 attribute, if it exists.
  auto colorAccessorIt = primitive.attributes.find("COLOR_0");
  bool hasVertexColors =
      colorAccessorIt != primitive.attributes.end() &&
      validateVertexColors(gltf, colorAccessorIt->second, positionView.size());
  if (hasVertexColors) {
    assert(numberOfAttributes < MAX_ATTRIBUTES);

    // Unity expects the vertex colors to come as 4 normalized uint8s.
    descriptor[numberOfAttributes].attribute = VertexAttribute::Color;
    descriptor[numberOfAttributes].format = VertexAttributeFormat::UNorm8;
    descriptor[numberOfAttributes].dimension = 4;
    descriptor[numberOfAttributes].stream = streamIndex;
    ++numberOfAttributes;

    const int8_t numComponents =
        gltf.accessors[colorAccessorIt->second].computeNumberOfComponents();
    if (numComponents == 4) {
      primitiveInfo.isTranslucent = true;
    }
  }

  // Max number of texture coordinates supported by Unity, see
  // VertexAttribute.
  constexpr int MAX_TEX_COORDS = 8;
  int numTexCoords = 0;
  AccessorView<UnityEngine::Vector2> texCoordViews[MAX_TEX_COORDS];

  // Add all texture coordinate sets TEXCOORD_i
  for (int i = 0; i < 8 && numTexCoords < MAX_TEX_COORDS; ++i) {
    // TODO: Only add texture coordinates that are needed.
    // E.g., might not need UV coords for metadata.

    // Build accessor view for glTF attribute.
    auto texCoordAccessorIt =
        primitive.attributes.find("TEXCOORD_" + std::to_string(i));
    if (texCoordAccessorIt == primitive.attributes.end()) {
      continue;
    }

    AccessorView<UnityEngine::Vector2> texCoordView(
        gltf,
        texCoordAccessorIt->second);
    if (texCoordView.status() != AccessorViewStatus::Valid &&
        texCoordView.size() >= positionView.size()) {
      // TODO: report invalid accessor?
      continue;
    }

    texCoordViews[numTexCoords] = texCoordView;
    primitiveInfo.uvIndexMap[i] = numTexCoords;

    // Build Unity descriptor for this attribute.
    assert(numberOfAttributes < MAX_ATTRIBUTES);

    descriptor[numberOfAttributes].attribute =
        (VertexAttribute)((int)VertexAttribute::TexCoord0 + numTexCoords);
    descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
    descriptor[numberOfAttributes].dimension = 2;
    descriptor[numberOfAttributes].stream = streamIndex;

    ++numTexCoords;
    ++numberOfAttributes;
  }

  // Add all texture coordinate sets _CESIUMOVERLAY_i
  for (int i = 0; i < 8 && numTexCoords < MAX_TEX_COORDS; ++i) {
    // Build accessor view for glTF attribute.
    auto overlayAccessorIt =
        primitive.attributes.find("_CESIUMOVERLAY_" + std::to_string(i));
    if (overlayAccessorIt == primitive.attributes.end()) {
      continue;
    }

    AccessorView<UnityEngine::Vector2> overlayTexCoordView(
        gltf,
        overlayAccessorIt->second);
    if (overlayTexCoordView.status() != AccessorViewStatus::Valid &&
        overlayTexCoordView.size() >= positionView.size()) {
      // TODO: report invalid accessor?
      continue;
    }

    texCoordViews[numTexCoords] = overlayTexCoordView;
    primitiveInfo.rasterOverlayUvIndexMap[i] = numTexCoords;

    // Build Unity descriptor for this attribute.
    assert(numberOfAttributes < MAX_ATTRIBUTES);

    descriptor[numberOfAttributes].attribute =
        (VertexAttribute)((int)VertexAttribute::TexCoord0 + numTexCoords);
    descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
    descriptor[numberOfAttributes].dimension = 2;
    descriptor[numberOfAttributes].stream = streamIndex;

    ++numTexCoords;
    ++numberOfAttributes;
  }

  System::Array1<VertexAttributeDescriptor> attributes(numberOfAttributes);
  for (int32_t i = 0; i < numberOfAttributes; ++i) {
    attributes.Item(i, descriptor[i]);
  }

  int32_t vertexCount = computeFlatNormals
                            ? indexCount
                            : static_cast<int32_t>(positionView.size());
  meshData.SetVertexBufferParams(vertexCount, attributes);

  NativeArray1<uint8_t> nativeVertexBuffer =
      meshData.GetVertexData<uint8_t>(streamIndex);
  uint8_t* pBufferStart = static_cast<uint8_t*>(
      NativeArrayUnsafeUtility::GetUnsafeBufferPointerWithoutChecks(
          nativeVertexBuffer));
  uint8_t* pWritePos = pBufferStart;

  // Since the vertex buffer is dynamically interleaved, we don't have a
  // convenient struct to represent the vertex data.
  // The vertex layout will be as follows:
  // 1. position
  // 2. normals (skip if N/A)
  // 3. vertex colors (skip if N/A)
  // 4. texcoords (first all TEXCOORD_i, then all _CESIUMOVERLAY_i)

  size_t stride = sizeof(Vector3);
  size_t normalByteOffset, colorByteOffset;
  if (hasNormals) {
    normalByteOffset = stride;
    stride += sizeof(Vector3);
  }
  if (hasVertexColors) {
    colorByteOffset = stride;
    stride += sizeof(uint32_t);
  }
  stride += numTexCoords * sizeof(Vector2);

  if (computeFlatNormals) {
    ::computeFlatNormals(
        pWritePos + normalByteOffset,
        stride,
        indices,
        indexCount,
        positionView);
    for (int64_t i = 0; i < vertexCount; ++i) {
      TIndex vertexIndex = indices[i];
      *reinterpret_cast<Vector3*>(pWritePos) = positionView[vertexIndex];
      // skip position and normal
      pWritePos += 2 * sizeof(Vector3);
      // Skip the slot allocated for vertex colors, we will fill them in
      // bulk later.
      if (hasVertexColors) {
        pWritePos += sizeof(uint32_t);
      }
      for (uint32_t texCoordIndex = 0; texCoordIndex < numTexCoords;
           ++texCoordIndex) {
        *reinterpret_cast<Vector2*>(pWritePos) =
            texCoordViews[texCoordIndex][vertexIndex];
        pWritePos += sizeof(Vector2);
      }
    }
  } else {
    for (int64_t i = 0; i < vertexCount; ++i) {
      *reinterpret_cast<Vector3*>(pWritePos) = positionView[i];
      pWritePos += sizeof(Vector3);

      if (hasNormals) {
        *reinterpret_cast<Vector3*>(pWritePos) = normalView[i];
        pWritePos += sizeof(Vector3);
      }

      // Skip the slot allocated for vertex colors, we will fill them in
      // bulk later.
      if (hasVertexColors) {
        pWritePos += sizeof(uint32_t);
      }

      for (uint32_t texCoordIndex = 0; texCoordIndex < numTexCoords;
           ++texCoordIndex) {
        *reinterpret_cast<Vector2*>(pWritePos) =
            texCoordViews[texCoordIndex][i];
        pWritePos += sizeof(Vector2);
      }
    }
  }

  // Fill in vertex colors separately, if they exist.
  if (hasVertexColors) {
    // Color comes after position and normal.
    createAccessorView(
        gltf,
        colorAccessorIt->second,
        CopyVertexColors<TIndex>{
            pBufferStart + colorByteOffset,
            stride,
            static_cast<size_t>(vertexCount),
            computeFlatNormals,
            indices});
  }

  if (computeFlatNormals) {
    // rewrite indices
    for (TIndex i = 0; i < indexCount; i++) {
      indices[i] = i;
    }
  }

  meshData.subMeshCount(1);

  // TODO: use sub-meshes for glTF primitives, instead of a separate mesh
  // for each.
  SubMeshDescriptor subMeshDescriptor{};

  switch (primitive.mode) {
  case MeshPrimitive::Mode::POINTS:
    subMeshDescriptor.topology = MeshTopology::Points;
    break;
  case MeshPrimitive::Mode::LINES:
    subMeshDescriptor.topology = MeshTopology::Lines;
    break;
  case MeshPrimitive::Mode::TRIANGLES:
  default:
    subMeshDescriptor.topology = MeshTopology::Triangles;
    break;
  }
  primitiveInfo.mode = primitive.mode;

  subMeshDescriptor.indexStart = 0;
  subMeshDescriptor.indexCount = indexCount;
  subMeshDescriptor.baseVertex = 0;

  // These are calculated automatically by SetSubMesh
  subMeshDescriptor.firstVertex = 0;
  subMeshDescriptor.vertexCount = 0;

  meshData.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags::Default);
}
} // namespace

std::tuple<int32_t, int32_t> countPrimitives(const CesiumGltf::Model& model) {
  int32_t numberOfMeshPrimitives = 0;
  int32_t numberOfSplatPrimitives = 0;
  model.forEachPrimitiveInScene(
      -1,
      [&numberOfMeshPrimitives, &numberOfSplatPrimitives](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        if (primitive.hasExtension<ExtensionKhrGaussianSplatting>()) {
          ++numberOfSplatPrimitives;
        } else {
          ++numberOfMeshPrimitives;
        }
      });
  return {numberOfMeshPrimitives, numberOfSplatPrimitives};
}

int32_t countShCoeffsOnPrimitive(const CesiumGltf::MeshPrimitive& primitive) {
  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_3_COEF_6")) {
    return 15;
  }

  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_2_COEF_4")) {
    return 8;
  }

  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_1_COEF_2")) {
    return 3;
  }

  return 0;
}

bool writeShCoeffs(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    System::Array1<UnityEngine::Vector3>& data,
    int32_t stride,
    int32_t offset,
    int32_t degree) {
  const int32_t numCoeffs = 3 + 2 * (degree - 1);
  for (int32_t i = 0; i < numCoeffs; i++) {
    std::unordered_map<std::string, int32_t>::const_iterator accessorIt =
        meshPrimitive.attributes.find(
            fmt::format(
                "KHR_gaussian_splatting:SH_DEGREE_{}_COEF_{}",
                degree,
                i));
    if (accessorIt == meshPrimitive.attributes.end()) {
      UnityEngine::Debug::LogError(
          System::String(
              fmt::format(
                  "Could not find spherical harmonic attribute for degree {} "
                  "index "
                  "{} on mesh primitive",
                  degree,
                  i)));
      return false;
    }

    CesiumGltf::AccessorView<glm::vec3> accessorView(model, accessorIt->second);
    if (accessorView.status() != CesiumGltf::AccessorViewStatus::Valid) {
      UnityEngine::Debug::LogError(
          System::String(
              fmt::format(
                  "Accessor view for spherical harmonic attribute degree {} "
                  "index "
                  "{} on mesh primitive returned invalid status: {}",
                  degree,
                  i,
                  (int32_t)accessorView.status())));
      return false;
    }

    for (int32_t j = 0; j < accessorView.size(); j++) {
      data.Item(
          j * stride + offset + i,
          UnityEngine::Vector3(
              accessorView[j].x,
              accessorView[j].y,
              accessorView[j].z));
    }
  }

  return true;
}

template <typename T, typename ComponentT>
void writeConvertedAccessor(
    const CesiumGltf::AccessorView<T>& accessorView,
    System::Array1<UnityEngine::Vector4>& data) {
  for (int32_t i = 0; i < accessorView.size(); i++) {
    data.Item(
        i,
        UnityEngine::Vector4(
            accessorView[i].x /
                static_cast<float>(std::numeric_limits<ComponentT>::max()),
            accessorView[i].y /
                static_cast<float>(std::numeric_limits<ComponentT>::max()),
            accessorView[i].z /
                static_cast<float>(std::numeric_limits<ComponentT>::max()),
            accessorView[i].w /
                static_cast<float>(std::numeric_limits<ComponentT>::max())));
  }
}

void populateGsplatAsset(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    DotNet::Gsplat::GsplatAssetUncompressed& asset) {

  const int32_t numShCoeffs = countShCoeffsOnPrimitive(meshPrimitive);
  asset.SHBands(numShCoeffs);

  const std::unordered_map<std::string, int32_t>::const_iterator positionIt =
      meshPrimitive.attributes.find("POSITION");
  if (positionIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String("Mesh primitive has no 'POSITION' attribute"));
    return;
  }

  CesiumGltf::AccessorView<glm::vec3> positionView(model, positionIt->second);
  if (positionView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'POSITION' accessor view on mesh primitive returned invalid "
                "status: "
                "{}",
                (int32_t)positionView.status())));
    return;
  }

  asset.SplatCount(positionView.size());
  DotNet::System::Array1<UnityEngine::Vector3> positions(positionView.size());

  std::optional<std::tuple<UnityEngine::Vector3, UnityEngine::Vector3>> bounds;
  for (int32_t i = 0; i < positionView.size(); i++) {
    UnityEngine::Vector3 position(
        positionView[i].x,
        positionView[i].y,
        positionView[i].z);
    positions.Item(i, position);

    // Take this opportunity to update the bounds.
    if (bounds) {
      bounds = {
          UnityEngine::Vector3(
              std::min(std::get<0>(*bounds).x, position.x),
              std::min(std::get<0>(*bounds).y, position.y),
              std::min(std::get<0>(*bounds).z, position.z)),
          UnityEngine::Vector3(
              std::max(std::get<1>(*bounds).x, position.x),
              std::max(std::get<1>(*bounds).y, position.y),
              std::max(std::get<1>(*bounds).z, position.z))};
    } else {
      bounds = {position, position};
    }
  }

  asset.Positions(positions);

  if (bounds) {
    UnityEngine::Vector3 min = std::get<0>(*bounds);
    UnityEngine::Vector3 max = std::get<1>(*bounds);
    UnityEngine::Bounds finalBounds = UnityEngine::Bounds::Construct(
        UnityEngine::Vector3(
            (min.x + max.x) * 0.5f,
            (min.y + max.y) * 0.5f,
            (min.z + max.z) * 0.5f),
        UnityEngine::Vector3(max.x - min.x, max.y - min.y, max.z - min.z));
    asset.Bounds(finalBounds);
  }

  const std::unordered_map<std::string, int32_t>::const_iterator scaleIt =
      meshPrimitive.attributes.find("KHR_gaussian_splatting:SCALE");
  if (scaleIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String(
            "Mesh primitive has no 'KHR_gaussian_splatting:SCALE' attribute"));
    return;
  }

  CesiumGltf::AccessorView<glm::vec3> scaleView(model, scaleIt->second);
  if (scaleView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:SCALE' accessor view on mesh "
                "primitive returned invalid status: {}",
                (int32_t)scaleView.status())));
    return;
  }

  System::Array1<UnityEngine::Vector3> scales(scaleView.size());

  for (int32_t i = 0; i < scaleView.size(); i++) {
    scales.Item(
        i,
        UnityEngine::Vector3(scaleView[i].x, scaleView[i].y, scaleView[i].z));
  }

  asset.Scales(scales);

  const std::unordered_map<std::string, int32_t>::const_iterator rotationIt =
      meshPrimitive.attributes.find("KHR_gaussian_splatting:ROTATION");
  if (rotationIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String(
            "Mesh primitive has no 'KHR_gaussian_splatting:ROTATION' "
            "attribute"));
    return;
  }

  CesiumGltf::AccessorView<glm::vec4> rotationView(model, rotationIt->second);
  if (rotationView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:ROTATION' accessor view on mesh "
                "primitive returned invalid status: {}",
                (int32_t)rotationView.status())));
    return;
  }

  System::Array1<UnityEngine::Vector4> rotations(rotationView.size());

  for (int32_t i = 0; i < rotationView.size(); i++) {
    rotations.Item(
        i,
        UnityEngine::Vector4(
            rotationView[i].x,
            rotationView[i].y,
            rotationView[i].z,
            rotationView[i].w));
  }

  asset.Rotations(rotations);

  const std::unordered_map<std::string, int32_t>::const_iterator colorIt =
      meshPrimitive.attributes.find("COLOR_0");
  if (colorIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String("Mesh primitive has no 'COLOR_0' attribute"));
    return;
  }

  if (colorIt->second < 0 || colorIt->second >= model.accessors.size()) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "Mesh primitive has invalid 'COLOR_0' accessor index {}",
                colorIt->second)));
    return;
  }

  const CesiumGltf::Accessor& colorAccessor = model.accessors[colorIt->second];
  if (colorAccessor.componentType ==
      CesiumGltf::AccessorSpec::ComponentType::UNSIGNED_BYTE) {
    CesiumGltf::AccessorView<glm::u8vec4> accessorView(model, colorIt->second);
    if (accessorView.status() != CesiumGltf::AccessorViewStatus::Valid) {
      UnityEngine::Debug::LogError(
          System::String(
              fmt::format(
                  "'COLOR_0' accessor view on mesh primitive returned invalid "
                  "status: {}",
                  (int32_t)accessorView.status())));
      return;
    }

    System::Array1<UnityEngine::Vector4> colors(accessorView.size());
    writeConvertedAccessor<glm::u8vec4, uint8_t>(accessorView, colors);
    asset.Colors(colors);
  } else if (
      colorAccessor.componentType ==
      CesiumGltf::AccessorSpec::ComponentType::UNSIGNED_SHORT) {
    CesiumGltf::AccessorView<glm::u16vec4> accessorView(model, colorIt->second);
    if (accessorView.status() != CesiumGltf::AccessorViewStatus::Valid) {
      UnityEngine::Debug::LogError(
          System::String(
              fmt::format(
                  "'COLOR_0' accessor view on mesh primitive returned invalid "
                  "status: {}",
                  (int32_t)accessorView.status())));
      return;
    }

    System::Array1<UnityEngine::Vector4> colors(accessorView.size());
    writeConvertedAccessor<glm::u16vec4, uint16_t>(accessorView, colors);
    asset.Colors(colors);
  } else if (
      colorAccessor.componentType ==
      CesiumGltf::AccessorSpec::ComponentType::FLOAT) {
    CesiumGltf::AccessorView<glm::vec4> accessorView(model, colorIt->second);
    if (accessorView.status() != CesiumGltf::AccessorViewStatus::Valid) {
      UnityEngine::Debug::LogError(
          System::String(
              fmt::format(
                  "'COLOR_0' accessor view on mesh primitive returned invalid "
                  "status: {}",
                  (int32_t)accessorView.status())));
      return;
    }

    System::Array1<UnityEngine::Vector4> colors(accessorView.size());
    for (int32_t i = 0; i < accessorView.size(); i++) {
      colors.Item(
          i,
          UnityEngine::Vector4(
              accessorView[i].x,
              accessorView[i].y,
              accessorView[i].z,
              accessorView[i].w));
    }
    asset.Colors(colors);
  } else {
    UnityEngine::Debug::LogError(
        System::String(
            "Invalid 'COLOR_0' componentType. Allowed values are "
            "UNSIGNED_BYTE, UNSIGNED_SHORT, and FLOAT."));
    return;
  }

  System::Array1<UnityEngine::Vector3> harmonics(
      numShCoeffs * asset.SplatCount());
  if (numShCoeffs >= 3) {
    if (!writeShCoeffs(model, meshPrimitive, harmonics, numShCoeffs, 0, 1)) {
      return;
    }
  }

  if (numShCoeffs >= 8) {
    if (!writeShCoeffs(model, meshPrimitive, harmonics, numShCoeffs, 3, 2)) {
      return;
    }
  }

  if (numShCoeffs >= 15) {
    if (!writeShCoeffs(
            model,
            meshPrimitive,
            harmonics,
            numShCoeffs,
            5 + 3,
            3)) {
      return;
    }
  }

  asset.SHs(harmonics);
}

void populateMeshDataArray(
    MeshDataResult& meshDataResult,
    TileLoadResult& tileLoadResult,
    const CreateModelOptions& options) {
  CesiumGltf::Model* pModel =
      std::get_if<CesiumGltf::Model>(&tileLoadResult.contentKind);
  if (!pModel)
    return;

  int32_t meshDataInstance = 0;
  int32_t splatDataInstance = 0;

  meshDataResult.primitiveInfos.reserve(
      meshDataResult.meshDataArray.Length() +
      meshDataResult.splatDataArray.Length());

  pModel->forEachPrimitiveInScene(
      -1,
      [&meshDataResult,
       &splatDataInstance,
       &meshDataInstance,
       pModel,
       &options](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        CesiumPrimitiveInfo& primitiveInfo =
            meshDataResult.primitiveInfos.emplace_back();

        if (const auto* pGaussianSplattingExtension =
                primitive.getExtension<
                    CesiumGltf::ExtensionKhrGaussianSplatting>()) {
          if (primitive.mode != MeshPrimitive::Mode::POINTS) {
            // Gaussian splatting extension requires the primitive to be of type
            // POINTS. If it's not, we ignore this primitive.
            return;
          }

          DotNet::Gsplat::GsplatAssetUncompressed splatAsset =
              meshDataResult.splatDataArray[splatDataInstance];
          populateGsplatAsset(
              *pModel,
              primitive, splatAsset);
          primitiveInfo.gsplatIndex = splatDataInstance++;
          return;
        }

        UnityEngine::MeshData meshData =
            meshDataResult.meshDataArray[meshDataInstance++];

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

        generateMipMapsForPrimitive(pModel, primitive);

        if (primitive.indices < 0 ||
            primitive.indices >= gltf.accessors.size()) {
          int32_t indexCount = static_cast<int32_t>(positionView.size());
          if (indexCount > std::numeric_limits<std::uint16_t>::max()) {
            loadPrimitive<std::uint32_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                generateIndices<std::uint32_t>(indexCount),
                UnityEngine::Rendering::IndexFormat::UInt32,
                positionView);
          } else {
            loadPrimitive<std::uint16_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                generateIndices<std::uint16_t>(indexCount),
                UnityEngine::Rendering::IndexFormat::UInt16,
                positionView);
          }
        } else {
          const Accessor& indexAccessorGltf = gltf.accessors[primitive.indices];
          switch (indexAccessorGltf.componentType) {
          case Accessor::ComponentType::BYTE: {
            AccessorView<int8_t> indexAccessor(gltf, primitive.indices);
            loadPrimitive<std::uint16_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                indexAccessor,
                UnityEngine::Rendering::IndexFormat::UInt16,
                positionView);
            break;
          }
          case Accessor::ComponentType::UNSIGNED_BYTE: {
            AccessorView<uint8_t> indexAccessor(gltf, primitive.indices);
            loadPrimitive<std::uint16_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                indexAccessor,
                UnityEngine::Rendering::IndexFormat::UInt16,
                positionView);
            break;
          }
          case Accessor::ComponentType::SHORT: {
            AccessorView<int16_t> indexAccessor(gltf, primitive.indices);
            loadPrimitive<std::uint16_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                indexAccessor,
                UnityEngine::Rendering::IndexFormat::UInt16,
                positionView);
            break;
          }
          case Accessor::ComponentType::UNSIGNED_SHORT: {
            AccessorView<uint16_t> indexAccessor(gltf, primitive.indices);
            loadPrimitive<std::uint16_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                indexAccessor,
                UnityEngine::Rendering::IndexFormat::UInt16,
                positionView);
            break;
          }
          case Accessor::ComponentType::UNSIGNED_INT: {
            AccessorView<uint32_t> indexAccessor(gltf, primitive.indices);
            loadPrimitive<std::uint32_t>(
                meshData,
                primitiveInfo,
                options,
                gltf,
                node,
                mesh,
                primitive,
                transform,
                indexAccessor,
                UnityEngine::Rendering::IndexFormat::UInt32,
                positionView);
            break;
          }
          default:
            return;
          }
        }
      });
}

bool isDegenerateTriangleMesh(const UnityEngine::Mesh& mesh) {
  int32_t vertexCount = mesh.vertexCount();
  if (vertexCount < 3) {
    return true;
  }

  if (vertexCount == 3) {
    System::Array1<UnityEngine::Vector3> vertices = mesh.vertices();
    glm::vec3 Vertex0(vertices[0].x, vertices[0].y, vertices[0].z);
    glm::vec3 Vertex1(vertices[1].x, vertices[1].y, vertices[1].z);
    glm::vec3 Vertex2(vertices[2].x, vertices[2].y, vertices[2].z);

    return Vertex0 == Vertex1 || Vertex1 == Vertex2 || Vertex2 == Vertex0;
  }

  return false;
}

/**
 * @brief The result of the async part of mesh loading.
 */
struct LoadThreadResult {
  System::Array1<UnityEngine::Mesh> meshes;
  System::Array1<DotNet::Gsplat::GsplatAssetUncompressed> splatAssets;
  std::vector<CesiumPrimitiveInfo> primitiveInfos{};
};

UnityPrepareRendererResources::UnityPrepareRendererResources(
    const UnityEngine::GameObject& tilesetGameObject)
    : _tilesetGameObject(tilesetGameObject), _materialProperties() {}

CesiumAsync::Future<TileLoadResultAndRenderResources>
UnityPrepareRendererResources::prepareInLoadThread(
    const CesiumAsync::AsyncSystem& asyncSystem,
    TileLoadResult&& tileLoadResult,
    const glm::dmat4& transform,
    const std::any& rendererOptions) {

  CesiumGltf::Model* pModel =
      std::get_if<CesiumGltf::Model>(&tileLoadResult.contentKind);
  if (!pModel)
    return asyncSystem.createResolvedFuture(
        TileLoadResultAndRenderResources{std::move(tileLoadResult), nullptr});

  auto [numberOfMeshPrimitives, numberOfSplatPrimitives] =
      countPrimitives(*pModel);

  struct IntermediateLoadThreadResult {
    MeshDataResult meshDataResult;
    TileLoadResult tileLoadResult;
  };

  return asyncSystem
      .runInMainThread(
          [numberOfMeshPrimitives, numberOfSplatPrimitives]()
              -> std::tuple<
                  UnityEngine::MeshDataArray,
                  System::Array1<DotNet::Gsplat::GsplatAssetUncompressed>> {
            System::Array1<DotNet::Gsplat::GsplatAssetUncompressed>
                gsplatAssets(numberOfSplatPrimitives);
            for (int32_t i = 0; i < numberOfSplatPrimitives; i++) {
              gsplatAssets.Item(
                  i,
                  UnityEngine::ScriptableObject::CreateInstance<
                      DotNet::Gsplat::GsplatAssetUncompressed>());
            }
            // Allocate a MeshDataArray for the primitives.
            // Unfortunately, this must be done on the main thread.
            return {
                UnityEngine::Mesh::AllocateWritableMeshData(
                    numberOfMeshPrimitives),
                gsplatAssets};
          })
#ifndef __EMSCRIPTEN__
      .thenInWorkerThread(
#else
      // Unity Wasm can only access managed code from the main thread.
      .thenInMainThread(
#endif
          [tileLoadResult = std::move(tileLoadResult), rendererOptions](
              std::tuple<
                  UnityEngine::MeshDataArray,
                  System::Array1<DotNet::Gsplat::GsplatAssetUncompressed>>&&
                  result) mutable {
            MeshDataResult meshDataResult{
                std::move(std::get<0>(result)),
                std::move(std::get<1>(result)),
                {}};
            // Free the MeshDataArray if something goes wrong.
            ScopeGuard sg([&meshDataResult]() {
              meshDataResult.meshDataArray.Dispose();
            });

            const auto* pOptions =
                std::any_cast<CreateModelOptions>(&rendererOptions);
            if (pOptions)
              populateMeshDataArray(meshDataResult, tileLoadResult, *pOptions);
            else
              populateMeshDataArray(meshDataResult, tileLoadResult, {});

            // We're returning the MeshDataArray, so don't free it.
            sg.release();
            return IntermediateLoadThreadResult{
                std::move(meshDataResult),
                std::move(tileLoadResult)};
          })
      .thenInMainThread(
          [asyncSystem, tileset = this->_tilesetGameObject](
              IntermediateLoadThreadResult&& workerResult) mutable {
            if (tileset == nullptr) {
              // Tileset GameObject was deleted while we were loading a tile
              // (possibly play mode was exited or another cause).
              return asyncSystem.createResolvedFuture(
                  TileLoadResultAndRenderResources{
                      std::move(workerResult.tileLoadResult),
                      nullptr});
            }
            bool shouldCreatePhysicsMeshes = false;
            bool shouldShowTilesInHierarchy = false;

            auto tilesetComponent =
                tileset.GetComponent<DotNet::CesiumForUnity::Cesium3DTileset>();
            if (tilesetComponent != nullptr) {
              shouldCreatePhysicsMeshes =
                  tilesetComponent.createPhysicsMeshes();
              shouldShowTilesInHierarchy =
                  tilesetComponent.showTilesInHierarchy();
            }

            const UnityEngine::MeshDataArray& meshDataArray =
                workerResult.meshDataResult.meshDataArray;
            const std::vector<CesiumPrimitiveInfo>& primitiveInfos =
                workerResult.meshDataResult.primitiveInfos;

            // Create meshes and populate them from the MeshData created in
            // the worker thread. Sadly, this must be done in the main
            // thread, too.
            System::Array1<UnityEngine::Mesh> meshes(meshDataArray.Length());
            for (int32_t i = 0, len = meshes.Length(); i < len; ++i) {
              UnityEngine::Mesh unityMesh =
                  CesiumForUnity::CesiumObjectPools::MeshPool().Get();
              // Don't let Unity unload this mesh during the time in between
              // when we create it and when we attach it to a GameObject.
              if (shouldShowTilesInHierarchy) {
                unityMesh.hideFlags(UnityEngine::HideFlags::HideAndDontSave);
              } else {
                unityMesh.hideFlags(
                    UnityEngine::HideFlags::HideAndDontSave |
                    UnityEngine::HideFlags::HideInHierarchy);
              }

              meshes.Item(i, unityMesh);
            }

            // TODO: Validate indices in the worker thread, and then ask Unity
            // not to do it here by setting
            // MeshUpdateFlags::DontValidateIndices.
            UnityEngine::Mesh::ApplyAndDisposeWritableMeshData(
                meshDataArray,
                meshes,
                UnityEngine::Rendering::MeshUpdateFlags::Default);

            // TODO: we should be able to do this in the worker thread, even if
            // we have to do it manually.
            for (int32_t i = 0, len = meshes.Length(); i < len; ++i) {
              meshes[i].RecalculateBounds();
            }

            if (shouldCreatePhysicsMeshes) {
              // Baking physics meshes takes awhile, so do that in a
              // worker thread.
              const std::uint64_t len = meshes.Length();
              std::vector<std::uint64_t> objectIds;
              for (uint64_t i = 0; i < len; ++i) {
                // Don't attempt to bake a physics mesh from points, lines, or
                // an invalid triangle mesh.
                switch (primitiveInfos[i].mode) {
                case CesiumGltf::MeshPrimitive::Mode::POINTS:
                case CesiumGltf::MeshPrimitive::Mode::LINES:
                  continue;
                default:
                  if (isDegenerateTriangleMesh(meshes[i])) {
                    continue;
                  }
                }

                objectIds.push_back(
                    CesiumForUnity::Helpers::GetObjectId(meshes[i]));
              }

              if (objectIds.size() > 0) {
#ifndef __EMSCRIPTEN__
                return asyncSystem.runInWorkerThread(
#else
                // Unity Wasm can only access managed code from the main thread.
                return asyncSystem.runInMainThread(
#endif
                    [workerResult = std::move(workerResult),
                     objectIds = std::move(objectIds),
                     meshes = std::move(meshes)]() mutable {
                      for (std::uint64_t objectID : objectIds) {
                        CesiumForUnity::Helpers::BakeMeshFromId(objectID);
                      }

                      LoadThreadResult* pResult = new LoadThreadResult{
                          std::move(meshes),
                          std::move(workerResult.meshDataResult.splatDataArray),
                          std::move(
                              workerResult.meshDataResult.primitiveInfos)};
                      return TileLoadResultAndRenderResources{
                          std::move(workerResult.tileLoadResult),
                          pResult};
                    });
              }
            }

            LoadThreadResult* pResult = new LoadThreadResult{
                std::move(meshes),
                std::move(workerResult.meshDataResult.splatDataArray),
                std::move(workerResult.meshDataResult.primitiveInfos)};
            return asyncSystem.createResolvedFuture(
                TileLoadResultAndRenderResources{
                    std::move(workerResult.tileLoadResult),
                    pResult});
          });
}

namespace {
UnityEngine::Vector4
gltfVectorToUnityVector(const std::vector<double>& values, float defaultValue) {
  UnityEngine::Vector4 result{
      defaultValue,
      defaultValue,
      defaultValue,
      defaultValue};

  if (values.size() > 0) {
    result.x = static_cast<float>(values[0]);
  }

  if (values.size() > 1) {
    result.y = static_cast<float>(values[1]);
  }

  if (values.size() > 2) {
    result.z = static_cast<float>(values[2]);
  }

  if (values.size() > 3) {
    result.w = static_cast<float>(values[3]);
  }

  return result;
}

void setGltfMaterialParameterValues(
    const CesiumGltf::Model& model,
    const CesiumPrimitiveInfo& primitiveInfo,
    const CesiumGltf::Material& gltfMaterial,
    const UnityEngine::Material& unityMaterial,
    const TilesetMaterialProperties& materialProperties) {
  CESIUM_TRACE("Cesium::CreateMaterials");

  // These similar-sounding material properties are used in various render
  // pipelines (built-in, URP, HDRP). Rather than try to figure out which
  // applies, we just set them all.
  if (gltfMaterial.doubleSided) {
    unityMaterial.SetFloat(materialProperties.getDoubleSidedEnableID(), 1.0f);
    unityMaterial.SetFloat(
        materialProperties.getCullID(),
        float(UnityEngine::Rendering::CullMode::Off));
    unityMaterial.SetFloat(
        materialProperties.getCullModeID(),
        float(UnityEngine::Rendering::CullMode::Off));
    unityMaterial.SetFloat(
        materialProperties.getBuiltInCullModeID(),
        float(UnityEngine::Rendering::CullMode::Off));
  } else {
    unityMaterial.SetFloat(materialProperties.getDoubleSidedEnableID(), 0.0f);
    unityMaterial.SetFloat(
        materialProperties.getCullID(),
        float(UnityEngine::Rendering::CullMode::Back));
    unityMaterial.SetFloat(
        materialProperties.getCullModeID(),
        float(UnityEngine::Rendering::CullMode::Back));
    unityMaterial.SetFloat(
        materialProperties.getBuiltInCullModeID(),
        float(UnityEngine::Rendering::CullMode::Back));
  }

  const CesiumGltf::MaterialPBRMetallicRoughness& pbr =
      gltfMaterial.pbrMetallicRoughness
          ? gltfMaterial.pbrMetallicRoughness.value()
          : defaultPbrMetallicRoughness;

  // Add base color factor and metallic-roughness factor regardless
  // of whether the textures are present.
  const std::vector<double>& baseColorFactor = pbr.baseColorFactor;
  unityMaterial.SetVector(
      materialProperties.getBaseColorFactorID(),
      gltfVectorToUnityVector(baseColorFactor, 1.0f));

  UnityEngine::Vector4 metallicRoughnessFactor;
  unityMaterial.SetVector(
      materialProperties.getMetallicRoughnessFactorID(),
      {(float)pbr.metallicFactor, (float)pbr.roughnessFactor, 0, 0});

  const std::optional<TextureInfo>& baseColorTexture = pbr.baseColorTexture;
  if (baseColorTexture) {
    auto texCoordIndexIt =
        primitiveInfo.uvIndexMap.find(baseColorTexture->texCoord);
    if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
      UnityEngine::Texture texture =
          TextureLoader::loadTexture(model, baseColorTexture->index, true);
      if (texture != nullptr) {
        texture.hideFlags(DotNet::UnityEngine::HideFlags::HideAndDontSave);
        unityMaterial.SetTexture(
            materialProperties.getBaseColorTextureID(),
            texture);
        unityMaterial.SetFloat(
            materialProperties.getBaseColorTextureCoordinateIndexID(),
            static_cast<float>(texCoordIndexIt->second));
      }
    }
  }

  const std::optional<TextureInfo>& metallicRoughness =
      pbr.metallicRoughnessTexture;
  if (metallicRoughness) {
    auto texCoordIndexIt =
        primitiveInfo.uvIndexMap.find(metallicRoughness->texCoord);
    if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
      UnityEngine::Texture texture =
          TextureLoader::loadTexture(model, metallicRoughness->index, false);
      if (texture != nullptr) {
        texture.hideFlags(DotNet::UnityEngine::HideFlags::HideAndDontSave);
        unityMaterial.SetTexture(
            materialProperties.getMetallicRoughnessTextureID(),
            texture);
        unityMaterial.SetFloat(
            materialProperties.getMetallicRoughnessTextureCoordinateIndexID(),
            static_cast<float>(texCoordIndexIt->second));
      }
    }
  }

  const std::vector<double>& emissiveFactor = gltfMaterial.emissiveFactor;
  unityMaterial.SetVector(
      materialProperties.getEmissiveFactorID(),
      gltfVectorToUnityVector(emissiveFactor, 0.0f));
  if (gltfMaterial.emissiveTexture) {
    auto texCoordIndexIt =
        primitiveInfo.uvIndexMap.find(gltfMaterial.emissiveTexture->texCoord);
    if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
      UnityEngine::Texture texture = TextureLoader::loadTexture(
          model,
          gltfMaterial.emissiveTexture->index,
          true);
      if (texture != nullptr) {
        texture.hideFlags(DotNet::UnityEngine::HideFlags::HideAndDontSave);
        unityMaterial.SetTexture(
            materialProperties.getEmissiveTextureID(),
            texture);
        unityMaterial.SetFloat(
            materialProperties.getEmissiveTextureCoordinateIndexID(),
            static_cast<float>(texCoordIndexIt->second));
      }
    }
  }

  if (gltfMaterial.normalTexture) {
    auto texCoordIndexIt =
        primitiveInfo.uvIndexMap.find(gltfMaterial.normalTexture->texCoord);
    if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
      UnityEngine::Texture texture = TextureLoader::loadTexture(
          model,
          gltfMaterial.normalTexture->index,
          false);
      if (texture != nullptr) {
        texture.hideFlags(DotNet::UnityEngine::HideFlags::HideAndDontSave);
        unityMaterial.SetTexture(
            materialProperties.getNormalMapTextureID(),
            texture);
        unityMaterial.SetFloat(
            materialProperties.getNormalMapTextureCoordinateIndexID(),
            static_cast<float>(texCoordIndexIt->second));
        unityMaterial.SetFloat(
            materialProperties.getNormalMapScaleID(),
            static_cast<float>(gltfMaterial.normalTexture->scale));
      }
    }
  }

  if (gltfMaterial.occlusionTexture) {
    auto texCoordIndexIt =
        primitiveInfo.uvIndexMap.find(gltfMaterial.occlusionTexture->texCoord);
    if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
      UnityEngine::Texture texture = TextureLoader::loadTexture(
          model,
          gltfMaterial.occlusionTexture->index,
          false);
      if (texture != nullptr) {
        texture.hideFlags(DotNet::UnityEngine::HideFlags::HideAndDontSave);
        unityMaterial.SetTexture(
            materialProperties.getOcclusionTextureID(),
            texture);
        unityMaterial.SetFloat(
            materialProperties.getOcclusionTextureCoordinateIndexID(),
            static_cast<float>(texCoordIndexIt->second));
        unityMaterial.SetFloat(
            materialProperties.getOcclusionStrengthID(),
            static_cast<float>(gltfMaterial.occlusionTexture->strength));
      }
    }
  }

  // Handle KHR_texture_transform for each available texture.
  KhrTextureTransform textureTransform;

  const ExtensionKhrTextureTransform* pBaseColorTextureTransform =
      pbr.baseColorTexture
          ? pbr.baseColorTexture->getExtension<ExtensionKhrTextureTransform>()
          : nullptr;
  if (pBaseColorTextureTransform) {
    textureTransform = KhrTextureTransform(*pBaseColorTextureTransform);
    if (textureTransform.status() == KhrTextureTransformStatus::Valid) {
      const glm::dvec2& scale = textureTransform.scale();
      const glm::dvec2& offset = textureTransform.offset();
      unityMaterial.SetTextureOffset(
          materialProperties.getBaseColorTextureID(),
          {static_cast<float>(offset[0]), static_cast<float>(offset[1])});
      unityMaterial.SetTextureScale(
          materialProperties.getBaseColorTextureID(),
          {static_cast<float>(scale[0]), static_cast<float>(scale[1])});

      const glm::dvec2& rotationSineCosine =
          textureTransform.rotationSineCosine();
      unityMaterial.SetVector(
          materialProperties.getBaseColorTextureRotationID(),
          {static_cast<float>(rotationSineCosine[0]),
           static_cast<float>(rotationSineCosine[1]),
           0.0f,
           0.0f});
    }
  }

  const ExtensionKhrTextureTransform* pMetallicRoughnessTextureTransform =
      pbr.metallicRoughnessTexture
          ? pbr.metallicRoughnessTexture
                ->getExtension<ExtensionKhrTextureTransform>()
          : nullptr;
  if (pMetallicRoughnessTextureTransform) {
    textureTransform = KhrTextureTransform(*pMetallicRoughnessTextureTransform);
    if (textureTransform.status() == KhrTextureTransformStatus::Valid) {
      const glm::dvec2& scale = textureTransform.scale();
      const glm::dvec2& offset = textureTransform.offset();
      unityMaterial.SetTextureOffset(
          materialProperties.getMetallicRoughnessTextureID(),
          {static_cast<float>(offset[0]), static_cast<float>(offset[1])});
      unityMaterial.SetTextureScale(
          materialProperties.getMetallicRoughnessTextureID(),
          {static_cast<float>(scale[0]), static_cast<float>(scale[1])});

      const glm::dvec2& rotationSineCosine =
          textureTransform.rotationSineCosine();
      unityMaterial.SetVector(
          materialProperties.getMetallicRoughnessTextureRotationID(),
          {static_cast<float>(rotationSineCosine[0]),
           static_cast<float>(rotationSineCosine[1]),
           0.0f,
           0.0f});
    }
  }

  const ExtensionKhrTextureTransform* pNormalTextureTransform =
      gltfMaterial.normalTexture
          ? gltfMaterial.normalTexture
                ->getExtension<ExtensionKhrTextureTransform>()
          : nullptr;
  if (pNormalTextureTransform) {
    textureTransform = KhrTextureTransform(*pNormalTextureTransform);
    if (textureTransform.status() == KhrTextureTransformStatus::Valid) {
      const glm::dvec2& scale = textureTransform.scale();
      const glm::dvec2& offset = textureTransform.offset();

      unityMaterial.SetTextureOffset(
          materialProperties.getNormalMapTextureID(),
          {static_cast<float>(offset[0]), static_cast<float>(offset[1])});
      unityMaterial.SetTextureScale(
          materialProperties.getNormalMapTextureID(),
          {static_cast<float>(scale[0]), static_cast<float>(scale[1])});

      const glm::dvec2& rotationSineCosine =
          textureTransform.rotationSineCosine();
      unityMaterial.SetVector(
          materialProperties.getNormalMapTextureRotationID(),
          {static_cast<float>(rotationSineCosine[0]),
           static_cast<float>(rotationSineCosine[1]),
           0.0f,
           0.0f});
    }
  }

  const ExtensionKhrTextureTransform* pEmissiveTextureTransform =
      gltfMaterial.emissiveTexture
          ? gltfMaterial.emissiveTexture
                ->getExtension<ExtensionKhrTextureTransform>()
          : nullptr;
  if (pEmissiveTextureTransform) {
    textureTransform = KhrTextureTransform(*pEmissiveTextureTransform);
    if (textureTransform.status() == KhrTextureTransformStatus::Valid) {
      const glm::dvec2& scale = textureTransform.scale();
      const glm::dvec2& offset = textureTransform.offset();

      unityMaterial.SetTextureOffset(
          materialProperties.getEmissiveTextureID(),
          {static_cast<float>(offset[0]), static_cast<float>(offset[1])});
      unityMaterial.SetTextureScale(
          materialProperties.getEmissiveTextureID(),
          {static_cast<float>(scale[0]), static_cast<float>(scale[1])});

      const glm::dvec2& rotationSineCosine =
          textureTransform.rotationSineCosine();
      unityMaterial.SetVector(
          materialProperties.getEmissiveTextureRotationID(),
          {static_cast<float>(rotationSineCosine[0]),
           static_cast<float>(rotationSineCosine[1]),
           0.0f,
           0.0f});
    }
  }

  const ExtensionKhrTextureTransform* pOcclusionTextureTransform =
      gltfMaterial.occlusionTexture
          ? gltfMaterial.occlusionTexture
                ->getExtension<ExtensionKhrTextureTransform>()
          : nullptr;
  if (pOcclusionTextureTransform) {
    textureTransform = KhrTextureTransform(*pOcclusionTextureTransform);
    if (textureTransform.status() == KhrTextureTransformStatus::Valid) {
      const glm::dvec2& scale = textureTransform.scale();
      const glm::dvec2& offset = textureTransform.offset();

      unityMaterial.SetTextureOffset(
          materialProperties.getOcclusionTextureID(),
          {static_cast<float>(offset[0]), static_cast<float>(offset[1])});
      unityMaterial.SetTextureScale(
          materialProperties.getOcclusionTextureID(),
          {static_cast<float>(scale[0]), static_cast<float>(scale[1])});

      const glm::dvec2& rotationSineCosine =
          textureTransform.rotationSineCosine();
      unityMaterial.SetVector(
          materialProperties.getOcclusionTextureRotationID(),
          {static_cast<float>(rotationSineCosine[0]),
           static_cast<float>(rotationSineCosine[1]),
           0.0f,
           0.0f});
    }
  }
}
} // namespace

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult_) {
  std::unique_ptr<LoadThreadResult> pLoadThreadResult(
      static_cast<LoadThreadResult*>(pLoadThreadResult_));

  const System::Array1<UnityEngine::Mesh>& meshes = pLoadThreadResult->meshes;
  const System::Array1<DotNet::Gsplat::GsplatAssetUncompressed>& splatAssets =
      pLoadThreadResult->splatAssets;
  const std::vector<CesiumPrimitiveInfo>& primitiveInfos =
      pLoadThreadResult->primitiveInfos;

  const Cesium3DTilesSelection::TileContent& content = tile.getContent();
  const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
      content.getRenderContent();
  if (!pRenderContent) {
    return nullptr;
  }

  CESIUM_TRACE("Cesium::LoadModel");
  const Model& model = pRenderContent->getModel();

  std::string name = "glTF";
  auto urlIt = model.extras.find("Cesium3DTiles_TileUrl");
  if (urlIt != model.extras.end()) {
    name = urlIt->second.getStringOrDefault("glTF");
  }

  DotNet::CesiumForUnity::Cesium3DTileset tilesetComponent =
      this->_tilesetGameObject
          .GetComponent<DotNet::CesiumForUnity::Cesium3DTileset>();

  uint32_t currentOverlayCount =
      static_cast<uint32_t>(tilesetComponent.NativeImplementation()
                                .getTileset()
                                ->getOverlays()
                                .size());

  auto pModelGameObject =
      std::make_unique<UnityEngine::GameObject>(System::String(name));

  const bool showTilesInHierarchy = tilesetComponent.showTilesInHierarchy();

  if (showTilesInHierarchy) {
    pModelGameObject->hideFlags(UnityEngine::HideFlags::DontSave);
  } else {
    pModelGameObject->hideFlags(
        UnityEngine::HideFlags::DontSave |
        UnityEngine::HideFlags::HideInHierarchy);
  }

  pModelGameObject->transform().SetParent(
      this->_tilesetGameObject.transform(),
      false);
  pModelGameObject->layer(this->_tilesetGameObject.layer());
  pModelGameObject->SetActive(false);

  glm::dmat4 tileTransform = tile.getTransform();
  tileTransform = GltfUtilities::applyRtcCenter(model, tileTransform);
  tileTransform = GltfUtilities::applyGltfUpAxisTransform(model, tileTransform);

  DotNet::CesiumForUnity::CesiumGeoreference georeferenceComponent =
      this->_tilesetGameObject
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();

  const LocalHorizontalCoordinateSystem* pCoordinateSystem = nullptr;
  if (georeferenceComponent != nullptr) {
    pCoordinateSystem =
        &georeferenceComponent.NativeImplementation().getCoordinateSystem(
            georeferenceComponent);
  }

  const bool createPhysicsMeshes = tilesetComponent.createPhysicsMeshes();

  int32_t meshIndex = 0;
  int32_t primitiveIndex = 0;

  // For backwards compatibility.
  CesiumForUnity::CesiumMetadata metadataComponent =
      pModelGameObject
          ->GetComponentInParent<DotNet::CesiumForUnity::CesiumMetadata>();

  if (metadataComponent == nullptr) {
    // Only add the model metadata component here if the older component isn't
    // attached.
    auto pModelMetadata =
        model.getExtension<ExtensionModelExtStructuralMetadata>();
    if (pModelMetadata) {
      CesiumFeaturesMetadataUtility::addModelMetadata(
          *pModelGameObject,
          model,
          *pModelMetadata);
    }
  }

  model.forEachPrimitiveInScene(
      -1,
      [&meshes,
       &splatAssets,
       &primitiveInfos,
       &pModelGameObject,
       &tileTransform,
       &meshIndex,
       &primitiveIndex,
       &tilesetComponent,
       pCoordinateSystem,
       createPhysicsMeshes,
       showTilesInHierarchy,
       &metadataComponent,
       &tile,
       &materialProperties = this->_materialProperties,
       tilesetLayer = this->_tilesetGameObject.layer()](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        const CesiumPrimitiveInfo& primitiveInfo =
            primitiveInfos[primitiveIndex++];

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

        int64_t primitiveIndex = &primitive - &mesh.primitives[0];
        UnityEngine::GameObject primitiveGameObject(
            System::String(
                "Mesh " + std::to_string(meshIndex - 1) + " Primitive " +
                std::to_string(primitiveIndex)));
        if (showTilesInHierarchy) {
          primitiveGameObject.hideFlags(UnityEngine::HideFlags::DontSave);
        } else {
          primitiveGameObject.hideFlags(
              UnityEngine::HideFlags::DontSave |
              UnityEngine::HideFlags::HideInHierarchy);
        }

        primitiveGameObject.transform().parent(pModelGameObject->transform());
        primitiveGameObject.layer(tilesetLayer);
        glm::dmat4 modelToEcef = tileTransform * transform;

        CesiumForUnity::CesiumGlobeAnchor anchor =
            primitiveGameObject
                .AddComponent<CesiumForUnity::CesiumGlobeAnchor>();
        anchor.detectTransformChanges(false);
        anchor.adjustOrientationForGlobeWhenMoving(false);
        anchor.localToGlobeFixedMatrix(
            UnityTransforms::toUnityMathematics(modelToEcef));

        if (primitiveInfo.gsplatIndex != -1) {
          DotNet::Gsplat::GsplatRenderer renderer =
              primitiveGameObject
                  .AddComponent<DotNet::Gsplat::GsplatRenderer>();
          renderer.GsplatAsset((
              DotNet::Gsplat::
                  GsplatAsset)(splatAssets[primitiveInfo.gsplatIndex]));
          return;
        }

        UnityEngine::Mesh unityMesh = meshes[meshIndex++];
        if (unityMesh == nullptr) {
          // This indicates Unity destroyed the mesh already, which really
          // shouldn't happen.
          return;
        }
        UnityEngine::MeshFilter meshFilter =
            primitiveGameObject.AddComponent<UnityEngine::MeshFilter>();
        meshFilter.sharedMesh(unityMesh);

        UnityEngine::MeshRenderer meshRenderer =
            primitiveGameObject.AddComponent<UnityEngine::MeshRenderer>();

        const Material* pMaterial =
            Model::getSafe(&gltf.materials, primitive.material);

        UnityEngine::Material opaqueMaterial =
            tilesetComponent.opaqueMaterial();

        if (opaqueMaterial == nullptr) {
          if (primitiveInfo.isUnlit) {
            opaqueMaterial =
                UnityEngine::Resources::Load<UnityEngine::Material>(
                    System::String("CesiumUnlitTilesetMaterial"));
          } else {
            opaqueMaterial =
                UnityEngine::Resources::Load<UnityEngine::Material>(
                    System::String("CesiumDefaultTilesetMaterial"));
          }
        }

        UnityEngine::Material material =
            UnityEngine::Object::Instantiate(opaqueMaterial);
        material.hideFlags(UnityEngine::HideFlags::HideAndDontSave);
        meshRenderer.material(material);
        if (pMaterial) {
          setGltfMaterialParameterValues(
              gltf,
              primitiveInfo,
              *pMaterial,
              material,
              materialProperties);
        }

        if (primitiveInfo.mode == CesiumGltf::MeshPrimitive::Mode::POINTS) {
          CesiumForUnity::CesiumPointCloudRenderer pointCloudRenderer =
              primitiveGameObject
                  .AddComponent<CesiumForUnity::CesiumPointCloudRenderer>();

          CesiumForUnity::Cesium3DTileInfo tileInfo;
          tileInfo.usesAdditiveRefinement =
              tile.getRefine() == Cesium3DTilesSelection::TileRefine::Add;
          tileInfo.geometricError =
              static_cast<float>(tile.getGeometricError());

          // TODO: can we make AccessorView retrieve the min/max for us?
          const Accessor* pPositionAccessor =
              Model::getSafe(&gltf.accessors, positionAccessorID);
          glm::vec3 min(
              pPositionAccessor->min[0],
              pPositionAccessor->min[1],
              pPositionAccessor->min[2]);
          glm::vec3 max(
              pPositionAccessor->max[0],
              pPositionAccessor->max[1],
              pPositionAccessor->max[2]);
          glm::vec3 dimensions(transform * glm::dvec4(max - min, 0));

          tileInfo.dimensions =
              UnityEngine::Vector3{dimensions.x, dimensions.y, dimensions.z};
          tileInfo.isTranslucent = primitiveInfo.isTranslucent;
          pointCloudRenderer.tileInfo(tileInfo);
        }

        if (createPhysicsMeshes) {
          switch (primitiveInfo.mode) {
          case CesiumGltf::MeshPrimitive::Mode::POINTS:
          case CesiumGltf::MeshPrimitive::Mode::LINES:
            break;
          default:
            if (!isDegenerateTriangleMesh(unityMesh)) {
              // This should not trigger mesh baking for physics, because the
              // meshes were already baked in the worker thread.
              UnityEngine::MeshCollider meshCollider =
                  primitiveGameObject.AddComponent<UnityEngine::MeshCollider>();
              meshCollider.sharedMesh(unityMesh);
            }
            break;
          }
        }

        // For backwards compatibility.
        if (metadataComponent != nullptr) {
          metadataComponent.NativeImplementation().addMetadata(
              CesiumForUnity::Helpers::GetObjectId(
                  primitiveGameObject.transform()),
              &gltf,
              &primitive);
        } else {
          const ExtensionExtMeshFeatures* pFeatures =
              primitive.getExtension<ExtensionExtMeshFeatures>();
          if (pFeatures) {
            CesiumFeaturesMetadataUtility::addPrimitiveFeatures(
                primitiveGameObject,
                gltf,
                primitive,
                *pFeatures);
          }
        }
      });

  tilesetComponent.BroadcastNewGameObjectCreated(*pModelGameObject);

  CesiumGltfGameObject* pCesiumGameObject = new CesiumGltfGameObject{
      std::move(pModelGameObject),
      std::move(pLoadThreadResult->primitiveInfos)};

  return pCesiumGameObject;
}

namespace {

void freePrimitiveFeatures(
    const DotNet::UnityEngine::GameObject& primitiveGameObject) {
  DotNet::CesiumForUnity::CesiumPrimitiveFeatures features =
      primitiveGameObject
          .GetComponent<DotNet::CesiumForUnity::CesiumPrimitiveFeatures>();
  if (features == nullptr) {
    return;
  }

  auto featureIdSets = features.featureIdSets();
  if (featureIdSets.Length() == 0) {
    return;
  }

  for (int32_t i = 0; i < featureIdSets.Length(); i++) {
    featureIdSets[i].Dispose();
  }
}

void freePrimitiveGameObject(
    const DotNet::UnityEngine::GameObject& primitiveGameObject,
    const DotNet::CesiumForUnity::CesiumMetadata& metadataComponent) {
  // Kept for backwards compatibility.
  if (metadataComponent != nullptr) {
    metadataComponent.NativeImplementation().removeMetadata(
        CesiumForUnity::Helpers::GetObjectId(primitiveGameObject.transform()));
  } else {
    freePrimitiveFeatures(primitiveGameObject);
  }

  UnityEngine::MeshRenderer meshRenderer =
      primitiveGameObject.GetComponent<UnityEngine::MeshRenderer>();
  if (meshRenderer != nullptr) {
    UnityEngine::Material material = meshRenderer.sharedMaterial();

    System::Collections::Generic::List1<int> textureIDs;
    material.GetTexturePropertyNameIDs(textureIDs);
    for (int32_t i = 0, len = textureIDs.Count(); i < len; ++i) {
      int32_t textureID = textureIDs[i];
      UnityEngine::Texture texture = material.GetTexture(textureID);
      if (texture != nullptr &&
          (texture.hideFlags() & UnityEngine::HideFlags::HideAndDontSave) ==
              UnityEngine::HideFlags::HideAndDontSave) {
        UnityLifetime::Destroy(texture);
      }
    }

    UnityLifetime::Destroy(material);
  }

  UnityEngine::MeshFilter meshFilter =
      primitiveGameObject.GetComponent<UnityEngine::MeshFilter>();
  if (meshFilter != nullptr) {
    CesiumForUnity::CesiumObjectPools::MeshPool().Release(
        meshFilter.sharedMesh());
  }

  // The MeshCollider shares a mesh with the MeshFilter, so no need to
  // destroy it explicitly.
}

void freeModelMetadata(const DotNet::UnityEngine::GameObject& modelGameObject) {
  auto modelMetadata =
      modelGameObject
          .GetComponent<DotNet::CesiumForUnity::CesiumModelMetadata>();
  if (modelMetadata == nullptr) {
    return;
  }

  auto propertyTables = modelMetadata.propertyTables();
  if (propertyTables.Length() == 0) {
    return;
  }

  for (int32_t i = 0; i < propertyTables.Length(); i++) {
    propertyTables[i].DisposeProperties();
  }
}

} // namespace

void UnityPrepareRendererResources::free(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  try {
    if (pLoadThreadResult) {
      LoadThreadResult* pTyped =
          static_cast<LoadThreadResult*>(pLoadThreadResult);
      for (int32_t i = 0, len = pTyped->meshes.Length(); i < len; ++i) {
        CesiumForUnity::CesiumObjectPools::MeshPool().Release(
            pTyped->meshes[i]);
      }
      delete pTyped;
    }

    if (pMainThreadResult) {
      std::unique_ptr<CesiumGltfGameObject> pCesiumGameObject(
          static_cast<CesiumGltfGameObject*>(pMainThreadResult));

      // It's possible that the game object has already been destroyed. In which
      // case Unity will throw a MissingReferenceException if we try to use it.
      // So don't do that.
      if (*pCesiumGameObject->pGameObject != nullptr) {
        auto metadataComponent =
            pCesiumGameObject->pGameObject->GetComponentInParent<
                DotNet::CesiumForUnity::CesiumMetadata>();

        UnityEngine::Transform parentTransform =
            pCesiumGameObject->pGameObject->transform();

        // Destroying primitives will remove them from the child list, so
        // work backwards.
        for (int32_t i = parentTransform.childCount() - 1; i >= 0; --i) {
          UnityEngine::GameObject primitiveGameObject =
              parentTransform.GetChild(i).gameObject();
          freePrimitiveGameObject(primitiveGameObject, metadataComponent);
          UnityLifetime::Destroy(primitiveGameObject);
        }

        if (metadataComponent == nullptr) {
          freeModelMetadata(*pCesiumGameObject->pGameObject);
        }

        UnityLifetime::Destroy(*pCesiumGameObject->pGameObject);
      }
    }
  } catch (...) {
    // This function is a hotspot for crashes caused by AppDomain reloads.
    UnityEngine::Debug::Log(
        System::String(
            "A tile was not cleaned up properly, probably due to an "
            "AppDomain reload."));
  }
}

void* UnityPrepareRendererResources::prepareRasterInLoadThread(
    CesiumImage::ImageAsset& image,
    const std::any& rendererOptions) {
  CesiumImage::ImageDecoder::generateMipMaps(image);
  return nullptr;
}

void* UnityPrepareRendererResources::prepareRasterInMainThread(
    CesiumRasterOverlays::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult) {
  auto pTexture = std::make_unique<UnityEngine::Texture>(
      TextureLoader::loadTexture(*rasterTile.getImage(), true));
  pTexture->wrapMode(UnityEngine::TextureWrapMode::Clamp);
  pTexture->filterMode(UnityEngine::FilterMode::Trilinear);
  pTexture->anisoLevel(16);
  return pTexture.release();
}

void UnityPrepareRendererResources::freeRaster(
    const CesiumRasterOverlays::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  if (pMainThreadResult) {
    std::unique_ptr<UnityEngine::Texture> pTexture(
        static_cast<UnityEngine::Texture*>(pMainThreadResult));
    if (*pTexture != nullptr) {
      UnityLifetime::Destroy(*pTexture);
    }
  }
}

void UnityPrepareRendererResources::attachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const CesiumRasterOverlays::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources,
    const glm::dvec2& translation,
    const glm::dvec2& scale) {
  const Cesium3DTilesSelection::TileContent& content = tile.getContent();
  const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
      content.getRenderContent();
  if (!pRenderContent) {
    return;
  }

  CesiumGltfGameObject* pCesiumGameObject =
      static_cast<CesiumGltfGameObject*>(pRenderContent->getRenderResources());
  UnityEngine::Texture* pTexture =
      static_cast<UnityEngine::Texture*>(pMainThreadRendererResources);
  if (!pCesiumGameObject || !pCesiumGameObject->pGameObject || !pTexture)
    return;

  std::string key = rasterTile.getOverlay().getName();

  // We're assuming here that the order of primitives in the transform chain
  // is the same as the order in the `primitiveInfos`, which should
  // always be true.
  uint32_t primitiveIndex = 0;

  UnityEngine::Transform transform =
      pCesiumGameObject->pGameObject->transform();
  for (int32_t i = 0, len = transform.childCount(); i < len; ++i) {
    UnityEngine::Transform childTransform = transform.GetChild(i);
    if (childTransform == nullptr)
      continue;

    UnityEngine::GameObject child = childTransform.gameObject();
    if (child == nullptr)
      continue;

    UnityEngine::MeshRenderer meshRenderer =
        child.GetComponent<UnityEngine::MeshRenderer>();
    if (meshRenderer == nullptr)
      continue;

    UnityEngine::Material material = meshRenderer.sharedMaterial();
    if (material == nullptr)
      continue;

    const CesiumPrimitiveInfo& primitiveInfo =
        pCesiumGameObject->primitiveInfos[primitiveIndex++];

    // Note: The overlay texture coordinate index corresponds to the glTF
    // attribute _CESIUMOVERLAY_<i>. Here we retrieve the Unity texture
    // coordinate index corresponding to the glTF texture coordinate index for
    // this primitive.
    auto texCoordIndexIt =
        primitiveInfo.rasterOverlayUvIndexMap.find(overlayTextureCoordinateID);
    if (texCoordIndexIt == primitiveInfo.rasterOverlayUvIndexMap.end()) {
      // The associated UV coords for this overlay are missing.
      // TODO: log warning?
      continue;
    }

    // Note: The overlay index is NOT the same as the overlay texture coordinate
    // index. For instance, multiple overlays could point to the same overlay UV
    // index - multiple overlays can use the _CESIUMOVERLAY_0 attribute for
    // example. The _CESIUMOVERLAY_<i> attributes correspond to unique
    // _projections_, not unique overlays.
    auto maybeID =
        this->_materialProperties.getOverlayTextureCoordinateIndexID(key);
    if (maybeID) {
      material.SetFloat(*maybeID, static_cast<float>(texCoordIndexIt->second));
    }

    maybeID = this->_materialProperties.getOverlayTextureID(key);
    if (maybeID) {
      material.SetTexture(*maybeID, *pTexture);
    }

    UnityEngine::Vector4 translationAndScale{
        float(translation.x),
        float(translation.y),
        float(scale.x),
        float(scale.y)};

    maybeID = this->_materialProperties.getOverlayTranslationAndScaleID(key);
    if (maybeID) {
      material.SetVector(*maybeID, translationAndScale);
    }
  }
}

void UnityPrepareRendererResources::detachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const CesiumRasterOverlays::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources) noexcept {
  const Cesium3DTilesSelection::TileContent& content = tile.getContent();
  const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
      content.getRenderContent();
  if (!pRenderContent) {
    return;
  }

  CesiumGltfGameObject* pCesiumGameObject =
      static_cast<CesiumGltfGameObject*>(pRenderContent->getRenderResources());
  UnityEngine::Texture* pTexture =
      static_cast<UnityEngine::Texture*>(pMainThreadRendererResources);
  if (pCesiumGameObject == nullptr ||
      pCesiumGameObject->pGameObject == nullptr ||
      *pCesiumGameObject->pGameObject == nullptr || pTexture == nullptr ||
      *pTexture == nullptr)
    return;

  UnityEngine::Transform transform =
      pCesiumGameObject->pGameObject->transform();
  for (int32_t i = 0, len = transform.childCount(); i < len; ++i) {
    UnityEngine::Transform childTransform = transform.GetChild(i);
    if (childTransform == nullptr)
      continue;

    UnityEngine::GameObject child = childTransform.gameObject();
    if (child == nullptr)
      continue;

    UnityEngine::MeshRenderer meshRenderer =
        child.GetComponent<UnityEngine::MeshRenderer>();
    if (meshRenderer == nullptr)
      continue;

    UnityEngine::Material material = meshRenderer.sharedMaterial();
    if (material == nullptr)
      continue;

    auto maybeID = this->_materialProperties.getOverlayTextureID(
        rasterTile.getOverlay().getName());
    if (maybeID) {
      material.SetTexture(*maybeID, UnityEngine::Texture(nullptr));
    }
  }
}
