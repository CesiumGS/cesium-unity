using Reinterop;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCreditSystemImpl", "CesiumCreditSystemImpl.h")]
    public partial class CesiumCreditSystem : MonoBehaviour, IPointerClickHandler
    {
        private GameObject _popupGameObject = null!;
        private TextMeshProUGUI _popupText = null!;
        private TextMeshProUGUI _onScreenText = null!;

        private void Awake() {
            GameObject canvasGameObject = gameObject.transform.GetChild(0).gameObject;

            _popupGameObject = canvasGameObject.transform.Find("Popup").gameObject;
            _popupGameObject.SetActive(false);
            GameObject popupTextGameObject = _popupGameObject.transform.GetChild(0).gameObject;
            _popupText = popupTextGameObject.GetComponent<TextMeshProUGUI>();

            GameObject onScreenGameObject = canvasGameObject.transform.Find("OnScreen").gameObject;
            GameObject onScreenTextGameObject = onScreenGameObject.transform.GetChild(0).gameObject;
            _onScreenText = onScreenTextGameObject.GetComponent<TextMeshProUGUI>();
            
            // If no event system exists, create one.
            if(EventSystem.current == null) {
                GameObject eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();
                
                #if ENABLE_INPUT_SYSTEM
                eventSystemGameObject.AddComponent<InputSystemUIInputModule>();
                #elif ENABLE_LEGACY_INPUT_MANAGER
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
                #endif
            }
        }

        private partial void Update();
        private partial void OnApplicationQuit();

        public void OnPointerClick(PointerEventData eventData) {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_onScreenText, eventData.position, null);
            if (linkIndex != -1) {
                TMP_LinkInfo linkInfo = _onScreenText.textInfo.linkInfo[linkIndex];
                Debug.Log(linkInfo);
                string linkId = linkInfo.GetLinkID();
                if(linkId == "popup") {
                    _popupGameObject.SetActive(!_popupGameObject.activeSelf);
                } else {
                    Application.OpenURL(linkInfo.GetLinkID());
                }
            }
        }

        public void SetCreditsText(string popupCredits, string onScreenCredits) {
            _popupText.text = popupCredits;
            _onScreenText.text = onScreenCredits;
        }

    }
}