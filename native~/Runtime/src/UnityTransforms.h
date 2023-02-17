#pragma once

#include <glm/gtc/quaternion.hpp>
#include <glm/mat3x3.hpp>

namespace DotNet::UnityEngine {
struct Matrix4x4;
struct Quaternion;
struct Vector3;
} // namespace DotNet::UnityEngine

namespace DotNet::Unity::Mathematics {
struct double3;
struct double4;
struct double3x3;
struct double4x4;
struct quaternion;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class UnityTransforms {
public:
  /**
   * @brief Convert a 3-component, double-precision GLM vector to a 3-component,
   * single-precision Unity Vector3.
   */
  static DotNet::UnityEngine::Vector3 toUnity(const glm::dvec3& vector);

  /**
   * @brief Convert a double-precision GLM quaternion to a single-precision
   * Unity Quaternion.
   */
  static DotNet::UnityEngine::Quaternion toUnity(const glm::dquat& quaternion);

  /**
   * @brief Convert a double-precision GLM 4x4 matrix to a single-precision
   * Unity Matrix4x4.
   */
  static DotNet::UnityEngine::Matrix4x4 toUnity(const glm::dmat4& matrix);

  /**
   * Convert a double-precision GLM 3-component vector to a double-precision
   * Unity.Mathematics double3.
   */
  static DotNet::Unity::Mathematics::double3
  toUnityMathematics(const glm::dvec3& vector);

  /**
   * Convert a double-precision GLM 4-component vector to a double-precision
   * Unity.Mathematics double4.
   */
  static DotNet::Unity::Mathematics::double4
  toUnityMathematics(const glm::dvec4& vector);

  /**
   * Converts a double-precision GLM quaterion to a single-precision
   * Unity.Mathematics quaternion.
   */
  static DotNet::Unity::Mathematics::quaternion
  toUnityMathematics(const glm::dquat& quaternion);

  /**
   * @brief Convert a double-precision GLM 3x3 matrix to a double-precision
   * Unity.Mathematics double3x3.
   */
  static DotNet::Unity::Mathematics::double3x3
  toUnityMathematics(const glm::dmat3& matrix);

  /**
   * @brief Convert a double-precision GLM 4x4 matrix to a double-precision
   * Unity.Mathematics double3x3.
   */
  static DotNet::Unity::Mathematics::double4x4
  toUnityMathematics(const glm::dmat4& matrix);

  /**
   * @brief Convert a 3-component, single-precision Unity Vector3 to a
   * 3-component, double-precision GLM vector.
   */
  static glm::dvec3 fromUnity(const DotNet::UnityEngine::Vector3& vector);

  /**
   * @brief Convert a 3-component, double-precision Unity double3 to a
   * 3-component, double-precision GLM vector.
   */
  static glm::dvec3
  fromUnity(const DotNet::Unity::Mathematics::double3& vector);

  /**
   * @brief Convert a 4-component, double-precision Unity double4 to a
   * 4-component, double-precision GLM vector.
   */
  static glm::dvec4
  fromUnity(const DotNet::Unity::Mathematics::double4& vector);

  /**
   * @brief Convert a single-precision Unity Quaternion to a
   * double-precision GLM quaternion.
   */
  static glm::dquat
  fromUnity(const DotNet::UnityEngine::Quaternion& quaternion);

  /**
   * @brief Convert a single-precision Unity.Mathematics quaternion to a
   * double-precision GLM quaternion.
   */
  static glm::dquat
  fromUnity(const DotNet::Unity::Mathematics::quaternion& quaternion);

  /**
   * @brief Convert a single-precision Unity Matrix4x4 to a double-precision
   * GLM 4x4 matrix.
   */
  static glm::dmat4 fromUnity(const DotNet::UnityEngine::Matrix4x4& matrix);

  /**
   * @brief Convert a double-precision Unity double4x4 to a double-precision
   * GLM 4x4 matrix.
   */
  static glm::dmat4
  fromUnity(const DotNet::Unity::Mathematics::double4x4& matrix);

  /**
   * @brief Convert a double-precision Unity double4x4 to a double-precision
   * GLM 3x3 matrix by ignoring the last row and last column.
   */
  static glm::dmat3
  fromUnity3x3(const DotNet::Unity::Mathematics::double4x4& matrix);
};

} // namespace CesiumForUnityNative
