using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.TextCore;
using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Displays the credits / attributions managed by a <see cref="CesiumCreditSystem"/>.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(CesiumCreditSystem))]
    [RequireComponent(typeof(UIDocument))]
    internal class CesiumCreditSystemUI : MonoBehaviour
    {
        private CesiumCreditSystem _creditSystem;

        private UIDocument _uiDocument;

        private VisualElement _onScreenCredits;
        private VisualElement _popupCredits;

        // The delimiter refers to the string used to separate credit entries
        // when they are presented on-screen.
        private string _defaultDelimiter = "\u2022";

        internal string defaultDelimiter
        {
            get => this._defaultDelimiter;
        }

        private void OnEnable()
        {
            this._creditSystem = this.GetComponent<CesiumCreditSystem>();
            this._creditSystem.OnCreditsUpdate += this.SetCredits;

            this._uiDocument = this.GetComponent<UIDocument>();
            this._onScreenCredits = this._uiDocument.rootVisualElement.Q("OnScreenCredits");
            this._popupCredits = this._uiDocument.rootVisualElement.Q("PopupCredits");

            // If no EventSystem exists, create one to handle clicking on credit links.
            if (EventSystem.current == null)
            {
                GameObject eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
                eventSystemGameObject.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
#endif
            }

#if UNITY_EDITOR
            // TODO: can / should you handle multiple scene views?
            this.AttachCreditsToSceneView(SceneView.lastActiveSceneView);
#endif
        }

        private void AttachCreditsToSceneView(SceneView sceneView)
        {
#if UNITY_EDITOR
            /*if (sceneView.rootVisualElement != null && sceneView.rootVisualElement.Q("CesiumCreditsOverlay") == null)
            {
                VisualTreeAsset visualTreeAsset = this._uiDocument.visualTreeAsset;
                TemplateContainer tree = visualTreeAsset.Instantiate();
                tree.name = "CesiumCreditsOverlay";
                tree.style.height = new StyleLength(Length.Percent(100));
                sceneView.rootVisualElement.Add(tree);
            }*/
#endif
        }

        private Label CreateLabelFromText(string text)
        {
            Label label = new Label();
            label.text = text;
            label.style.marginLeft = new StyleLength(0.0f);
            label.style.paddingLeft = new StyleLength(0.0f);
            label.style.paddingRight = new StyleLength(0.0f);

            return label;
        }

        private void HandleClickedLink(string link)
        {
            if(link == "popup")
            {
                this._popupCredits.SetEnabled(!this._popupCredits.enabledSelf);
            } else
            {
                Application.OpenURL(link);
            }
        }

        private List<VisualElement> ConvertCreditToVisualElements(CesiumCredit credit)
        {
            List<VisualElement> visualElements = new List<VisualElement>();

            for (int i = 0, componentCount = credit.components.Count; i < componentCount; i++)
            {
                CesiumCreditComponent creditComponent = credit.components[i];
                VisualElement element;

                if (creditComponent.imageId >= 0)
                {
                    Texture2D image = this._creditSystem.images[creditComponent.imageId];
                    element = new VisualElement();
                    element.style.width = new StyleLength(image.width);
                    element.style.height = new StyleLength(image.height);
                    element.style.backgroundImage = new StyleBackground(image);
                }
                else
                {
                    element = this.CreateLabelFromText(creditComponent.text);
                }

                if (!string.IsNullOrEmpty(creditComponent.link))
                {
                    element.AddManipulator(new Clickable(evt => HandleClickedLink(creditComponent.link)));
                }

                visualElements.Add(element);
            }

            return visualElements;
        }

        private void SetCredits(List<CesiumCredit> onScreenCredits, List<CesiumCredit> popupCredits)
        {
            this._onScreenCredits.Clear();
            this._popupCredits.Clear();

            for (int i = 0, creditCount = onScreenCredits.Count; i < creditCount; i++)
            {
                if (i > 0)
                {
                    this._onScreenCredits.Add(this.CreateLabelFromText(this._defaultDelimiter));
                }

                CesiumCredit credit = onScreenCredits[i];
                List<VisualElement> visualElements = this.ConvertCreditToVisualElements(credit);
                for (int j = 0, elementCount = visualElements.Count; j < elementCount; j++)
                {
                    this._onScreenCredits.Add(visualElements[j]);
                }
            }
        }
    }
}