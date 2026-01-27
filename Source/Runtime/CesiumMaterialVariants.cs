using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents KHR_materials_variants of a glTF primitive in a <see cref="Cesium3DTileset"/>.
    /// Allows switching between different material variants at runtime.
    /// </summary>
    /// <remarks>
    /// This component is automatically added to primitive game objects if they
    /// contain the KHR_materials_variants extension.
    /// </remarks>
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [AddComponentMenu("")]
    public partial class CesiumMaterialVariants : MonoBehaviour
    {
        /// <summary>
        /// Names of all available variants for this model (from the root glTF extension).
        /// </summary>
        public string[] variantNames
        {
            get; internal set;
        }

        /// <summary>
        /// The default material (the one specified in primitive.material).
        /// </summary>
        internal Material defaultMaterial
        {
            get; set;
        }

        /// <summary>
        /// Dictionary mapping variant indices to their corresponding materials.
        /// Key = variant index, Value = Unity Material for that variant.
        /// </summary>
        internal Dictionary<int, Material> variantMaterials
        {
            get; set;
        } = new Dictionary<int, Material>();

        private MeshRenderer _meshRenderer;
        private int _currentVariantIndex = -1; // -1 means default material is active

        void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Gets the index of the currently active variant, or -1 if the default material is active.
        /// </summary>
        public int GetCurrentVariantIndex()
        {
            return _currentVariantIndex;
        }

        /// <summary>
        /// Gets the name of the currently active variant, or "Default" if the default material is active.
        /// </summary>
        public string GetCurrentVariantName()
        {
            if (_currentVariantIndex < 0 || _currentVariantIndex >= variantNames.Length)
            {
                return "Default";
            }
            return variantNames[_currentVariantIndex];
        }

        /// <summary>
        /// Sets the active material variant by index.
        /// Use -1 to switch to the default material.
        /// </summary>
        /// <param name="variantIndex">The index of the variant to activate, or -1 for default.</param>
        /// <returns>True if the variant was successfully set, false otherwise.</returns>
        public bool SetVariant(int variantIndex)
        {
            if (_meshRenderer == null)
            {
                Debug.LogWarning("CesiumMaterialVariants: MeshRenderer not found.");
                return false;
            }

            // Handle default material case
            if (variantIndex < 0)
            {
                if (defaultMaterial != null)
                {
                    _meshRenderer.material = defaultMaterial;
                    _currentVariantIndex = -1;
                    return true;
                }
                Debug.LogWarning("CesiumMaterialVariants: Default material not available.");
                return false;
            }

            // Validate variant index
            if (variantIndex >= variantNames.Length)
            {
                Debug.LogWarning($"CesiumMaterialVariants: Variant index {variantIndex} is out of range. Available variants: {variantNames.Length}");
                return false;
            }

            // Check if we have a material for this variant
            if (variantMaterials.TryGetValue(variantIndex, out Material variantMaterial))
            {
                if (variantMaterial != null)
                {
                    _meshRenderer.material = variantMaterial;
                    _currentVariantIndex = variantIndex;
                    return true;
                }
                else
                {
                    Debug.LogWarning($"CesiumMaterialVariants: Material for variant '{variantNames[variantIndex]}' is null.");
                    return false;
                }
            }

            Debug.LogWarning($"CesiumMaterialVariants: No material found for variant '{variantNames[variantIndex]}' (index {variantIndex}).");
            return false;
        }

        /// <summary>
        /// Sets the active material variant by name.
        /// Use "Default" or an empty string to switch to the default material.
        /// </summary>
        /// <param name="variantName">The name of the variant to activate.</param>
        /// <returns>True if the variant was successfully set, false otherwise.</returns>
        public bool SetVariant(string variantName)
        {
            if (string.IsNullOrEmpty(variantName) || variantName.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                return SetVariant(-1);
            }

            for (int i = 0; i < variantNames.Length; i++)
            {
                if (variantNames[i].Equals(variantName, StringComparison.OrdinalIgnoreCase))
                {
                    return SetVariant(i);
                }
            }

            Debug.LogWarning($"CesiumMaterialVariants: Variant '{variantName}' not found. Available variants: {string.Join(", ", variantNames)}");
            return false;
        }

        /// <summary>
        /// Toggles between two specific variants. Useful for simple A/B switching.
        /// </summary>
        /// <param name="variantIndexA">First variant index.</param>
        /// <param name="variantIndexB">Second variant index.</param>
        public void ToggleBetween(int variantIndexA, int variantIndexB)
        {
            if (_currentVariantIndex == variantIndexA)
            {
                SetVariant(variantIndexB);
            }
            else
            {
                SetVariant(variantIndexA);
            }
        }

        /// <summary>
        /// Toggles between two specific variants by name.
        /// </summary>
        /// <param name="variantNameA">First variant name.</param>
        /// <param name="variantNameB">Second variant name.</param>
        public void ToggleBetween(string variantNameA, string variantNameB)
        {
            string currentName = GetCurrentVariantName();
            if (currentName.Equals(variantNameA, StringComparison.OrdinalIgnoreCase))
            {
                SetVariant(variantNameB);
            }
            else
            {
                SetVariant(variantNameA);
            }
        }

        /// <summary>
        /// Gets a list of all available variant names.
        /// </summary>
        /// <returns>Array of variant names, or an empty array if none are available.</returns>
        public string[] GetAvailableVariants()
        {
            return variantNames ?? Array.Empty<string>();
        }
    }
}
