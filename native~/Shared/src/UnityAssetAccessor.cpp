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
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem,
                                      url,
                                      headers,
                                      &cesiumRequestHeaders =
                                      this->_cesiumRequestHeaders,
                                      &requestList = this->_activeRequests]() {
    UnityEngine::Networking::UnityWebRequest request =
        UnityEngine::Networking::UnityWebRequest::Get(System::String(url));

    DotNet::CesiumForUnity::NativeDownloadHandler handler{};
    request.downloadHandler(handler);

    HttpHeaders requestHeaders = cesiumRequestHeaders;
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

    auto assetRequest = std::make_shared<UnityAssetRequest>(request, requestHeaders, handler);
    // requestList.insertAtTail(*pAssetRequest);

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();

    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request,
         headers = std::move(requestHeaders),
         promise = std::move(promise),
         handler = std::move(handler),
         assetRequest
         ](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(assetRequest);
            // promise.resolve(std::make_shared<UnityAssetRequest>(request, headers, handler));
          } else {
            promise.reject(std::runtime_error(fmt::format(
                "Request for `{}` failed: {}",
                request.url().ToStlString(),
                request.error().ToStlString())));
          }
        }));

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


  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem,
                                      url,
                                      verb,
                                      headers,
                                      payloadBytes,
                                      &cesiumRequestHeaders =
                                          this->_cesiumRequestHeaders]() {
    DotNet::CesiumForUnity::NativeDownloadHandler downloadHandler{};
    UnityEngine::Networking::UploadHandlerRaw uploadHandler(payloadBytes, true);
    UnityEngine::Networking::UnityWebRequest request(
        System::String(url),
        System::String(verb),
        downloadHandler,
        uploadHandler);

    HttpHeaders requestHeaders = cesiumRequestHeaders;
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

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();
    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request,
         headers = std::move(requestHeaders),
         promise = std::move(promise),
         handler = std::move(downloadHandler)](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(std::make_shared<UnityAssetRequest>(request, headers, handler));
          } else {
            promise.reject(std::runtime_error(fmt::format(
                "Request for `{}` failed: {}",
                request.url().ToStlString(),
                request.error().ToStlString())));
          }
        }));

    return future;
  });
}

void UnityAssetAccessor::tick() noexcept {}

} // namespace CesiumForUnityNative
