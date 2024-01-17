#include "TestGltfModelImpl.h"

#include "CesiumFeaturesMetadataUtility.h"

#include <CesiumGltf/Buffer.h>
#include <CesiumGltf/PropertyTablePropertyView.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumPropertyArray.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>
#include <DotNet/System/Array1.h>
#include <DotNet/Unity/Mathematics/float2.h>
#include <DotNet/Unity/Mathematics/float2x2.h>
#include <DotNet/Unity/Mathematics/float3.h>
#include <DotNet/Unity/Mathematics/float3x3.h>
#include <DotNet/Unity/Mathematics/float4.h>
#include <DotNet/Unity/Mathematics/float4x4.h>

#include <string_view>

using namespace DotNet;

namespace CesiumForUnityNative {

TestGltfModelImpl::TestGltfModelImpl(
    const DotNet::CesiumForUnity::TestGltfModel& model)
    : _nativeModel(CesiumGltf::Model()) {}

TestGltfModelImpl::~TestGltfModelImpl() {}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddBooleanPropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<bool>& values) {
  int64_t size = values.Length();
  size_t numBytes =
      static_cast<size_t>(std::ceil(static_cast<float>(size) / 8.0f));
  std::vector<uint8_t> compressedValues;
  compressedValues.resize(numBytes);

  for (int64_t i = 0; i < size; i++) {
    bool value = values[i];
    size_t byteIndex = static_cast<size_t>(i / 8);
    int64_t bitIndex = i % 8;
    compressedValues[byteIndex] =
        static_cast<uint8_t>((value << bitIndex) | compressedValues[byteIndex]);
  }

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = compressedValues.size();
  buffer.cesium.data.resize(buffer.byteLength);
  std::memcpy(
      buffer.cesium.data.data(),
      compressedValues.data(),
      compressedValues.size());

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::BOOLEAN;

  CesiumGltf::PropertyTablePropertyView<bool> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddIntPropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<std::int32_t>& values,
    bool normalized) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(std::int32_t);
  buffer.cesium.data.resize(buffer.byteLength);
  std::int32_t* pValue =
      reinterpret_cast<std::int32_t*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    *pValue = values[i];
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::SCALAR;
  classProperty.componentType = CesiumGltf::ClassProperty::ComponentType::INT32;
  classProperty.normalized = normalized;

  if (!normalized) {
    CesiumGltf::PropertyTablePropertyView<std::int32_t, false> propertyView(
        property,
        classProperty,
        size,
        gsl::span<const std::byte>(
            buffer.cesium.data.data(),
            buffer.cesium.data.size()));

    return CesiumFeaturesMetadataUtility::makePropertyTableProperty(
        propertyView);
  }

  CesiumGltf::PropertyTablePropertyView<std::int32_t, true> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddDoublePropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<double>& values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(double);
  buffer.cesium.data.resize(buffer.byteLength);
  double* pValue = reinterpret_cast<double*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    *pValue = values[i];
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::SCALAR;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT64;

  CesiumGltf::PropertyTablePropertyView<double> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddVec2PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float2>& values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::vec2);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::vec2* pValue = reinterpret_cast<glm::vec2*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float2 float2Value = values[i];
    *pValue = glm::vec2(float2Value[0], float2Value[1]);
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::VEC2;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::vec2> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddVec3PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float3>& values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::vec3);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::vec3* pValue = reinterpret_cast<glm::vec3*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float3 float3Value = values[i];
    *pValue = glm::vec3(float3Value[0], float3Value[1], float3Value[2]);
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::VEC3;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::vec3> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddVec4PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float4>& values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::vec4);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::vec4* pValue = reinterpret_cast<glm::vec4*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float4 float4Value = values[i];
    *pValue = glm::vec4(
        float4Value[0],
        float4Value[1],
        float4Value[2],
        float4Value[3]);
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::VEC4;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::vec4> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddMat2PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float2x2>&
        values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::mat2);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::mat2* pValue = reinterpret_cast<glm::mat2*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float2x2 value = values[i];
    // clang-format off
    *pValue = glm::mat2(
        value[0][0], value[0][1],
        value[1][0], value[1][1]);
    // clang-format on
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::MAT2;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::mat2> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddMat3PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float3x3>&
        values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::mat3);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::mat3* pValue = reinterpret_cast<glm::mat3*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float3x3 value = values[i];
    // clang-format off
    *pValue = glm::mat3(
       value[0][0], value[0][1], value[0][2],
       value[1][0], value[1][1], value[1][2],
       value[2][0], value[2][1], value[2][2]);
    // clang-format on
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::MAT3;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::mat3> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddMat4PropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::float4x4>&
        values) {
  int64_t size = values.Length();

  CesiumGltf::Buffer& buffer = this->_nativeModel.buffers.emplace_back();
  buffer.byteLength = size * sizeof(glm::mat4);
  buffer.cesium.data.resize(buffer.byteLength);
  glm::mat4* pValue = reinterpret_cast<glm::mat4*>(buffer.cesium.data.data());
  for (int64_t i = 0; i < size; i++) {
    DotNet::Unity::Mathematics::float4x4 value = values[i];
    // clang-format off
    *pValue = glm::mat4(
        value[0][0], value[0][1], value[0][2], value[0][3],
        value[1][0], value[1][1], value[1][2], value[1][3],
        value[2][0], value[2][1], value[2][2], value[2][3],
        value[3][0], value[3][1], value[3][2], value[3][3]);
    // clang-format on
    pValue++;
  }

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::MAT4;
  classProperty.componentType =
      CesiumGltf::ClassProperty::ComponentType::FLOAT32;

  CesiumGltf::PropertyTablePropertyView<glm::mat4> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          buffer.cesium.data.data(),
          buffer.cesium.data.size()));

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

DotNet::CesiumForUnity::CesiumPropertyTableProperty
TestGltfModelImpl::AddStringPropertyTableProperty(
    const DotNet::CesiumForUnity::TestGltfModel& model,
    const DotNet::System::Array1<DotNet::System::String>& values) {
  int64_t size = values.Length();
  int64_t totalByteSize = 0;

  for (int64_t i = 0; i < size; i++) {
    totalByteSize += values[i].Length();
  }

  this->_nativeModel.buffers.emplace_back();
  size_t stringBufferIndex = this->_nativeModel.buffers.size() - 1;

  this->_nativeModel.buffers.emplace_back();
  size_t offsetBufferIndex = this->_nativeModel.buffers.size() - 1;

  CesiumGltf::Buffer& stringBuffer =
      this->_nativeModel.buffers[stringBufferIndex];
  stringBuffer.byteLength = totalByteSize;
  stringBuffer.cesium.data.resize(stringBuffer.byteLength);

  CesiumGltf::Buffer& offsetBuffer =
      this->_nativeModel.buffers[offsetBufferIndex];
  offsetBuffer.byteLength = ((size + 1) * sizeof(uint8_t));
  offsetBuffer.cesium.data.resize(offsetBuffer.byteLength);

  uint8_t currentOffset = 0;
  for (int64_t i = 0; i < size; i++) {
    std::string stlString = values[i].ToStlString();
    std::memcpy(
        stringBuffer.cesium.data.data() + currentOffset,
        stlString.data(),
        stlString.size());
    std::memcpy(
        offsetBuffer.cesium.data.data() + i * sizeof(uint8_t),
        &currentOffset,
        sizeof(uint8_t));
    currentOffset += static_cast<uint8_t>(stlString.size());
  }
  std::memcpy(
      offsetBuffer.cesium.data.data() + size * sizeof(uint8_t),
      &currentOffset,
      sizeof(uint8_t));

  CesiumGltf::PropertyTableProperty property;
  CesiumGltf::ClassProperty classProperty;
  classProperty.type = CesiumGltf::ClassProperty::Type::STRING;

  CesiumGltf::PropertyTablePropertyView<std::string_view> propertyView(
      property,
      classProperty,
      size,
      gsl::span<const std::byte>(
          stringBuffer.cesium.data.data(),
          stringBuffer.cesium.data.size()),
      gsl::span<const std::byte>(),
      gsl::span<const std::byte>(
          offsetBuffer.cesium.data.data(),
          offsetBuffer.cesium.data.size()),
      CesiumGltf::PropertyComponentType::None,
      CesiumGltf::PropertyComponentType::Uint8);

  return CesiumFeaturesMetadataUtility::makePropertyTableProperty(propertyView);
}

} // namespace CesiumForUnityNative
