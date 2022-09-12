using System;
using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGeoreferenceImpl", "CesiumGeoreferenceImpl.h")]
    public partial class CesiumGeoreference : MonoBehaviour
    {
        [SerializeField]
        [Header("Origin")]
        [Tooltip("The latitude of the origin in degrees, in the range [-90, 90].")]
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
        [Tooltip("The height of the origin in meters above the ellipsoid.")]
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

        [Tooltip("An event raised when the georeference changes.")]
        public event Action? changed;

        public void UpdateOrigin()
        {
            this.RecalculateOrigin();
            if (this.changed != null)
            {
                this.changed();
            }
        }

        private partial void RecalculateOrigin();

        private partial void OnValidate();
    }
}