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
#include <DotNet/UnityEngine/MeshFilter.h>
#include <DotNet/UnityEngine/MeshRenderer.h>
#include <DotNet/UnityEngine/MeshTopology.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/TextureWrapMode.h>
#include <DotNet/UnityEngine/FilterMode.h>
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

} // namespace

UnityPrepareRendererResources::UnityPrepareRendererResources(
    const UnityEngine::GameObject& tileset)
    : _tileset(tileset) {}

CesiumAsync::Future<TileLoadResultAndRenderResources>
UnityPrepareRendererResources::prepareInLoadThread(
    const CesiumAsync::AsyncSystem& asyncSystem,
    TileLoadResult&& tileLoadResult,
    const glm::dmat4& transform,
    const std::any& rendererOptions) {
  return asyncSystem.createResolvedFuture(
      TileLoadResultAndRenderResources{std::move(tileLoadResult), nullptr});
}

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult) {
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

  model.forEachPrimitiveInScene(
      -1,
      [&pModelGameObject,
       &tileTransform,
       opaqueMaterial,
       pCoordinateSystem,
       createPhysicsMeshes](
          const Model& gltf,
          const Node& node,
          const Mesh& mesh,
          const MeshPrimitive& primitive,
          const glm::dmat4& transform) {
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

        UnityEngine::Mesh unityMesh{};

        {
          Unity::Collections::NativeArray1<UnityEngine::Vector3>
              nativeArrayVertices(
                  positionView.size(),
                  Unity::Collections::Allocator::Temp,
                  Unity::Collections::NativeArrayOptions::UninitializedMemory);
          ScopeGuard sgVertices(
              [&nativeArrayVertices]() { nativeArrayVertices.Dispose(); });
          UnityEngine::Vector3* vertices = static_cast<UnityEngine::Vector3*>(
              Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                  GetUnsafeBufferPointerWithoutChecks(nativeArrayVertices));
          for (int64_t i = 0; i < positionView.size(); ++i) {
            vertices[i] = positionView[i];
          }
          unityMesh.SetVertices<UnityEngine::Vector3>(nativeArrayVertices);
        }

        auto normalAccessorIt = primitive.attributes.find("NORMAL");
        if (normalAccessorIt != primitive.attributes.end()) {
          int32_t normalAccessorID = normalAccessorIt->second;
          AccessorView<UnityEngine::Vector3> normalView(gltf, normalAccessorID);
          if (normalView.status() == AccessorViewStatus::Valid) {
            Unity::Collections::NativeArray1<UnityEngine::Vector3>
                nativeArrayNormals(
                    normalView.size(),
                    Unity::Collections::Allocator::Temp,
                    Unity::Collections::NativeArrayOptions::
                        UninitializedMemory);
            ScopeGuard sgNormals(
                [&nativeArrayNormals]() { nativeArrayNormals.Dispose(); });
            UnityEngine::Vector3* normals = static_cast<UnityEngine::Vector3*>(
                Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                    GetUnsafeBufferPointerWithoutChecks(nativeArrayNormals));

            for (int64_t i = 0; i < normalView.size(); ++i) {
              normals[i] = normalView[i];
            }
            unityMesh.SetNormals(nativeArrayNormals);
          }
        }

        auto texCoord0AccessorIt = primitive.attributes.find("TEXCOORD_0");
        if (texCoord0AccessorIt != primitive.attributes.end()) {
          int32_t texCoord0AccessorID = texCoord0AccessorIt->second;
          AccessorView<UnityEngine::Vector2> texCoord0View(
              gltf,
              texCoord0AccessorID);

          if (texCoord0View.status() == AccessorViewStatus::Valid) {
            Unity::Collections::NativeArray1<UnityEngine::Vector2>
                nativeArrayTexCoord0s(
                    texCoord0View.size(),
                    Unity::Collections::Allocator::Temp,
                    Unity::Collections::NativeArrayOptions::
                        UninitializedMemory);
            ScopeGuard sgTexCoord0([&nativeArrayTexCoord0s]() {
              nativeArrayTexCoord0s.Dispose();
            });
            UnityEngine::Vector2* texCoord0s = static_cast<
                UnityEngine::Vector2*>(
                Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                    GetUnsafeBufferPointerWithoutChecks(nativeArrayTexCoord0s));

            for (int64_t i = 0; i < texCoord0View.size(); ++i) {
              texCoord0s[i] = texCoord0View[i];
            }
            unityMesh.SetUVs(0, nativeArrayTexCoord0s);
          }
        }

        auto overlay0AccessorIt = primitive.attributes.find("_CESIUMOVERLAY_0");
        if (overlay0AccessorIt != primitive.attributes.end()) {
          int32_t overlay0AccessorID = overlay0AccessorIt->second;
          AccessorView<UnityEngine::Vector2> overlay0View(
              gltf,
              overlay0AccessorID);

          if (overlay0View.status() == AccessorViewStatus::Valid) {
            Unity::Collections::NativeArray1<UnityEngine::Vector2>
                nativeArrayOverlay0s(
                    overlay0View.size(),
                    Unity::Collections::Allocator::Temp,
                    Unity::Collections::NativeArrayOptions::
                        UninitializedMemory);
            ScopeGuard sgOverlay0(
                [&nativeArrayOverlay0s]() { nativeArrayOverlay0s.Dispose(); });
            UnityEngine::Vector2* overlay0s = static_cast<
                UnityEngine::Vector2*>(
                Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                    GetUnsafeBufferPointerWithoutChecks(nativeArrayOverlay0s));

            for (int64_t i = 0; i < overlay0View.size(); ++i) {
              overlay0s[i] = overlay0View[i];
            }
            unityMesh.SetUVs(1, nativeArrayOverlay0s);

            material.SetFloat(
                System::String("_overlay0TextureCoordinateIndex"),
                1);
          }
        }

        AccessorView<uint8_t> indices8(gltf, primitive.indices);
        if (indices8.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices8);
        }

        AccessorView<uint16_t> indices16(gltf, primitive.indices);
        if (indices16.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices16);
        }

        AccessorView<uint32_t> indices32(gltf, primitive.indices);
        if (indices32.status() == AccessorViewStatus::Valid) {
          setTriangles(unityMesh, indices32);
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
