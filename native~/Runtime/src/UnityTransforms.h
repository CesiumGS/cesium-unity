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
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

struct RotationAndScale {
  glm::dquat rotation;
  glm::dvec3 scale;
};

class UnityTransforms {
public:
  /**
   * @brief Break a 3x3 matrix into a rotation quaternion and a scale vector, as
   * required by Unity's Transform.
   *
   * The return value uses glm types to preserve precision, but these may be
   * easily converted to Unity types.
   *
   * The scale may be negative (i.e. when switching from a right-handed to a
   * left-handed system), but skew and other funny business will result in
   * undefined behavior.
   *
   * @param matrix The 3x3 matrix.
   * @returns The separate rotation and scale.
   */
  static RotationAndScale matrixToRotationAndScale(const glm::dmat3& matrix);

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
  UnityTransforms::toUnityMathematics(const glm::dvec3& vector);

  /**
   * Convert a double-precision GLM 4-component vector to a double-precision
   * Unity.Mathematics double4.
   */
  static DotNet::Unity::Mathematics::double4
  UnityTransforms::toUnityMathematics(const glm::dvec4& vector);

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
