#pragma once

#include <DotNet/CesiumForUnity/NativeDownloadHandler.h>

#include <CesiumAsync/HttpHeaders.h>
#include <CesiumAsync/IAssetAccessor.h>
#include <CesiumAsync/IAssetRequest.h>
#include <CesiumAsync/IAssetResponse.h>
#include <CesiumUtility/DoublyLinkedList.h>

#include <DotNet/UnityEngine/Networking/UnityWebRequest.h>
#include <DotNet/System/String.h>
#include <DotNet/System/Collections/Generic/Dictionary2.h>
#include <DotNet/System/Collections/Generic/Enumerator0.h>
#include <DotNet/System/Collections/Generic/KeyValuePair2.h>

namespace CesiumForUnityNative {

class UnityAssetResponse : public CesiumAsync::IAssetResponse {
public:
  UnityAssetResponse(
      const DotNet::UnityEngine::Networking::UnityWebRequest& request,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _statusCode(uint16_t(request.responseCode())),
        _contentType(),
        _data(std::move(handler.NativeImplementation().getData())) {
    DotNet::System::Collections::Generic::Dictionary2<DotNet::System::String, DotNet::System::String>
        responseHeaders = request.GetResponseHeaders();
    if (responseHeaders != nullptr) {
      DotNet::System::Collections::Generic::Enumerator0 enumerator =
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

  virtual const CesiumAsync::HttpHeaders& headers() const override { return _headers; }

  virtual std::span<const std::byte> data() const override {
    return this->_data;
  }

private:
  uint16_t _statusCode;
  std::string _contentType;
  CesiumAsync::HttpHeaders _headers;
  std::vector<std::byte> _data;
};

class UnityAssetRequest : public CesiumAsync::IAssetRequest {
public:
  UnityAssetRequest(
      const DotNet::UnityEngine::Networking::UnityWebRequest& request,
      const CesiumAsync::HttpHeaders& headers,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _method(request.method().ToStlString()),
        _url(request.url().ToStlString()),
        _headers(headers),
        _pResponse(std::make_unique<UnityAssetResponse>(request, handler)) {}

  // UnityAssetRequest() = default;

  // void initialize(const DotNet::UnityEngine::Networking::UnityWebRequest& request,
  //             const CesiumAsync::HttpHeaders& headers,
  //             const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
  // {
  //   _method = request.method().ToStlString();
  //   _url = request.url().ToStlString();
  //   _headers = headers;
  //   _pResponse = std::make_unique<UnityAssetResponse>(request, handler);
  // }

  virtual const std::string& method() const override { return _method; }

  virtual const std::string& url() const override { return _url; }

  virtual const CesiumAsync::HttpHeaders& headers() const override { return _headers; }

  virtual const CesiumAsync::IAssetResponse* response() const override { return &*_pResponse; }

  CesiumUtility::DoublyLinkedListPointers<UnityAssetRequest> links;

private:
  std::string _method;
  std::string _url;
  CesiumAsync::HttpHeaders _headers;
  std::unique_ptr<UnityAssetResponse> _pResponse;
};

class UnityAssetAccessor : public CesiumAsync::IAssetAccessor {
public:
  UnityAssetAccessor();

  virtual CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
  get(const CesiumAsync::AsyncSystem& asyncSystem,
      const std::string& url,
      const std::vector<THeader>& headers = {}) override;

  virtual CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
  request(
      const CesiumAsync::AsyncSystem& asyncSystem,
      const std::string& verb,
      const std::string& url,
      const std::vector<THeader>& headers = std::vector<THeader>(),
      const std::span<const std::byte>& contentPayload = {}) override;

  virtual void tick() noexcept override;

private:
  CesiumAsync::HttpHeaders _cesiumRequestHeaders;
  CesiumUtility::DoublyLinkedList<UnityAssetRequest, &UnityAssetRequest::links> _activeRequests;
};


} // namespace CesiumForUnityNative
