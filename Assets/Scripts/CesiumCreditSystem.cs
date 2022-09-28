using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCreditSystemImpl", "CesiumCreditSystemImpl.h")]
    public partial class CesiumCreditSystem : MonoBehaviour
    {
        private partial void Awake();
        private partial void Update();
    }
}