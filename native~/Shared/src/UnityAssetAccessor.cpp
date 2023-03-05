#include "UnityAssetAccessor.h"

#include "Cesium.h"

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

class UnityAssetResponse : public IAssetResponse {
public:
  UnityAssetResponse(
      const UnityEngine::Networking::UnityWebRequest& request,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _statusCode(uint16_t(request.responseCode())),
        _contentType(),
        _data(std::move(handler.NativeImplementation().getData())) {
    System::Collections::Generic::Dictionary2<System::String, System::String>
        responseHeaders = request.GetResponseHeaders();
    if (responseHeaders != nullptr) {
      System::Collections::Generic::Enumerator0 enumerator =
          responseHeaders.GetEnumerator();
      while (enumerator.MoveNext()) {
        this->_headers.emplace(
            enumerator.Current().Key().ToStlString(),
            enumerator.Current().Value().ToStlString());
      }
      auto find = this->_headers.find("content-type");
      if (find != this->_headers.end()) {
        this->_contentType = find->second;
      }
    }
  }

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override { return _contentType; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual gsl::span<const std::byte> data() const override {
    return this->_data;
  }

private:
  uint16_t _statusCode;
  std::string _contentType;
  HttpHeaders _headers;
  std::vector<std::byte> _data;
};

class UnityAssetRequest : public IAssetRequest {
public:
  UnityAssetRequest(
      const DotNet::UnityEngine::Networking::UnityWebRequest& request,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _method(request.method().ToStlString()),
        _url(request.url().ToStlString()),
        _headers(),
        _response(request, handler) {
    // TODO: get request headers
  }

  virtual const std::string& method() const override { return _method; }

  virtual const std::string& url() const override { return _url; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual const IAssetResponse* response() const override { return &_response; }

private:
  std::string _method;
  std::string _url;
  HttpHeaders _headers;
  UnityAssetResponse _response;
};

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

UnityAssetAccessor::UnityAssetAccessor()
    : _cesiumPlatformHeader(
          CesiumForUnity::Helpers::ToString(
              UnityEngine::Application::platform())
              .ToStlString() +
          " " + System::Environment::OSVersion().VersionString().ToStlString() +
          " " +
          replaceInvalidChars(
              UnityEngine::Application::productName().ToStlString())),
      _cesiumVersionHeader(
          CesiumForUnityNative::Cesium::version + " " +
          CesiumForUnityNative::Cesium::commit) {}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {
  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem,
                                      url,
                                      headers,
                                      cesiumPlatformHeader =
                                          this->_cesiumPlatformHeader,
                                      cesiumVersionHeader =
                                          this->_cesiumVersionHeader]() {
    UnityEngine::Networking::UnityWebRequest request =
        UnityEngine::Networking::UnityWebRequest::Get(System::String(url));

    DotNet::CesiumForUnity::NativeDownloadHandler handler{};
    request.downloadHandler(handler);

    for (const auto& header : headers) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    request.SetRequestHeader(
        System::String("X-Cesium-Platform"),
        cesiumPlatformHeader);
    request.SetRequestHeader(
        System::String("X-Cesium-Version"),
        cesiumVersionHeader);

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();
    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request, promise = std::move(promise), handler = std::move(handler)](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(
                std::make_shared<UnityAssetRequest>(request, handler));
          } else {
            promise.reject(std::runtime_error(
                "Request failed: " + request.error().ToStlString()));
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
    const gsl::span<const std::byte>& contentPayload) {
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
                                      cesiumPlatformHeader =
                                          this->_cesiumPlatformHeader,
                                      cesiumVersionHeader =
                                          this->_cesiumVersionHeader]() {
    DotNet::CesiumForUnity::NativeDownloadHandler downloadHandler{};
    UnityEngine::Networking::UploadHandlerRaw uploadHandler(payloadBytes, true);
    UnityEngine::Networking::UnityWebRequest request(
        System::String(url),
        System::String(verb),
        downloadHandler,
        uploadHandler);

    for (const auto& header : headers) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    request.SetRequestHeader(
        System::String("X-Cesium-Platform"),
        cesiumPlatformHeader);
    request.SetRequestHeader(
        System::String("X-Cesium-Version"),
        cesiumVersionHeader);

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();
    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request,
         promise = std::move(promise),
         handler = std::move(downloadHandler)](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(
                std::make_shared<UnityAssetRequest>(request, handler));
          } else {
            promise.reject(std::runtime_error(
                "Request failed: " + request.error().ToStlString()));
          }
        }));

    return future;
  });
}

void UnityAssetAccessor::tick() noexcept {}

} // namespace CesiumForUnityNative
