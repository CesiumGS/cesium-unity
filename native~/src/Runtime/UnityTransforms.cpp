#include "UnityTransforms.h"

#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/Unity/Mathematics/double3x3.h>
#include <DotNet/Unity/Mathematics/double4x4.h>
#include <DotNet/Unity/Mathematics/quaternion.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Vector3.h>

namespace CesiumForUnityNative {

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

DotNet::Unity::Mathematics::double4x4
UnityTransforms::toUnityMathematics(const glm::dmat4& matrix) {
  return DotNet::Unity::Mathematics::double4x4{
      toUnityMathematics(matrix[0]),
      toUnityMathematics(matrix[1]),
      toUnityMathematics(matrix[2]),
      toUnityMathematics(matrix[3])};
}

DotNet::Unity::Mathematics::double3
UnityTransforms::toUnityMathematics(const glm::dvec3& vector) {
  return DotNet::Unity::Mathematics::double3{vector.x, vector.y, vector.z};
}

DotNet::Unity::Mathematics::double4
UnityTransforms::toUnityMathematics(const glm::dvec4& vector) {
  return DotNet::Unity::Mathematics::double4{
      vector.x,
      vector.y,
      vector.z,
      vector.w};
}

DotNet::Unity::Mathematics::quaternion
UnityTransforms::toUnityMathematics(const glm::dquat& quaternion) {
  return DotNet::Unity::Mathematics::quaternion{
      (float)quaternion.x,
      (float)quaternion.y,
      (float)quaternion.z,
      (float)quaternion.w};
}

glm::dvec3
UnityTransforms::fromUnity(const DotNet::UnityEngine::Vector3& vector) {
  return glm::dvec3(vector.x, vector.y, vector.z);
}

glm::dvec3
UnityTransforms::fromUnity(const DotNet::Unity::Mathematics::double3& vector) {
  return glm::dvec3(vector.x, vector.y, vector.z);
}

glm::dvec4
UnityTransforms::fromUnity(const DotNet::Unity::Mathematics::double4& vector) {
  return glm::dvec4(vector.x, vector.y, vector.z, vector.w);
}

glm::dquat
UnityTransforms::fromUnity(const DotNet::UnityEngine::Quaternion& quaternion) {
  return glm::dquat(quaternion.w, quaternion.x, quaternion.y, quaternion.z);
}

glm::dquat UnityTransforms::fromUnity(
    const DotNet::Unity::Mathematics::quaternion& quaternion) {
  return glm::dquat(
      quaternion.value.w,
      quaternion.value.x,
      quaternion.value.y,
      quaternion.value.z);
}

glm::dmat4
UnityTransforms::fromUnity(const DotNet::UnityEngine::Matrix4x4& matrix) {
  return glm::dmat4(
      glm::dvec4(matrix.m00, matrix.m10, matrix.m20, matrix.m30),
      glm::dvec4(matrix.m01, matrix.m11, matrix.m21, matrix.m31),
      glm::dvec4(matrix.m02, matrix.m12, matrix.m22, matrix.m32),
      glm::dvec4(matrix.m03, matrix.m13, matrix.m23, matrix.m33));
}

glm::dmat4 UnityTransforms::fromUnity(
    const DotNet::Unity::Mathematics::double4x4& matrix) {
  return glm::dmat4(
      fromUnity(matrix.c0),
      fromUnity(matrix.c1),
      fromUnity(matrix.c2),
      fromUnity(matrix.c3));
}

glm::dmat3 UnityTransforms::fromUnity3x3(
    const DotNet::Unity::Mathematics::double4x4& matrix) {
  return glm::dmat3(
      glm::dvec3(matrix.c0.x, matrix.c0.y, matrix.c0.z),
      glm::dvec3(matrix.c1.x, matrix.c1.y, matrix.c1.z),
      glm::dvec3(matrix.c2.x, matrix.c2.y, matrix.c2.z));
}

} // namespace CesiumForUnityNative
