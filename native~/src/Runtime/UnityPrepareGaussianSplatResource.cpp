#include "UnityPrepareGaussianSplatResource.h"

#include "float16.h"

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/PropertyType.h>

#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Mathematics/uint4.h>
#include <DotNet/UnityEngine/Bounds.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <DotNet/UnityEngine/Vector4.h>
#include <fmt/format.h>
#include <glm/ext/quaternion_float.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>
#include <glm/vec3.hpp>

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <limits>
#include <numbers>
#include <tuple>
#include <unordered_map>
#include <utility>
#include <vector>

using namespace DotNet;

namespace CesiumForUnityNative {

namespace {
std::tuple<int32_t, int32_t>
countShCoeffsOnPrimitive(const CesiumGltf::MeshPrimitive& primitive) {
  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_3_COEF_6")) {
    return {3, 15};
  }

  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_2_COEF_4")) {
    return {2, 8};
  }

  if (primitive.attributes.contains(
          "KHR_gaussian_splatting:SH_DEGREE_1_COEF_2")) {
    return {1, 3};
  }

  return {0, 0};
}

bool writeShCoeffs(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    std::vector<float>& data,
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
      int32_t base = (j * stride + offset + i) * 3;
      data[base] = accessorView[j].x;
      data[base + 1] = accessorView[j].y;
      data[base + 2] = accessorView[j].z;
    }
  }

  return true;
}

uint32_t PackSint8Bytes(float b0, float b1, float b2, float b3) {
  int8_t i0 =
      static_cast<int8_t>(std::clamp(std::round(b0 * 127.0f), -127.0f, 127.0f));
  int8_t i1 =
      static_cast<int8_t>(std::clamp(std::round(b1 * 127.0f), -127.0f, 127.0f));
  int8_t i2 =
      static_cast<int8_t>(std::clamp(std::round(b2 * 127.0f), -127.0f, 127.0f));
  int8_t i3 =
      static_cast<int8_t>(std::clamp(std::round(b3 * 127.0f), -127.0f, 127.0f));
  uint8_t u0 = static_cast<uint8_t>(i0 & 0xff);
  uint8_t u1 = static_cast<uint8_t>(i1 & 0xff);
  uint8_t u2 = static_cast<uint8_t>(i2 & 0xff);
  uint8_t u3 = static_cast<uint8_t>(i3 & 0xff);
  return (u0 << 0) | (u1 << 8) | (u2 << 16) | (u3 << 24);
}

template <typename T> float fromNormalizedAccessor(T value);

template <> float fromNormalizedAccessor(float value) { return value; }
template <> float fromNormalizedAccessor(uint16_t value) {
  return std::clamp(static_cast<float>(value) / 65535.0f, 0.0f, 1.0f);
}
template <> float fromNormalizedAccessor(uint8_t value) {
  return std::clamp(static_cast<float>(value) / 255.0f, 0.0f, 1.0f);
}
template <> float fromNormalizedAccessor(int16_t value) {
  return std::clamp(static_cast<float>(value) / 32767.0f, -1.0f, 1.0f);
}
template <> float fromNormalizedAccessor(int8_t value) {
  return std::clamp(static_cast<float>(value) / 127.0f, -1.0f, 1.0f);
}

struct Uint4 {
  uint32_t x;
  uint32_t y;
  uint32_t z;
  uint32_t w;
};

template <typename VectorT, typename ComponentT>
bool writeColors(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    const CesiumGltf::Accessor& colorAccessor,
    std::vector<Uint4>& splatData) {
  const CesiumGltf::AccessorView<VectorT> colorView(model, colorAccessor);

  if (colorView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'COLOR_0' accessor view on mesh primitive returned invalid "
                "status: {}",
                (int32_t)colorView.status())));
    return false;
  }

  if (colorView.size() != splatData.size()) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'COLOR_0' accessor view on mesh primitive has size {} but "
                "expected {}",
                colorView.size(),
                splatData.size())));
    return false;
  }

  for (int32_t i = 0; i < colorView.size(); i++) {
    const VectorT& color = colorView[i];
    const float r = fromNormalizedAccessor(color.x);
    const float g = fromNormalizedAccessor(color.y);
    const float b = fromNormalizedAccessor(color.z);
    const float a = fromNormalizedAccessor(color.w);

    const uint8_t uR =
        static_cast<uint8_t>(std::clamp(std::round(r * 255.0f), 0.0f, 255.0f));
    const uint8_t uG =
        static_cast<uint8_t>(std::clamp(std::round(g * 255.0f), 0.0f, 255.0f));
    const uint8_t uB =
        static_cast<uint8_t>(std::clamp(std::round(b * 255.0f), 0.0f, 255.0f));
    const uint8_t uA =
        static_cast<uint8_t>(std::clamp(std::round(a * 255.0f), 0.0f, 255.0f));
    splatData[i].x =
        static_cast<uint32_t>(uR) | (static_cast<uint32_t>(uG) << 8) |
        (static_cast<uint32_t>(uB) << 16) | (static_cast<uint32_t>(uA) << 24);
  }

  return true;
}

template <typename VectorT, typename ComponentT>
bool writeRotations(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    const CesiumGltf::Accessor& rotationAccessor,
    std::vector<Uint4>& splatData) {
  const CesiumGltf::AccessorView<VectorT> rotationView(model, rotationAccessor);

  if (rotationView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:ROTATION' accessor view on mesh "
                "primitive returned invalid status: {}",
                (int32_t)rotationView.status())));
    return false;
  }

  if (rotationView.size() != splatData.size()) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:ROTATION' accessor view on mesh "
                "primitive has size {} but expected {}",
                rotationView.size(),
                splatData.size())));
    return false;
  }

  for (int32_t i = 0; i < rotationView.size(); i++) {
    // Adaptation of the EncodeQuatOctXy88R8 method from gsplat-unity's
    // GsplatAssetSpark class (used under MIT license).
    glm::fquat q = glm::normalize(
        glm::fquat(
            fromNormalizedAccessor(rotationView[i].z),
            fromNormalizedAccessor(rotationView[i].w),
            fromNormalizedAccessor(rotationView[i].x),
            fromNormalizedAccessor(rotationView[i].y)));
    if (q.w < 0.0f) {
      q = -q;
    }

    float xyz_norm = std::sqrtf(q.x * q.x + q.y * q.y + q.z * q.z);
    double theta = 2.0 * std::acos((double)q.w);
    glm::fvec3 axis = xyz_norm < 1e-6f ? glm::fvec3(1.0f, 0.0f, 0.0f)
                                       : (glm::fvec3(q.x, q.y, q.z) / xyz_norm);

    float sum = std::fabsf(axis.x) + std::fabsf(axis.y) + std::fabsf(axis.z);
    float p_x = axis.x / sum;
    float p_y = axis.y / sum;

    if (axis.z < 0) {
      float tmp = p_x;
      p_x = (1.0f - std::fabsf(p_y)) * (p_x >= 0.0f ? 1.0f : -1.0f);
      p_y = (1.0f - std::fabsf(tmp)) * (p_y >= 0.0f ? 1.0f : -1.0f);
    }

    float u_f = p_x * 0.5f + 0.5f;
    float v_f = p_y * 0.5f + 0.5f;

    uint8_t quantU = static_cast<uint8_t>(
        std::clamp(std::roundf(u_f * 255.0f), 0.0f, 255.0f));
    uint8_t quantV = static_cast<uint8_t>(
        std::clamp(std::roundf(v_f * 255.0f), 0.0f, 255.0f));
    uint8_t angleInt = static_cast<uint8_t>(std::clamp(
        std::roundf((theta / std::numbers::pi_v<float>)*255.0f),
        0.0f,
        255.0f));

    splatData[i].z |= static_cast<uint32_t>(quantU << 16) |
                      static_cast<uint32_t>(quantV << 24);
    splatData[i].w |= static_cast<uint32_t>(angleInt << 24);
  }
  return true;
}

const float LN_SCALE_MIN = -12.0f;
const float LN_SCALE_MAX = 9.0f;
const float LN_SCALE_ZERO = -30.0f;
const float LN_SCALE_SCALE = 254.0f / (LN_SCALE_MAX - LN_SCALE_MIN);
const float SCALE_ZERO = std::expf(LN_SCALE_ZERO);

uint8_t encodeScale(float scale) {
  if (scale < SCALE_ZERO) {
    return 0.0f;
  }

  float encoded = ((std::log(scale) - LN_SCALE_MIN) * LN_SCALE_SCALE) + 1.0f;
  return static_cast<uint8_t>(std::clamp(std::round(encoded), 1.0f, 255.0f));
}

template <typename VectorT, typename ComponentT>
bool writeScales(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    const CesiumGltf::Accessor& scaleAccessor,
    std::vector<Uint4>& splatData) {
  const CesiumGltf::AccessorView<VectorT> scaleView(model, scaleAccessor);

  if (scaleView.status() != CesiumGltf::AccessorViewStatus::Valid) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:SCALE' accessor view on mesh "
                "primitive returned invalid status: {}",
                (int32_t)scaleView.status())));
    return false;
  }

  if (scaleView.size() != splatData.size()) {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:SCALE' accessor view on mesh "
                "primitive has size {} but expected {}",
                scaleView.size(),
                splatData.size())));
    return false;
  }

  for (int32_t i = 0; i < scaleView.size(); i++) {
    const VectorT& scale = scaleView[i];
    const float sX = fromNormalizedAccessor(scale.x);
    const float sY = fromNormalizedAccessor(scale.y);
    const float sZ = fromNormalizedAccessor(scale.z);
    const uint8_t uSx = encodeScale(std::exp(sX));
    const uint8_t uSy = encodeScale(std::exp(sY));
    const uint8_t uSz = encodeScale(std::exp(sZ));
    splatData[i].w |= static_cast<uint32_t>(uSx) |
                      (static_cast<uint32_t>(uSy) << 8) |
                      (static_cast<uint32_t>(uSz) << 16);
  }

  return true;
}

} // namespace

void populateGsplatAsset(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& meshPrimitive,
    DotNet::Gsplat::GsplatAssetSpark& asset) {

  const auto [numShBands, numShCoeffs] =
      countShCoeffsOnPrimitive(meshPrimitive);
  asset.SHBands(numShBands);

  // We need to get:
  // - Color as a uint32
  // - PosXYZ as three shorts
  // - rotation as U, V, angle
  // - ScaleXYZ as three bytes

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
  std::vector<Uint4> splatData(positionView.size());
  DotNet::System::Array1<DotNet::Unity::Mathematics::uint4> splats(
      positionView.size());

  if (positionView.size() == 0) {
    // Just ignore it.
    return;
  }

  // We are going to do some very precise bit twiddling to put the components
  // into the `uint4` array in a way that's generic across
  // component types and doesn't require intermediate copies.
  // The structure of each `uint4` for a splat is:
  // x: r8g8b8a8
  // y: posx16posy16
  // z: posz16rotu8rotv8
  // w: scalex8scaley8scalez8rotang8

  // Let's start by writing the positions
  glm::vec3 min = positionView[0];
  glm::vec3 max = positionView[0];
  for (int32_t i = 0; i < positionView.size(); i++) {
    uint16_t posX =
        float_to_half(*reinterpret_cast<const uint32_t*>(&positionView[i].x));
    uint16_t posY =
        float_to_half(*reinterpret_cast<const uint32_t*>(&positionView[i].y));
    uint16_t posZ =
        float_to_half(*reinterpret_cast<const uint32_t*>(&positionView[i].z));

    splatData[i].y =
        static_cast<uint32_t>(posX) | (static_cast<uint32_t>(posY) << 16);
    // We will fill in the rotation values later
    splatData[i].z = static_cast<uint32_t>(posZ);

    min = glm::min(min, positionView[i]);
    max = glm::max(max, positionView[i]);
  }

  UnityEngine::Bounds finalBounds = UnityEngine::Bounds::Construct(
      UnityEngine::Vector3(
          (min.x + max.x) * 0.5f,
          (min.y + max.y) * 0.5f,
          (min.z + max.z) * 0.5f),
      UnityEngine::Vector3(max.x - min.x, max.y - min.y, max.z - min.z));
  asset.Bounds(finalBounds);

  // Now handle colors.
  // TODO: handle SH_DEGREE_0_COEF_0 and OPACITY if present - these should take
  // priority over COLOR_0 if present.

  const std::unordered_map<std::string, int32_t>::const_iterator colorIt =
      meshPrimitive.attributes.find("COLOR_0");
  if (colorIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String("Mesh primitive has no 'COLOR_0' attribute"));
    return;
  }

  CesiumGltf::Accessor colorAccessor =
      CesiumGltf::Model::getSafe(model.accessors, colorIt->second);
  if (colorAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::FLOAT) {
    if (!writeColors<glm::vec4, float>(
            model,
            meshPrimitive,
            colorAccessor,
            splatData)) {
      return;
    }
  } else if (
      colorAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE) {
    if (!writeColors<glm::u8vec4, uint8_t>(
            model,
            meshPrimitive,
            colorAccessor,
            splatData)) {
      return;
    }
  } else if (
      colorAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT) {
    if (!writeColors<glm::u16vec4, uint16_t>(
            model,
            meshPrimitive,
            colorAccessor,
            splatData)) {
      return;
    }
  } else {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'COLOR_0' accessor on mesh primitive has unsupported "
                "component type: {}",
                CesiumGltf::convertPropertyComponentTypeToString(
                    CesiumGltf::
                        convertAccessorComponentTypeToPropertyComponentType(
                            colorAccessor.componentType)))));
    return;
  }

  const std::unordered_map<std::string, int32_t>::const_iterator rotationIt =
      meshPrimitive.attributes.find("KHR_gaussian_splatting:ROTATION");
  if (rotationIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String(
            "Mesh primitive has no 'KHR_gaussian_splatting:ROTATION' "
            "attribute"));
    return;
  }

  CesiumGltf::Accessor rotationAccessor =
      CesiumGltf::Model::getSafe(model.accessors, rotationIt->second);
  if (rotationAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::FLOAT) {
    if (!writeRotations<glm::vec4, float>(
            model,
            meshPrimitive,
            rotationAccessor,
            splatData)) {
      return;
    }
  } else if (
      rotationAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::BYTE) {
    if (!writeRotations<glm::i8vec4, int8_t>(
            model,
            meshPrimitive,
            rotationAccessor,
            splatData)) {
      return;
    }
  } else if (
      rotationAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::SHORT) {
    if (!writeRotations<glm::i16vec4, int16_t>(
            model,
            meshPrimitive,
            rotationAccessor,
            splatData)) {
      return;
    }
  } else {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:ROTATION' accessor on mesh primitive "
                "has unsupported "
                "component type: {}",
                CesiumGltf::convertPropertyComponentTypeToString(
                    CesiumGltf::
                        convertAccessorComponentTypeToPropertyComponentType(
                            rotationAccessor.componentType)))));
    return;
  }

  const std::unordered_map<std::string, int32_t>::const_iterator scaleIt =
      meshPrimitive.attributes.find("KHR_gaussian_splatting:SCALE");
  if (scaleIt == meshPrimitive.attributes.end()) {
    UnityEngine::Debug::LogError(
        System::String(
            "Mesh primitive has no 'KHR_gaussian_splatting:SCALE' attribute"));
    return;
  }

  CesiumGltf::Accessor scaleAccessor =
      CesiumGltf::Model::getSafe(model.accessors, scaleIt->second);
  if (scaleAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::FLOAT) {
    if (!writeScales<glm::vec3, float>(
            model,
            meshPrimitive,
            scaleAccessor,
            splatData)) {
      return;
    }
  } else if (
      scaleAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE) {
    if (!writeScales<glm::u8vec3, uint8_t>(
            model,
            meshPrimitive,
            scaleAccessor,
            splatData)) {
      return;
    }
  } else if (
      scaleAccessor.componentType ==
      CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT) {
    if (!writeScales<glm::u16vec3, uint16_t>(
            model,
            meshPrimitive,
            scaleAccessor,
            splatData)) {
      return;
    }
  } else {
    UnityEngine::Debug::LogError(
        System::String(
            fmt::format(
                "'KHR_gaussian_splatting:SCALE' accessor on mesh primitive has "
                "unsupported "
                "component type: {}",
                CesiumGltf::convertPropertyComponentTypeToString(
                    CesiumGltf::
                        convertAccessorComponentTypeToPropertyComponentType(
                            scaleAccessor.componentType)))));
    return;
  }

  for (int32_t i = 0; i < splats.Length(); i++) {
    splats.Item(
        i,
        Unity::Mathematics::uint4(
            splatData[i].x,
            splatData[i].y,
            splatData[i].z,
            splatData[i].w));
  }

  asset.PackedSplats(splats);

  // TODO: avoid copy
  std::vector<float> harmonics(numShCoeffs * asset.SplatCount() * 3);
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

  if (numShCoeffs >= 3) {
    // pack SH1
    System::Array1<uint32_t> sh1(asset.SplatCount() * 2);
    for (int32_t i = 0; i < asset.SplatCount(); i++) {
      uint32_t out[2] = {0, 0};
      for (int32_t j = 0; j < 3; j++) {
        for (int32_t k = 0; k < 3; k++) {
          const int32_t baseIndex = (i * numShCoeffs + j) * 3 + k;
          float shScaled = harmonics[baseIndex] * 63.0f;
          int shBounded = (int)std::round(std::clamp(shScaled, -63.0f, 63.0f));
          int sint7SH = shBounded & 0x7f;

          int bitStart = (j * 3 + k) * 7;
          int bitEnd = bitStart + 7;

          int wordStart = (int)std::floor((double)(bitStart / 32));
          int bitOffset = bitStart - wordStart * 32;
          uint32_t firstWord = (uint32_t)((sint7SH << bitOffset) & 0xffffffff);
          out[wordStart] |= firstWord;

          if (bitEnd > wordStart * 32 + 32) {
            uint32_t secondWord =
                ((uint32_t)sint7SH >> (32 - bitOffset)) & 0xffffffff;
            out[wordStart + 1] |= secondWord;
          }
        }
      }
      sh1.Item(i * 2, out[0]);
      sh1.Item(i * 2 + 1, out[1]);
    }
    asset.PackedSH1(sh1);
  }

  if (numShCoeffs >= 8) {
    System::Array1<uint32_t> sh2(asset.SplatCount() * 4);
    for (int32_t i = 0; i < asset.SplatCount(); i++) {
      const int32_t baseIndex = (i * numShCoeffs + 3) * 3;
      uint32_t a = PackSint8Bytes(
          harmonics[baseIndex + 0],
          harmonics[baseIndex + 1],
          harmonics[baseIndex + 2],
          harmonics[baseIndex + 3]);
      uint32_t b = PackSint8Bytes(
          harmonics[baseIndex + 4],
          harmonics[baseIndex + 5],
          harmonics[baseIndex + 6],
          harmonics[baseIndex + 7]);
      uint32_t c = PackSint8Bytes(
          harmonics[baseIndex + 8],
          harmonics[baseIndex + 9],
          harmonics[baseIndex + 10],
          harmonics[baseIndex + 11]);
      uint32_t d = PackSint8Bytes(
          harmonics[baseIndex + 12],
          harmonics[baseIndex + 13],
          harmonics[baseIndex + 14],
          0);

      sh2.Item(i * 4, a);
      sh2.Item(i * 4 + 1, b);
      sh2.Item(i * 4 + 2, c);
      sh2.Item(i * 4 + 3, d);
    }
    asset.PackedSH2(sh2);
  }

  if (numShCoeffs >= 15) {
    System::Array1<uint32_t> sh3(asset.SplatCount() * 4);
    for (int32_t i = 0; i < asset.SplatCount(); i++) {
      uint32_t out[4] = {0, 0, 0, 0};
      for (int32_t j = 0; j < 7; j++) {
        for (int32_t k = 0; k < 3; k++) {
          const int32_t baseIndex = (i * numShCoeffs + j + 8) * 3 + k;
          float shScaled = harmonics[baseIndex] * 31.0f;
          int shBounded = (int)std::round(std::clamp(shScaled, -31.0f, 31.0f));
          int sint6SH = shBounded & 0x3f;
          int bitStart = (j * 3 + k) * 6;
          int bitEnd = bitStart + 6;

          int wordStart = (int)std::floor((double)(bitStart / 32));
          int bitOffset = bitStart - wordStart * 32;
          uint32_t firstWord = (uint32_t)((sint6SH << bitOffset) & 0xffffffff);

          out[wordStart] |= firstWord;
          if (bitEnd > wordStart * 32 + 32) {
            uint32_t secondWord =
                ((uint32_t)sint6SH >> (32 - bitOffset)) & 0xffffffff;
            out[wordStart + 1] |= secondWord;
          }
        }
      }
      sh3.Item(i * 4, out[0]);
      sh3.Item(i * 4 + 1, out[1]);
      sh3.Item(i * 4 + 2, out[2]);
      sh3.Item(i * 4 + 3, out[3]);
    }
    asset.PackedSH3(sh3);
  }
}

} // namespace CesiumForUnityNative
