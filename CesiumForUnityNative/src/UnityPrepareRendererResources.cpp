#include "UnityPrepareRendererResources.h"

#include "TextureLoader.h"
#include "UnityLifetime.h"

#include <Cesium3DTilesSelection/GltfUtilities.h>
#include <Cesium3DTilesSelection/Tile.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumGltf/AccessorView.h>
#include <CesiumUtility/ScopeGuard.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/System/Text/Encoding.h>
#include <DotNet/Unity/Collections/Allocator.h>
#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/Unity/Collections/NativeArrayOptions.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
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

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnityNative;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace CesiumGltf;
using namespace CesiumUtility;
using namespace DotNet;

namespace {

template <typename T>
void setTriangles(UnityEngine::Mesh& mesh, const AccessorView<T>& indices) {
  Unity::Collections::NativeArray1<std::int32_t> nativeArrayTriangles(
      indices.size(),
      Unity::Collections::Allocator::Temp,
      Unity::Collections::NativeArrayOptions::UninitializedMemory);
  ScopeGuard sg([&nativeArrayTriangles]() { nativeArrayTriangles.Dispose(); });

  std::int32_t* triangles = static_cast<std::int32_t*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(nativeArrayTriangles));

  for (int64_t i = 0; i < indices.size(); ++i) {
    triangles[i] = indices[i];
  }

  mesh.SetIndices<std::int32_t>(
      nativeArrayTriangles,
      UnityEngine::MeshTopology::Triangles,
      0,
      true,
      0);
}

template <typename TDest, typename TSource>
void setTriangles(
    Unity::Collections::NativeArray1<TDest>& dest,
    const AccessorView<TSource>& source) {
  assert(dest.Length() == source.size());

  TDest* triangles = static_cast<TDest*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(dest));

  for (int64_t i = 0; i < source.size(); ++i) {
    triangles[i] = source[i];
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

void populateMeshDataArray(
    const UnityEngine::MeshDataArray& meshDataArray,
    const TileLoadResult& tileLoadResult,
    const glm::dmat4& transform) {
  const CesiumGltf::Model* pModel =
      std::get_if<CesiumGltf::Model>(&tileLoadResult.contentKind);
  if (!pModel)
    return;

  glm::dmat4 tileTransform = GltfUtilities::applyRtcCenter(*pModel, transform);
  tileTransform =
      GltfUtilities::applyGltfUpAxisTransform(*pModel, tileTransform);

  size_t meshDataInstance = 0;

  pModel->forEachPrimitiveInScene(
      -1,
      [&tileTransform, &meshDataArray, &meshDataInstance](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        UnityEngine::MeshData meshData = meshDataArray[meshDataInstance++];

        if (primitive.indices < 0) {
          // TODO: support non-indexed primitives.
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

        auto normalAccessorIt = primitive.attributes.find("NORMAL");
        AccessorView<UnityEngine::Vector3> normalView =
            normalAccessorIt != primitive.attributes.end()
                ? AccessorView<UnityEngine::Vector3>(
                      gltf,
                      normalAccessorIt->second)
                : AccessorView<UnityEngine::Vector3>();

        auto texCoord0AccessorIt = primitive.attributes.find("TEXCOORD_0");
        AccessorView<UnityEngine::Vector2> texCoord0View =
            texCoord0AccessorIt != primitive.attributes.end()
                ? AccessorView<UnityEngine::Vector2>(
                      gltf,
                      texCoord0AccessorIt->second)
                : AccessorView<UnityEngine::Vector2>();

        auto overlay0AccessorIt = primitive.attributes.find("_CESIUMOVERLAY_0");
        AccessorView<UnityEngine::Vector2> overlay0View =
            overlay0AccessorIt != primitive.attributes.end()
                ? AccessorView<UnityEngine::Vector2>(
                      gltf,
                      overlay0AccessorIt->second)
                : AccessorView<UnityEngine::Vector2>();

        using namespace DotNet::UnityEngine;
        using namespace DotNet::UnityEngine::Rendering;
        using namespace DotNet::Unity::Collections;
        using namespace DotNet::Unity::Collections::LowLevel::Unsafe;

        const int MAX_ATTRIBUTES = 5;
        VertexAttributeDescriptor descriptor[MAX_ATTRIBUTES];

        std::int32_t numberOfAttributes = 0;
        std::int32_t nextStream = 0;

        std::int32_t positionStream = -1;
        std::int32_t normalStream = -1;
        std::int32_t texCoord0Stream = -1;
        std::int32_t overlay0Stream = -1;

        // TODO: is it better to interleave the attributes, rather than a stream
        // per attribute?

        assert(numberOfAttributes < MAX_ATTRIBUTES);
        descriptor[numberOfAttributes].attribute = VertexAttribute::Position;
        descriptor[numberOfAttributes].format = VertexAttributeFormat::Float32;
        descriptor[numberOfAttributes].dimension = 3;
        positionStream = nextStream++;
        descriptor[numberOfAttributes].stream = positionStream;
        ++numberOfAttributes;

        if (normalView.size() > 0) {
          assert(numberOfAttributes < MAX_ATTRIBUTES);
          descriptor[numberOfAttributes].attribute = VertexAttribute::Normal;
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
          descriptor[numberOfAttributes].dimension = 3;
          normalStream = nextStream++;
          descriptor[numberOfAttributes].stream = normalStream;
          ++numberOfAttributes;
        }

        if (texCoord0View.size() > 0) {
          assert(numberOfAttributes < MAX_ATTRIBUTES);
          descriptor[numberOfAttributes].attribute = VertexAttribute::TexCoord0;
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
          descriptor[numberOfAttributes].dimension = 2;
          texCoord0Stream = nextStream++;
          descriptor[numberOfAttributes].stream = texCoord0Stream;
          ++numberOfAttributes;
        }

        if (overlay0View.size() > 0) {
          assert(numberOfAttributes < MAX_ATTRIBUTES);
          descriptor[numberOfAttributes].attribute = VertexAttribute::TexCoord1;
          descriptor[numberOfAttributes].format =
              VertexAttributeFormat::Float32;
          descriptor[numberOfAttributes].dimension = 2;
          overlay0Stream = nextStream++;
          descriptor[numberOfAttributes].stream = overlay0Stream;
          ++numberOfAttributes;
        }

        System::Array1<VertexAttributeDescriptor> attributes(
            numberOfAttributes);
        for (int32_t i = 0; i < numberOfAttributes; ++i) {
          attributes.Item(i, descriptor[i]);
        }

        meshData.SetVertexBufferParams(positionView.size(), attributes);

        // Copy positions into the MeshData
        NativeArray1<Vector3> nativeArrayVertices =
            meshData.GetVertexData<Vector3>(positionStream);
        Vector3* vertices = static_cast<Vector3*>(
            NativeArrayUnsafeUtility::GetUnsafeBufferPointerWithoutChecks(
                nativeArrayVertices));
        for (int64_t i = 0; i < positionView.size(); ++i) {
          vertices[i] = positionView[i];
        }

        // Copy normals (if any) into the MeshData
        if (normalStream >= 0) {
          NativeArray1<Vector3> nativeArrayNormals =
              meshData.GetVertexData<Vector3>(normalStream);
          Vector3* normals = static_cast<Vector3*>(
              NativeArrayUnsafeUtility::GetUnsafeBufferPointerWithoutChecks(
                  nativeArrayNormals));

          int64_t normalsToCopy =
              glm::min(normalView.size(), positionView.size());
          for (int64_t i = 0; i < normalsToCopy; ++i) {
            normals[i] = normalView[i];
          }

          // Just in case there are more positions than normals
          for (int64_t i = normalsToCopy; i < positionView.size(); ++i) {
            normals[i] = Vector3{0.0f, 0.0f, 0.0f};
          }
        }

        // Copy texture coordinates (if any) into the MeshData
        if (texCoord0Stream >= 0) {
          NativeArray1<Vector2> nativeArrayTexCoord0 =
              meshData.GetVertexData<Vector2>(texCoord0Stream);
          Vector2* texCoord0 = static_cast<Vector2*>(
              NativeArrayUnsafeUtility::GetUnsafeBufferPointerWithoutChecks(
                  nativeArrayTexCoord0));

          int64_t texCoord0sToCopy =
              glm::min(texCoord0View.size(), positionView.size());
          for (int64_t i = 0; i < texCoord0sToCopy; ++i) {
            texCoord0[i] = texCoord0View[i];
          }

          // Just in case there are more positions than texture coordinates
          for (int64_t i = texCoord0sToCopy; i < positionView.size(); ++i) {
            texCoord0[i] = Vector2{0.0f, 0.0f};
          }
        }

        // Copy overlay texture coordinates (if any) into the MeshData
        if (overlay0Stream >= 0) {
          NativeArray1<Vector2> nativeArrayOverlay0 =
              meshData.GetVertexData<Vector2>(overlay0Stream);
          Vector2* overlay0 = static_cast<Vector2*>(
              NativeArrayUnsafeUtility::GetUnsafeBufferPointerWithoutChecks(
                  nativeArrayOverlay0));

          int64_t overlay0sToCopy =
              glm::min(overlay0View.size(), positionView.size());
          for (int64_t i = 0; i < overlay0sToCopy; ++i) {
            overlay0[i] = overlay0View[i];
          }

          // Just in case there are more positions than overlay texture
          // coordinates
          for (int64_t i = overlay0sToCopy; i < positionView.size(); ++i) {
            overlay0[i] = Vector2{0.0f, 0.0f};
          }
        }

        int32_t indexCount = 0;

        AccessorView<uint8_t> indices8(gltf, primitive.indices);
        if (indices8.status() == AccessorViewStatus::Valid) {
          indexCount = indices8.size();
          meshData.SetIndexBufferParams(
              indexCount,
              UnityEngine::Rendering::IndexFormat::UInt16);
          setTriangles(meshData.GetIndexData<std::uint16_t>(), indices8);
        }

        AccessorView<uint16_t> indices16(gltf, primitive.indices);
        if (indices16.status() == AccessorViewStatus::Valid) {
          indexCount = indices16.size();
          meshData.SetIndexBufferParams(
              indexCount,
              UnityEngine::Rendering::IndexFormat::UInt16);
          setTriangles(meshData.GetIndexData<std::uint16_t>(), indices16);
        }

        AccessorView<uint32_t> indices32(gltf, primitive.indices);
        if (indices32.status() == AccessorViewStatus::Valid) {
          indexCount = indices32.size();
          meshData.SetIndexBufferParams(
              indexCount,
              UnityEngine::Rendering::IndexFormat::UInt32);
          setTriangles(meshData.GetIndexData<std::uint32_t>(), indices32);
        }

        meshData.subMeshCount(1);

        // TODO: use sub-meshes for glTF primitives, instead of a separate mesh
        // for each.
        UnityEngine::Rendering::SubMeshDescriptor subMeshDescriptor{};
        subMeshDescriptor.topology = UnityEngine::MeshTopology::Triangles;
        subMeshDescriptor.indexStart = 0;
        subMeshDescriptor.indexCount = indexCount;
        subMeshDescriptor.baseVertex = 0;

        // These are calculated automatically by SetSubMesh
        subMeshDescriptor.firstVertex = 0;
        subMeshDescriptor.vertexCount = 0;

        meshData.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags::Default);

        // if (createPhysicsMeshes) {
        //   UnityEngine::MeshCollider meshCollider =
        //       primitiveGameObject.AddComponent<UnityEngine::MeshCollider>();
        //   meshCollider.sharedMesh(unityMesh);
        // }
      });
}
} // namespace

UnityPrepareRendererResources::UnityPrepareRendererResources(
    const UnityEngine::GameObject& tileset)
    : _tileset(tileset) {}

CesiumAsync::Future<TileLoadResultAndRenderResources>
UnityPrepareRendererResources::prepareInLoadThread(
    const CesiumAsync::AsyncSystem& asyncSystem,
    TileLoadResult&& tileLoadResult,
    const glm::dmat4& transform) {
  CesiumGltf::Model* pModel =
      std::get_if<CesiumGltf::Model>(&tileLoadResult.contentKind);
  if (!pModel)
    return asyncSystem.createResolvedFuture(
        TileLoadResultAndRenderResources{std::move(tileLoadResult), nullptr});

  int32_t numberOfPrimitives = countPrimitives(*pModel);

  return asyncSystem
      .runInMainThread([numberOfPrimitives, tileset = this->_tileset]() {
        // Allocate a MeshDataArray for the primitives.
        // Unfortunately, this must be done on the main thread.
        return UnityEngine::Mesh::AllocateWritableMeshData(numberOfPrimitives);
      })
      .thenInWorkerThread(
          [tileLoadResult = std::move(tileLoadResult),
           transform](UnityEngine::MeshDataArray&& meshDataArray) mutable {
            // Free the MeshDataArray if something goes wrong.
            ScopeGuard sg([&meshDataArray]() { meshDataArray.Dispose(); });

            populateMeshDataArray(meshDataArray, tileLoadResult, transform);

            // We're returning the MeshDataArray, so don't free it.
            sg.release();
            return TileLoadResultAndRenderResources{
                std::move(tileLoadResult),
                new UnityEngine::MeshDataArray(std::move(meshDataArray))};
          });
}

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult) {
  std::unique_ptr<UnityEngine::MeshDataArray> pMeshDataArray(
      static_cast<UnityEngine::MeshDataArray*>(pLoadThreadResult));

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

  auto pModelGameObject =
      std::make_unique<UnityEngine::GameObject>(System::String(name));
  pModelGameObject->hideFlags(UnityEngine::HideFlags::DontSave);
  pModelGameObject->transform().parent(this->_tileset.transform());
  pModelGameObject->SetActive(false);

  glm::dmat4 tileTransform = tile.getTransform();
  tileTransform = GltfUtilities::applyRtcCenter(model, tileTransform);
  tileTransform = GltfUtilities::applyGltfUpAxisTransform(model, tileTransform);

  DotNet::CesiumForUnity::Cesium3DTileset tilesetComponent =
      this->_tileset.GetComponent<DotNet::CesiumForUnity::Cesium3DTileset>();

  DotNet::CesiumForUnity::CesiumGeoreference georeferenceComponent =
      this->_tileset
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();

  const LocalHorizontalCoordinateSystem* pCoordinateSystem = nullptr;
  if (georeferenceComponent != nullptr) {
    pCoordinateSystem =
        &georeferenceComponent.NativeImplementation().getCoordinateSystem();
  }

  UnityEngine::Material opaqueMaterial = tilesetComponent.opaqueMaterial();
  if (opaqueMaterial == nullptr) {
    opaqueMaterial = UnityEngine::Resources::Load<UnityEngine::Material>(
        System::String("CesiumDefaultTilesetMaterial"));
  }

  const bool createPhysicsMeshes = tilesetComponent.createPhysicsMeshes();

  // Create meshes and populate them from the MeshData created in the worker
  // thread.
  System::Array1<UnityEngine::Mesh> meshes(pMeshDataArray->Length());
  for (int32_t i = 0, len = meshes.Length(); i < len; ++i) {
    meshes.Item(i, UnityEngine::Mesh());
  }

  UnityEngine::Mesh::ApplyAndDisposeWritableMeshData(
      *pMeshDataArray,
      meshes,
      UnityEngine::Rendering::MeshUpdateFlags::Default);

  size_t meshIndex = 0;

  model.forEachPrimitiveInScene(
      -1,
      [&meshes,
       &pModelGameObject,
       &tileTransform,
       &meshIndex,
       opaqueMaterial,
       pCoordinateSystem,
       createPhysicsMeshes](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
        UnityEngine::Mesh unityMesh = meshes[meshIndex++];

        if (primitive.indices < 0) {
          // TODO: support non-indexed primitives.
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
        primitiveGameObject.hideFlags(UnityEngine::HideFlags::DontSave);
        primitiveGameObject.transform().parent(pModelGameObject->transform());

        glm::dmat4 fixedToUnity =
            pCoordinateSystem
                ? pCoordinateSystem->getEcefToLocalTransformation()
                : glm::dmat4();

        glm::dmat4 fullTransform = fixedToUnity * (tileTransform * transform);

        glm::dvec3 translation = glm::dvec3(fullTransform[3]);
        glm::dmat3 rotationScale = glm::dmat3(fullTransform);
        double lengthColumn0 = glm::length(rotationScale[0]);
        double lengthColumn1 = glm::length(rotationScale[1]);
        double lengthColumn2 = glm::length(rotationScale[2]);
        glm::dmat3 rotationMatrix(
            rotationScale[0] / lengthColumn0,
            rotationScale[1] / lengthColumn1,
            rotationScale[2] / lengthColumn2);

        glm::dvec3 scale(lengthColumn0, lengthColumn1, lengthColumn2);

        glm::dvec3 cross = glm::cross(rotationScale[0], rotationScale[1]);
        if (glm::dot(cross, rotationScale[2]) < 0.0) {
          rotationMatrix *= -1.0;
          scale *= -1.0;
        }

        glm::dquat rotation = glm::quat_cast(rotationMatrix);

        primitiveGameObject.transform().position(UnityEngine::Vector3{
            float(translation.x),
            float(translation.y),
            float(translation.z)});
        primitiveGameObject.transform().rotation(UnityEngine::Quaternion{
            float(rotation.x),
            float(rotation.y),
            float(rotation.z),
            float(rotation.w)});
        primitiveGameObject.transform().localScale(UnityEngine::Vector3{
            float(scale.x),
            float(scale.y),
            float(scale.z)});

        UnityEngine::MeshFilter meshFilter =
            primitiveGameObject.AddComponent<UnityEngine::MeshFilter>();
        UnityEngine::MeshRenderer meshRenderer =
            primitiveGameObject.AddComponent<UnityEngine::MeshRenderer>();

        UnityEngine::Material material =
            UnityEngine::Object::Instantiate(opaqueMaterial);
        meshRenderer.material(material);

        const Material* pMaterial =
            Model::getSafe(&gltf.materials, primitive.material);
        if (pMaterial && pMaterial->pbrMetallicRoughness) {
          const std::optional<TextureInfo>& baseColorTexture =
              pMaterial->pbrMetallicRoughness->baseColorTexture;
          if (baseColorTexture) {
            UnityEngine::Texture texture =
                TextureLoader::loadTexture(gltf, baseColorTexture->index);
            if (texture != nullptr) {
              material.SetTexture(System::String("_baseColorTexture"), texture);
            }
          }
        }

        auto overlay0AccessorIt = primitive.attributes.find("_CESIUMOVERLAY_0");
        if (overlay0AccessorIt != primitive.attributes.end()) {
          material.SetFloat(
              System::String("_overlay0TextureCoordinateIndex"),
              1);
        }

        meshFilter.mesh(unityMesh);

        if (createPhysicsMeshes) {
          UnityEngine::MeshCollider meshCollider =
              primitiveGameObject.AddComponent<UnityEngine::MeshCollider>();
          meshCollider.sharedMesh(unityMesh);
        }
      });

  return pModelGameObject.release();
}

void UnityPrepareRendererResources::free(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  if (pMainThreadResult) {
    std::unique_ptr<UnityEngine::GameObject> pGameObject(
        static_cast<UnityEngine::GameObject*>(pMainThreadResult));
    UnityLifetime::Destroy(*pGameObject);
  }
}

void* UnityPrepareRendererResources::prepareRasterInLoadThread(
    const CesiumGltf::ImageCesium& image,
    const std::any& rendererOptions) {
  return nullptr;
}

void* UnityPrepareRendererResources::prepareRasterInMainThread(
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult) {
  auto pTexture = std::make_unique<UnityEngine::Texture>(
      TextureLoader::loadTexture(rasterTile.getImage()));
  pTexture->wrapMode(UnityEngine::TextureWrapMode::Clamp);
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

  UnityEngine::GameObject* pGameObject = static_cast<UnityEngine::GameObject*>(
      pRenderContent->getRenderResources());
  UnityEngine::Texture* pTexture =
      static_cast<UnityEngine::Texture*>(pMainThreadRendererResources);
  if (!pGameObject || !pTexture)
    return;

  UnityEngine::Transform transform = pGameObject->transform();
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

    material.SetTexture(System::String("_overlay0Texture"), *pTexture);

    UnityEngine::Vector4 translationAndScale{
        float(translation.x),
        float(translation.y),
        float(scale.x),
        float(scale.y)};
    material.SetVector(
        System::String("_overlay0TranslationAndScale"),
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

  UnityEngine::GameObject* pGameObject = static_cast<UnityEngine::GameObject*>(
      pRenderContent->getRenderResources());
  UnityEngine::Texture* pTexture =
      static_cast<UnityEngine::Texture*>(pMainThreadRendererResources);
  if (!pGameObject || !pTexture)
    return;

  UnityEngine::Transform transform = pGameObject->transform();
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
        System::String("_overlay0Texture"),
        UnityEngine::Texture(nullptr));
  }
}
