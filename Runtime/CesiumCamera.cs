using System;
using UnityEngine;

namespace CesiumForUnity
{
    //add this component to a camera to identify the camera to use by cesium
    public class CesiumCamera : MonoBehaviour
    {
        public static Camera camera;

        private void OnEnable()
        {
            //try to get the camera if any; 
            camera = GetComponent<Camera>();
            
        }
    }
}