using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#if UNITY_IOS || UNITY_ANDROID
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
#endif
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// A camera controller that can easily move around and view the globe while 
    /// maintaining a sensible orientation. As the camera moves across the horizon, 
    /// it automatically changes its own up direction such that the world always 
    /// looks right-side up.
    /// </summary>
    [RequireComponent(typeof(CesiumOriginShift))]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Cesium/Cesium Camera Controller")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumCameraController : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        private bool _enableMovement = true;

        /// <summary>
        /// Whether movement is enabled on this controller. Movement is
        /// controlled using the W, A, S, D keys, as well as the Q and E
        /// keys for vertical movement with respect to the globe.
        /// </summary>
        public bool enableMovement
        {
            get => this._enableMovement;
            set
            {
                this._enableMovement = value;
                this.ResetSpeed();
            }
        }

        [SerializeField]
        private bool _enableRotation = true;

        /// <summary>
        /// Whether rotation is enabled on this controller. Rotation is
        /// controlled by movement of the mouse.
        /// </summary>
        public bool enableRotation
        {
            get => this._enableRotation;
            set => this._enableRotation = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _defaultMaximumSpeed = 100.0f;

        /// <summary>
        /// The maximum speed of this controller when dynamic speed is disabled.
        /// If dynamic speed is enabled, this value will not be used.
        /// </summary>
        public float defaultMaximumSpeed
        {
            get => this._defaultMaximumSpeed;
            set => this._defaultMaximumSpeed = Mathf.Max(value, 0.0f);
        }

        [SerializeField]
        private bool _enableDynamicSpeed = true;

        /// <summary>
        /// Whether to enable dynamic speed on this controller. If enabled, 
        /// the controller's speed will change dynamically based on elevation 
        /// and other factors.
        /// </summary>
        public bool enableDynamicSpeed
        {
            get => this._enableDynamicSpeed;
            set => this._enableDynamicSpeed = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicSpeedMinHeight = 20.0f;

        /// <summary>
        /// The minimum height where dynamic speed starts to take effect.
        /// Below this height, the speed will be set to the object's height 
        /// from the Earth, which makes it move slowly when it is right above a tileset.
        /// </summary>
        public float dynamicSpeedMinHeight
        {
            get => this._dynamicSpeedMinHeight;
            set => this._dynamicSpeedMinHeight = Mathf.Max(value, 0.0f);
        }

        [SerializeField]
        private bool _enableDynamicClippingPlanes = true;

        /// <summary>
        /// Whether to dynamically adjust the camera's clipping planes so that
        /// the globe will not be clipped from far away. Objects that are close 
        /// to the camera but far above the globe in space may not appear.
        /// </summary>
        public bool enableDynamicClippingPlanes
        {
            get => this._enableDynamicClippingPlanes;
            set => this._enableDynamicClippingPlanes = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicClippingPlanesMinHeight = 10000.0f;

        /// <summary>
        /// The height to start dynamically adjusting the camera's clipping planes. 
        /// Below this height, the clipping planes will be set to their initial values.
        /// </summary>
        public float dynamicClippingPlanesMinHeight
        {
            get => this._dynamicClippingPlanesMinHeight;
            set => this._dynamicClippingPlanesMinHeight = Mathf.Max(value, 0.0f);
        }

        #endregion

        #region Private variables

        private Camera _camera;
        private float _initialNearClipPlane;
        private float _initialFarClipPlane;

        private CharacterController _controller;
        private CesiumGeoreference _georeference;
        private CesiumGlobeAnchor _globeAnchor;

        private Vector3 _velocity = Vector3.zero;
        private float _lookSpeed = 10.0f;

        // These numbers are borrowed from Cesium for Unreal.
        private float _acceleration = 20000.0f;
        private float _deceleration = 9999999959.0f;
        private float _maxRaycastDistance = 1000 * 1000; // 1000 km;

        private float _maxSpeed = 100.0f; // Maximum speed with the speed multiplier applied.
        private float _maxSpeedPreMultiplier = 0.0f; // Max speed without the multiplier applied.
        private AnimationCurve _maxSpeedCurve;

        private float _speedMultiplier = 1.0f;
        private float _speedMultiplierIncrement = 1.5f;

        // If the near clip gets too large, Unity will throw errors. Keeping it 
        // at this value works fine even when the far clip plane gets large.
        private float _maximumNearClipPlane = 1000.0f;
        private float _maximumFarClipPlane = 500000000.0f;

        // The maximum ratio that the far clip plane is allowed to be larger
        // than the near clip plane. The near clip plane is set so that this
        // ratio is never exceeded.
        private float _maximumNearToFarRatio = 100000.0f;

        #endregion

        #region Input configuration

#if ENABLE_INPUT_SYSTEM
        InputAction lookAction;
        InputAction moveAction;
        InputAction moveUpAction;
        InputAction speedChangeAction;
        InputAction speedResetAction;
        InputAction toggleDynamicSpeedAction;

        void ConfigureInputs()
        {
#if UNITY_IOS || UNITY_ANDROID
            EnhancedTouch.EnhancedTouchSupport.Enable();
#endif
            InputActionMap map = new InputActionMap("Cesium Camera Controller");

            lookAction = map.AddAction("look", binding: "<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");

            moveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveUpAction = map.AddAction("moveUp");
            moveUpAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/space")
                .With("Down", "<Keyboard>/c")
                .With("Up", "<Keyboard>/e")
                .With("Down", "<Keyboard>/q")
                .With("Up", "<Gamepad>/rightTrigger")
                .With("Down", "<Gamepad>/leftTrigger");

            speedChangeAction = map.AddAction("speedChange", binding: "<Mouse>/scroll");
            speedChangeAction.AddCompositeBinding("Dpad")
                .With("Up", "<Gamepad>/rightShoulder")
                .With("Down", "<Gamepad>/leftShoulder");

            speedResetAction = map.AddAction("speedReset", binding: "<Mouse>/middleButton");
            speedResetAction.AddBinding("<Gamepad>/buttonNorth");

            toggleDynamicSpeedAction =
                map.AddAction("toggleDynamicSpeed", binding: "<Keyboard>/g");
            toggleDynamicSpeedAction.AddBinding("<Gamepad>/buttonEast");

            moveAction.Enable();
            lookAction.Enable();
            moveUpAction.Enable();
            speedChangeAction.Enable();
            speedResetAction.Enable();
            toggleDynamicSpeedAction.Enable();
        }
#endif

        #endregion

        #region Initialization

        void InitializeCamera()
        {
            this._camera = this.gameObject.GetComponent<Camera>();
            this._initialNearClipPlane = this._camera.nearClipPlane;
            this._initialFarClipPlane = this._camera.farClipPlane;
        }

        void InitializeController()
        {
            if (this.gameObject.GetComponent<CharacterController>() != null)
            {
                Debug.LogWarning(
                    "A CharacterController component was manually " +
                    "added to the CesiumCameraController's game object. " +
                    "This may interfere with the CesiumCameraController's movement.");

                this._controller = this.gameObject.GetComponent<CharacterController>();
            }
            else
            {
                this._controller = this.gameObject.AddComponent<CharacterController>();
                this._controller.hideFlags = HideFlags.HideInInspector;
            }

            this._controller.radius = 1.0f;
            this._controller.height = 1.0f;
            this._controller.center = Vector3.zero;
            this._controller.detectCollisions = true;
        }

        /// <summary>
        /// Creates a curve to control the bounds of the maximum speed before it is
        /// multiplied by the speed multiplier. This prevents the camera from achieving 
        /// an unreasonably low or high speed.
        /// </summary>
        private void CreateMaxSpeedCurve()
        {
            // This creates a curve that is linear between the first two keys,
            // then smoothly interpolated between the last two keys.
            Keyframe[] keyframes = {
                new Keyframe(0.0f, 4.0f),
                new Keyframe(10000000.0f, 10000000.0f),
                new Keyframe(13000000.0f, 2000000.0f)
            };

            keyframes[0].weightedMode = WeightedMode.Out;
            keyframes[0].outTangent = keyframes[1].value / keyframes[0].value;
            keyframes[0].outWeight = 0.0f;

            keyframes[1].weightedMode = WeightedMode.In;
            keyframes[1].inWeight = 0.0f;
            keyframes[1].inTangent = keyframes[1].value / keyframes[0].value;
            keyframes[1].outTangent = 0.0f;

            keyframes[2].inTangent = 0.0f;

            this._maxSpeedCurve = new AnimationCurve(keyframes);
            this._maxSpeedCurve.preWrapMode = WrapMode.ClampForever;
            this._maxSpeedCurve.postWrapMode = WrapMode.ClampForever;
        }

        void Awake()
        {
            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (this._georeference == null)
            {
                Debug.LogError(
                    "CesiumCameraController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            // CesiumOriginShift will add a CesiumGlobeAnchor automatically.
            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();

            this.InitializeCamera();
            this.InitializeController();
            this.CreateMaxSpeedCurve();

#if ENABLE_INPUT_SYSTEM
            this.ConfigureInputs();
#endif
        }

        #endregion

        #region Update

        void Update()
        {
            this.HandlePlayerInputs();

            if (this._enableDynamicClippingPlanes)
            {
                this.UpdateClippingPlanes();
            }
        }

        #endregion

        #region Raycasting helpers

        private bool RaycastTowardsEarthCenter(out float hitDistance)
        {
            double3 center =
                this._georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));

            RaycastHit hitInfo;
            if (Physics.Linecast(this.transform.position, (float3)center, out hitInfo))
            {
                hitDistance = Vector3.Distance(this.transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        private bool RaycastAlongForwardVector(float raycastDistance, out float hitDistance)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(
                this.transform.position,
                this.transform.forward,
                out hitInfo,
                raycastDistance))
            {
                hitDistance = Vector3.Distance(this.transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        #endregion

        #region Player movement

        private void HandlePlayerInputs()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 lookDelta;
            Vector2 moveDelta;
            lookDelta = lookAction.ReadValue<Vector2>();
            moveDelta = moveAction.ReadValue<Vector2>();

#if UNITY_IOS || UNITY_ANDROID
            bool handledMove = false;
            bool handledLook = false;

            foreach(var touch in EnhancedTouch.Touch.activeTouches)
            {
                if(touch.startScreenPosition.x < Screen.width / 2)
                {
                    if(!handledMove)
                    {
                        handledMove = true;
                        moveDelta = touch.screenPosition - touch.startScreenPosition;
                    }
                }
                else
                {
                    if(!handledLook)
                    {
                        handledLook = true;
                        lookDelta = touch.delta;
                    }
                }
            }
#endif

            float inputRotateHorizontal = lookDelta.x;
            float inputRotateVertical = lookDelta.y;

            float inputForward = moveDelta.y;
            float inputRight = moveDelta.x;

            float inputUp = moveUpAction.ReadValue<Vector2>().y;

            float inputSpeedChange = speedChangeAction.ReadValue<Vector2>().y;
            bool inputSpeedReset = speedResetAction.ReadValue<float>() > 0.5f;

            bool toggleDynamicSpeed = toggleDynamicSpeedAction.ReadValue<float>() > 0.5f;
#else
            float inputRotateHorizontal = Input.GetAxis("Mouse X");
            float inputRotateVertical = Input.GetAxis("Mouse Y");

            float inputForward = Input.GetAxis("Vertical");
            float inputRight = Input.GetAxis("Horizontal");
            float inputUp = 0.0f;

            if (Input.GetKeyDown("q"))
            {
                inputUp -= 1.0f;
            }

            if (Input.GetKeyDown("e"))
            {
                inputUp += 1.0f;
            }

            float inputSpeedChange = Input.GetAxis("Mouse ScrollWheel");
            bool inputSpeedReset =
                Input.GetMouseButtonDown(2) || Input.GetKeyDown("joystick button 3");

            bool toggleDynamicSpeed =
                Input.GetKeyDown("g") || Input.GetKeyDown("joystick button 1");
#endif

            Vector3 movementInput = new Vector3(inputRight, inputUp, inputForward);

            if (this._enableRotation)
            {
                this.Rotate(inputRotateHorizontal, inputRotateVertical);
            }

            if (toggleDynamicSpeed)
            {
                this._enableDynamicSpeed = !this._enableDynamicSpeed;
            }

            if (inputSpeedReset ||
                (this._enableDynamicSpeed && movementInput == Vector3.zero))
            {
                this.ResetSpeedMultiplier();
            }
            else
            {
                this.HandleSpeedChange(inputSpeedChange);
            }

            if (this._enableMovement)
            {
                this.Move(movementInput);
            }
        }

        private void HandleSpeedChange(float speedChangeInput)
        {
            if (this._enableDynamicSpeed)
            {
                this.UpdateDynamicSpeed();
            }
            else
            {
                this.SetMaxSpeed(this._defaultMaximumSpeed);
            }

            if (speedChangeInput != 0.0f)
            {
                if (speedChangeInput > 0.0f)
                {
                    this._speedMultiplier *= this._speedMultiplierIncrement;
                }
                else
                {
                    this._speedMultiplier /= this._speedMultiplierIncrement;
                }

                float max = this._enableDynamicSpeed ? 50.0f : 50000.0f;
                this._speedMultiplier = Mathf.Clamp(this._speedMultiplier, 0.1f, max);
            }
        }

        /// <summary>
        /// Rotate the camera with the specified amounts.
        /// </summary>
        /// <remarks>
        /// Horizontal rotation (i.e. looking left or right) corresponds to rotation around the Y-axis.
        /// Vertical rotation (i.e. looking up or down) corresponds to rotation around the X-axis.
        /// </remarks>
        /// <param name="horizontalRotation">The amount to rotate horizontally, i.e. around the Y-axis.</param>
        /// <param name="verticalRotation">The amount to rotate vertically, i.e. around the X-axis.</param>
        private void Rotate(float horizontalRotation, float verticalRotation)
        {
            if (horizontalRotation == 0.0f && verticalRotation == 0.0f)
            {
                return;
            }

            float valueX = verticalRotation * this._lookSpeed * Time.smoothDeltaTime;
            float valueY = horizontalRotation * this._lookSpeed * Time.smoothDeltaTime;

            // Rotation around the X-axis occurs counter-clockwise, so the look range
            // maps to [270, 360] degrees for the upper quarter-sphere of motion, and
            // [0, 90] degrees for the lower. Euler angles only work with positive values,
            // so map the [0, 90] range to [360, 450] so the entire range is [270, 450].
            // This makes it easy to clamp the values.
            float rotationX = this.transform.localEulerAngles.x;
            if (rotationX <= 90.0f)
            {
                rotationX += 360.0f;
            }

            float newRotationX = Mathf.Clamp(rotationX - valueX, 270.0f, 450.0f);
            float newRotationY = this.transform.localEulerAngles.y + valueY;
            this.transform.localRotation =
                Quaternion.Euler(newRotationX, newRotationY, this.transform.localEulerAngles.z);
        }

        /// <summary>
        /// Moves the controller with the given player input.
        /// </summary>
        /// <remarks>
        /// The x-coordinate affects movement along the transform's right axis.
        /// The y-coordinate affects movement along the georeferenced up axis.
        /// The z-coordinate affects movement along the transform's forward axis.
        /// </remarks>
        /// <param name="movementInput">The player input.</param>
        private void Move(Vector3 movementInput)
        {
            Vector3 inputDirection =
                this.transform.right * movementInput.x + this.transform.forward * movementInput.z;

            if (this._georeference != null)
            {
                double3 positionECEF = this._globeAnchor.positionGlobeFixed;
                double3 upECEF = CesiumWgs84Ellipsoid.GeodeticSurfaceNormal(positionECEF);
                double3 upUnity =
                    this._georeference.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

                inputDirection = (float3)inputDirection + (float3)upUnity * movementInput.y;
            }

            if (inputDirection != Vector3.zero)
            {
                // If the controller was already moving, handle the direction change
                // separately from the magnitude of the velocity.
                if (this._velocity.magnitude > 0.0f)
                {
                    Vector3 directionChange = inputDirection - this._velocity.normalized;
                    this._velocity +=
                        directionChange * this._velocity.magnitude * Time.deltaTime;
                }

                this._velocity += inputDirection * this._acceleration * Time.deltaTime;
                this._velocity = Vector3.ClampMagnitude(this._velocity, this._maxSpeed);
            }
            else
            {
                // Decelerate
                float speed = Mathf.Max(
                    this._velocity.magnitude - this._deceleration * Time.deltaTime,
                    0.0f);

                this._velocity = Vector3.ClampMagnitude(this._velocity, speed);
            }

            if (this._velocity != Vector3.zero)
            {
                this._controller.Move(this._velocity * Time.deltaTime);

                // Other controllers may disable detectTransformChanges to control their own
                // movement, but the globe anchor should be synced even if detectTransformChanges
                // is false.
                if (!this._globeAnchor.detectTransformChanges)
                {
                    this._globeAnchor.Sync();
                }
            }
        }

        #endregion

        #region Dynamic speed computation

        /// <summary>
        /// Gets the dynamic speed of the controller based on the camera's height from 
        /// the earth's center and its distance from objects along the forward vector.
        /// </summary>
        /// <param name="overrideSpeed">Whether the returned speed should override the 
        /// previous speed, even if the new value is lower.</param>
        /// <param name="newSpeed">The new dynamic speed of the controller.</param>
        /// <returns>Whether a valid speed value was found.</returns>
        private bool GetDynamicSpeed(out bool overrideSpeed, out float newSpeed)
        {
            if (this._georeference == null)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            float height, viewDistance;

            // Raycast from the camera to the Earth's center and compute the distance.
            // Ignore the result if the height is approximately 0.
            if (!this.RaycastTowardsEarthCenter(out height) || height < 0.000001f)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            // Also ignore the result if the speed will increase or decrease by too much at once.
            // This can be an issue when 3D tiles are loaded/unloaded from the scene.
            if (this._maxSpeedPreMultiplier > 0.5f)
            {
                float heightToMaxSpeedRatio = height / this._maxSpeedPreMultiplier;

                // The asymmetry of these ratios is intentional. When traversing tilesets
                // with many height differences (e.g. a city with tall buildings), flying over
                // taller geometry will cause the camera to slow down suddenly, and sometimes
                // cause it to stutter.
                if (heightToMaxSpeedRatio > 1000.0f || heightToMaxSpeedRatio < 0.01f)
                {
                    overrideSpeed = false;
                    newSpeed = 0.0f;

                    return false;
                }
            }

            // Raycast along the camera's view (forward) vector.
            float raycastDistance =
                Mathf.Clamp(this._maxSpeed * 3.0f, 0.0f, this._maxRaycastDistance);

            // If the raycast does not hit, then only override speed if the height
            // is lower than the maximum threshold. Otherwise, if both raycasts hit,
            // always override the speed.
            if (!this.RaycastAlongForwardVector(raycastDistance, out viewDistance) ||
                viewDistance < 0.000001f)
            {
                overrideSpeed = height <= this._dynamicSpeedMinHeight;
            }
            else
            {
                overrideSpeed = true;
            }

            // Set the speed to be the height of the camera from the Earth's center.
            newSpeed = height;

            return true;
        }

        private void ResetSpeedMultiplier()
        {
            this._speedMultiplier = 1.0f;
        }

        private void SetMaxSpeed(float speed)
        {
            float actualSpeed = this._maxSpeedCurve.Evaluate(speed);
            this._maxSpeed = this._speedMultiplier * actualSpeed;
            this._acceleration =
                Mathf.Clamp(this._maxSpeed * 5.0f, 20000.0f, 10000000.0f);
        }

        private void UpdateDynamicSpeed()
        {
            bool overrideSpeed;
            float newSpeed;
            if (this.GetDynamicSpeed(out overrideSpeed, out newSpeed))
            {
                if (overrideSpeed || newSpeed >= this._maxSpeedPreMultiplier)
                {
                    this._maxSpeedPreMultiplier = newSpeed;
                }
            }

            this.SetMaxSpeed(this._maxSpeedPreMultiplier);
        }

        private void ResetSpeed()
        {
            this._maxSpeed = this._defaultMaximumSpeed;
            this._maxSpeedPreMultiplier = 0.0f;
            this.ResetSpeedMultiplier();
        }

        #endregion

        #region Dynamic clipping plane adjustment

        private void UpdateClippingPlanes()
        {
            if (this._camera == null)
            {
                return;
            }

            // Raycast from the camera to the Earth's center and compute the distance.
            float height = 0.0f;
            if (!this.RaycastTowardsEarthCenter(out height))
            {
                return;
            }

            float nearClipPlane = this._initialNearClipPlane;
            float farClipPlane = this._initialFarClipPlane;

            if (height >= this._dynamicClippingPlanesMinHeight)
            {
                farClipPlane = height + (float)(2.0 * CesiumWgs84Ellipsoid.GetMaximumRadius());
                farClipPlane = Mathf.Min(farClipPlane, this._maximumFarClipPlane);

                float farClipRatio = farClipPlane / this._maximumNearToFarRatio;

                if (farClipRatio > nearClipPlane)
                {
                    nearClipPlane = Mathf.Min(farClipRatio, this._maximumNearClipPlane);
                }
            }

            this._camera.nearClipPlane = nearClipPlane;
            this._camera.farClipPlane = farClipPlane;
        }

        #endregion
    }
}
