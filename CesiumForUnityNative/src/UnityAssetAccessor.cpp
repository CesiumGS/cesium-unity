#include "UnityAssetAccessor.h"

#include <CesiumAsync/IAssetResponse.h>

#include <Oxidize/CesiumForUnity/NativeDownloadHandler.h>
#include <Oxidize/System/Action1.h>
#include <Oxidize/System/String.h>
#include <Oxidize/UnityEngine/Networking/DownloadHandler.h>
#include <Oxidize/UnityEngine/Networking/UnityWebRequest.h>
#include <Oxidize/UnityEngine/Networking/UnityWebRequestAsyncOperation.h>

using namespace CesiumAsync;
using namespace Oxidize;

namespace {

class UnityAssetResponse : public IAssetResponse {
public:
  UnityAssetResponse(
      const UnityEngine::Networking::UnityWebRequest& request)
      : _statusCode(uint16_t(request.responseCode())),
        _contentType(request.GetResponseHeader(System::String("Content-Type"))
                         .ToStlString()) {
    this->_headers.emplace("Content-Type", this->_contentType);
    // TODO: get all response headers
  }

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override { return _contentType; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual gsl::span<const std::byte> data() const override {
    //return this->_pHandler->getData();
    return gsl::span<const std::byte>();
  }

private:
  uint16_t _statusCode;
  std::string _contentType;
  HttpHeaders _headers;
  std::shared_ptr<::Oxidize::CesiumForUnity::NativeDownloadHandler> _pHandler;
};

class UnityAssetRequest : public IAssetRequest {
public:
  UnityAssetRequest(
      Oxidize::UnityEngine::Networking::UnityWebRequest& request)
      : _method(request.method().ToStlString()),
        _url(request.url().ToStlString()),
        _headers(),
        _response(request) {
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

struct WebRequestCompleted : public System::Action1<UnityEngine::AsyncOperation> {
  UnityEngine::Networking::UnityWebRequest _request;
  Promise<std::shared_ptr<CesiumAsync::IAssetRequest>> _promise;

  WebRequestCompleted(
      const UnityEngine::Networking::UnityWebRequest& request,
      Promise<std::shared_ptr<CesiumAsync::IAssetRequest>>& promise)
      : System::Action1<UnityEngine::AsyncOperation>(), _request(request), _promise(promise) {}

  virtual void operator()(UnityEngine::AsyncOperation& obj) /*override*/ {
    if (this->_request.isDone() && this->_request.error() == nullptr) {
      this->_promise.resolve(
          std::make_shared<UnityAssetRequest>(this->_request));
    } else {
      this->_promise.reject(std::runtime_error(
          "Request failed: " + this->_request.error().ToStlString()));
    }
  }
};

} // namespace

namespace CesiumForUnity {

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {
  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem, url, headers]() {
    UnityEngine::Networking::UnityWebRequest request =
        UnityEngine::Networking::UnityWebRequest::Get(System::String(url));

    request.downloadHandler(Oxidize::CesiumForUnity::NativeDownloadHandler());

    for (const auto& header : headers) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto pCompleted =
        std::make_shared<WebRequestCompleted>(request, promise);

    UnityEngine::Networking::UnityWebRequestAsyncOperation op = request.SendWebRequest();
    //op.AddCompleted(*pCompleted);

    return promise.getFuture().thenImmediately(
        [pCompleted](std::shared_ptr<IAssetRequest>&& pRequest) {
          // The lambda capture here keeps the managed WebRequestCompleted alive
          // until we're done with it.
          return std::move(pRequest);
        });
  });
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::request(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& verb,
    const std::string& url,
    const std::vector<THeader>& headers,
    const gsl::span<const std::byte>& contentPayload) {
  throw std::exception();
}

void UnityAssetAccessor::tick() noexcept {}

} // namespace CesiumForUnity
