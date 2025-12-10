using System;
using UnityEngine;

namespace CesiumForUnity
{
    [Serializable]
    /// <summary>
    /// Options for adjusting how point clouds are rendered using 3D Tiles.
    /// </summary>
    public class CesiumPointCloudShading
    {
        [SerializeField]
        private bool _attenuation = false;

        /// <summary>
        /// Whether or not to perform point attenuation. Attenuation controls the size of
        /// the points rendered based on the geometric error of their tile.
        /// </summary>
        public bool attenuation
        {
            get => this._attenuation;
            set => this._attenuation = value;
        }

        [SerializeField]
        private float _geometricErrorScale = 1.0f;

        /// <summary>
        /// The scale to be applied to the tile's geometric error before it is used
        /// to compute attenuation. Larger values will result in larger points.
        /// </summary>
        public float geometricErrorScale
        {
            get => this._geometricErrorScale;
            set
            {
                this._geometricErrorScale = Mathf.Max(value, 0.0f);
            }
        }

        [SerializeField]
        private float _maximumAttenuation = 0.0f;

        /// <summary>
        /// The maximum point attenuation in pixels. If this is zero, the Cesium3DTileset's 
        /// maximumScreenSpaceError will be used as the maximum point attenuation.
        /// </summary>
        public float maximumAttenuation
        {
            get => this._maximumAttenuation;
            set
            {
                this._maximumAttenuation = Mathf.Max(value, 0.0f);
            }
        }

        [SerializeField]
        private float _baseResolution = 0.0f;

        /// <summary>
        /// The average base resolution for the dataset in meters. For example, 
        /// a base resolution of 0.05 assumes an original capture resolution of
        /// 5 centimeters between neighboring points.
        /// <para>
        /// This is used in place of geometric error when the tile's geometric error is 0. 
        /// If this value is zero, each tile with a geometric error of 0 will have its 
        /// geometric error approximated instead.
        /// </para>
        /// </summary>
        public float baseResolution
        {
            get => this._baseResolution;
            set => this._baseResolution = Mathf.Max(value, 0.0f);
        }
    }
}