#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public static class CesiumCartographicPolygonGeoJsonWriter
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        
        [Serializable]
        private class GeoJsonProperties
        {
            public string name;
        }

        /// <summary>
        /// Saves the polygon's current spline as a GeoJSON FeatureCollection
        /// (containing a single Polygon Feature) at <paramref name="filePath"/>.
        /// </summary>
        /// <returns>True on success, false on any validation or I/O failure.</returns>
        public static bool SaveAsGeoJson(CesiumCartographicPolygon polygon, string filePath)
        {
            if (polygon == null)
            {
                Debug.LogError("CesiumCartographicPolygonGeoJsonWriter: polygon is null.");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("CesiumCartographicPolygonGeoJsonWriter: filePath is null or empty.");
                return false;
            }

            List<double2> points = polygon.GetCartographicPoints(Matrix4x4.identity);
            if (points == null || points.Count < 3)
            {
                Debug.LogError("CesiumCartographicPolygonGeoJsonWriter: the polygon must " +
                    "have at least 3 points to be saved as GeoJSON.");
                return false;
            }

            try
            {
                string geoJson = BuildGeoJson(points, polygon.gameObject.name);
                File.WriteAllText(filePath, geoJson, Encoding.UTF8);
                Debug.Log($"Saved cartographic polygon to '{filePath}'.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"CesiumCartographicPolygonGeoJsonWriter: failed to write " +
                    $"'{filePath}': {ex.Message}");
                return false;
            }
        }


        public static bool SaveAsGeoJsonWithDialog(CesiumCartographicPolygon polygon)
        {
            string defaultName =
                (polygon != null ? polygon.gameObject.name : "CartographicPolygon") + ".geojson";

            string path = EditorUtility.SaveFilePanel(
                "Save Cartographic Polygon as GeoJSON", "", defaultName, "geojson");

            return !string.IsNullOrEmpty(path) && SaveAsGeoJson(polygon, path);
        }


        private static string BuildGeoJson(List<double2> points, string name)
        {
            string propertiesJson = BuildPropertiesJson(name);
            string coordinatesJson = BuildCoordinatesJson(points);

            StringBuilder sb = new StringBuilder(256 + propertiesJson.Length + coordinatesJson.Length);
            sb.Append("{\n")
              .Append("  \"type\": \"FeatureCollection\",\n")
              .Append("  \"features\": [\n")
              .Append("    {\n")
              .Append("      \"type\": \"Feature\",\n")
              .Append("      \"properties\": ").Append(propertiesJson).Append(",\n")
              .Append("      \"geometry\": {\n")
              .Append("        \"type\": \"Polygon\",\n")
              .Append("        \"coordinates\": ").Append(coordinatesJson).Append("\n")
              .Append("      }\n")
              .Append("    }\n")
              .Append("  ]\n")
              .Append("}");

            return sb.ToString();
        }


        private static string BuildPropertiesJson(string name)
        {
            GeoJsonProperties properties = new GeoJsonProperties
            {
                name = name ?? string.Empty
            };
            return JsonUtility.ToJson(properties);
        }


        private static string BuildCoordinatesJson(List<double2> points)
        {
            StringBuilder sb = new StringBuilder(32 + points.Count * 32);
            sb.Append("[[[");

            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0)
                    sb.Append(",[");

                sb.Append(points[i].x.ToString("R", Invariant))
                  .Append(',')
                  .Append(points[i].y.ToString("R", Invariant))
                  .Append(']');
            }

            sb.Append(",[")
              .Append(points[0].x.ToString("R", Invariant))
              .Append(',')
              .Append(points[0].y.ToString("R", Invariant))
              .Append(']');

            sb.Append("]]"); 

            return sb.ToString();
        }
    }
}
#endif
