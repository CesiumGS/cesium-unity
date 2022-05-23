using Cesium3DTilesSelection;
using UnityEngine;

namespace CesiumForUnity
{
    internal class UnityLogger : Cesium3DTilesSelection.Logger
    {
        public override void Log(int level, string message)
        {
            Debug.Log(message);
        }
    }
}
