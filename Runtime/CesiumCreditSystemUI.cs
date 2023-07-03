using System;
using System.Collections;
using System.Collections.Generic;
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
    [RequireComponent(typeof(UIDocument))]
    [AddComponentMenu("Cesium/Cesium Credit System UI")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
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

            if (this._creditSystem == null)
            {
                this._creditSystem = CesiumCreditSystem.GetDefaultCreditSystem();
            }

            this._creditSystem.OnCreditsUpdate += this.SetCredits;

            this._uiDocument = this.GetComponent<UIDocument>();

            if (this._uiDocument.rootVisualElement != null)
            {
                this._onScreenCredits = this._uiDocument.rootVisualElement.Q("OnScreenCredits");
                this._popupCredits = this._uiDocument.rootVisualElement.Q("PopupCredits");
            }

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

            if (sceneView.rootVisualElement.Q("OnScreenCredits") == null)
            {
                VisualTreeAsset visualTreeAsset = this._uiDocument.visualTreeAsset;
                TemplateContainer tree = visualTreeAsset.Instantiate();

                // If we add the tree directly and scale the height to fit the whole screen,
                // it will block any non-UI mouse inputs, preventing the user from looking
                // around the scene or selecting objects. Add the individual elements instead.
                VisualElement onScreenCredits = tree.Q("OnScreenCredits");
                VisualElement popupCredits = tree.Q("PopupCredits");
                sceneView.rootVisualElement.Add(onScreenCredits);
                sceneView.rootVisualElement.Add(popupCredits);

                this.UpdateCreditsInSceneView(
                    sceneView,
                    this._creditSystem.onScreenCredits,
                    this._creditSystem.popupCredits);
            }
        }

        private void UpdateCreditsInSceneView(
            SceneView sceneView,
            List<CesiumCredit> onScreenCredits,
            List<CesiumCredit> popupCredits)
        {
            if (sceneView.rootVisualElement == null)
            {
                return;
            }

            VisualElement onScreenElement = sceneView.rootVisualElement.Q("OnScreenCredits");
            VisualElement popupElement = sceneView.rootVisualElement.Q("PopupCredits");

            if (onScreenElement != null && popupElement != null)
            {
                this.SetCreditsOnVisualElements(
                    onScreenElement,
                    onScreenCredits,
                    popupElement,
                    popupCredits,
                    false);
            }
        }

        private void RemoveCreditsFromSceneView(SceneView sceneView)
        {
            if (sceneView.rootVisualElement != null)
            {
                VisualElement onScreenElement = sceneView.rootVisualElement.Q("OnScreenCredits");
                if (onScreenElement != null)
                {
                    VisualElement parent = onScreenElement.parent;
                    parent.Remove(onScreenElement);
                }

                VisualElement popupElement = sceneView.rootVisualElement.Q("PopupCredits");
                if (popupElement != null)
                {
                    VisualElement parent = popupElement.parent;
                    parent.Remove(popupElement);
                }
            }
        }

#endif

        private void Update()
        {
            if (this._creditSystem == null)
            {
                return;
            }

#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for (int i = 0; i < sceneViews.Count; i++)
            {
                this.AddCreditsToSceneView((SceneView)sceneViews[i]);
            }
#endif
        }

        /// <summary>
        /// Creates a UIElements Label with the given text.
        /// </summary>
        /// <param name="text">The desired text.</param>
        /// <param name="removeExtraSpace">Whether to strip the label of extra space. This is used to 
        /// reduce space between inline labels, so that they appear as normally spaced text.</param>
        /// <returns></returns>
        private Label CreateLabelFromText(string text, bool removeExtraSpace)
        {
            Label label = new Label();
            label.text = text;
            label.style.whiteSpace = WhiteSpace.Normal;

            if (removeExtraSpace)
            {
                label.style.marginLeft = new StyleLength(0.0f);
                label.style.paddingLeft = new StyleLength(0.0f);
                label.style.paddingRight = new StyleLength(0.0f);
            }

            return label;
        }

        /// <summary>
        /// Converts a Cesium credit representation into VisualElements for rendering with UI Toolkit.
        /// </summary>
        /// <param name="credit">The credit to convert to VisualElements.</param>
        /// <param name="removeExtraSpace">Whether to strip the credit's label elements of extra space.
        /// This is used to reduce space between inline labels, so that they appear as normally spaced text.</param>
        /// <returns></returns>
        private List<VisualElement> ConvertCreditToVisualElements(CesiumCredit credit, bool removeExtraSpace)
        {
            List<VisualElement> visualElements = new List<VisualElement>();

            for (int i = 0, componentCount = credit.components.Count; i < componentCount; i++)
            {
                CesiumCreditComponent creditComponent = credit.components[i];
                VisualElement element;

                bool hasLink = !string.IsNullOrEmpty(creditComponent.link);

                if (creditComponent.imageId >= 0)
                {
                    Texture2D image = null;
                    if (creditComponent.imageId < this._creditSystem.images.Count)
                    {
                        image = this._creditSystem.images[creditComponent.imageId];
                    }

                    if (image == null)
                    {
                        continue;
                    }

                    element = new VisualElement();
                    element.style.backgroundImage = new StyleBackground(image);
                    element.style.width = new StyleLength(image.width);
                    element.style.height = new StyleLength(image.height);
                }
                else
                {
                    string text = creditComponent.text;

                    if (hasLink)
                    {
                        text = string.Format("<u>{0}</u>", text);
                    }

                    element = this.CreateLabelFromText(text, removeExtraSpace);
                }

                if (hasLink)
                {
                    element.AddManipulator(new Clickable(evt => Application.OpenURL(creditComponent.link)));
                }

                visualElements.Add(element);
            }

            return visualElements;
        }

        /// <summary>
        /// Creates a "Data Attribution" VisualElement that, when clicked, toggles the visibility
        /// of the given attribution panel.
        /// </summary>
        /// <param name="popupElement">The VisualElement that represents the "Data Attribution" panel.</param>
        /// <returns>The clickable "Data Attribution" VisualElement.</returns>
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

        /// <summary>
        /// Creates a wrapper for a credit to be displayed in the "Data Attribution" panel.
        /// This encapsulates the VisualElements of a credit so that it can be vertically
        /// stacked with other credits in the panel, while keeping its components inline.
        /// </summary>
        /// <param name="removeExtraSpace">Whether or not to remove extra space between
        /// entries in the popup panel. This is mainly used to account for visual discrepancies
        /// between the SceneView and GameView windows.</param>
        /// <returns>The wrapper VisualElement.</returns>
        private VisualElement CreatePopupCreditElement(bool removeExtraSpace)
        {
            VisualElement popupCreditElement = new VisualElement();
            popupCreditElement.style.flexDirection = FlexDirection.Row;
            popupCreditElement.style.flexWrap = Wrap.Wrap;
            popupCreditElement.style.alignItems = Align.Center;

            if (!removeExtraSpace)
            {
                popupCreditElement.style.marginTop = new StyleLength(2.5f);
            }
            else
            {
                popupCreditElement.style.marginTop = new StyleLength(0.0f);
                popupCreditElement.style.marginBottom = new StyleLength(0.0f);
                popupCreditElement.style.paddingTop = new StyleLength(0.0f);
                popupCreditElement.style.paddingBottom = new StyleLength(0.0f);
            }

            return popupCreditElement;
        }

        private void SetCredits(List<CesiumCredit> onScreenCredits, List<CesiumCredit> popupCredits)
        {
            this.SetCreditsOnVisualElements(
                this._onScreenCredits,
                onScreenCredits,
                this._popupCredits,
                popupCredits,
                true);

#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for (int i = 0; i < sceneViews.Count; i++)
            {
                this.UpdateCreditsInSceneView((SceneView)sceneViews[i], onScreenCredits, popupCredits);
            }
#endif
        }

        /// <summary>
        /// Takes the given <see cref="CesiumCredit"/> lists and converts them to UI components. These
        /// elements are then added to the given VisualElements, which each represent the on-screen and popup
        /// portions of the credit display.
        /// </summary>
        /// <param name="onScreenElement">The VisualElement used to display on-screen credits.</param>
        /// <param name="onScreenCredits">The list of CesiumCredits to be shown on-screen.</param>
        /// <param name="popupElement">The VisualElement used to display credits in the attribution panel.</param>
        /// <param name="popupCredits">The list of CesiumCredits to be shown in the data attribution panel.</param>
        /// <param name="removeExtraSpace">Whether or not to remove extra space between entries.
        /// This is mainly used to account for visual discrepancies between the SceneView and GameView windows.</param>
        private void SetCreditsOnVisualElements(
            VisualElement onScreenElement,
            List<CesiumCredit> onScreenCredits,
            VisualElement popupElement,
            List<CesiumCredit> popupCredits,
            bool removeExtraSpace)
        {
            onScreenElement.Clear();
            popupElement.Clear();

            for (int i = 0, creditCount = onScreenCredits.Count; i < creditCount; i++)
            {
                CesiumCredit credit = onScreenCredits[i];
                List<VisualElement> visualElements = this.ConvertCreditToVisualElements(credit, removeExtraSpace);

                if (i > 0)
                {
                    onScreenElement.Add(this.CreateLabelFromText(this._delimiter, false));
                }

                for (int j = 0, elementCount = visualElements.Count; j < elementCount; j++)
                {
                    onScreenElement.Add(visualElements[j]);
                }
            }

            for (int i = 0, creditCount = popupCredits.Count; i < creditCount; i++)
            {
                CesiumCredit credit = popupCredits[i];
                List<VisualElement> visualElements = this.ConvertCreditToVisualElements(credit, removeExtraSpace);

                // Put the inline credit components in one container so they can be vertically stacked by the popup.
                VisualElement popupCreditElement = CreatePopupCreditElement(removeExtraSpace);

                for (int j = 0, elementCount = visualElements.Count; j < elementCount; j++)
                {
                    popupCreditElement.Add(visualElements[j]);
                }

                popupElement.Add(popupCreditElement);
            }

            if (popupCredits.Count > 0)
            {
                if (onScreenCredits.Count > 0)
                {
                    onScreenElement.Add(this.CreateLabelFromText(this._delimiter, false));
                }

                onScreenElement.Add(this.CreateDataAttributionElement(popupElement));
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            ArrayList sceneViews = SceneView.sceneViews;
            for (int i = 0; i < sceneViews.Count; i++)
            {
                this.RemoveCreditsFromSceneView((SceneView)sceneViews[i]);
            }
#endif
        }
    }
}