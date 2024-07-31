using System;
using UnityEngine;
using System.Collections.Generic;

namespace CesiumForUnity
{
    //add this component to a camera to identify the cameras to use by cesium
    public class CesiumCamera : MonoBehaviour
    {
        public static List<Camera> cameraList = new();

        private void OnEnable()
        {
            if (TryGetComponent<Camera>(out Camera camera))
            {
                cameraList.Add(camera);
            }
        }

        private void OnDisable()
        {
             if (TryGetComponent<Camera>(out Camera camera))
            {
                cameraList.Remove(camera);
            }
            
        }
    }
}