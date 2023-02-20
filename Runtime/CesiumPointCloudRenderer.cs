using UnityEngine;

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

        private int _pointCount = 0;

        private ComputeBuffer _positionsBuffer;
        private ComputeBuffer _colorsBuffer;
        private ComputeBuffer _normalsBuffer;
        private ComputeBuffer _argsBuffer; // The buffer containing the arguments for DrawProceduralIndirect.
        private bool _initialized = false;

        private Material _material;
        private Vector4 _constantColor;
        private Vector4 _attenuationParameters;

        private float _estimatedGeometricError;

        private Cesium3DTileInfo _tileInfo;

        public Cesium3DTileInfo tileInfo
        {
            get => this._tileInfo;
            set => this._tileInfo = value;
        }

        void OnEnable()
        {
            this._tileset = this.gameObject.GetComponentInParent<Cesium3DTileset>();
            this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            this._mesh = meshFilter.sharedMesh;
            this._pointCount = this._mesh.vertexCount;

            this._material = UnityEngine.Object.Instantiate(
                Resources.Load<Material>("CesiumUnlitPointCloudMaterial"));
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

        private void ComputeAttenuationParameters()
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
            float useConstantColor = this._colorsBuffer == null ? 1.0f : 0.0f;

            this._attenuationParameters =
                new Vector4(maximumPointSize, geometricError, depthMultplier, useConstantColor);
        }

        private void InitializeResources()
        {
            if (this._initialized)
            {
                return;
            }

            // Initialize and populate the compute buffers.
            this._positionsBuffer = new ComputeBuffer(this._pointCount, 3 * sizeof(float));
            this._positionsBuffer.SetData(this._mesh.vertices);

            Color32[] colors = this._mesh.colors32;
            if (colors.Length > 0)
            {
                // The colors are sent as 32-bit unsigned integers and will be unpacked
                // in the shader.
                this._colorsBuffer = new ComputeBuffer(this._pointCount, sizeof(uint));
                this._colorsBuffer.SetData(colors);
            }
            else if (this._tileset.opaqueMaterial != null)
            {
                Material tilesetMaterial = this._tileset.opaqueMaterial;
                Color color = tilesetMaterial.HasColor("baseColorFactor") ?
                    tilesetMaterial.GetColor("baseColorFactor") :
                    this._constantColor = tilesetMaterial.color;
                this._constantColor = color;
            }
            else
            {
                this._constantColor = Color.white;
            }

            Vector3[] normals = this._mesh.normals;
            if (normals.Length > 0)
            {
                this._normalsBuffer = new ComputeBuffer(this._pointCount, 3 * sizeof(float));
                this._normalsBuffer.SetData(normals);
            }

            // Argument buffer used by DrawProceduralIndirect.
            uint[] args = new uint[] { 0, 0, 0, 0 };
            // Arguments for drawing mesh.
            args[0] = (uint)this._pointCount * 6; // Number of vertices to be drawn
            args[1] = (uint)1; // Number of instances.
            args[2] = (uint)0; // Only relevant if using GraphicsBuffer.
            args[3] = (uint)0; // Same as above.
            this._argsBuffer =
                new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            this._argsBuffer.SetData(args);

            this._initialized = true;
        }

        private void DestroyResources()
        {
            if (!this._initialized)
            {
                return;
            }

            this._positionsBuffer.Release();
            this._positionsBuffer = null;

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

            this._argsBuffer.Release();
            this._argsBuffer = null;

            this._initialized = false;
        }

        private void DrawPointCloudWithAttenuation()
        {
            this._material.SetBuffer("_inPositions", this._positionsBuffer);

            if (this._colorsBuffer != null)
            {
                this._material.SetBuffer("_inColors", this._colorsBuffer);
            }
            else
            {
                this._material.SetVector("_constantColor", this._constantColor);
            }

            this._material.SetMatrix("_worldTransform", this.gameObject.transform.localToWorldMatrix);

            this.ComputeAttenuationParameters();
            this._material.SetVector("_attenuationParameters", this._attenuationParameters);

            Graphics.DrawProceduralIndirect(
                this._material,
                this._meshRenderer.bounds,
                MeshTopology.Triangles,
                this._argsBuffer);
        }

        // Update is called once per frame
        void Update()
        {
            if (this._tileset.pointCloudShading.attenuation)
            {
                this.InitializeResources();

                // Disable the mesh renderer, since points will be rendered using
                // the material and shaders.
                this._meshRenderer.enabled = false;
                DrawPointCloudWithAttenuation();
            }
            else
            {
                // Re-enable to mesh renderer to draw the points as normal.
                this._meshRenderer.enabled = true;
            }
        }

        void OnDisable()
        {
            this.DestroyResources();
        }
    }
}