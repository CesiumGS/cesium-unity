using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CesiumPointCloudShading : MonoBehaviour
{
    private int _pointCount = 0;

    private MeshRenderer _meshRenderer;

    private ComputeBuffer _positionsBuffer;
    private ComputeBuffer _colorsBuffer;
    private ComputeBuffer _normalsBuffer;

    private ComputeBuffer _argsBuffer; // The buffer containing the arguments for DrawProceduralIndirect.

    private Material _material;

    void OnEnable()
    {
        // Disable the mesh renderer, since points will be rendered using
        // the material and shaders.
        this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        this._meshRenderer.enabled = false;

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        this._pointCount = mesh.vertexCount;

        this._positionsBuffer = new ComputeBuffer(this._pointCount, 3 * sizeof(float));
        this._positionsBuffer.SetData(mesh.vertices);

        Color32[] colors = mesh.colors32;
        if (colors.Length > 0)
        {   
            // Unity has no byte vec4 structs, so just pack the 
            this._colorsBuffer = new ComputeBuffer(this._pointCount, sizeof(uint));
            this._colorsBuffer.SetData(colors);
        }

        Vector3[] normals = mesh.normals;
        if (normals.Length > 0)
        {
            this._normalsBuffer = new ComputeBuffer(this._pointCount, 3 * sizeof(float));
            this._normalsBuffer.SetData(normals);
        }

        // Argument buffer used by DrawProceduralIndirect.
        uint[] args = new uint[] { 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        args[0] = (uint)this._pointCount * 6; // Number of vertices
        args[1] = (uint)1; // Number of instances.
        args[2] = (uint)0; // Only relevant if using GraphicsBuffer.
        args[3] = (uint)0; // Same as above.
        this._argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        this._argsBuffer.SetData(args);

        this._material = Object.Instantiate(Resources.Load<Material>("CesiumUnlitPointCloudMaterial"));
    }


    // Update is called once per frame
    void Update()
    {
        this._material.SetBuffer("_inPositions", this._positionsBuffer);
        if (this._colorsBuffer != null)
        {
            this._material.SetBuffer("_inColors", this._colorsBuffer);
        }

        this._material.SetMatrix("_worldTransform", this.gameObject.transform.localToWorldMatrix);

        Graphics.DrawProceduralIndirect(
            this._material,
            this._meshRenderer.bounds,
            MeshTopology.Triangles,
            this._argsBuffer);
    }

    void OnDisable()
    {
        if(this._positionsBuffer != null)
        {
            this._positionsBuffer.Release();
            this._positionsBuffer = null;
        }

        if (this._colorsBuffer != null)
        {
            this._colorsBuffer.Release();
            this._colorsBuffer = null;
        }

        if (this._normalsBuffer != null)
        {
            this._normalsBuffer.Release();
            this._normalsBuffer = null;
        }

        if(this._argsBuffer != null)
        {
            this._argsBuffer.Release();
            this._argsBuffer = null;
        }
    }
}