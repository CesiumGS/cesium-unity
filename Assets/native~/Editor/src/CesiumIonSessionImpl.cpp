#include "CesiumIonSessionImpl.h"

#include "UnityAssetAccessor.h"
#include "UnityTaskProcessor.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>

#include <DotNet/UnityEditor/EditorPrefs.h>
#include <DotNet/UnityEngine/Application.h>

using namespace DotNet;

namespace CesiumForUnityNative {

	const std::string CesiumIonSessionImpl::userAccessTokenKey =
		"CesiumUserAccessToken";

	CesiumIonSessionImpl::CesiumIonSessionImpl(
		const DotNet::CesiumForUnity::CesiumIonSession& session)
		: _asyncSystem(
			CesiumAsync::AsyncSystem(std::make_shared<UnityTaskProcessor>())),
		_pAssetAccessor(std::make_shared<UnityAssetAccessor>()),
		_connection(std::nullopt),
		_profile(std::nullopt),
		_assets(std::nullopt),
		_tokens(std::nullopt),
		_isConnecting(false),
		_isResuming(false),
		_isLoadingProfile(false),
		_isLoadingAssets(false),
		_isLoadingTokens(false) {}

	CesiumIonSessionImpl::~CesiumIonSessionImpl() {}

	void CesiumIonSessionImpl::JustBeforeDelete(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {}

	bool CesiumIonSessionImpl::IsConnected(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_connection.has_value();
	}

	bool CesiumIonSessionImpl::IsConnecting(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_isConnecting;
	}

	bool CesiumIonSessionImpl::IsResuming(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_isResuming;
	}

	bool CesiumIonSessionImpl::IsProfileLoaded(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_profile.has_value();
	}

	bool CesiumIonSessionImpl::IsLoadingProfile(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_isLoadingProfile;
	}

	bool CesiumIonSessionImpl::IsAssetListLoaded(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_assets.has_value();
	}

	bool CesiumIonSessionImpl::IsLoadingAssetList(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_isLoadingAssets;
	}

	bool CesiumIonSessionImpl::IsTokenListLoaded(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_tokens.has_value();
	}

	bool CesiumIonSessionImpl::IsLoadingTokenList(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return this->_isLoadingTokens;
	}

	void CesiumIonSessionImpl::Connect(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		if (this->IsConnecting(session) || this->IsConnected(session) ||
			this->IsResuming(session)) {
			return;
		}

		this->_isConnecting = true;

		CesiumIonClient::Connection::authorize(
			this->_asyncSystem,
			this->_pAssetAccessor,
			"Cesium for Unity",
			381,
			"/cesium-for-unity/oauth2/callback",
			{ "assets:list",
			 "assets:read",
			 "profile:read",
			 "tokens:read",
			 "tokens:write",
			 "geocode" },
			[this](const std::string& url) {
			this->_authorizeUrl = url;
			UnityEngine::Application::OpenURL(url);
		})
			.thenInMainThread([this, session](CesiumIonClient::Connection&& connection) {
			this->_isConnecting = false;
			this->_connection = std::move(connection);

			UnityEditor::EditorPrefs::SetString(
				userAccessTokenKey,
				this->_connection.value().getAccessToken());

			/*CesiumSourceControl::PromptToCheckoutConfigFile(
				pSettings->GetClass()->GetConfigName());*/

			session.TriggerConnectionUpdate();
		})
			.catchInMainThread([this, session](std::exception&& e) {
			this->_isConnecting = false;
			this->_connection = std::nullopt;
			session.TriggerConnectionUpdate();
		});
	}

	void CesiumIonSessionImpl::Resume(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		if (this->IsConnecting(session) || this->IsConnected(session) ||
			this->IsResuming(session)) {
			return;
		}

		System::String userAccessToken =
			UnityEditor::EditorPrefs::GetString(System::String(userAccessTokenKey));

		if (userAccessToken == System::String("")) {
			// No user access token was stored, so there's no existing session to resume.
			return;
		}

		this->_isResuming = true;

		this->_connection = CesiumIonClient::Connection(
			this->_asyncSystem,
			this->_pAssetAccessor,
			userAccessToken.ToStlString());

		// Verify that the connection actually works.
		this->_connection.value()
			.me()
			.thenInMainThread(
				[this, session](
					CesiumIonClient::Response<CesiumIonClient::Profile>&& response) {
			if (!response.value.has_value()) {
				this->_connection.reset();
			}
			this->_isResuming = false;
			session.TriggerConnectionUpdate();
		})
			.catchInMainThread([this](std::exception&& e) {
			this->_isResuming = false;
			this->_connection.reset();
		});
	}

	void CesiumIonSessionImpl::Disconnect(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		this->_connection.reset();
		this->_profile.reset();
		this->_assets.reset();
		this->_tokens.reset();

		UnityEditor::EditorPrefs::DeleteKey(System::String(userAccessTokenKey));

		/* CesiumSourceControl::PromptToCheckoutConfigFile(
			pSettings->GetClass()->GetConfigName());*/

		session.TriggerConnectionUpdate();
		session.TriggerAssetsUpdate();
		session.TriggerProfileUpdate();
		session.TriggerTokensUpdate();
	}

	void CesiumIonSessionImpl::Tick(const DotNet::CesiumForUnity::CesiumIonSession& session) {
		this->_asyncSystem.dispatchMainThreadTasks();
	}

	System::String CesiumIonSessionImpl::GetProfileUsername(
		const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return System::String(this->getProfile().username);
	}

	System::String
		CesiumIonSessionImpl::GetAuthorizeUrl(const DotNet::CesiumForUnity::CesiumIonSession& session) {
		return System::String(this->_authorizeUrl);
	}

	void CesiumIonSessionImpl::refreshProfile() {
		if (!this->_connection || this->_isLoadingProfile) {
			// this->_loadProfileQueued = true;
			return;
		}

		this->_isLoadingProfile = true;
		// this->_loadProfileQueued = false;

		this->_connection->me()
			.thenInMainThread(
				[this](
					CesiumIonClient::Response<CesiumIonClient::Profile>&& profile) {
			this->_isLoadingProfile = false;
			this->_profile = std::move(profile.value);
			// this->ProfileUpdated.Broadcast();
			// this->refreshProfileIfNeeded();
		})
			.catchInMainThread([this](std::exception&& e) {
			this->_isLoadingProfile = false;
			this->_profile = std::nullopt;
			// this->ProfileUpdated.Broadcast();
			// this->refreshProfileIfNeeded();
		});
	}

	const CesiumIonClient::Profile& CesiumIonSessionImpl::getProfile() {
		static const CesiumIonClient::Profile empty{};
		if (this->_profile) {
			return *this->_profile;
		}
		else {
			this->refreshProfile();
			return empty;
		}
	}

} // namespace CesiumForUnityNative
