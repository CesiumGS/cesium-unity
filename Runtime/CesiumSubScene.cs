using System;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
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
                this.originAuthority =CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
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

        // The Unity world-space position that the coordinates currently correspond to.
        private Vector3 _unityWorldPosition;

        public Vector3 unityWorldPosition
        {
            get => this._unityWorldPosition;
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
            CesiumGeoreference? georeference = this.GetComponentInParent<CesiumGeoreference>();
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
                CesiumVector3 ecef = CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new CesiumVector3()
                {
                    x = this._longitude,
                    y = this._latitude,
                    z = this._height
                });

                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }

            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                CesiumVector3 llh = CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(new CesiumVector3()
                {
                    x = this._ecefX,
                    y = this._ecefY,
                    z = this._ecefZ
                });

                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }

            if (this.isActiveAndEnabled)
            {
                CesiumGeoreference? georeference = this.GetComponentInParent<CesiumGeoreference>();
                if (georeference == null)
                    throw new InvalidOperationException("CesiumSubScene is not nested inside a game object with a CesiumGeoreference.");
              
                CesiumVector3 ecefPosition = new CesiumVector3()
                {
                    x = this._ecefX,
                    y = this._ecefY,
                    z = this._ecefZ
                };

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

                CesiumVector3 unityWorldPosition =
                    georeference.TransformEarthCenteredEarthFixedPositionToUnityWorld(ecefPosition);
                this._unityWorldPosition = new Vector3(
                    (float)unityWorldPosition.x,
                    (float)unityWorldPosition.y,
                    (float)unityWorldPosition.z);
            }
        }

        private void OnDrawGizmos()
        {
            if (this._showActivationRadius)
            {
                // TODO: would be nice to draw a better wireframe sphere.
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(this._unityWorldPosition, (float)this._activationRadius);
            }
        }
    }
}
