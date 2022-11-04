using System;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    public class CesiumSubLevel : MonoBehaviour, INotifyOfChanges
    {
        [SerializeField]
        [Tooltip("The set of coordinates that authoritatively define the origin of this georeference.")]
        [NotifyOfChanges]
        private CesiumGeoreferenceOriginAuthority _originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;

        public CesiumGeoreferenceOriginAuthority originAuthority
        {
            get => this._originAuthority;
            set
            {
                this._originAuthority = value;
                this.UpdateOrigin();
            }
        }

        [SerializeField]
        [Header("Origin")]
        [Tooltip("The latitude of the origin in degrees, in the range [-90, 90].")]
        [NotifyOfChanges]
        private double _latitude = 39.736401;

        public double latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
                this.UpdateOrigin();
            }
        }

        [SerializeField]
        [Tooltip("The longitude of the origin in degrees, in the range [-180, 180].")]
        [NotifyOfChanges]
        private double _longitude = -105.25737;

        public double longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
                this.UpdateOrigin();
            }
        }

        [SerializeField]
        [Tooltip("The height of the origin in meters above the ellipsoid (usually WGS84). " +
                 "Do not confuse this with a geoid height or height above mean sea level, which " +
                 "can be tens of meters higher or lower depending on where in the world the " +
                 "origin is located.")]
        [NotifyOfChanges]
        private double _height = 2250.0;

        public double height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.UpdateOrigin();
            }
        }

        [SerializeField]
        [Header("Position (Earth Centered, Earth Fixed)")]
        [Tooltip("The Earth-Centered, Earth-Fixed X-coordinate of the origin of this georeference in meters.\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
                 "and the positive X axis points toward where the Prime Meridian crosses the Equator.")]
        [NotifyOfChanges]
        private double _ecefX = 6378137.0;

        public double ecefX
        {
            get => this._ecefX;
            set
            {
                this._ecefX = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this georeference in meters.\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
                 "and the positive Y axis points toward the Equator at 90 degrees longitude.")]
        [NotifyOfChanges]
        private double _ecefY = 0.0;

        public double ecefY
        {
            get => this._ecefY;
            set
            {
                this._ecefY = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this georeference in meters.\n" +
         "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
         "and the positive Z axis points toward the North pole.")]
        [NotifyOfChanges]
        private double _ecefZ = 0.0;

        public double ecefZ
        {
            get => this._ecefZ;
            set
            {
                this._ecefZ = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        private void OnEnable()
        {
            // When this sub-level is enabled, all others are disabled.
            CesiumGeoreference? georeference = this.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
                throw new InvalidOperationException("CesiumSubLevel is not nested inside a game object with a CesiumGeoreference.");

            CesiumSubLevel[] sublevels = georeference.GetComponentsInChildren<CesiumSubLevel>();
            foreach (CesiumSubLevel level in sublevels)
            {
                if (level == this)
                    continue;
                level.gameObject.SetActive(false);
            }

            this.UpdateOrigin();
        }

        private void UpdateOrigin()
        {
            if (this.isActiveAndEnabled)
            {
                CesiumGeoreference? georeference = this.GetComponentInParent<CesiumGeoreference>();
                if (georeference == null)
                    throw new InvalidOperationException("CesiumSubLevel is not nested inside a game object with a CesiumGeoreference.");

                if (this.originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
                    georeference.SetOriginEarthCenteredEarthFixed(this._ecefX, this._ecefY, this._ecefZ);
                else
                    georeference.SetOriginLongitudeLatitudeHeight(this._longitude, this._latitude, this._height);
            }
        }

        void INotifyOfChanges.NotifyPropertyChanged(SerializedProperty property)
        {
#if UNITY_EDITOR
            switch (property.name)
            {
                case "_longitude":
                case "_latitude":
                case "_height":
                    this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
                    break;
                case "_ecefX":
                case "_ecefY":
                case "_ecefZ":
                    this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
                    break;
            }

            EditorApplication.QueuePlayerLoopUpdate();
#endif
        }
    }
}
