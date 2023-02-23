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
        private Material _material;
        private Material _unlitMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GraphicsBuffer _meshVertexBuffer;
        private int _pointCount = 0;

        private Bounds _bounds;

        private MaterialPropertyBlock _oldProperties;
        private MaterialPropertyBlock _materialProperties;
        private Vector4 _attenuationParameters;
        private bool _useConstantColor;

        private Mesh _pointMesh;

        private Cesium3DTileInfo _tileInfo;

        public Cesium3DTileInfo tileInfo
        {
            get => this._tileInfo;
            set => this._tileInfo = value;
        }

        void OnEnable()
        {
            this._tileset = this.gameObject.GetComponentInParent<Cesium3DTileset>();

            this._meshFilter = this.gameObject.GetComponent<MeshFilter>();
            this._mesh = this._meshFilter.sharedMesh;
            this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            this._material = this._meshRenderer.sharedMaterial;
            this._oldProperties = new MaterialPropertyBlock();
            this._meshRenderer.GetPropertyBlock(this._oldProperties);
            this._mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
            this._meshVertexBuffer = this._mesh.GetVertexBuffer(0);

            this._pointCount = this._mesh.vertexCount;
            this._pointMesh = new Mesh();

            Vector3[] vertices = this._mesh.vertices;
            int expandedPointCount = this._pointCount * 4;

            Vector3[] points = new Vector3[expandedPointCount];
            int[] triangles = new int[this._pointCount * 6];
            for (int i = 0, j = 0, k = 0; k < this._pointCount; k++, i += 4, j += 6)
            {
                points[i] = vertices[k];
                points[i + 1] = new Vector3(points[i].x, points[i].y + 1, points[i].z);
                points[i + 2] = new Vector3(points[i].x + 1, points[i].y + 1, points[i].z);
                points[i + 3] = new Vector3(points[i].x + 1, points[i].y, points[i].z);
                triangles[j] = i;
                triangles[j + 1] = i + 1;
                triangles[j + 2] = i + 2;
                triangles[j + 3] = i;
                triangles[j + 4] = i + 2;
                triangles[j + 5] = i + 3;
            }

            this._pointMesh.vertices = points;
            this._pointMesh.indexFormat = IndexFormat.UInt32;
            this._pointMesh.triangles = triangles;

            this._unlitMaterial = UnityEngine.Object.Instantiate(
                        Resources.Load<Material>("CesiumUnlitPointCloudMaterial"));

            this._materialProperties = new MaterialPropertyBlock();
            this._materialProperties.SetBuffer("_inVertices", this._meshVertexBuffer);

            this._useConstantColor = !this._mesh.HasVertexAttribute(VertexAttribute.Color);
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

            //Bounds localBounds = this._mesh.bounds;
            //positionsScratch[0] = localBounds.center;
            //positionsScratch[1] = localBounds.min;
            //positionsScratch[2] = localBounds.max;

            //this._bounds = GeometryUtility.CalculateBounds(positionsScratch, transformMatrix);
        }

        private void DestroyResources()
        {
            if (this._meshVertexBuffer != null)
            {
                this._meshVertexBuffer.Release();
                this._meshVertexBuffer = null;
            }

            if(this._unlitMaterial != null)
            {
                DestroyImmediate(this._unlitMaterial);
            }

            if(this._pointMesh != null)
            {
                DestroyImmediate(this._pointMesh);
            }
        }

        private void DrawPointsWithAttenuation()
        {
            this.UpdateAttenuationParameters();
            this._materialProperties.SetVector("_attenuationParameters", this._attenuationParameters);
            Graphics.DrawMesh(
                this._pointMesh,
                this.gameObject.transform.localToWorldMatrix,
                this._tileset.pointCloudShading.unlitMaterial,
                this.gameObject.layer,
                null,
                0,
                this._materialProperties);
        }

        // Update is called once per frame
        void Update()
        {
            this.UpdateTransformAndBounds();

            if (this._tileset.pointCloudShading.attenuation)
            {
                this.DrawPointsWithAttenuation();
                this._meshRenderer.enabled = false;
                //this._meshFilter.sharedMesh = this._pointMesh;
                //this._meshRenderer.sharedMaterial = this._unlitMaterial;
            }
            else
            {
                this._meshRenderer.enabled = true;
                //this._meshFilter.sharedMesh = this._mesh;
                //this._meshRenderer.sharedMaterial = this._material;
            }
        }

        void OnDisable()
        {
            this.DestroyResources();
        }
    }
}