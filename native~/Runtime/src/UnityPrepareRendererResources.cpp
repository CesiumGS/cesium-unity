#include "UnityPrepareRendererResources.h"

#include "TextureLoader.h"
#include "UnityLifetime.h"
#include "UnityTransforms.h"

#include <Cesium3DTilesSelection/GltfUtilities.h>
#include <Cesium3DTilesSelection/Tile.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeometry/Transforms.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionKhrMaterialsUnlit.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>
#include <CesiumGltf/ExtensionModelExtFeatureMetadata.h>
#include <CesiumShaderProperties.h>
#include <CesiumUtility/ScopeGuard.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/CesiumForUnity/CesiumMetadata.h>
#include <DotNet/CesiumForUnity/CesiumObjectPool.h>
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
#include <DotNet/UnityEngine/Physics.h>
#include <DotNet/UnityEngine/Pool/ObjectPool1.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Rendering/IndexFormat.h>
#include <DotNet/UnityEngine/Rendering/MeshUpdateFlags.h>
#include <DotNet/UnityEngine/Rendering/SubMeshDescriptor.h>
#include <DotNet/UnityEngine/Rendering/VertexAttributeDescriptor.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/TextureWrapMode.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector2.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <DotNet/UnityEngine/Vector4.h>
#include <glm/gtc/matrix_inverse.hpp>
#include <glm/gtc/quaternion.hpp>

#include <algorithm>
#include <unordered_map>
#include <variant>

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnityNative;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace CesiumGltf;
using namespace CesiumUtility;
using namespace DotNet;

namespace {

template <typename TDest, typename TSource>
void setIndices(
    const Unity::Collections::NativeArray1<TDest>& dest,
    const AccessorView<TSource>& source) {
  assert(dest.Length() == source.size());

  TDest* indices = static_cast<TDest*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(dest));

  for (int64_t i = 0; i < source.size(); ++i) {
    indices[i] = source[i];
  }
}

template <typename T>
void generateIndices(
    const Unity::Collections::NativeArray1<T>& dest,
    const int32_t count) {
  assert(dest.Length() == count);

  T* indices = static_cast<T*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(dest));

  for (int64_t i = 0; i < count; ++i) {
    indices[i] = static_cast<T>(i);
  }
}

int32_t countPrimitives(const CesiumGltf::Model& model) {
  int32_t numberOfPrimitives = 0;
  model.forEachPrimitiveInScene(
      -1,
      [&numberOfPrimitives](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) { ++numberOfPrimitives; });
  return numberOfPrimitives;
}

/**
 * @brief The result after populating Unity mesh data with loaded glTF content.
 */
struct MeshDataResult {
  UnityEngine::MeshDataArray meshDataArray;
  std::vector<CesiumPrimitiveInfo> primitiveInfos;
};

struct CopyVertexColors {
  uint8_t* pWritePos;
  size_t stride;
  size_t vertexCount;

  struct Color32 {
    uint8_t r;
    uint8_t g;
    uint8_t b;
    uint8_t a;
  };

  bool operator()(AccessorView<nullptr_t>&& invalidView) { return false; }

  template <typename TColorView> bool operator()(TColorView&& colorView) {
    if (colorView.status() != AccessorViewStatus::Valid) {
      return false;
    }

    bool success = true;
    for (size_t i = 0; success && i < vertexCount; ++i) {
      if (i >= colorView.size()) {
        success = false;
      } else {
        Color32& packedColor = *reinterpret_cast<Color32*>(pWritePos);
        success = CopyVertexColors::convertColor(colorView[i], packedColor);
        pWritePos += stride;
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

void populateMeshDataArray(
    MeshDataResult& meshDataResult,
    const TileLoadResult& tileLoadResult) {
  const CesiumGltf::Model* pModel =
      std::get_if<CesiumGltf::Model>(&tileLoadResult.contentKind);
  if (!pModel)
    return;

  size_t meshDataInstance = 0;

  meshDataResult.primitiveInfos.reserve(countPrimitives(*pModel));

  pModel->forEachPrimitiveInScene(
      -1,
      [&meshDataResult, &meshDataInstance](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        UnityEngine::MeshData meshData =
            meshDataResult.meshDataArray[meshDataInstance++];
        CesiumPrimitiveInfo& primitiveInfo =
            meshDataResult.primitiveInfos.emplace_back();

        using namespace DotNet::UnityEngine;
        using namespace DotNet::UnityEngine::Rendering;
        using namespace DotNet::Unity::Collections;
        using namespace DotNet::Unity::Collections::LowLevel::Unsafe;

        // Max attribute count supported by Unity, see VertexAttribute.
        const int MAX_ATTRIBUTES = 14;
        VertexAttributeDescriptor descriptor[MAX_ATTRIBUTES];

        // Interleave all attributes into single stream.
        std::int32_t numberOfAttributes = 0;
        std::int32_t streamIndex = 0;

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

        assert(numberOfAttributes < MAX_ATTRIBUTES);
        descriptor[numberOfAttributes].attribute = VertexAttribute::Position;
        descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
        descriptor[numberOfAttributes].dimension = 3;
        descriptor[numberOfAttributes].stream = streamIndex;
        ++numberOfAttributes;

        // Add the NORMAL attribute, if it exists.
        auto normalAccessorIt = primitive.attributes.find("NORMAL");
        AccessorView<UnityEngine::Vector3> normalView =
            normalAccessorIt != primitive.attributes.end()
                ? AccessorView<UnityEngine::Vector3>(
                      gltf,
                      normalAccessorIt->second)
                : AccessorView<UnityEngine::Vector3>();

        if (normalView.status() == AccessorViewStatus::Valid &&
            normalView.size() >= positionView.size()) {
          assert(numberOfAttributes < MAX_ATTRIBUTES);
          descriptor[numberOfAttributes].attribute = VertexAttribute::Normal;
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
          descriptor[numberOfAttributes].dimension = 3;
          descriptor[numberOfAttributes].stream = streamIndex;
          ++numberOfAttributes;
        }

        // Add the COLOR_0 attribute, if it exists.
        auto colorAccessorIt = primitive.attributes.find("COLOR_0");
        bool hasVertexColors = colorAccessorIt != primitive.attributes.end() &&
                               validateVertexColors(
                                   gltf,
                                   colorAccessorIt->second,
                                   positionView.size());
        if (hasVertexColors) {
          assert(numberOfAttributes < MAX_ATTRIBUTES);

          // Unity expects the vertex colors to come as 4 normalized uint8s.
          descriptor[numberOfAttributes].attribute = VertexAttribute::Color;
          descriptor[numberOfAttributes].format = VertexAttributeFormat::UNorm8;
          descriptor[numberOfAttributes].dimension = 4;
          descriptor[numberOfAttributes].stream = streamIndex;
          ++numberOfAttributes;
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
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
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
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
          descriptor[numberOfAttributes].dimension = 2;
          descriptor[numberOfAttributes].stream = streamIndex;

          ++numTexCoords;
          ++numberOfAttributes;
        }

        System::Array1<VertexAttributeDescriptor> attributes(
            numberOfAttributes);
        for (int32_t i = 0; i < numberOfAttributes; ++i) {
          attributes.Item(i, descriptor[i]);
        }

        meshData.SetVertexBufferParams(positionView.size(), attributes);

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
        for (int64_t i = 0; i < positionView.size(); ++i) {
          *reinterpret_cast<Vector3*>(pWritePos) = positionView[i];
          pWritePos += sizeof(Vector3);

          if (normalView.status() == AccessorViewStatus::Valid) {
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

        // Fill in vertex colors separately, if they exist.
        if (hasVertexColors) {
          // Color comes after position and normal.
          size_t colorByteOffset = sizeof(Vector3);
          if (normalView.status() == AccessorViewStatus::Valid) {
            colorByteOffset += sizeof(Vector3);
          }

          // Stride includes position, normal, ...
          size_t stride = colorByteOffset;
          // color, ...
          stride += sizeof(uint32_t);
          // and tex coords.
          stride += numTexCoords * sizeof(Vector2);

          createAccessorView(
              gltf,
              colorAccessorIt->second,
              CopyVertexColors{
                  pBufferStart + colorByteOffset,
                  stride,
                  static_cast<size_t>(positionView.size())});
        }

        int32_t indexCount = 0;

        if (primitive.indices >= 0) {
          AccessorView<uint8_t> indices8(gltf, primitive.indices);
          if (indices8.status() == AccessorViewStatus::Valid) {
            indexCount = indices8.size();
            meshData.SetIndexBufferParams(indexCount, IndexFormat::UInt16);
            setIndices(meshData.GetIndexData<std::uint16_t>(), indices8);
          }

          AccessorView<uint16_t> indices16(gltf, primitive.indices);
          if (indices16.status() == AccessorViewStatus::Valid) {
            indexCount = indices16.size();
            meshData.SetIndexBufferParams(indexCount, IndexFormat::UInt16);
            setIndices(meshData.GetIndexData<std::uint16_t>(), indices16);
          }

          AccessorView<uint32_t> indices32(gltf, primitive.indices);
          if (indices32.status() == AccessorViewStatus::Valid) {
            indexCount = indices32.size();
            meshData.SetIndexBufferParams(indexCount, IndexFormat::UInt32);
            setIndices(meshData.GetIndexData<std::uint32_t>(), indices32);
          }
        } else {
          // Generate indices for primitives without them.
          indexCount = positionView.size();

          if (indexCount > std::numeric_limits<uint16_t>::max()) {
            meshData.SetIndexBufferParams(indexCount, IndexFormat::UInt32);
            generateIndices(meshData.GetIndexData<std::uint32_t>(), indexCount);
          } else {
            meshData.SetIndexBufferParams(indexCount, IndexFormat::UInt16);
            generateIndices(meshData.GetIndexData<std::uint16_t>(), indexCount);
          }
        }

        meshData.subMeshCount(1);

        // TODO: use sub-meshes for glTF primitives, instead of a separate mesh
        // for each.
        SubMeshDescriptor subMeshDescriptor{};

        if (primitive.mode == MeshPrimitive::Mode::POINTS) {
          subMeshDescriptor.topology = MeshTopology::Points;
          primitiveInfo.containsPoints = true;
        } else {
          subMeshDescriptor.topology = MeshTopology::Triangles;
        }

        subMeshDescriptor.indexStart = 0;
        subMeshDescriptor.indexCount = indexCount;
        subMeshDescriptor.baseVertex = 0;

        // These are calculated automatically by SetSubMesh
        subMeshDescriptor.firstVertex = 0;
        subMeshDescriptor.vertexCount = 0;

        meshData.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags::Default);
      });
}

/**
 * @brief The result of the async part of mesh loading.
 */
struct LoadThreadResult {
  System::Array1<UnityEngine::Mesh> meshes;
  std::vector<CesiumPrimitiveInfo> primitiveInfos{};
};
} // namespace

UnityPrepareRendererResources::UnityPrepareRendererResources(
    const UnityEngine::GameObject& tileset)
    : _tileset(tileset), _shaderProperty() {}

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

  int32_t numberOfPrimitives = countPrimitives(*pModel);

  struct IntermediateLoadThreadResult {
    MeshDataResult meshDataResult;
    TileLoadResult tileLoadResult;
  };

  return asyncSystem
      .runInMainThread([numberOfPrimitives]() {
        // Allocate a MeshDataArray for the primitives.
        // Unfortunately, this must be done on the main thread.
        return UnityEngine::Mesh::AllocateWritableMeshData(numberOfPrimitives);
      })
      .thenInWorkerThread(
          [tileLoadResult = std::move(tileLoadResult)](
              UnityEngine::MeshDataArray&& meshDataArray) mutable {
            MeshDataResult meshDataResult{std::move(meshDataArray), {}};
            // Free the MeshDataArray if something goes wrong.
            ScopeGuard sg([&meshDataResult]() {
              meshDataResult.meshDataArray.Dispose();
            });

            populateMeshDataArray(meshDataResult, tileLoadResult);

            // We're returning the MeshDataArray, so don't free it.
            sg.release();
            return IntermediateLoadThreadResult{
                std::move(meshDataResult),
                std::move(tileLoadResult)};
          })
      .thenInMainThread(
          [asyncSystem, tileset = this->_tileset](
              IntermediateLoadThreadResult&& workerResult) mutable {
            bool shouldCreatePhysicsMeshes = false;
            bool shouldShowTilesInHierarchy = false;

            DotNet::CesiumForUnity::Cesium3DTileset tilesetComponent =
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
                  CesiumForUnity::CesiumObjectPool::MeshPool().Get();
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
              const std::int32_t len = meshes.Length();
              std::vector<std::int32_t> instanceIDs;
              for (int32_t i = 0; i < len; ++i) {
                // Don't attempt to bake a physics mesh from a point cloud.
                if (primitiveInfos[i].containsPoints) {
                  continue;
                }
                instanceIDs.push_back(meshes[i].GetInstanceID());
              }

              if (instanceIDs.size() > 0) {
                return asyncSystem.runInWorkerThread(
                    [workerResult = std::move(workerResult),
                     instanceIDs = std::move(instanceIDs),
                     meshes = std::move(meshes)]() mutable {
                      for (std::int32_t instanceID : instanceIDs) {
                        UnityEngine::Physics::BakeMesh(instanceID, false);
                      }

                      LoadThreadResult* pResult = new LoadThreadResult{
                          std::move(meshes),
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
                std::move(workerResult.meshDataResult.primitiveInfos)};
            return asyncSystem.createResolvedFuture(
                TileLoadResultAndRenderResources{
                    std::move(workerResult.tileLoadResult),
                    pResult});
          });
}

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult_) {
  std::unique_ptr<LoadThreadResult> pLoadThreadResult(
      static_cast<LoadThreadResult*>(pLoadThreadResult_));

  const System::Array1<UnityEngine::Mesh>& meshes = pLoadThreadResult->meshes;
  const std::vector<CesiumPrimitiveInfo>& primitiveInfos =
      pLoadThreadResult->primitiveInfos;

  const Cesium3DTilesSelection::TileContent& content = tile.getContent();
  const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
      content.getRenderContent();
  if (!pRenderContent) {
    return nullptr;
  }

  const Model& model = pRenderContent->getModel();

  std::string name = "glTF";
  auto urlIt = model.extras.find("Cesium3DTiles_TileUrl");
  if (urlIt != model.extras.end()) {
    name = urlIt->second.getStringOrDefault("glTF");
  }

  DotNet::CesiumForUnity::Cesium3DTileset tilesetComponent =
      this->_tileset.GetComponent<DotNet::CesiumForUnity::Cesium3DTileset>();

  uint32_t currentOverlayCount =
      static_cast<uint32_t>(tilesetComponent.NativeImplementation()
                                .getTileset()
                                ->getOverlays()
                                .size());

  auto pModelGameObject =
      std::make_unique<UnityEngine::GameObject>(System::String(name));

  if (tilesetComponent.showTilesInHierarchy()) {
    pModelGameObject->hideFlags(UnityEngine::HideFlags::DontSave);
  } else {
    pModelGameObject->hideFlags(
        UnityEngine::HideFlags::DontSave |
        UnityEngine::HideFlags::HideInHierarchy);
  }

  pModelGameObject->transform().SetParent(this->_tileset.transform(), false);
  pModelGameObject->SetActive(false);

  glm::dmat4 tileTransform = tile.getTransform();
  tileTransform = GltfUtilities::applyRtcCenter(model, tileTransform);
  tileTransform = GltfUtilities::applyGltfUpAxisTransform(model, tileTransform);

  DotNet::CesiumForUnity::CesiumGeoreference georeferenceComponent =
      this->_tileset
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();

  const LocalHorizontalCoordinateSystem* pCoordinateSystem = nullptr;
  if (georeferenceComponent != nullptr) {
    pCoordinateSystem =
        &georeferenceComponent.NativeImplementation().getCoordinateSystem(
            georeferenceComponent);
  }

  const bool createPhysicsMeshes = tilesetComponent.createPhysicsMeshes();
  const bool showTilesInHierarchy = tilesetComponent.showTilesInHierarchy();

  size_t meshIndex = 0;

  DotNet::CesiumForUnity::CesiumMetadata pMetadataComponent = nullptr;
  if (model.getExtension<ExtensionModelExtFeatureMetadata>()) {
    pMetadataComponent =
        pModelGameObject
            ->GetComponentInParent<DotNet::CesiumForUnity::CesiumMetadata>();
    if (pMetadataComponent == nullptr) {
      pMetadataComponent =
          this->_tileset.AddComponent<DotNet::CesiumForUnity::CesiumMetadata>();
    }
  }

  model.forEachPrimitiveInScene(
      -1,
      [&meshes,
       &primitiveInfos,
       &pModelGameObject,
       &tileTransform,
       &meshIndex,
       &tilesetComponent,
       pCoordinateSystem,
       createPhysicsMeshes,
       showTilesInHierarchy,
       currentOverlayCount,
       &pMetadataComponent,
       &shaderProperty = _shaderProperty](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        const CesiumPrimitiveInfo& primitiveInfo = primitiveInfos[meshIndex];
        UnityEngine::Mesh unityMesh = meshes[meshIndex++];
        if (unityMesh == nullptr) {
          // This indicates Unity destroyed the mesh already, which really
          // shouldn't happen.
          return;
        }

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

        int64_t primitiveIndex = &mesh.primitives[0] - &primitive;
        UnityEngine::GameObject primitiveGameObject(
            System::String("Primitive " + std::to_string(primitiveIndex)));
        if (showTilesInHierarchy) {
          primitiveGameObject.hideFlags(UnityEngine::HideFlags::DontSave);
        } else {
          primitiveGameObject.hideFlags(
              UnityEngine::HideFlags::DontSave |
              UnityEngine::HideFlags::HideInHierarchy);
        }

        primitiveGameObject.transform().parent(pModelGameObject->transform());

        glm::dmat4 modelToEcef = tileTransform * transform;

        CesiumForUnity::CesiumGlobeAnchor anchor =
            primitiveGameObject
                .AddComponent<CesiumForUnity::CesiumGlobeAnchor>();
        anchor.detectTransformChanges(false);
        anchor.adjustOrientationForGlobeWhenMoving(false);
        anchor.localToGlobeFixedMatrix(
            UnityTransforms::toUnityMathematics(modelToEcef));

        UnityEngine::MeshFilter meshFilter =
            primitiveGameObject.AddComponent<UnityEngine::MeshFilter>();
        UnityEngine::MeshRenderer meshRenderer =
            primitiveGameObject.AddComponent<UnityEngine::MeshRenderer>();

        const Material* pMaterial =
            Model::getSafe(&gltf.materials, primitive.material);

        UnityEngine::Material opaqueMaterial =
            tilesetComponent.opaqueMaterial();
        if (opaqueMaterial == nullptr) {
          if (pMaterial &&
              pMaterial->hasExtension<ExtensionKhrMaterialsUnlit>()) {
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
          if (pMaterial->pbrMetallicRoughness) {
            // Add base color factor and metallic-roughness factor regardless of
            // if the textures are present.
            const std::vector<double>& baseColorFactorSrc =
                pMaterial->pbrMetallicRoughness->baseColorFactor;
            UnityEngine::Vector4 baseColorFactor;
            baseColorFactor.x = baseColorFactorSrc.size() > 0
                                    ? static_cast<float>(baseColorFactorSrc[0])
                                    : 1.0f;
            baseColorFactor.y = baseColorFactorSrc.size() > 1
                                    ? static_cast<float>(baseColorFactorSrc[1])
                                    : 1.0f;
            baseColorFactor.z = baseColorFactorSrc.size() > 2
                                    ? static_cast<float>(baseColorFactorSrc[2])
                                    : 1.0f;
            baseColorFactor.w = baseColorFactorSrc.size() > 3
                                    ? static_cast<float>(baseColorFactorSrc[3])
                                    : 1.0f;
            material.SetVector(
                shaderProperty.getBaseColorFactorID(),
                baseColorFactor);

            UnityEngine::Vector4 metallicRoughnessFactor;
            metallicRoughnessFactor.x =
                pMaterial->pbrMetallicRoughness->metallicFactor;
            metallicRoughnessFactor.y =
                pMaterial->pbrMetallicRoughness->roughnessFactor;
            material.SetVector(
                shaderProperty.getMetallicRoughnessFactorID(),
                metallicRoughnessFactor);

            const std::optional<TextureInfo>& baseColorTexture =
                pMaterial->pbrMetallicRoughness->baseColorTexture;
            if (baseColorTexture) {
              auto texCoordIndexIt =
                  primitiveInfo.uvIndexMap.find(baseColorTexture->texCoord);
              if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
                UnityEngine::Texture texture =
                    TextureLoader::loadTexture(gltf, baseColorTexture->index);
                if (texture != nullptr) {
                  material.SetTexture(
                      shaderProperty.getBaseColorTextureID(),
                      texture);
                  material.SetFloat(
                      shaderProperty.getBaseColorTextureCoordinateIndexID(),
                      static_cast<float>(texCoordIndexIt->second));
                }
              }
            }

            const std::optional<TextureInfo>& metallicRoughness =
                pMaterial->pbrMetallicRoughness->metallicRoughnessTexture;
            if (metallicRoughness) {
              auto texCoordIndexIt =
                  primitiveInfo.uvIndexMap.find(metallicRoughness->texCoord);
              if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
                UnityEngine::Texture texture =
                    TextureLoader::loadTexture(gltf, metallicRoughness->index);
                if (texture != nullptr) {
                  material.SetTexture(
                      shaderProperty.getMetallicRoughnessTextureID(),
                      texture);
                  material.SetFloat(
                      shaderProperty
                          .getMetallicRoughnessTextureCoordinateIndexID(),
                      static_cast<float>(texCoordIndexIt->second));
                }
              }
            }
          }

          if (pMaterial->normalTexture) {
            auto texCoordIndexIt = primitiveInfo.uvIndexMap.find(
                pMaterial->normalTexture->texCoord);
            if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
              UnityEngine::Texture texture = TextureLoader::loadTexture(
                  gltf,
                  pMaterial->normalTexture->index);
              if (texture != nullptr) {
                material.SetTexture(
                    shaderProperty.getNormalMapTextureID(),
                    texture);
                material.SetFloat(
                    shaderProperty.getNormalMapTextureCoordinateIndexID(),
                    static_cast<float>(texCoordIndexIt->second));
                material.SetFloat(
                    shaderProperty.getNormalMapScaleID(),
                    static_cast<float>(pMaterial->normalTexture->scale));
              }
            }
          }

          if (pMaterial->occlusionTexture) {
            auto texCoordIndexIt = primitiveInfo.uvIndexMap.find(
                pMaterial->occlusionTexture->texCoord);
            if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
              UnityEngine::Texture texture = TextureLoader::loadTexture(
                  gltf,
                  pMaterial->occlusionTexture->index);
              if (texture != nullptr) {
                material.SetTexture(
                    shaderProperty.getOcclusionTextureID(),
                    texture);
                material.SetFloat(
                    shaderProperty.getOcclusionTextureCoordinateIndexID(),
                    static_cast<float>(texCoordIndexIt->second));
                material.SetFloat(
                    shaderProperty.getOcclusionStrengthID(),
                    static_cast<float>(pMaterial->occlusionTexture->strength));
              }
            }
          }

          const std::vector<double>& emissiveFactorSrc =
              pMaterial->emissiveFactor;
          UnityEngine::Vector4 emissiveFactor;
          emissiveFactor.x = emissiveFactorSrc.size() > 0
                                 ? static_cast<float>(emissiveFactorSrc[0])
                                 : 0.0f;
          emissiveFactor.y = emissiveFactorSrc.size() > 1
                                 ? static_cast<float>(emissiveFactorSrc[1])
                                 : 0.0f;
          emissiveFactor.z = emissiveFactorSrc.size() > 2
                                 ? static_cast<float>(emissiveFactorSrc[2])
                                 : 0.0f;
          material.SetVector(
              shaderProperty.getEmissiveFactorID(),
              emissiveFactor);
          if (pMaterial->emissiveTexture) {
            auto texCoordIndexIt = primitiveInfo.uvIndexMap.find(
                pMaterial->emissiveTexture->texCoord);
            if (texCoordIndexIt != primitiveInfo.uvIndexMap.end()) {
              UnityEngine::Texture texture = TextureLoader::loadTexture(
                  gltf,
                  pMaterial->emissiveTexture->index);
              if (texture != nullptr) {
                material.SetTexture(
                    shaderProperty.getEmissiveTextureID(),
                    texture);
                material.SetFloat(
                    shaderProperty.getEmissiveTextureCoordinateIndexID(),
                    static_cast<float>(texCoordIndexIt->second));
              }
            }
          }
        }

        // Initialize overlay UVs to all use index 0, attachRasterTile will
        // update the uniforms with the correct UV index.
        for (uint32_t i = 0; i < currentOverlayCount; ++i) {
          material.SetFloat(
              shaderProperty.getOverlayTextureCoordinateIndexID(i),
              0);
        }

        meshFilter.sharedMesh(unityMesh);

        if (createPhysicsMeshes &&
            primitive.mode != MeshPrimitive::Mode::POINTS) {
          // This should not trigger mesh baking for physics, because the meshes
          // were already baked in the worker thread.
          UnityEngine::MeshCollider meshCollider =
              primitiveGameObject.AddComponent<UnityEngine::MeshCollider>();
          meshCollider.sharedMesh(unityMesh);
        }
        const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
            primitive.getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();
        if (pMetadata) {
          pMetadataComponent.NativeImplementation().addMetadata(
              primitiveGameObject.transform().GetInstanceID(),
              &gltf,
              &primitive);
        }
      });

  CesiumGltfGameObject* pCesiumGameObject = new CesiumGltfGameObject{
      std::move(pModelGameObject),
      std::move(pLoadThreadResult->primitiveInfos)};

  return pCesiumGameObject;
}

namespace {

void freePrimitiveGameObject(
    const DotNet::UnityEngine::GameObject& primitiveGameObject,
    const DotNet::CesiumForUnity::CesiumMetadata& maybeMetadata) {
  if (maybeMetadata != nullptr) {
    maybeMetadata.NativeImplementation().removeMetadata(
        primitiveGameObject.transform().GetInstanceID());
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
      if (texture != nullptr)
        UnityLifetime::Destroy(texture);
    }

    UnityLifetime::Destroy(material);
  }

  UnityEngine::MeshFilter meshFilter =
      primitiveGameObject.GetComponent<UnityEngine::MeshFilter>();
  if (meshFilter != nullptr) {
    CesiumForUnity::CesiumObjectPool::MeshPool().Release(
        meshFilter.sharedMesh());
  }

  // The MeshCollider shares a mesh with the MeshFilter, so no need to
  // destroy it explicitly.
}

} // namespace

void UnityPrepareRendererResources::free(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  if (pLoadThreadResult) {
    LoadThreadResult* pTyped =
        static_cast<LoadThreadResult*>(pLoadThreadResult);
    for (int32_t i = 0, len = pTyped->meshes.Length(); i < len; ++i) {
      CesiumForUnity::CesiumObjectPool::MeshPool().Release(pTyped->meshes[i]);
    }
    delete pTyped;
  }

  if (pMainThreadResult) {
    std::unique_ptr<CesiumGltfGameObject> pCesiumGameObject(
        static_cast<CesiumGltfGameObject*>(pMainThreadResult));

    auto metadataComponent =
        pCesiumGameObject->pGameObject
            ->GetComponentInParent<DotNet::CesiumForUnity::CesiumMetadata>();

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

    UnityLifetime::Destroy(*pCesiumGameObject->pGameObject);
  }
}

void* UnityPrepareRendererResources::prepareRasterInLoadThread(
    CesiumGltf::ImageCesium& image,
    const std::any& rendererOptions) {
  return nullptr;
}

void* UnityPrepareRendererResources::prepareRasterInMainThread(
    Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult) {
  auto pTexture = std::make_unique<UnityEngine::Texture>(
      TextureLoader::loadTexture(rasterTile.getImage()));
  pTexture->wrapMode(UnityEngine::TextureWrapMode::Clamp);
  pTexture->filterMode(UnityEngine::FilterMode::Trilinear);
  pTexture->anisoLevel(16);
  return pTexture.release();
}

void UnityPrepareRendererResources::freeRaster(
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  if (pMainThreadResult) {
    std::unique_ptr<UnityEngine::Texture> pTexture(
        static_cast<UnityEngine::Texture*>(pMainThreadResult));
    UnityLifetime::Destroy(*pTexture);
  }
}

void UnityPrepareRendererResources::attachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
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

  DotNet::CesiumForUnity::Cesium3DTileset tilesetComponent =
      this->_tileset.GetComponent<DotNet::CesiumForUnity::Cesium3DTileset>();
  Tileset* pTileset = tilesetComponent.NativeImplementation().getTileset();
  if (!pTileset)
    return;

  uint32_t overlayIndex = 0;
  bool overlayFound = false;
  for (const CesiumUtility::IntrusivePointer<RasterOverlay>& pOverlay :
       pTileset->getOverlays()) {
    // TODO: Is it safe to compare pointers like this?
    if (&rasterTile.getOverlay() == pOverlay.get()) {
      overlayFound = true;
      break;
    }

    ++overlayIndex;
  }

  if (!overlayFound)
    return;

  // TODO: Can we count on the order of primitives in the transform chain
  // to match the order of primitives using gltf->forEachPrimitive??
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
    material.SetFloat(
        _shaderProperty.getOverlayTextureCoordinateIndexID(overlayIndex),
        static_cast<float>(texCoordIndexIt->second));

    material.SetTexture(
        _shaderProperty.getOverlayTextureID(overlayIndex),
        *pTexture);

    UnityEngine::Vector4 translationAndScale{
        float(translation.x),
        float(translation.y),
        float(scale.x),
        float(scale.y)};
    material.SetVector(
        _shaderProperty.getOverlayTranslationAndScaleID(overlayIndex),
        translationAndScale);
  }
}

void UnityPrepareRendererResources::detachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
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
  if (!pCesiumGameObject || !pCesiumGameObject->pGameObject || !pTexture)
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

    material.SetTexture(
        _shaderProperty.getOverlayTextureID(overlayTextureCoordinateID),
        UnityEngine::Texture(nullptr));
  }
}
