using Reinterop;

using UnityEngine;
using UnityEditor;
using CesiumForUnity;

namespace CesiumForUnity
{ 
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumSkyControllerImpl", "CesiumSkyControllerImpl.h")]
    public partial class CesiumSkyController : MonoBehaviour
    {
        [SerializeField]
        GameObject _sunLight = default;

        [SerializeField]
        private bool _updateOnTick = false;

        public bool updateOnTick
        {
            get => this._updateOnTick;
            set
            {
                this._updateOnTick = value;
            }
        }

        [SerializeField]
        bool _updateInEditor = false;

        public bool updateInEditor
        {
            get => this._updateInEditor;
            set
            {
                this._updateInEditor = value;
            }
        }

        [SerializeField]
        [Range(-90.0f, 90.0f)]
        private float _latitude = 0.0f;

        public float latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
            }
        }

        [SerializeField]
        [Range(-180.0f, 180.0f)]
        private float _longitude = 0.0f;

        public float longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
            }
        }

        [SerializeField]
        [Range(0.0f, 24.0f)]
        private float _timeOfDay = 12.0f;

        public float timeOfDay
        {
            get => this._timeOfDay;
            set
            {
                this._timeOfDay = value;
                this.SetSunPosition();
            }
        }

        // Pretty sure this should always be -90, so this should probably not be a variable and should instead be hardcoded into the native implementation.
        [SerializeField]
        [Range(-360.0f, 360.0f)]
        private float _northOffset = -90.0f;

        public float northOffset
        {
            get => this._northOffset;
            set
            {
                this._northOffset = value;
                this.UpdateSky();
            }
        }

        [SerializeField]
        [Range(1, 31)]
        private int _day = 1;

        public int day
        {
            get => this._day;
            set
            {
                this._day = value;
                this.UpdateSky();
            }
        }

        [SerializeField]
        [Range(1, 12)]
        private int _month = 6;

        public int month
        {
            get => this._month;
            set
            {
                this._month = value;
                this.UpdateSky();
            }
        }

        [SerializeField]
        private int _year = 2022;

        public int year
        {
            get => this._year;
            set
            {
                this._year = value;
                this.UpdateSky();
            }
        }

        [SerializeField]
        private float _timeZone = 0.0f;

        public float timeZone
        {
            get => this._timeZone;
            set
            {
                this._timeZone = value;
                this.UpdateSky();
            }
        }

        [SerializeField]
        private bool _useCesiumSkybox = true;

        public bool useCesiumSkybox
        {
            get => this._useCesiumSkybox;
            set
            {
                this._useCesiumSkybox = value;
                //this.ChangeSkyboxMaterial();
            }
        }

        //[SerializeField] // This can be serialized for easy testing of the skybox shader.
        [Range(0.0f, 1.0f)]
        private float groundSpaceBlend = 0.0f;

        private float lastBlendValue;

        [SerializeField]
        float _groundBlendHeight = 2000.0f;
        [SerializeField]
        float _spaceBlendHeight = 800000.0f;

        //Todo: Additional options for HDRP vs URP. In URP, skybox-specific options, including perhaps the ability to set the skybox used in the level. For HDRP, atmosphere scaling options for Physically Based Sky.

        Camera activeCamera;

        CesiumGlobeAnchor globeAnchor;

        public void UpdateSky()
        {
            SetSunPosition();
            if (_useCesiumSkybox)
            {        
                GetCameraHeight();

            }
        }

        public void ChangeSkyboxMaterial()
        {
            if (_useCesiumSkybox)
            {
                RenderSettings.skybox = Resources.Load("CesiumSkyController/CesiumDynamicSkybox", typeof(Material)) as Material;
                DynamicGI.UpdateEnvironment();
            }

        }

        void ResolveCamera()
        {
            if (Application.IsPlaying(gameObject))
            {
                activeCamera = Camera.main;
                globeAnchor = activeCamera.GetComponent<CesiumGlobeAnchor>();
            }
            else if (_updateInEditor)
            {
                SceneView sceneWindow = SceneView.lastActiveSceneView;
                if (sceneWindow)
                {
                    if (sceneWindow.camera != null)
                    {
                        activeCamera = sceneWindow.camera;

                    }
                }
            }
        }

        //public partial void CalculateSunPosition(float time) // This will be a partial class that is entirely implemented in Cesium Native
        //{
        // float hourToAngle = ((timeOfDay * 15.0f) - 90.0f);
        //  Vector3 positionToRotation = new Vector3(hourToAngle, _northOffset, 0);

        // return positionToRotation;
        // }
        private partial Vector3 CalculateSunPosition(float t);

        public void SetSunPosition()
        {
            Vector3 newSunRotation = CalculateSunPosition(timeOfDay);

            if (_sunLight != null)
            {
                _sunLight.transform.localEulerAngles = newSunRotation;
                Shader.SetGlobalVector("_SunDirection", -_sunLight.transform.forward);
            }
        }

        void GetCameraHeight()
        {
            if (activeCamera != null)
            {
                float camHeight;
                if (globeAnchor) camHeight = (float)globeAnchor.height;
                else camHeight = activeCamera.transform.position.y;


                if (camHeight <= _groundBlendHeight)
                {
                    groundSpaceBlend = 0.0f;
                }
                else if (camHeight >= _spaceBlendHeight)
                {
                    groundSpaceBlend = 1.0f;
                }
                else
                {
                    groundSpaceBlend = (camHeight - _groundBlendHeight) / (_spaceBlendHeight - _groundBlendHeight);
                    // Linear interpolation seems too abrupt here at the beginning and end of the blend. That, or the math is wrong. Perhaps a smoother interpolation, especially in space.
                }

                // TODO: Add a check to see if the scene is using the Cesium skybox material
                if (groundSpaceBlend != lastBlendValue)
                {
                    Shader.SetGlobalFloat("_GroundSpaceBlend", groundSpaceBlend);
                    lastBlendValue = groundSpaceBlend;

                    Debug.Log("camera height is " + camHeight + ". Blend factor is " + groundSpaceBlend + ". Disable Check for Camera Updates on Sky Controller.");
                }
            }
            else ResolveCamera();

        }

        void Awake()
        {

            ResolveCamera();

            // If the application has started and the directional light reference is not set, set it to the prefab's child Directional Light object.
            if (Application.IsPlaying(gameObject) && !_sunLight)
            {
                _sunLight = this.transform.Find("Directional Light").gameObject;

            }

        }

        void LateUpdate()
        { 
            if (updateOnTick) 
            {
                if (Application.IsPlaying(gameObject) || _updateInEditor)
                {
                    UpdateSky();
                }
            }
        }

 
    }
}