using Reinterop;
using System;
using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents an object in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON object can be a geometry (Point, LineString, Polygon, etc.),
    /// a Feature (geometry with properties), or a collection of other objects.
    /// Use <see cref="GetObjectAsFeature"/> to access Feature-specific data,
    /// or <see cref="GetObjectAsFeatureCollection"/> to access the features
    /// in a FeatureCollection.
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
        /// Gets this object as a <see cref="CesiumGeoJsonFeature"/>, if it
        /// is a Feature.
        /// </summary>
        /// <returns>A <see cref="CesiumGeoJsonFeature"/> if this object is a
        /// Feature; null otherwise.</returns>
        public partial CesiumGeoJsonFeature GetObjectAsFeature();

        /// <summary>
        /// Gets the features in this FeatureCollection.
        /// </summary>
        /// <returns>An array of <see cref="CesiumGeoJsonFeature"/> if this object
        /// is a FeatureCollection; null otherwise.</returns>
        public partial CesiumGeoJsonFeature[] GetObjectAsFeatureCollection();

        /// <summary>
        /// Gets this object as a Point geometry.
        /// </summary>
        /// <returns>A <see cref="double3"/> with the point's LLH coordinates,
        /// or a zero vector if this object is not a Point.</returns>
        public partial double3 GetObjectAsPoint();

        /// <summary>
        /// Gets this object as a MultiPoint geometry.
        /// </summary>
        /// <returns>An array of <see cref="double3"/> with the points' LLH
        /// coordinates, or null if this object is not a MultiPoint.</returns>
        public partial double3[] GetObjectAsMultiPoint();

        /// <summary>
        /// Gets this object as a LineString geometry.
        /// </summary>
        /// <returns>A <see cref="CesiumGeoJsonLineString"/> if this object is a
        /// LineString; null otherwise.</returns>
        public partial CesiumGeoJsonLineString GetObjectAsLineString();

        /// <summary>
        /// Gets this object as a MultiLineString geometry.
        /// </summary>
        /// <returns>An array of <see cref="CesiumGeoJsonLineString"/> if this
        /// object is a MultiLineString; null otherwise.</returns>
        public partial CesiumGeoJsonLineString[] GetObjectAsMultiLineString();

        /// <summary>
        /// Gets this object as a Polygon geometry.
        /// </summary>
        /// <returns>A <see cref="CesiumGeoJsonPolygon"/> if this object is a
        /// Polygon; null otherwise.</returns>
        public partial CesiumGeoJsonPolygon GetObjectAsPolygon();

        /// <summary>
        /// Gets this object as a MultiPolygon geometry.
        /// </summary>
        /// <returns>An array of <see cref="CesiumGeoJsonPolygon"/> if this
        /// object is a MultiPolygon; null otherwise.</returns>
        public partial CesiumGeoJsonPolygon[] GetObjectAsMultiPolygon();

        /// <summary>
        /// Gets this object as a GeometryCollection.
        /// </summary>
        /// <returns>An array of <see cref="CesiumGeoJsonObject"/> if this
        /// object is a GeometryCollection; null otherwise.</returns>
        public partial CesiumGeoJsonObject[] GetObjectAsGeometryCollection();

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
    }
}
