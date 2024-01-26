
using Reinterop;
using System;
using Unity.Mathematics;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::TestGltfModelImpl", "TestGltfModelImpl.h")]
    internal partial class TestGltfModel
    {
        #region EXT_mesh_features

        public partial CesiumFeatureIdAttribute AddFeatureIdAttribute(UInt16[] featureIds, Int64 featureCount);

        public partial CesiumFeatureIdTexture AddFeatureIdTexture(UInt16[] featureIds, Int64 featureCount, float2[] uvs);
        #endregion

        #region EXT_structural_metadata
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

        public partial CesiumPropertyTableProperty AddFixedLengthArrayPropertyTableProperty(
            double[] values, Int64 count);

        public partial CesiumPropertyTableProperty AddVariableLengthArrayPropertyTableProperty(
            double[] values, UInt16[] offsets);
        #endregion
    }
}
