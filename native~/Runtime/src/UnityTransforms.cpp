#include "UnityTransforms.h"

#include <DotNet/Unity/Mathematics/double3x3.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Vector3.h>

namespace CesiumForUnityNative {

RotationAndScale
UnityTransforms::matrixToRotationAndScale(const glm::dmat3& matrix) {
  double lengthColumn0 = glm::length(matrix[0]);
  double lengthColumn1 = glm::length(matrix[1]);
  double lengthColumn2 = glm::length(matrix[2]);

  glm::dmat3 rotationMatrix(
      matrix[0] / lengthColumn0,
      matrix[1] / lengthColumn1,
      matrix[2] / lengthColumn2);

  glm::dvec3 scale(lengthColumn0, lengthColumn1, lengthColumn2);

  glm::dvec3 cross = glm::cross(matrix[0], matrix[1]);
  if (glm::dot(cross, matrix[2]) < 0.0) {
    rotationMatrix *= -1.0;
    scale *= -1.0;
  }

  glm::dquat rotation = glm::quat_cast(rotationMatrix);

  return RotationAndScale{rotation, scale};
}

DotNet::UnityEngine::Vector3
UnityTransforms::toUnity(const glm::dvec3& vector) {
  return DotNet::UnityEngine::Vector3{
      float(vector.x),
      float(vector.y),
      float(vector.z)};
}

DotNet::UnityEngine::Quaternion
UnityTransforms::toUnity(const glm::dquat& quaternion) {
  return DotNet::UnityEngine::Quaternion{
      float(quaternion.x),
      float(quaternion.y),
      float(quaternion.z),
      float(quaternion.w)};
}

DotNet::UnityEngine::Matrix4x4
UnityTransforms::toUnity(const glm::dmat4& matrix) {
  return DotNet::UnityEngine::Matrix4x4{
      float(matrix[0].x),
      float(matrix[0].y),
      float(matrix[0].z),
      float(matrix[0].w),
      float(matrix[1].x),
      float(matrix[1].y),
      float(matrix[1].z),
      float(matrix[1].w),
      float(matrix[2].x),
      float(matrix[2].y),
      float(matrix[2].z),
      float(matrix[2].w),
      float(matrix[3].x),
      float(matrix[3].y),
      float(matrix[3].z),
      float(matrix[3].w),
  };
}

DotNet::Unity::Mathematics::double3x3
UnityTransforms::toUnityMathematics(const glm::dmat3& matrix) {
  return DotNet::Unity::Mathematics::double3x3{
      matrix[0].x,
      matrix[0].y,
      matrix[0].z,
      matrix[1].x,
      matrix[1].y,
      matrix[1].z,
      matrix[2].x,
      matrix[2].y,
      matrix[2].z,
  };
}

glm::dvec3
UnityTransforms::fromUnity(const DotNet::UnityEngine::Vector3& vector) {
  return glm::dvec3(vector.x, vector.y, vector.z);
}

glm::dquat
UnityTransforms::fromUnity(const DotNet::UnityEngine::Quaternion& quaternion) {
  return glm::dquat(quaternion.w, quaternion.x, quaternion.y, quaternion.z);
}

glm::dmat4
UnityTransforms::fromUnity(const DotNet::UnityEngine::Matrix4x4& matrix) {
  return glm::dmat4(
      glm::dvec4(matrix.m00, matrix.m10, matrix.m20, matrix.m30),
      glm::dvec4(matrix.m01, matrix.m11, matrix.m21, matrix.m31),
      glm::dvec4(matrix.m02, matrix.m12, matrix.m22, matrix.m32),
      glm::dvec4(matrix.m03, matrix.m13, matrix.m23, matrix.m33));
}

} // namespace CesiumForUnityNative
