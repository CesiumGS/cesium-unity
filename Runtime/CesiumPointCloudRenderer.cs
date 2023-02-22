using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace CesiumForUnity
{
    internal struct Cesium3DTileInfo
    {
        public bool usesAdditiveRefinement;
        public float geometricError;
        public Vector3 dimensions;
    }

    [ExecuteInEditMode]
    internal class CesiumPointCloudRenderer : MonoBehaviour
    {
        private Cesium3DTileset _tileset;

        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private GraphicsBuffer _meshVertexBuffer;
        private ComputeBuffer _argsBuffer; // Buffer with arguments
        private int _pointCount = 0;

        private Bounds _bounds;

        private MaterialPropertyBlock _materialProperties;
        private Material _material;
        private Vector4 _attenuationParameters;
        private bool _useConstantColor;

        private Mesh _planeMesh;

        private Cesium3DTileInfo _tileInfo;

        public Cesium3DTileInfo tileInfo
        {
            get => this._tileInfo;
            set => this._tileInfo = value;
        }

        void OnEnable()
        {
            this._tileset = this.gameObject.GetComponentInParent<Cesium3DTileset>();

            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            this._mesh = meshFilter.sharedMesh;
            this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            this._mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
            this._meshVertexBuffer = this._mesh.GetVertexBuffer(0);

            this._planeMesh = new Mesh();
            this._planeMesh.vertices = new Vector3[]{
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            };

            this._planeMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            this._pointCount = this._mesh.vertexCount;

            this._materialProperties = new MaterialPropertyBlock();

            this._useConstantColor = !this._mesh.HasVertexAttribute(VertexAttribute.Color);

            uint[] args = new uint[]
            {
                6,
                (uint)this._pointCount,
                0, // only relevant if drawing submeshes
                0, // "
                0  // "
            };
            this._argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            this._argsBuffer.SetData(args);
        }

        private float GetGeometricError(CesiumPointCloudShading pointCloudShading)
        {
            float geometricError = this._tileInfo.geometricError;
            if (geometricError > 0.0f)
            {
                return geometricError;
            }

            if (pointCloudShading.baseResolution > 0.0f)
            {
                return pointCloudShading.baseResolution;
            }

            // Estimate the geometric error.
            Vector3 dimensions = this._tileInfo.dimensions;
            float volume = dimensions.x * dimensions.y * dimensions.z;
            return Mathf.Pow(volume / this._pointCount, 1.0f / 3.0f);
        }

        private void UpdateAttenuationParameters()
        {
            float maximumPointSize =
                this._tileInfo.usesAdditiveRefinement ?
                5.0f :
                this._tileset.maximumScreenSpaceError;

            if (this._tileset.pointCloudShading.maximumAttenuation > 0.0f)
            {
                maximumPointSize = this._tileset.pointCloudShading.maximumAttenuation;
            }

            if (Screen.dpi > 0)
            {
                // Approximation of device pixel ratio
                maximumPointSize *= Screen.dpi / 150;
            }

            CesiumPointCloudShading pointCloudShading =
                this._tileset.pointCloudShading;

            float geometricError = this.GetGeometricError(pointCloudShading);
            geometricError *= pointCloudShading.geometricErrorScale;

            // Depth multiplier
            Camera camera = Camera.main;
            float sseDenominator = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);
            float depthMultplier = camera.scaledPixelHeight / sseDenominator;

            // Whether or not to use constant color
            float useConstantColor = 0.0f;

            this._attenuationParameters =
                new Vector4(maximumPointSize, geometricError, depthMultplier, useConstantColor);
        }

        private void UpdateTransformAndBounds()
        {
            Matrix4x4 transformMatrix = this.gameObject.transform.localToWorldMatrix;
            this._materialProperties.SetMatrix("_worldTransform", transformMatrix);

            this._materialProperties.SetBuffer("_inVertices", this._meshVertexBuffer);
            Bounds localBounds = this._mesh.bounds;
            positionsScratch[0] = localBounds.center;
            positionsScratch[1] = localBounds.min;
            positionsScratch[2] = localBounds.max;

            this._bounds = GeometryUtility.CalculateBounds(positionsScratch, transformMatrix);
        }

        private void DestroyResources()
        {
            if (this._meshVertexBuffer != null)
            {
                this._meshVertexBuffer.Release();
                this._meshVertexBuffer = null;
            }

            if (this._argsBuffer != null)
            {
                this._argsBuffer.Release();
                this._argsBuffer = null;
            }
        }

        Vector3[] positionsScratch = new Vector3[3];

        private void DrawPointsWithAttenuation()
        {
            this.UpdateAttenuationParameters();
            this._materialProperties.SetVector("_attenuationParameters", this._attenuationParameters);

            /*Graphics.DrawMeshInstancedProcedural(
                this._planeMesh,
                0,
                this._tileset.pointCloudShading.unlitMaterial,
                this._bounds,
                this._pointCount,
                this._materialProperties);*/

            Graphics.DrawProcedural(
                this._tileset.pointCloudShading.unlitMaterial,
                this._bounds,
                MeshTopology.Triangles,
                this._pointCount * 6,  // vertex count
                1,                     // instance count
                null,                  // which camera to render to. null draws in all cameras
                this._materialProperties,
                ShadowCastingMode.On,
                false
             );
        }

        // Update is called once per frame
        void Update()
        {
            this.UpdateTransformAndBounds();

            if (this._tileset.pointCloudShading.attenuation)
            {
                this._meshRenderer.enabled = false;
                this.DrawPointsWithAttenuation();
            }
            else
            {
                this._meshRenderer.enabled = true;
            }
        }

        void OnDisable()
        {
            this.DestroyResources();
        }
    }
}