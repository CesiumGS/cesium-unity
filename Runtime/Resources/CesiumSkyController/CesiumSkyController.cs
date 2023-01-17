using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class CesiumSkyController : MonoBehaviour
{
    /* TODO
    Support game camera as well as scene camera
    Update skybox when outside of the blend min/max - perhaps on GUI update
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

    float groundBlendHeight = 2000.0f;
    float spaceBlendHeight = 200000.0f;

    new Camera camera;

    void LateUpdate()
    {
        SetSunPosition();  
        //Shader.SetGlobalFloat("_GroundSpaceBlend", groundSpaceBlend);
        if (checkForCameraUpdates) 
        {
            GetCameraHeight(); 

        }
        
    }

    void SetSunPosition()
    {
        float hourToAngle = ((timeOfDay*15.0f) - 90.0f);
        Vector3 newSunRotation = new Vector3(hourToAngle, northOffset, 0);

        if (sunLight != null) {
            sunLight.transform.localEulerAngles = new Vector3(hourToAngle, 0, 0);
            Shader.SetGlobalVector("_SunDirection", -sunLight.transform.forward); 
        }
    }

    void GetCameraHeight()
    {
        if (SceneView.GetAllSceneCameras()[0] != null)
        {
            camera = SceneView.GetAllSceneCameras()[0];
            //Debug.Log("Scene camera position is " + camera.transform.position + ". Disable Check for Camera Updates on Sky Controller.");

        }
        if (camera != null)
        {
            float camHeight = camera.transform.position.y;
            if (camHeight > groundBlendHeight && camHeight < spaceBlendHeight)
            {
                groundSpaceBlend = 0.0f + (1.0f - 0.0f) * ((camHeight - groundBlendHeight) / (spaceBlendHeight - groundBlendHeight));
                Shader.SetGlobalFloat("_GroundSpaceBlend", groundSpaceBlend);
            }
        }

    }
}
