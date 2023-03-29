using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
        private string _delimiter = "\u2022";

        private void OnEnable()
        {
            this._creditSystem = this.GetComponent<CesiumCreditSystem>();
            this._creditSystem.OnCreditsUpdate += this.SetCredits;

            this._uiDocument = this.GetComponent<UIDocument>();
            this._onScreenCredits = this._uiDocument.rootVisualElement.Q("OnScreenCredits");
            this._popupCredits = this._uiDocument.rootVisualElement.Q("PopupCredits");

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif

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
        }


#if UNITY_EDITOR
        private void AddCreditsToSceneView(SceneView sceneView)
        {
            if (sceneView.rootVisualElement == null)
            {
                return;
            }

            if (sceneView.rootVisualElement.Q("CesiumCreditsOverlay") == null)
            {
                VisualTreeAsset visualTreeAsset = this._uiDocument.visualTreeAsset;
                TemplateContainer tree = visualTreeAsset.Instantiate();
                tree.name = "CesiumCreditsOverlay";
                tree.style.height = new StyleLength(Length.Percent(100));
                sceneView.rootVisualElement.Add(tree);

                this.UpdateCreditsInSceneView(
                    sceneView,
                    this._creditSystem.onScreenCredits,
                    this._creditSystem.popupCredits);
            }
        }

        private void UpdateCreditsInSceneView(SceneView sceneView, List<CesiumCredit> onScreenCredits, List<CesiumCredit> popupCredits)
        {
            if (sceneView.rootVisualElement == null)
            {
                return;
            }

            VisualElement creditOverlay = sceneView.rootVisualElement.Q("CesiumCreditsOverlay");
            if (creditOverlay != null)
            {
                VisualElement onScreenElement = creditOverlay.Q("OnScreenCredits");
                VisualElement popupElement = creditOverlay.Q("PopupCredits");
                this.SetCreditsOnVisualElements(
                    onScreenElement,
                    onScreenCredits,
                    popupElement,
                    popupCredits);
            }
        }

        private void RemoveCreditsFromSceneView(SceneView sceneView)
        {
            if (sceneView.rootVisualElement != null)
            {
                VisualElement creditOverlay = sceneView.rootVisualElement.Q("CesiumCreditsOverlay");
                if (creditOverlay != null)
                {
                    VisualElement parent = creditOverlay.parent;
                    parent.Remove(creditOverlay);
                }
            }
        }

#endif

        private void Update()
        {
#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for(int i = 0; i < sceneViews.Count; i++)
            {
                this.AddCreditsToSceneView((SceneView)sceneViews[i]);
            }
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
                    element.AddManipulator(new Clickable(evt => Application.OpenURL(creditComponent.link)));
                }

                visualElements.Add(element);
            }

            return visualElements;
        }

        private VisualElement CreateDataAttributionElement(VisualElement popupElement)
        {
            Label label = new Label();
            label.text = "<u>Data Attribution</u>";
            label.AddManipulator(new Clickable(evt =>
            {
                if (popupElement.style.display == DisplayStyle.Flex)
                {
                    popupElement.style.display = DisplayStyle.None;
                }
                else
                {
                    popupElement.style.display = DisplayStyle.Flex;
                }
            }));

            return label;
        }

        private void SetCredits(List<CesiumCredit> onScreenCredits, List<CesiumCredit> popupCredits)
        {
            this.SetCreditsOnVisualElements(this._onScreenCredits, onScreenCredits, this._popupCredits, popupCredits);

#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for (int i = 0; i < sceneViews.Count; i++)
            {
                this.UpdateCreditsInSceneView((SceneView)sceneViews[i], onScreenCredits, popupCredits);
            }
#endif
        }

        private void SetCreditsOnVisualElements(
            VisualElement onScreenElement,
            List<CesiumCredit> onScreenCredits,
            VisualElement popupElement,
            List<CesiumCredit> popupCredits)
        {
            onScreenElement.Clear();
            popupElement.Clear();

            for (int i = 0, creditCount = onScreenCredits.Count; i < creditCount; i++)
            {
                if (i > 0)
                {
                    onScreenElement.Add(this.CreateLabelFromText(this._delimiter));
                }

                CesiumCredit credit = onScreenCredits[i];
                List<VisualElement> visualElements = this.ConvertCreditToVisualElements(credit);
                for (int j = 0, elementCount = visualElements.Count; j < elementCount; j++)
                {
                    onScreenElement.Add(visualElements[j]);
                }
            }

            for (int i = 0, creditCount = popupCredits.Count; i < creditCount; i++)
            {
                
            }

            onScreenElement.Add(this.CreateDataAttributionElement(popupElement));
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for(int i = 0; i < sceneViews.Count; i++)
            {
                this.RemoveCreditsFromSceneView((SceneView)sceneViews[i]);
            }
#endif
        }
    }
}