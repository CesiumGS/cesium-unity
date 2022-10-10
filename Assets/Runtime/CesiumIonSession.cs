using Reinterop;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonSessionImpl", "CesiumIonSessionImpl.h")]
    public partial class CesiumIonSession
    {
        public static CesiumIonSession currentSession = null!;

        public delegate void GUIUpdateDelegate();
        public static event GUIUpdateDelegate OnConnectionUpdated;
        public static event GUIUpdateDelegate OnAssetsUpdated;
        public static event GUIUpdateDelegate OnTokensUpdated;

        public CesiumIonSession() {
        }

        public partial bool IsConnected();
        public partial bool IsConnecting();
        public partial bool IsResuming();
        
        public partial bool IsProfileLoaded();
        public partial bool IsLoadingProfile();

        public partial bool IsAssetListLoaded();
        public partial bool IsLoadingAssetList();

        public partial bool IsTokenListLoaded();
        public partial bool IsLoadingTokenList();

        public partial void Connect();
        public partial void Resume();
        public partial void Disconnect();

        public static void TriggerConnectionUpdate() {
            if (OnConnectionUpdated != null)
            {
                OnConnectionUpdated();
            }
        }

        public static void TriggerAssetsUpdate()
        {
            if (OnAssetsUpdated != null)
            {
                OnAssetsUpdated();
            }
        }

        public static void TriggerTokensUpdate()
        {
            if (OnTokensUpdated != null)
            {
                OnTokensUpdated();
            }
        }
    }
}