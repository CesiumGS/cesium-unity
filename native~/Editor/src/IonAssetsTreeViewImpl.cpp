#include "IonAssetsTreeViewImpl.h"

#include "SelectIonTokenWindowImpl.h"

#include <CesiumAsync/HttpHeaders.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumEditorUtility.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/IonAssetDetails.h>
#include <DotNet/CesiumForUnity/IonAssetsTreeView.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/StringComparison.h>
#include <DotNet/UnityEditor/EditorApplication.h>
#include <DotNet/UnityEditor/IMGUI/Controls/MultiColumnHeader.h>
#include <DotNet/UnityEditor/Selection.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GUI.h>
#include <DotNet/UnityEngine/GameObject.h>

#include <algorithm>

using namespace DotNet;
using namespace DotNet::UnityEditor::IMGUI::Controls;

namespace CesiumForUnityNative {

IonAssetsTreeViewImpl::IonAssetsTreeViewImpl(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView)
    : _assets() {}

IonAssetsTreeViewImpl::~IonAssetsTreeViewImpl() {}

int IonAssetsTreeViewImpl::GetAssetsCount(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {
  return this->_assets.size();
}

System::String IonAssetsTreeViewImpl::GetAssetName(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  return System::String(pAsset->name);
}

int64_t IonAssetsTreeViewImpl::GetAssetID(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  return pAsset->id;
}

System::String IonAssetsTreeViewImpl::GetAssetType(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  return System::String(pAsset->type);
}

System::String IonAssetsTreeViewImpl::GetAssetDescription(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  return System::String(pAsset->description);
}

System::String IonAssetsTreeViewImpl::GetAssetAttribution(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  return System::String(pAsset->attribution);
}

void IonAssetsTreeViewImpl::CellGUI(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    DotNet::UnityEngine::Rect cellRect,
    int assetIndex,
    DotNet::CesiumForUnity::IonAssetsColumn column) {

  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[assetIndex];

  if (column == CesiumForUnity::IonAssetsColumn::Name) {
    System::String name = System::String(pAsset->name);
    UnityEngine::GUI::Label(cellRect, name);
  } else if (column == CesiumForUnity::IonAssetsColumn::Type) {
    System::String type = System::String(pAsset->type);
    UnityEngine::GUI::Label(
        cellRect,
        CesiumForUnity::IonAssetDetails::FormatType(type));
  } else if (column == CesiumForUnity::IonAssetsColumn::DateAdded) {
    System::String date = System::String(pAsset->dateAdded);
    UnityEngine::GUI::Label(
        cellRect,
        CesiumForUnity::IonAssetDetails::FormatDate(date));
  }
}

void IonAssetsTreeViewImpl::Refresh(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {
  CesiumIonSessionImpl& session = CesiumIonSessionImpl::ion();
  const CesiumIonClient::Assets& assets = session.getAssets();

  this->_assets.resize(assets.items.size());

  for (size_t i = 0; i < assets.items.size(); ++i) {
    this->_assets[i] =
        std::make_shared<CesiumIonClient::Asset>(assets.items[i]);
  }

  System::String searchString = treeView.searchString();
  if (searchString != nullptr && searchString.Length() > 0) {
    applyFilter(searchString);
  }

  int sortedColumnIndex = treeView.multiColumnHeader().sortedColumnIndex();
  if (sortedColumnIndex >= 0) {
    CesiumForUnity::IonAssetsColumn column =
        (CesiumForUnity::IonAssetsColumn)sortedColumnIndex;
    bool sortAscending =
        treeView.multiColumnHeader().IsSortedAscending(sortedColumnIndex);
    applySorting(column, sortAscending);
  }

  treeView.Reload();
}

void IonAssetsTreeViewImpl::applyFilter(
    const DotNet::System::String searchString) {
  auto predicate = std::remove_if(
      this->_assets.begin(),
      this->_assets.end(),
      [&](const std::shared_ptr<CesiumIonClient::Asset>& pAsset) {
        // This mimics the behavior of the ion web UI, which
        // searches for the given text in the name and description.
        const System::String name = System::String(pAsset->name);
        if (name.Contains(
                searchString,
                System::StringComparison::InvariantCultureIgnoreCase)) {
          return false;
        }

        const System::String description = System::String(pAsset->description);
        if (description.Contains(
                searchString,
                System::StringComparison::InvariantCultureIgnoreCase)) {
          return false;
        }

        return true;
      });

  this->_assets.erase(predicate, this->_assets.end());
}

static std::function<bool(
    const std::shared_ptr<CesiumIonClient::Asset>&,
    const std::shared_ptr<CesiumIonClient::Asset>&)>
getComparator(const DotNet::CesiumForUnity::IonAssetsColumn column) {

  if (column == CesiumForUnity::IonAssetsColumn::Name) {
    return [](const std::shared_ptr<CesiumIonClient::Asset>& pLeft,
              const std::shared_ptr<CesiumIonClient::Asset>& pRight) {
      return CesiumAsync::CaseInsensitiveCompare()(pLeft->name, pRight->name);
    };
  } else if (column == CesiumForUnity::IonAssetsColumn::Type) {
    return [](const std::shared_ptr<CesiumIonClient::Asset>& pLeft,
              const std::shared_ptr<CesiumIonClient::Asset>& pRight) {
      return CesiumAsync::CaseInsensitiveCompare()(pLeft->type, pRight->type);
    };
  } else {
    return [](const std::shared_ptr<CesiumIonClient::Asset>& pLeft,
              const std::shared_ptr<CesiumIonClient::Asset>& pRight) {
      return CesiumAsync::CaseInsensitiveCompare()(
          pLeft->dateAdded,
          pRight->dateAdded);
    };
  }
}

void IonAssetsTreeViewImpl::applySorting(
    const DotNet::CesiumForUnity::IonAssetsColumn column,
    const bool sortAscending) {
  auto comparator = getComparator(column);
  if (sortAscending) {
    std::sort(this->_assets.begin(), this->_assets.end(), comparator);
  } else {
    std::sort(
        this->_assets.begin(),
        this->_assets.end(),
        [&comparator](
            const std::shared_ptr<CesiumIonClient::Asset>& pLeft,
            const std::shared_ptr<CesiumIonClient::Asset>& pRight) {
          return comparator(pRight, pLeft);
        });
  }
}

void IonAssetsTreeViewImpl::AddAssetToLevel(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  SelectIonTokenWindowImpl::SelectAndAuthorizeToken({pAsset->id})
      .thenInMainThread(
          [pAsset](
              const std::optional<CesiumIonClient::Token>& /*maybeToken*/) {
            // If token selection was canceled, or if an error occurred while
            // selecting the token, ignore it and create the tileset anyway.
            // It's already been logged if necessary, and we can let the user
            // sort out the problem using the resulting Troubleshooting panel.
            CesiumForUnity::Cesium3DTileset tileset =
                CesiumForUnity::CesiumEditorUtility::CreateTileset(
                    System::String(pAsset->name),
                    pAsset->id);
            tileset.RecreateTileset();

            UnityEditor::Selection::activeGameObject(tileset.gameObject());
            UnityEditor::EditorApplication::ExecuteMenuItem(
                System::String("Window/General/Hierarchy"));
          });
}

void IonAssetsTreeViewImpl::AddOverlayToTerrain(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  // This behavior needs to change when we support multiple overlays.
  std::shared_ptr<CesiumIonClient::Asset> pAsset = this->_assets[index];
  SelectIonTokenWindowImpl::SelectAndAuthorizeToken({pAsset->id})
      .thenInMainThread([pAsset](const std::optional<
                                 CesiumIonClient::Token>& /*maybeToken*/) {
        UnityEngine::GameObject selected =
            UnityEditor::Selection::activeGameObject();
        CesiumForUnity::Cesium3DTileset tileset = nullptr;
        if (selected != nullptr) {
          tileset = selected.GetComponent<CesiumForUnity::Cesium3DTileset>();
        }

        if (tileset == nullptr) {
          tileset = CesiumForUnity::CesiumEditorUtility::FindFirstTileset();

          if (tileset != nullptr) {
            UnityEngine::Debug::LogWarning(
                System::String("No tileset was selected. The overlay will be "
                               "added to the first tileset found."));
          } else {
            UnityEngine::Debug::LogWarning(System::String(
                "No existing tileset was found. Cesium World "
                "Terrain will be created with the overlay as a base layer."));
            tileset = CesiumForUnity::CesiumEditorUtility::CreateTileset(
                System::String("Cesium World Terrain"),
                1);
          }
        }

        CesiumForUnity::CesiumEditorUtility::AddBaseOverlayToTileset(
            tileset,
            pAsset->id);

        tileset.RecreateTileset();

        UnityEditor::Selection::activeGameObject(tileset.gameObject());
      });
}

} // namespace CesiumForUnityNative
