#pragma once

#include <CesiumGltf/Model.h>

#include <DotNet/System/Array1.h>

namespace DotNet::CesiumForUnity {
class CesiumPropertyTableProperty;
class TestGltfModel;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
} // namespace DotNet::System

namespace DotNet::Unity::Mathematics {
struct float2;
struct float3;
struct float4;
struct float2x2;
struct float3x3;
struct float4x4;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class TestGltfModelImpl {
public:
  TestGltfModelImpl(const DotNet::CesiumForUnity::TestGltfModel& model);
  ~TestGltfModelImpl();

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddBooleanPropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<bool>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddIntPropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<std::int32_t>& values,
      bool normalized);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddDoublePropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<double>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddVec2PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float2>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddVec3PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float3>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddVec4PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float4>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddMat2PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float2x2>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddMat3PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float3x3>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddMat4PropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::float4x4>& values);

  DotNet::CesiumForUnity::CesiumPropertyTableProperty
  AddStringPropertyTableProperty(
      const DotNet::CesiumForUnity::TestGltfModel& model,
      const DotNet::System::Array1<DotNet::System::String>& values);

private:
  CesiumGltf::Model _nativeModel;
};

} // namespace CesiumForUnityNative
