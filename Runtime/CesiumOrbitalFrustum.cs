using UnityEngine;
using Unity.Mathematics;
using System;

namespace CesiumForUnity
{
    public class CesiumOrbitalFrustum : MonoBehaviour
    {
        public static CesiumOrbitalFrustum Instance { get; set; }

        [SerializeField]
        private int viewPortPixelWidth = 1920;
        public int ViewPortPixelWidth { get => viewPortPixelWidth; set => viewPortPixelWidth = value; }
        [SerializeField]
        [Range(1, 180)]
        private double verticalFov = 60;
        public double VerticalFov { get => verticalFov; set => verticalFov = value; }
        [SerializeField]
        [Range(1, 180)]
        private double aspectRatio = 1.775193798;
        public double AspectRatio { get => aspectRatio; set => aspectRatio = value; }
        [SerializeField]
        private double3 cartographicPosition { get; set; } = new double3(0, 0, 4_000_000);
        public double3 CartographicPosition { get => cartographicPosition; set => cartographicPosition = value; } 

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"Duplicate singleton: CesiumCameraProvider");
                return;
            }

            Instance = this;
        }

        public void SetCartographicPosition(double3 cartographicPositionDegrees)
        {
            CartographicPosition = cartographicPositionDegrees;
        }
    }
}