#include "UnityPrepareRendererResources.h"

#include "TextureLoader.h"

#include <Cesium3DTilesSelection/GltfContent.h>
#include <Cesium3DTilesSelection/Tile.h>
#include <Cesium3DTilesSelection/TileContentLoadResult.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumGltf/AccessorView.h>

#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/System/Text/Encoding.h>
#include <DotNet/Unity/Collections/Allocator.h>
#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/Unity/Collections/NativeArrayOptions.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/Material.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/MeshFilter.h>
#include <DotNet/UnityEngine/MeshRenderer.h>
#include <DotNet/UnityEngine/MeshTopology.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector2.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <glm/gtc/matrix_inverse.hpp>
#include <glm/gtc/quaternion.hpp>

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnity;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace CesiumGltf;
using namespace DotNet;

namespace {

template <typename T>
void setTriangles(UnityEngine::Mesh& mesh, const AccessorView<T>& indices) {
  Unity::Collections::NativeArray1<std::int32_t> nativeArrayTriangles(
      indices.size(),
      Unity::Collections::Allocator::Temp,
      Unity::Collections::NativeArrayOptions::UninitializedMemory);
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

  nativeArrayTriangles.Dispose();
}

} // namespace

UnityPrepareRendererResources::UnityPrepareRendererResources(
    const UnityEngine::GameObject& tileset)
    : _tileset(tileset) {}

void* UnityPrepareRendererResources::prepareInLoadThread(
    const CesiumGltf::Model& model,
    const glm::dmat4& transform) {
  return nullptr;
}

void* UnityPrepareRendererResources::prepareInMainThread(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult) {
  TileContentLoadResult* pContent = tile.getContent();
  if (!pContent) {
    return nullptr;
  }

  if (!pContent->model) {
    return nullptr;
  }

  const QuadtreeTileID* pQ = std::get_if<QuadtreeTileID>(&tile.getTileID());
  if (pQ) {
    if (pQ->level >= 14) {
      UnityEngine::Debug::Log(System::String(">=14"));
    }
  }

  const Model& model = *pContent->model;

  std::string name = "glTF";
  auto urlIt = model.extras.find("Cesium3DTiles_TileUrl");
  if (urlIt != model.extras.end()) {
    name = urlIt->second.getStringOrDefault("glTF");
  }

  auto pModelGameObject = std::make_unique<UnityEngine::GameObject>(
      System::Text::Encoding::UTF8().GetString(
          reinterpret_cast<std::uint8_t*>(name.data()),
          name.size()));
  pModelGameObject->transform().parent(this->_tileset.transform());
  pModelGameObject->SetActive(false);

  glm::dmat4 tileTransform = tile.getTransform();
  tileTransform = GltfContent::applyRtcCenter(model, tileTransform);
  tileTransform = GltfContent::applyGltfUpAxisTransform(model, tileTransform);

  model.forEachPrimitiveInScene(
      -1,
      [&pModelGameObject, &tileTransform](
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

        // TODO: better name (index of mesh and primitive?)
        UnityEngine::GameObject primitiveGameObject(
            System::String("Primitive"));
        primitiveGameObject.transform().parent(pModelGameObject->transform());

        // Hard-coded "georeference" to put the Unity origin at a default
        // location in Melbourne and adjust for Unity left-handed, Y-up
        // convention.
        glm::dvec3 origin = Ellipsoid::WGS84.cartographicToCartesian(
            Cartographic::fromDegrees(144.96133, -37.81510, 2250.0));
        glm::dmat4 enuToFixed = Transforms::eastNorthUpToFixedFrame(origin);
        glm::dmat4 fixedToEnu = glm::affineInverse(enuToFixed);
        glm::dmat4 swapYandZ(
            glm::dvec4(1.0, 0.0, 0.0, 0.0),
            glm::dvec4(0.0, 0.0, 1.0, 0.0),
            glm::dvec4(0.0, 1.0, 0.0, 0.0),
            glm::dvec4(0.0, 0.0, 0.0, 1.0));

        glm::dmat4 fullTransform = fixedToEnu * (tileTransform * transform);
        fullTransform = swapYandZ * fullTransform;

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

        rotationMatrix[0] = glm::normalize(rotationMatrix[0]);
        rotationMatrix[1] = glm::normalize(rotationMatrix[1]);
        rotationMatrix[2] = glm::normalize(rotationMatrix[2]);

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

        UnityEngine::Matrix4x4 compare =
            primitiveGameObject.transform().localToWorldMatrix();

        UnityEngine::MeshFilter meshFilter =
            primitiveGameObject.AddComponent<UnityEngine::MeshFilter>();
        UnityEngine::MeshRenderer meshRenderer =
            primitiveGameObject.AddComponent<UnityEngine::MeshRenderer>();

        UnityEngine::Material sharedMaterial =
            UnityEngine::Resources::Load<UnityEngine::Material>(
                System::String("CesiumDefaultMaterial"));
        UnityEngine::Material material =
            UnityEngine::Object::Instantiate(sharedMaterial);
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
              material.SetTexture(System::String("_MainTex"), texture);
            }
          }
        }

        UnityEngine::Mesh unityMesh{};

        Unity::Collections::NativeArray1<UnityEngine::Vector3>
            nativeArrayVertices(
                positionView.size(),
                Unity::Collections::Allocator::Temp,
                Unity::Collections::NativeArrayOptions::UninitializedMemory);
        UnityEngine::Vector3* vertices = static_cast<UnityEngine::Vector3*>(
            Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                GetUnsafeBufferPointerWithoutChecks(nativeArrayVertices));
        for (int64_t i = 0; i < positionView.size(); ++i) {
          vertices[i] = positionView[i];
        }
        unityMesh.SetVertices<UnityEngine::Vector3>(nativeArrayVertices);
        nativeArrayVertices.Dispose();

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
            UnityEngine::Vector3* normals = static_cast<UnityEngine::Vector3*>(
                Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                    GetUnsafeBufferPointerWithoutChecks(nativeArrayNormals));

            for (int64_t i = 0; i < normalView.size(); ++i) {
              normals[i] = normalView[i];
            }
            unityMesh.SetNormals(nativeArrayNormals);
            nativeArrayNormals.Dispose();
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
            UnityEngine::Vector2* texCoord0s = static_cast<
                UnityEngine::Vector2*>(
                Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
                    GetUnsafeBufferPointerWithoutChecks(nativeArrayTexCoord0s));

            for (int64_t i = 0; i < texCoord0View.size(); ++i) {
              texCoord0s[i] = texCoord0View[i];
            }
            unityMesh.SetUVs(0, nativeArrayTexCoord0s);
            nativeArrayTexCoord0s.Dispose();
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
      });

  return pModelGameObject.release();
}

void UnityPrepareRendererResources::free(
    Cesium3DTilesSelection::Tile& tile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {
  if (pMainThreadResult) {
    UnityEngine::GameObject* pGameObject =
        static_cast<UnityEngine::GameObject*>(pMainThreadResult);

    // In the Editor, we must use DestroyImmediate because Destroy won't
    // actually destroy the object.
    if (UnityEngine::Application::isEditor())
      UnityEngine::Object::DestroyImmediate(*pGameObject);
    else
      UnityEngine::Object::Destroy(*pGameObject);

    delete pGameObject;
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
  return nullptr;
}

void UnityPrepareRendererResources::freeRaster(
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pLoadThreadResult,
    void* pMainThreadResult) noexcept {}

void UnityPrepareRendererResources::attachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources,
    const glm::dvec2& translation,
    const glm::dvec2& scale) {}

void UnityPrepareRendererResources::detachRasterInMainThread(
    const Cesium3DTilesSelection::Tile& tile,
    int32_t overlayTextureCoordinateID,
    const Cesium3DTilesSelection::RasterOverlayTile& rasterTile,
    void* pMainThreadRendererResources) noexcept {}
