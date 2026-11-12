using UnityEngine;
using UnityEngine.Rendering;

#if ENABLE_XR_MODULE
using UnityEngine.XR;
#endif

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
        private MeshRenderer _meshRenderer;
        private GraphicsBuffer _meshVertexBuffer;

        private int _pointCount = 0;
        private Material _pointMaterial;

        private Bounds _bounds;

        private Vector2 _attenuationParameters;
        private Vector4 _constantColor;

        private Cesium3DTileInfo _tileInfo;

        public Cesium3DTileInfo tileInfo
        {
            set => this._tileInfo = value;
        }

        private Mesh _emptyMesh;

        void SetShaderKeywords()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                this._pointMaterial.EnableKeyword("_BUILT_IN_RP");
            }

            if (this._mesh.HasVertexAttribute(VertexAttribute.Color))
            {
                this._pointMaterial.EnableKeyword("_HAS_POINT_COLORS");
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
                this._pointMaterial.EnableKeyword("_HAS_POINT_NORMALS");
            }

#if ENABLE_XR_MODULE
            if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced ||
                XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassMultiview)
            {
                this._pointMaterial.EnableKeyword("INSTANCING_ON");
            }
#endif
        }

        void OnEnable()
        {
            this._tileset = this.gameObject.GetComponentInParent<Cesium3DTileset>();

            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            this._mesh = meshFilter?.sharedMesh;
            this._meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

            this._pointCount = this._mesh.vertexCount;
            if (this._tileset.opaqueMaterial != null)
            {
                this._pointMaterial = UnityEngine.Object.Instantiate(this._tileset.opaqueMaterial);
            }
            else
            {
                this._pointMaterial = UnityEngine.Object.Instantiate(
                    Resources.Load<Material>("CesiumPointCloudTilesetMaterial"));
            }

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
            }
            else
            {
                this._mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
                this._meshVertexBuffer = sourceBuffer;
            }

            var layout = new VertexAttributeDescriptor[0];
            this._emptyMesh = CesiumObjectPools.MeshPool.Get();

            int[] indices = new int[this._pointCount * 6];
            for (int i = 0; i < indices.Length; ++i)
            {
                indices[i] = i;
            }

            this._emptyMesh.SetVertexBufferParams(this._pointCount * 6, layout);
            this._emptyMesh.indexFormat = IndexFormat.UInt32;
            this._emptyMesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            this._emptyMesh.bounds = this._mesh.bounds;

            SetShaderKeywords();
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

            CesiumPointCloudShading pointCloudShading = this._tileset.pointCloudShading;

            if (pointCloudShading.maximumAttenuation > 0.0f)
            {
                maximumPointSize = this._tileset.pointCloudShading.maximumAttenuation;
            }

            if (Screen.dpi > 0)
            {
                // Approximation of device pixel ratio
                maximumPointSize *= Screen.dpi / 150;
            }

            float geometricError = this.GetGeometricError(pointCloudShading);
            geometricError *= pointCloudShading.geometricErrorScale;

            // Since depth multiplier relies on the camera parameters,
            // it is computed in the shader itself.
            this._attenuationParameters =
                new Vector2(maximumPointSize, geometricError);
        }

        private Vector3[] positionsScratch = new Vector3[3];

        private void DestroyResources()
        {
            if (this._emptyMesh != null)
            {
                CesiumObjectPools.MeshPool.Release(this._emptyMesh);
                this._emptyMesh = null;
            }

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

            //if (this._tileInfo.isTranslucent || this._constantColor.w < 1.0f)
            //{
            //    this._pointMaterial.SetOverrideTag("RenderType", "Transparent");
            //    this._pointMaterial.renderQueue = (int)RenderQueue.Transparent;
            //    this._pointMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            //    this._pointMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            //}
            //else
            {
                this._pointMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
                this._pointMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
            }
        }

        private void DrawPointsWithAttenuation()
        {
            this.UpdateAttenuationParameters();
            this.UpdateMaterial();

            Graphics.DrawMesh(this._emptyMesh, this.gameObject.transform.localToWorldMatrix, this._pointMaterial, 0);
        }

        void Update()
        {
            CesiumPointCloudShading pointCloudShading = this._tileset.pointCloudShading;
            if (pointCloudShading != null && pointCloudShading.attenuation)
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