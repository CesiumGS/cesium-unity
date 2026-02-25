using Reinterop;
using System;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a Feature in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON Feature has an optional ID, a set of properties, and
    /// an associated geometry. Use <see cref="CesiumGeoJsonObject.GetObjectAsFeature"/>
    /// or <see cref="CesiumGeoJsonObject.GetObjectAsFeatureCollection"/> to obtain
    /// instances of this class.
    /// </remarks>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGeoJsonFeatureImpl",
        "CesiumGeoJsonFeatureImpl.h")]
    public partial class CesiumGeoJsonFeature
    {
        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonFeature()
        {
            CreateImplementation();
        }

        /// <summary>
        /// Gets the type of ID on this feature.
        /// </summary>
        /// <returns>The ID type: None, String, or Integer.</returns>
        public partial CesiumGeoJsonFeatureIdType GetIdType();

        /// <summary>
        /// Gets the feature ID as a string.
        /// </summary>
        /// <returns>The feature ID as a string, or an empty string if the
        /// feature has no ID or the ID is not a string.</returns>
        public partial string GetIdAsString();

        /// <summary>
        /// Gets the feature ID as an integer.
        /// </summary>
        /// <returns>The feature ID as a long, or 0 if the feature has no ID
        /// or the ID is not an integer.</returns>
        public partial long GetIdAsInteger();

        /// <summary>
        /// Gets the properties of this feature as a JSON string.
        /// </summary>
        /// <returns>A JSON string containing the properties, or "{}" if the
        /// feature has no properties.</returns>
        public partial string GetPropertiesAsJson();

        /// <summary>
        /// Gets a string property value from this feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value as a string, or an empty string if not found
        /// or not a string type.</returns>
        public partial string GetStringProperty(string propertyName);

        /// <summary>
        /// Gets a numeric property value from this feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value as a double, or 0 if not found or not
        /// a numeric type.</returns>
        public partial double GetNumericProperty(string propertyName);

        /// <summary>
        /// Checks whether a property exists on this feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property exists; false otherwise.</returns>
        public partial bool HasProperty(string propertyName);

        /// <summary>
        /// Gets the geometry associated with this feature.
        /// </summary>
        /// <returns>The geometry as a <see cref="CesiumGeoJsonObject"/>, or null
        /// if this feature has no geometry.</returns>
        public partial CesiumGeoJsonObject GetGeometry();

        /// <summary>
        /// Checks whether this feature has a style.
        /// </summary>
        /// <returns>True if this feature has a style; false otherwise.</returns>
        public partial bool HasStyle();

        /// <summary>
        /// Gets the style of this feature.
        /// </summary>
        /// <returns>The style, or the default style if no style is set.</returns>
        public partial CesiumVectorStyle GetStyle();

        /// <summary>
        /// Sets the style of this feature.
        /// </summary>
        /// <param name="style">The style to set.</param>
        public partial void SetStyle(CesiumVectorStyle style);

        /// <summary>
        /// Clears any style set on this feature.
        /// </summary>
        /// <remarks>
        /// After calling this method, the feature will use the default style.
        /// </remarks>
        public partial void ClearStyle();

        /// <summary>
        /// Releases native resources associated with this feature.
        /// </summary>
        internal partial void DisposeNative();

        /// <summary>
        /// Finalizer to release native resources.
        /// </summary>
        ~CesiumGeoJsonFeature()
        {
            DisposeNative();
        }
    }
}
