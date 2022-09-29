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
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

UnityEngine::GameObject CesiumCreditSystemImpl::_creditSystemPrefab = nullptr;

CesiumCreditSystemImpl::CesiumCreditSystemImpl(
    const CesiumForUnity::CesiumCreditSystem& creditSystem)
    : _pCreditSystem(std::make_shared<CreditSystem>()),
      _lastCreditsCount(0) {}

CesiumCreditSystemImpl::~CesiumCreditSystemImpl() {}

void CesiumCreditSystemImpl::JustBeforeDelete(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {}

void CesiumCreditSystemImpl::Update(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  if (!_pCreditSystem) {
    return;
  }

  const std::vector<Cesium3DTilesSelection::Credit>& creditsToShowThisFrame =
      _pCreditSystem->getCreditsToShowThisFrame();

  // If the credit list has changed, reformat the credits
  size_t creditsCount = creditsToShowThisFrame.size();
  bool creditsUpdated =
      creditsCount != _lastCreditsCount ||
      _pCreditSystem->getCreditsToNoLongerShowThisFrame().size() > 0;

  if (creditsUpdated) {
    std::string popupCredits = "";
    std::string onScreenCredits = "";

    bool firstCreditOnScreen = true;
    for (int i = 0; i < creditsCount; i++) {
      const Cesium3DTilesSelection::Credit& credit = creditsToShowThisFrame[i];
      if (i != 0) {
        popupCredits += "\n";
      }

      const std::string& html = _pCreditSystem->getHtml(credit);
      // TODO: convert HTML to RTF
      popupCredits += html;

      if (_pCreditSystem->shouldBeShownOnScreen(credit)) {
        if (firstCreditOnScreen) {
          firstCreditOnScreen = false;
        } else {
          onScreenCredits += " \u2022 ";
        }
        onScreenCredits += html;
      }
    }

    onScreenCredits += "<link=\"popup\"><u>Data Attribution</u></link>";
    creditSystem.SetCreditsText(popupCredits, onScreenCredits);
    _lastCreditsCount = creditsCount;
  }

  _pCreditSystem->startNextFrame();
}

//
//namespace {
//void htmlToRtf(
//    std::string& output,
//    std::string& parentUrl,
//    TidyDoc tdoc,
//    TidyNode tnod) {
//  TidyNode child;
//  TidyBuffer buf;
//  tidyBufInit(&buf);
//  for (child = tidyGetChild(tnod); child; child = tidyGetNext(child)) {
//    if (tidyNodeIsText(child)) {
//      tidyNodeGetText(tdoc, child, &buf);
//      if (buf.bp) {
//        std::string text = reinterpret_cast<const char*>(buf.bp);
//        tidyBufClear(&buf);
//        // could not find correct option in tidy html to not add new lines
//        if (text.size() != 0 && text[text.size() - 1] == '\n') {
//          text.pop_back();
//        }
//        if (!parentUrl.empty()) {
//          output +=
//              "<credits url=\"" + parentUrl + "\"" + " text=\"" + text + "\"/>";
//        } else {
//          output += text;
//        }
//      }
//    } else if (tidyNodeGetId(child) == TidyTagId::TidyTag_IMG) {
//      auto srcAttr = tidyAttrGetById(child, TidyAttrId::TidyAttr_SRC);
//      if (srcAttr) {
//        auto srcValue = tidyAttrValue(srcAttr);
//        if (srcValue) {
//          output += "<credits id=\"" +
//                    CreditsWidget->LoadImage(
//                        std::string(reinterpret_cast<const char*>(srcValue))) +
//                    "\"";
//          if (!parentUrl.empty()) {
//            output += " url=\"" + parentUrl + "\"";
//          }
//          output += "/>";
//        }
//      }
//    }
//    auto hrefAttr = tidyAttrGetById(child, TidyAttrId::TidyAttr_HREF);
//    if (hrefAttr) {
//      auto hrefValue = tidyAttrValue(hrefAttr);
//      parentUrl = std::string(reinterpret_cast<const char*>(hrefValue));
//    }
//    convertHtmlToRtf(output, parentUrl, tdoc, child, CreditsWidget);
//  }
//  tidyBufFree(&buf);
//}
//} // namespace
//
//std::string convertHtmlToRtf(std::string html) {
//  TidyDoc tdoc;
//  TidyBuffer tidy_errbuf = {0};
//  int err;
//
//  tdoc = tidyCreate();
//  tidyOptSetBool(tdoc, TidyForceOutput, yes);
//  tidyOptSetInt(tdoc, TidyWrapLen, 0);
//  tidyOptSetInt(tdoc, TidyNewline, TidyLF);
//
//  tidySetErrorBuffer(tdoc, &tidy_errbuf);
//
//  html = "<!DOCTYPE html><html><body>" + html + "</body></html>";
//
//  std::string output, url;
//  err = tidyParseString(tdoc, html.c_str());
//  if (err < 2) {
//    htmlToRtf(output, url, tdoc, tidyGetRoot(tdoc), _creditsWidget);
//  }
//  tidyBufFree(&tidy_errbuf);
//  tidyRelease(tdoc);
//
//  return output.c_str();
//}

void CesiumCreditSystemImpl::OnApplicationQuit(
    const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem) {
  // Dereference the prefab. If this isn't done, the Editor will try to
  // use the destroyed prefab the next time it enters play mode.
  CesiumCreditSystemImpl::_creditSystemPrefab = nullptr;
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
    if (CesiumCreditSystemImpl::_creditSystemPrefab == nullptr) {
      CesiumCreditSystemImpl::_creditSystemPrefab =
          UnityEngine::Resources::Load<UnityEngine::GameObject>(
              System::String("CesiumCreditSystem"));
    }

    defaultCreditSystemObject =
        UnityEngine::Object::Instantiate(_creditSystemPrefab);
    defaultCreditSystemObject.name(defaultName);
  }

  return defaultCreditSystemObject
      .GetComponent<CesiumForUnity::CesiumCreditSystem>();
}

} // namespace CesiumForUnityNative
