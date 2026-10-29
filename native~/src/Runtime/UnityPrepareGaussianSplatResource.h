#pragma once

#include <CesiumGltf/MeshPrimitive.h>
#include <CesiumGltf/Model.h>

#include <DotNet/Gsplat/GsplatAssetSpark.h>

namespace CesiumForUnityNative {
/**
 * @brief Populates a DotNet::Gsplat::GsplatAssetSpark with data from a
 * CesiumGltf::Model and CesiumGltf::MeshPrimitive with the
 * KHR_gaussian_splatting extension.
 *
 * @param model The CesiumGltf::Model containing the mesh primitive.
 * @param meshPrimitive The CesiumGltf::MeshPrimitive to extract data from.
 * @param asset The DotNet::Gsplat::GsplatAssetSpark to populate with data.
 */
void populateGsplatAsset(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    DotNet::Gsplat::GsplatAssetSpark& asset);
} // namespace CesiumForUnityNative
