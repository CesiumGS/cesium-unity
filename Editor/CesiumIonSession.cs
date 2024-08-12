using System.Collections.Generic;
using Reinterop;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonSessionImpl", "CesiumIonSessionImpl.h")]
    public partial class CesiumIonSession
    {
        public delegate void GUIUpdateDelegate();

        public static event GUIUpdateDelegate OnConnectionUpdated;
        public static event GUIUpdateDelegate OnAssetsUpdated;
        public static event GUIUpdateDelegate OnProfileUpdated;
        public static event GUIUpdateDelegate OnTokensUpdated;
        public static event GUIUpdateDelegate OnDefaultsUpdated;

        public CesiumIonServer server
        {
            get;
            internal set;
        }

        public CesiumIonSession(CesiumIonServer server)
        {
            this.server = server;
            this.CreateImplementation();
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

        public partial bool IsDefaultsLoaded();
        public partial bool IsLoadingDefaults();
        public partial bool IsAuthenticationRequired();

        public partial void Connect();
        public partial void Resume();
        public partial void Disconnect();

        public partial string GetProfileUsername();
        public partial string GetAuthorizeUrl();
        public partial string GetRedirectUrl();
        public partial List<QuickAddItem> GetQuickAddItems();

        public partial void Tick();

        public partial void RefreshProfile();
        public partial void RefreshTokens();
        public partial void RefreshAssets();
        public partial void RefreshDefaults();

        public void BroadcastConnectionUpdate()
        {
            if (OnConnectionUpdated != null)
            {
                OnConnectionUpdated();
            }
        }

        public void BroadcastAssetsUpdate()
        {
            if (OnAssetsUpdated != null)
            {
                OnAssetsUpdated();
            }
        }

        public void BroadcastProfileUpdate()
        {
            if (OnProfileUpdated != null)
            {
                OnProfileUpdated();
            }
        }

        public void BroadcastTokensUpdate()
        {
            if (OnTokensUpdated != null)
            {
                OnTokensUpdated();
            }
        }

        public void BroadcastDefaultsUpdate()
        {
            if (OnDefaultsUpdated != null)
            {
                OnDefaultsUpdated();
            }
        }
    }
}