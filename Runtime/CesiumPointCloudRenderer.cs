using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GraphicsBuffer _meshVertexBuffer;
        private int _pointCount = 0;

        private Bounds _bounds;

        private Vector4 _attenuationParameters;
        private bool _useConstantColor;

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
            this._mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
            this._meshVertexBuffer = this._mesh.GetVertexBuffer(0);

            this._pointCount = this._mesh.vertexCount;

            this._material = UnityEngine.Object.Instantiate(
                        Resources.Load<Material>("CesiumUnlitPointCloudMaterial"));

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

        private Vector3[] positionsScratch = new Vector3[3];

        private void UpdateBounds()
        {
            Matrix4x4 transformMatrix = this.gameObject.transform.localToWorldMatrix;

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

            if(this._material != null)
            {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(this._material);
                return;
            }
#endif
                Destroy(this._material);
            }
        }

        private void DrawPointsWithAttenuation()
        {
            this.UpdateBounds();
            this.UpdateAttenuationParameters();

            this._material.SetBuffer("_inVertices", this._meshVertexBuffer);
            this._material.SetMatrix("_worldTransform", this.gameObject.transform.localToWorldMatrix);
            this._material.SetVector("_attenuationParameters", this._attenuationParameters);

            Graphics.DrawProcedural(
                this._material,
                this._bounds,
                MeshTopology.Triangles,
                this._pointCount * 6,
                1);
        }

        // Update is called once per frame
        void Update()
        {
            if (this._tileset.pointCloudShading.attenuation)
            {
                this.DrawPointsWithAttenuation();
                this._meshRenderer.enabled = false;
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