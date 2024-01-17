
using Reinterop;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::TestGltfModelImpl", "TestGltfModelImpl.h")]
    internal partial class TestGltfModel
    {
        public partial CesiumPropertyTableProperty AddBooleanPropertyTableProperty(
            bool[] values);

        public partial CesiumPropertyTableProperty AddIntPropertyTableProperty(
            int[] values, bool normalized = false);

        public partial CesiumPropertyTableProperty AddDoublePropertyTableProperty(
            double[] values);

        public partial CesiumPropertyTableProperty AddVec2PropertyTableProperty(
            float2[] values);

        public partial CesiumPropertyTableProperty AddVec3PropertyTableProperty(
            float3[] values);

        public partial CesiumPropertyTableProperty AddVec4PropertyTableProperty(
            float4[] values);

        public partial CesiumPropertyTableProperty AddMat2PropertyTableProperty(
            float2x2[] values);

        public partial CesiumPropertyTableProperty AddMat3PropertyTableProperty(
            float3x3[] values);

        public partial CesiumPropertyTableProperty AddMat4PropertyTableProperty(
            float4x4[] values);

        public partial CesiumPropertyTableProperty AddStringPropertyTableProperty(
            string[] values);
    }
}
