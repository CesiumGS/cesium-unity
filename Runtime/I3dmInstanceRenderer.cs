using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CesiumForUnity
{
public static class MatrixUtils
{
    public static Matrix4x4 Double4x4ToMatrix4x4(double4x4 m)
    {
        return new Matrix4x4(
            new Vector4((float)m.c0.x, (float)m.c0.y, (float)m.c0.z, (float)m.c0.w),
            new Vector4((float)m.c1.x, (float)m.c1.y, (float)m.c1.z, (float)m.c1.w),
            new Vector4((float)m.c2.x, (float)m.c2.y, (float)m.c2.z, (float)m.c2.w),
            new Vector4((float)m.c3.x, (float)m.c3.y, (float)m.c3.z, (float)m.c3.w)
        );
    }

    public static Matrix4x4[] Double4x4ArrayToMatrix4x4Array(double4x4[] arr)
    {
        if (arr == null) return null;
        Matrix4x4[] result = new Matrix4x4[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            result[i] = Double4x4ToMatrix4x4(arr[i]);
        }
        return result;
    }
}
[ExecuteInEditMode]
public class I3dmInstanceRenderer : MonoBehaviour
{
    // Instance groups prepared in the C++ cesium-unity dll
    private Dictionary<string, InstanceGroupData> instanceGroups = new Dictionary<string, InstanceGroupData>();

    //private MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    [System.Serializable]
    public class InstanceGroupData
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4[] matrices;
        public int maxInstancesPerBatch = 1023;
    }

    void Update()
    {
        // Perform rendering for all instance groups
        foreach (var kvp in instanceGroups)
        {
            var groupData = kvp.Value;
            RenderInstanceGroup(groupData);
        }
    }

    void RenderInstanceGroup(InstanceGroupData groupData)
    {
        if (groupData.mesh == null || groupData.material == null || groupData.matrices == null)
            return;

        int totalInstances = groupData.matrices.Length;
        int maxBatchSize = groupData.maxInstancesPerBatch;

        // Rendering divided by batch
        for (int batchStart = 0; batchStart < totalInstances; batchStart += maxBatchSize)
        {
            int batchSize = Mathf.Min(maxBatchSize, totalInstances - batchStart);

            // Extract the matrices of the current batch
            Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];

            for(int i = 0; i < batchSize; i++)
            {
                batchMatrices[i] = transform.GetChild(i).localToWorldMatrix;
            }
            //System.Array.Copy(groupData.matrices, batchStart, batchMatrices, 0, batchSize);

            // Rendering with GPU Instancing
            Graphics.DrawMeshInstanced(
                groupData.mesh,
                0,
                groupData.material,
                batchMatrices,
                batchSize
            );
        }
    }

    // called from C++ cesium-unity dll
    public void AddInstanceGroup(string groupId, Mesh mesh, Material material, List<double4x4> matrices)
    {
        // MaterialPropertyBlock setting (for each instance)
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        
        // TODO: color setting and shader property setting
        /*
        std::vector<UnityEngine::Vector4> colorVector;
        colorVector.reserve(batchSize);
        
        for (size_t i = 0; i < batchSize; ++i) {
            size_t sourceIndex = batchStart + i;
            const glm::dvec4& glmColor = instanceGroup->colors[sourceIndex];
            
            std::vector<double> colorValues = {
                glmColor.x, glmColor.y, glmColor.z, glmColor.w
            };
            
            colorVector.push_back(gltfVectorToUnityVector(colorValues, 1.0f));
        }
        
        // Set the color array as a shader property
        propertyBlock.SetVectorArray(
            UnityEngine::Shader::PropertyToID(System::String("_InstanceColors")),
            colorArray);
        */

        instanceGroups[groupId] = new InstanceGroupData
        {
            mesh = mesh,
            material = material,
            matrices = MatrixUtils.Double4x4ArrayToMatrix4x4Array(matrices.ToArray())
        };
    }

    public void RemoveInstanceGroup(string groupId)
    {
        instanceGroups.Remove(groupId);
    }
}
}
