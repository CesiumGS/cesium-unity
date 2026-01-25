using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents an object in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON object can be a geometry (Point, LineString, Polygon, etc.),
    /// a Feature (geometry with properties), or a collection of other objects.
    /// </remarks>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGeoJsonObjectImpl",
        "CesiumGeoJsonObjectImpl.h")]
    public partial class CesiumGeoJsonObject
    {
        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonObject()
        {
            CreateImplementation();
        }
        /// <summary>
        /// Gets the type of this GeoJSON object.
        /// </summary>
        public partial CesiumGeoJsonObjectType GetObjectType();

        /// <summary>
        /// Checks whether this GeoJSON object is valid.
        /// </summary>
        /// <returns>True if this object is valid; false otherwise.</returns>
        public partial bool IsValid();

        /// <summary>
        /// Gets the number of child objects in this GeoJSON object.
        /// </summary>
        /// <remarks>
        /// For FeatureCollection, this returns the number of features.
        /// For GeometryCollection, this returns the number of geometries.
        /// For Feature, this returns 1 if it has a geometry, 0 otherwise.
        /// For geometry types, this returns 0.
        /// </remarks>
        /// <returns>The number of child objects.</returns>
        public partial int GetChildCount();

        /// <summary>
        /// Gets a child object at the specified index.
        /// </summary>
        /// <param name="index">The index of the child object.</param>
        /// <returns>The child object, or null if the index is out of range.</returns>
        public partial CesiumGeoJsonObject GetChild(int index);

        /// <summary>
        /// Gets all child objects of this GeoJSON object.
        /// </summary>
        /// <returns>A list of all child objects.</returns>
        public List<CesiumGeoJsonObject> GetChildren()
        {
            var children = new List<CesiumGeoJsonObject>();
            int count = GetChildCount();
            for (int i = 0; i < count; i++)
            {
                var child = GetChild(i);
                if (child != null)
                {
                    children.Add(child);
                }
            }
            return children;
        }

        /// <summary>
        /// Checks whether this GeoJSON object has a style.
        /// </summary>
        /// <returns>True if this object has a style; false otherwise.</returns>
        public partial bool HasStyle();

        /// <summary>
        /// Gets the style of this GeoJSON object.
        /// </summary>
        /// <returns>The style, or the default style if no style is set.</returns>
        public partial CesiumVectorStyle GetStyle();

        /// <summary>
        /// Sets the style of this GeoJSON object.
        /// </summary>
        /// <param name="style">The style to set.</param>
        public partial void SetStyle(CesiumVectorStyle style);

        /// <summary>
        /// Clears any style set on this GeoJSON object.
        /// </summary>
        /// <remarks>
        /// After calling this method, the object will inherit its style from
        /// its parent, or use the default style if no parent has a style.
        /// </remarks>
        public partial void ClearStyle();

        /// <summary>
        /// Gets the feature ID as a string, if this is a Feature with a string ID.
        /// </summary>
        /// <returns>The feature ID as a string, or an empty string if not applicable.</returns>
        public partial string GetFeatureIdString();

        /// <summary>
        /// Gets the feature ID as an integer, if this is a Feature with an integer ID.
        /// </summary>
        /// <returns>The feature ID as an integer, or 0 if not applicable.</returns>
        public partial long GetFeatureIdInt();

        /// <summary>
        /// Checks whether this Feature has an ID.
        /// </summary>
        /// <returns>True if this Feature has an ID; false otherwise.</returns>
        public partial bool HasFeatureId();

        /// <summary>
        /// Checks whether this Feature has a string ID.
        /// </summary>
        /// <returns>True if this Feature has a string ID; false otherwise.</returns>
        public partial bool HasFeatureIdString();

        /// <summary>
        /// Gets the properties of this Feature as a JSON string.
        /// </summary>
        /// <returns>A JSON string containing the properties, or an empty string if not a Feature or no properties.</returns>
        public partial string GetPropertiesAsJson();

        /// <summary>
        /// Gets a string property value from this Feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value as a string, or an empty string if not found.</returns>
        public partial string GetStringProperty(string propertyName);

        /// <summary>
        /// Gets a numeric property value from this Feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value as a double, or 0 if not found.</returns>
        public partial double GetNumericProperty(string propertyName);

        /// <summary>
        /// Checks whether a property exists on this Feature.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property exists; false otherwise.</returns>
        public partial bool HasProperty(string propertyName);

        /// <summary>
        /// Releases native resources associated with this GeoJSON object.
        /// </summary>
        internal partial void DisposeNative();

        /// <summary>
        /// Finalizer to release native resources.
        /// </summary>
        ~CesiumGeoJsonObject()
        {
            DisposeNative();
        }
    }
}
