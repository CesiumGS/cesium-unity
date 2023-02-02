#include "CameraManager.h"
#include "Cesium3DTilesetImpl.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/IEnumerator.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Coroutine.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Texture2D.h>
#include <DotNet/UnityEngine/Transform.h>
#include <tidybuffio.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumCreditSystemImpl::CesiumCreditSystemImpl(
    const CesiumForUnity::CesiumCreditSystem& creditSystem)
    : _pCreditSystem(std::make_shared<CreditSystem>()),
      _htmlToRtf(),
      _popupCreditsList(),
      _onScreenCreditsList(),
      _lastCreditsCount(0) {}

CesiumCreditSystemImpl::~CesiumCreditSystemImpl() {}

void CesiumCreditSystemImpl::Update(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  if (!_pCreditSystem) {
    return;
  }

  const std::vector<Cesium3DTilesSelection::Credit>& creditsToShowThisFrame =
      _pCreditSystem->getCreditsToShowThisFrame();

  // If the credit list has changed, reformat the credits.
  bool creditsUpdated =
      creditsToShowThisFrame.size() != _lastCreditsCount ||
      _pCreditSystem->getCreditsToNoLongerShowThisFrame().size() > 0;

  if (creditsUpdated) {
    _onScreenCreditsList.Clear();
    _popupCreditsList.Clear();

    size_t creditsCount = creditsToShowThisFrame.size();

    for (int i = 0; i < creditsCount; i++) {
      const Cesium3DTilesSelection::Credit& credit = creditsToShowThisFrame[i];

      System::String rtf = System::String("");
      const std::string& html = _pCreditSystem->getHtml(credit);

      auto htmlFind = _htmlToRtf.find(html);
      if (htmlFind != _htmlToRtf.end()) {
        rtf = htmlFind->second;
      } else {
        std::string rtfString = convertHtmlToRtf(html, creditSystem);
        rtf = System::String(rtfString);
        _htmlToRtf.insert({html, rtf});
      }
      _popupCreditsList.Add(rtf);

      if (_pCreditSystem->shouldBeShownOnScreen(credit)) {
        _onScreenCreditsList.Add(rtf);
      }
    }

    System::String popupCredits =
        System::String::Join(System::String("\n"), _popupCreditsList.ToArray());

    System::String onScreenCredits = System::String::Join(
        creditSystem.defaultDelimiter(),
        _onScreenCreditsList.ToArray());

    onScreenCredits = System::String::Concat(
        onScreenCredits,
        System::String("<link=\"popup\"><u>Data Attribution</u></link>"));

    creditSystem.SetCreditsText(
        System::String(popupCredits),
        System::String(onScreenCredits));

    _lastCreditsCount = creditsCount;
  }

  _pCreditSystem->startNextFrame();
}

namespace {
void htmlToRtf(
    std::string& output,
    std::string& parentUrl,
    TidyDoc tdoc,
    TidyNode tnod,
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  TidyNode child;
  TidyBuffer buf;
  tidyBufInit(&buf);

  for (child = tidyGetChild(tnod); child; child = tidyGetNext(child)) {
    if (tidyNodeIsText(child)) {
      tidyNodeGetText(tdoc, child, &buf);

      if (buf.bp) {
        std::string text = reinterpret_cast<const char*>(buf.bp);
        tidyBufClear(&buf);

        // could not find correct option in tidy html to not add new lines
        if (text.size() != 0 && text[text.size() - 1] == '\n') {
          text.pop_back();
        }

        if (!parentUrl.empty()) {
          // Output is <link="url">text</link>
          output += "<link=\"" + parentUrl + "\">" + text + "</link>";
        } else {
          output += text;
        }
      }
    } else if (tidyNodeGetId(child) == TidyTagId::TidyTag_IMG) {
      auto srcAttr = tidyAttrGetById(child, TidyAttrId::TidyAttr_SRC);

      if (srcAttr) {
        auto srcValue = tidyAttrValue(srcAttr);
        if (srcValue) {
          // Get the number of images that existed before LoadImage is called.
          const int numImages = creditSystem.numberOfImages();

          const std::string srcString =
              std::string(reinterpret_cast<const char*>(srcValue));
          creditSystem.StartCoroutine(
              creditSystem.LoadImage(System::String(srcString)));

          // Output is <link="url"><size=150%><sprite
          // name="credit-image-ID"></size></link> The ID of the image is just
          // the number of images before it was added.
          if (!parentUrl.empty()) {
            output += "<link=\"" + parentUrl + "\">";
          }
          output += "<size=150%><sprite name=\"credit-image-" +
                    std::to_string(numImages) + "\"></size>";
          if (!parentUrl.empty()) {
            output += "</link>";
          }
        }
      }
    }

    auto hrefAttr = tidyAttrGetById(child, TidyAttrId::TidyAttr_HREF);
    if (hrefAttr) {
      auto hrefValue = tidyAttrValue(hrefAttr);
      parentUrl = std::string(reinterpret_cast<const char*>(hrefValue));
    }
    htmlToRtf(output, parentUrl, tdoc, child, creditSystem);
  }

  tidyBufFree(&buf);
}
} // namespace

const std::string CesiumCreditSystemImpl::convertHtmlToRtf(
    const std::string& html,
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  TidyDoc tdoc;
  TidyBuffer tidy_errbuf = {0};
  int err;

  tdoc = tidyCreate();
  tidyOptSetBool(tdoc, TidyForceOutput, yes);
  tidyOptSetInt(tdoc, TidyWrapLen, 0);
  tidyOptSetInt(tdoc, TidyNewline, TidyLF);

  tidySetErrorBuffer(tdoc, &tidy_errbuf);

  std::string modifiedHtml =
      "<!DOCTYPE html><html><body>" + html + "</body></html>";

  std::string output, url;
  err = tidyParseString(tdoc, modifiedHtml.c_str());
  if (err < 2) {
    htmlToRtf(output, url, tdoc, tidyGetRoot(tdoc), creditSystem);
  }
  tidyBufFree(&tidy_errbuf);
  tidyRelease(tdoc);

  return output.c_str();
}

const std::shared_ptr<Cesium3DTilesSelection::CreditSystem>&
CesiumCreditSystemImpl::getExternalCreditSystem() const {
  return _pCreditSystem;
}

CesiumForUnity::CesiumCreditSystem
CesiumCreditSystemImpl::getDefaultCreditSystem() {
  UnityEngine::GameObject defaultCreditSystemObject = nullptr;
  System::String defaultName = System::String("CesiumCreditSystemDefault");

  // Look for an existing default credit system first.
  System::Array1<CesiumForUnity::CesiumCreditSystem> creditSystems =
      UnityEngine::Object::FindObjectsOfType<
          CesiumForUnity::CesiumCreditSystem>();

  for (int32_t i = 0, len = creditSystems.Length(); i < len; i++) {
    CesiumForUnity::CesiumCreditSystem creditSystem = creditSystems[i];
    if (creditSystem.name().StartsWith(defaultName)) {
      defaultCreditSystemObject = creditSystem.gameObject();
      break;
    }
  }

  // If no default credit system was found, instantiate one.
  if (defaultCreditSystemObject == nullptr) {
    UnityEngine::GameObject creditSystemPrefab =
        UnityEngine::Resources::Load<UnityEngine::GameObject>(
            System::String("CesiumCreditSystem"));

    defaultCreditSystemObject =
        UnityEngine::Object::Instantiate(creditSystemPrefab);
    defaultCreditSystemObject.name(defaultName);
  }

  return defaultCreditSystemObject
      .GetComponent<CesiumForUnity::CesiumCreditSystem>();
}

} // namespace CesiumForUnityNative
