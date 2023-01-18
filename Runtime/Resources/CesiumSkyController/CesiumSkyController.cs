using UnityEngine;
using UnityEditor;
using CesiumForUnity;

[ExecuteInEditMode]
public class CesiumSkyController : MonoBehaviour
{
    /* TODO
    Support game camera as well as scene camera
    Add options for time of day, latitude, longitude

    */

    [SerializeField]
    Transform sunLight = default;

    [SerializeField]
    bool checkForCameraUpdates = false;

    //bool checkForSunUpdates = false;

    [SerializeField]
    [Range(0.0f, 24.0f)]
    public float timeOfDay = 12.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float northOffset = 0.0f;

    //[SerializeField]
    [Range(0.0f, 1.0f)]
    float groundSpaceBlend = 0.0f;

    float lastBlendValue;

    float groundBlendHeight = 2000.0f;
    float spaceBlendHeight = 800000.0f;

    Camera camera;
    CesiumGlobeAnchor globeAnchor;

    void Awake()
    {
        ResolveCamera();

    }

    void LateUpdate()
    {
        SetSunPosition();  
        if (checkForCameraUpdates) 
        {
            GetCameraHeight(); 

        }
    }

    void ResolveCamera()
    {
        if (Application.IsPlaying(gameObject))
        {
            camera = Camera.main;
            globeAnchor = camera.GetComponent<CesiumGlobeAnchor>();


        }
        else
        {
            if (SceneView.GetAllSceneCameras()[0] != null)
            {
                camera = SceneView.GetAllSceneCameras()[0];

            }

        }
    }

    void SetSunPosition()
    {
        float hourToAngle = ((timeOfDay*15.0f) - 90.0f);
        Vector3 newSunRotation = new Vector3(hourToAngle, northOffset * 360, 0);

        if (sunLight != null) {
            sunLight.transform.localEulerAngles = newSunRotation;
            Shader.SetGlobalVector("_SunDirection", -sunLight.transform.forward); 
        }
    }

    void GetCameraHeight()
    {
        if (camera != null)
        {
            float camHeight;
            if (globeAnchor)
            {
                camHeight = (float)globeAnchor.height;
            }
            else camHeight = camera.transform.position.y;


            if (camHeight <= groundBlendHeight)
            {
                groundSpaceBlend = 0.0f;
            }
            else if (camHeight >= spaceBlendHeight)
            {
                groundSpaceBlend = 1.0f;
            }
            else
            {
                groundSpaceBlend = 0.0f + (1.0f - 0.0f) * ((camHeight - groundBlendHeight) / (spaceBlendHeight - groundBlendHeight));
            }
            if (groundSpaceBlend != lastBlendValue)
            {
                Shader.SetGlobalFloat("_GroundSpaceBlend", groundSpaceBlend);
                lastBlendValue = groundSpaceBlend;

                Debug.Log("camera height is " + camHeight + ". Blend factor is " + groundSpaceBlend + ". Disable Check for Camera Updates on Sky Controller.");
            }
        }
        else ResolveCamera();

    }
}
