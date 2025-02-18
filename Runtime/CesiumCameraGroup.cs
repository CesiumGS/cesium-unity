using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    [Serializable]
    public class CesiumCameraGroup
    {
        [SerializeField]
        private int _layer = 0;

        public int layer
        {
            get => this._layer;
        }

        [SerializeField]
        private List<Camera> _cameras = new List<Camera>();

        public List<Camera> cameras
        {
            get => this._cameras;
        }
    }
}
