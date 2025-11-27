#include "UnityAssetAccessor.h"

#include "Cesium.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetResponse.h>
#include <CesiumUtility/ScopeGuard.h>

#include <DotNet/CesiumForUnity/Helpers.h>
#include <DotNet/CesiumForUnity/NativeDownloadHandler.h>
#include <DotNet/System/Action1.h>
#include <DotNet/System/Collections/Generic/Dictionary2.h>
#include <DotNet/System/Collections/Generic/Enumerator0.h>
#include <DotNet/System/Collections/Generic/KeyValuePair2.h>
#include <DotNet/System/Environment.h>
#include <DotNet/System/OperatingSystem.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Collections/Allocator.h>
#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/Unity/Collections/NativeArrayOptions.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Networking/DownloadHandler.h>
#include <DotNet/UnityEngine/Networking/Result.h>
#include <DotNet/UnityEngine/Networking/UnityWebRequest.h>
#include <DotNet/UnityEngine/Networking/UnityWebRequestAsyncOperation.h>
#include <DotNet/UnityEngine/Networking/UploadHandler.h>
#include <DotNet/UnityEngine/Networking/UploadHandlerRaw.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/AssemblyReloadCallback.h>
#include <DotNet/UnityEditor/AssemblyReloadEvents.h>
#endif

#include <algorithm>

using namespace CesiumAsync;
using namespace CesiumUtility;
using namespace DotNet;

namespace {

std::string replaceInvalidChars(const std::string& input) {
  std::string result(input.size(), '?');
  std::transform(
      input.cbegin(),
      input.cend(),
      result.begin(),
      [](unsigned char c) { return (c >= 32 && c <= 126) ? c : '?'; });
  return result;
}

} // namespace

namespace CesiumForUnityNative {

UnityAssetRequest::UnityAssetRequest(
    const std::shared_ptr<UnityAssetAccessor>& pAccessor,
    DotNet::UnityEngine::Networking::UnityWebRequest&& request,
    HttpHeaders&& headers,
    Promise<std::shared_ptr<IAssetRequest>>&& promise)
    : _method(request.method().ToStlString()),
      _url(request.url().ToStlString()),
      _headers(std::move(headers)),
      _webRequest(std::move(request)),
      _promise(std::move(promise)),
      _pAccessor(pAccessor),
      _state(State::Pending),
      _maybeResponse() {
  // std::lock_guard<std::mutex> lock(requestMutex);
  // _activeRequests.insertAtTail(*this);
}

void UnityAssetRequest::start() {
  DotNet::CesiumForUnity::NativeDownloadHandler handler{};
  this->_webRequest.downloadHandler(handler);

  UnityEngine::Networking::UnityWebRequestAsyncOperation op =
      this->_webRequest.SendWebRequest();

  op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
      [handler = std::move(handler), thiz = this->shared_from_this()](
          const UnityEngine::AsyncOperation& operation) mutable {
        ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};

        State expected = State::Pending;
        if (thiz->_state.compare_exchange_strong(expected, State::Completed)) {
          if (thiz->_webRequest.isDone() &&
              thiz->_webRequest.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            thiz->_maybeResponse = std::make_optional<UnityAssetResponse>(
                thiz->_webRequest,
                handler);
            thiz->_promise.resolve(thiz);
          } else {
            thiz->_promise.reject(std::runtime_error(fmt::format(
                "Request for `{}` failed: {}",
                thiz->_webRequest.url().ToStlString(),
                thiz->_webRequest.error().ToStlString())));
          }
        }
      }));
}

const std::string& UnityAssetRequest::method() const { return this->_method; }

const std::string& UnityAssetRequest::url() const { return this->_url; }

const CesiumAsync::HttpHeaders& UnityAssetRequest::headers() const {
  return this->_headers;
}

const CesiumAsync::IAssetResponse* UnityAssetRequest::response() const {
  if (!this->_maybeResponse) {
    return nullptr;
  }

  return &*this->_maybeResponse;
}

const bool UnityAssetRequest::isCanceled() const {
  return this->_state == State::Canceled;
}

void UnityAssetRequest::cancel() {
  State expected = State::Pending;
  if (this->_state.compare_exchange_strong(expected, State::Canceled)) {
    // TODO: Unity probably requires us to call this on the main thread.
    // this->_webRequest.Abort();

    this->_promise.reject(std::runtime_error("Request was canceled."));
  }
}

UnityAssetRequest::~UnityAssetRequest() {
  this->cancel();
  this->_pAccessor->notifyRequestDestroyed(*this);
}

UnityAssetAccessor::UnityAssetAccessor() : _cesiumRequestHeaders() {
  std::string version = CesiumForUnityNative::Cesium::version + " " +
                        CesiumForUnityNative::Cesium::commit;
  std::string projectName = replaceInvalidChars(
      UnityEngine::Application::productName().ToStlString());
  std::string engine =
      "Unity " + UnityEngine::Application::unityVersion().ToStlString() + " " +
      CesiumForUnity::Helpers::ToString(UnityEngine::Application::platform())
          .ToStlString();
  std::string osVersion =
      System::Environment::OSVersion().VersionString().ToStlString();

  this->_cesiumRequestHeaders.insert({"X-Cesium-Client", "Cesium For Unity"});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Version", version});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Project", projectName});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Engine", engine});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-OS", osVersion});

#if UNITY_EDITOR
  UnityEditor::AssemblyReloadEvents::add_beforeAssemblyReload(
      UnityEditor::AssemblyReloadCallback(
          [this]() { this->cancelActiveRequests(); }));
#endif
}

void UnityAssetAccessor::cancelActiveRequests() {
  std::lock_guard<std::mutex> lock(this->_assetRequestMutex);

  UnityAssetRequest* p = this->_activeRequests.head();
  while (p) {
    UnityAssetRequest* pNext = this->_activeRequests.next(*p);
    p->cancel();
    p = pNext;
  }
}

UnityAssetAccessor::~UnityAssetAccessor() { this->cancelActiveRequests(); }

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {
  std::shared_ptr<UnityAssetAccessor> thiz = this->shared_from_this();

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem, url, headers, thiz]() {
    UnityEngine::Networking::UnityWebRequest request =
        UnityEngine::Networking::UnityWebRequest::Get(System::String(url));

    HttpHeaders requestHeaders = thiz->_cesiumRequestHeaders;
    for (const auto& header : headers) {
      requestHeaders.insert(header);
    }
    for (const auto& header : requestHeaders) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    auto pAssetRequest = std::make_shared<UnityAssetRequest>(
        thiz,
        std::move(request),
        std::move(requestHeaders),
        std::move(promise));
    pAssetRequest->start();

    return future;
  });
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::request(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& verb,
    const std::string& url,
    const std::vector<THeader>& headers,
    const std::span<const std::byte>& contentPayload) {
  if (contentPayload.size() >
      size_t(std::numeric_limits<std::int32_t>::max())) {
    // This implementation cannot be used to send more than 2 gigabytes - just
    // fail.
    return asyncSystem
        .createResolvedFuture<std::shared_ptr<CesiumAsync::IAssetRequest>>(
            nullptr);
  }

  Unity::Collections::NativeArray1<std::uint8_t> payloadBytes(
      std::int32_t(contentPayload.size()),
      Unity::Collections::Allocator::Persistent,
      Unity::Collections::NativeArrayOptions::UninitializedMemory);
  std::byte* pDest = static_cast<std::byte*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(payloadBytes));
  std::memcpy(pDest, contentPayload.data(), contentPayload.size());

  std::shared_ptr<UnityAssetAccessor> thiz = this->shared_from_this();

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread(
      [asyncSystem, url, verb, headers, payloadBytes, thiz]() {
        DotNet::CesiumForUnity::NativeDownloadHandler downloadHandler{};
        UnityEngine::Networking::UploadHandlerRaw uploadHandler(
            payloadBytes,
            true);
        UnityEngine::Networking::UnityWebRequest request(
            System::String(url),
            System::String(verb),
            downloadHandler,
            uploadHandler);

        HttpHeaders requestHeaders = thiz->_cesiumRequestHeaders;
        for (const auto& header : headers) {
          requestHeaders.insert(header);
        }

        for (const auto& header : requestHeaders) {
          request.SetRequestHeader(
              System::String(header.first),
              System::String(header.second));
        }

        auto promise =
            asyncSystem
                .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

        auto future = promise.getFuture();

        auto pAssetRequest = std::make_shared<UnityAssetRequest>(
            thiz,
            std::move(request),
            std::move(requestHeaders),
            std::move(promise));
        pAssetRequest->start();

        return future;
      });
}

void UnityAssetAccessor::tick() noexcept {}

void UnityAssetAccessor::notifyRequestDestroyed(
    UnityAssetRequest& request) noexcept {
  std::lock_guard<std::mutex> lock(this->_assetRequestMutex);
  this->_activeRequests.remove(request);
}

} // namespace CesiumForUnityNative
