using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CesiumPointCloudShading : MonoBehaviour
{
    private int _pointCount = 0;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private ComputeShader _geometryComputeShader;
    private ComputeBuffer _positionsBuffer; // The original point positions
    private ComputeBuffer _geometryBuffer;  // The generated geometry vertices.
    private ComputeBuffer _argsBuffer;

    private int _geometryKernelId;
    private int _geometryDispatchSize;

    private Material _material;

    void OnEnable()
    {
        this._meshFilter = this.gameObject.GetComponent<MeshFilter>();

        this._pointCount = this._meshFilter.sharedMesh.vertexCount;

        // Disable the mesh renderer, since points will be rendered using
        // the material and shaders.
        this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        this._meshRenderer.enabled = false;

        this._geometryComputeShader = (ComputeShader)Resources.Load("PointCloudGeometry");
        this._geometryKernelId = this._geometryComputeShader.FindKernel("Main");

        this._geometryBuffer = new ComputeBuffer(
            this._pointCount,
            8 * sizeof(float),
            ComputeBufferType.Append);
        this._geometryBuffer.SetCounterValue(0);

        this._geometryComputeShader.SetBuffer(
            this._geometryKernelId, "_outGeometry", this._geometryBuffer);
        this._geometryComputeShader.SetInt("_pointCount", this._pointCount);

        this._geometryComputeShader.GetKernelThreadGroupSizes(
            this._geometryKernelId, out uint threadGroupSize, out _, out _);
        this._geometryDispatchSize = Mathf.CeilToInt((float)this._pointCount / threadGroupSize);

        // Argument buffer used by DrawProceduralIndirect.
        uint[] args = new uint[] { 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        args[0] = (uint)this._pointCount * 6; // Number of vertices
        args[1] = (uint)1; // Number of instances.
        args[2] = (uint)0; // Only relevant if using GraphicsBuffer.
        args[3] = (uint)0; // Same as above.

        this._positionsBuffer = new ComputeBuffer(this._pointCount, 3 * sizeof(float));
        this._positionsBuffer.SetData(this._meshFilter.sharedMesh.vertices);

        this._argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        this._argsBuffer.SetData(args);

        this._material = Resources.Load<Material>("CesiumUnlitPointCloudMaterial");
        this._material.SetBuffer("_inGeometry", this._geometryBuffer);
    }


    // Update is called once per frame
    void Update()
    {
        //this._geometryBuffer.SetCounterValue(0);
        //this._geometryComputeShader.Dispatch(this._geometryKernelId, this._geometryDispatchSize, 1, 1);

        //Graphics.DrawMeshInstanced(this._pointMesh, 0, this._material, this._matrices, this._pointsCount);
        this._material.SetBuffer("_inPositions", this._positionsBuffer);
        this._material.SetMatrix("_worldTransform", this.gameObject.transform.localToWorldMatrix);
        Graphics.DrawProceduralIndirect(
            this._material,
            this._meshRenderer.bounds,
            MeshTopology.Triangles,
            this._argsBuffer);
    }

    void OnDisable()
    {
        this._positionsBuffer.Release();
        this._geometryBuffer.Release();
        this._argsBuffer.Release();

        this._positionsBuffer = null;
        this._geometryBuffer = null;
        this._argsBuffer = null;
    }
}