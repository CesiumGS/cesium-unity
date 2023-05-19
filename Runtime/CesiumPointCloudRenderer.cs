using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

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
        public bool isTranslucent;
    }

    [ExecuteInEditMode]
    [AddComponentMenu("")]
    internal class CesiumPointCloudRenderer : MonoBehaviour
    {
        private Cesium3DTileset _tileset;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private GraphicsBuffer _meshVertexBuffer;

        private int _pointCount = 0;
        private Material _pointMaterial;

        private Bounds _bounds;

        private Vector4 _attenuationParameters;
        private Vector4 _constantColor;

        private Cesium3DTileInfo _tileInfo;

        public Cesium3DTileInfo tileInfo
        {
            set => this._tileInfo = value;
        }

        void OnEnable()
        {
            this._tileset = this.gameObject.GetComponentInParent<Cesium3DTileset>();

            this._meshFilter = this.gameObject.GetComponent<MeshFilter>();
            this._mesh = this._meshFilter.sharedMesh;
            this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

            this._pointCount = this._mesh.vertexCount;
            this._pointMaterial = UnityEngine.Object.Instantiate(
                        Resources.Load<Material>("CesiumPointCloudShadingMaterial"));

            GraphicsBuffer sourceBuffer = this._mesh.GetVertexBuffer(0);

            bool usingDirect11 = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11;
            if (usingDirect11)
            {
                this._meshVertexBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination, 
                    sourceBuffer.count, 
                    sourceBuffer.stride);
                Graphics.CopyBuffer(sourceBuffer, this._meshVertexBuffer);
                sourceBuffer.Release();
            } else
            {
                this._mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
                this._meshVertexBuffer = sourceBuffer;
            }

            if (this._mesh.HasVertexAttribute(VertexAttribute.Color))
            {
                this._pointMaterial.EnableKeyword("HAS_POINT_COLORS");
            }
            else
            {
                Material material = this._meshRenderer.sharedMaterial;

                if (material.HasColor("_Color"))
                {
                    this._constantColor = material.color;
                }
                else if (material.HasVector("_baseColorFactor"))
                {
                    this._constantColor = material.GetVector("_baseColorFactor");
                }
                else
                {
                    this._constantColor = Color.gray;
                }
            }

            if (this._mesh.HasVertexAttribute(VertexAttribute.Normal))
            {
                this._pointMaterial.EnableKeyword("HAS_POINT_NORMALS");
            }

            if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced ||
                XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassMultiview)
            {
                this._pointMaterial.EnableKeyword("INSTANCING_ON");
            }
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

            CesiumPointCloudShading pointCloudShading = this._tileset.pointCloudShading;

            float geometricError = this.GetGeometricError(pointCloudShading);
            geometricError *= pointCloudShading.geometricErrorScale;

            // Depth multiplier
            Camera camera = Camera.main;
            float sseDenominator = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);
            float depthMultplier = camera.scaledPixelHeight / sseDenominator;

            this._attenuationParameters =
                new Vector4(maximumPointSize, geometricError, depthMultplier, 0);
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

            if (this._pointMaterial != null)
            {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(this._pointMaterial);
                return;
            }
#endif
                Destroy(this._pointMaterial);
            }
        }

        private void UpdateMaterial()
        {
            this._pointMaterial.SetBuffer("_inPoints", this._meshVertexBuffer);
            this._pointMaterial.SetMatrix("_worldTransform", this.gameObject.transform.localToWorldMatrix);
            this._pointMaterial.SetVector("_attenuationParameters", this._attenuationParameters);
            this._pointMaterial.SetVector("_constantColor", this._constantColor);

            if (this._tileInfo.isTranslucent || this._constantColor.w < 1.0f)
            {
                this._pointMaterial.SetOverrideTag("RenderType", "Transparent");
                this._pointMaterial.renderQueue = (int)RenderQueue.Transparent;
                this._pointMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                this._pointMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            }
            else
            {
                this._pointMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
                this._pointMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
            }
        }

        private void DrawPointsWithAttenuation()
        {
            this.UpdateBounds();
            this.UpdateAttenuationParameters();
            this.UpdateMaterial();

            Graphics.DrawProcedural(
                this._pointMaterial,
                this._bounds,
                MeshTopology.Triangles,
                this._pointCount * 6,
                1);
        }

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