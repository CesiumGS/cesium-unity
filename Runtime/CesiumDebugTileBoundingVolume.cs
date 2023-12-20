using UnityEngine;
using Reinterop;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumDebugTileBoundingVolumeImpl", "CesiumDebugTileBoundingVolumeImpl.h")]
    public partial class CesiumDebugTileBoundingVolume : MonoBehaviour
    {
        private partial void OnEnable();

        private partial void OnDrawGizmos();
    }
}