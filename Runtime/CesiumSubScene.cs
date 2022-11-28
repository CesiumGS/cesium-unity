using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A sub-scene with its own georeference origin. When a game object with a <see cref="CesiumOriginShift"/>
    /// comes close to a sub-scene, that sub-scene is activated and all other sub-scenes are deactivated.
    /// This allows relatively normal Unity scenes to be designed at multiple locations on the globe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When determining the distance to a sub-scene, only the <see cref="CesiumGeoreference"/>'s Transform
    /// is considered. The Transform associated with the sub-scene is ignored. The sub-scene transform _will_
    /// affect the transformation of the objects inside it once activated, however.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    public class CesiumSubScene : MonoBehaviour
    {
        [SerializeField]
        private double _activationRadius = 1000;

        public double activationRadius
        {
            get => this._activationRadius;
            set
            {
                this._activationRadius = value;
            }
        }

        [SerializeField]
        private bool _showActivationRadius = true;

        public bool showActivationRadius
        {
            get => this._showActivationRadius;
            set => this._showActivationRadius = value;
        }

        [SerializeField]
        private CesiumGeoreferenceOriginAuthority _originAuthority =
            CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;

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

        public void SetOriginEarthCenteredEarthFixed(double x, double y, double z)
        {
            this._ecefX = x;
            this._ecefY = y;
            this._ecefZ = z;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
        }

        public void SetOriginLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this._longitude = longitude;
            this._latitude = latitude;
            this._height = height;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
        }

        private void OnEnable()
        {
            // When this sub-scene is enabled, all others are disabled.
            CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
                throw new InvalidOperationException(
                    "CesiumSubScene is not nested inside a game object with a CesiumGeoreference.");

            CesiumSubScene[] subscenes = georeference.GetComponentsInChildren<CesiumSubScene>();
            foreach (CesiumSubScene scene in subscenes)
            {
                if (scene == this)
                    continue;
                scene.gameObject.SetActive(false);
            }

            this.UpdateOrigin();
        }

        public void UpdateOrigin()
        {
            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight)
            {
                double3 ecef = CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(
                    this._longitude,
                    this._latitude,
                    this._height
                ));

                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }

            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                double3 llh = CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(new double3(
                    this._ecefX,
                    this._ecefY,
                    this._ecefZ
                ));

                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }

            if (this.isActiveAndEnabled)
            {
                CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();
                if (georeference == null)
                    throw new InvalidOperationException("CesiumSubScene is not nested inside a game object with a CesiumGeoreference.");

                double3 ecefPosition = new double3(
                    this._ecefX,
                    this._ecefY,
                    this._ecefZ
                );

                if (this.originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
                    georeference.SetOriginEarthCenteredEarthFixed(
                        this._ecefX,
                        this._ecefY,
                        this._ecefZ);
                else
                    georeference.SetOriginLongitudeLatitudeHeight(
                        this._longitude,
                        this._latitude, 
                        this._height);
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (this._showActivationRadius)
            {
                // TODO: would be nice to draw a better wireframe sphere.
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(this.transform.position, (float)this._activationRadius);
            }
        }
        #endif
    }
}
