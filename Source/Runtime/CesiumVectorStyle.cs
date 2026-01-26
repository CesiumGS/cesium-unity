using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A simple 32-bit color struct that marshals correctly between C# and C++.
    /// Unlike Unity's Color32, this struct does not use a union layout that
    /// confuses Reinterop's code generation.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct CesiumColor32
    {
        /// <summary>Red component (0-255)</summary>
        public byte r;
        /// <summary>Green component (0-255)</summary>
        public byte g;
        /// <summary>Blue component (0-255)</summary>
        public byte b;
        /// <summary>Alpha component (0-255)</summary>
        public byte a;

        /// <summary>
        /// Creates a new CesiumColor32 with the specified components.
        /// </summary>
        public CesiumColor32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        /// Implicit conversion from Unity's Color32.
        /// </summary>
        public static implicit operator CesiumColor32(Color32 color)
        {
            return new CesiumColor32(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Implicit conversion to Unity's Color32.
        /// </summary>
        public static implicit operator Color32(CesiumColor32 color)
        {
            return new Color32(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Returns a string representation of the color.
        /// </summary>
        public override string ToString()
        {
            return $"RGBA({r}, {g}, {b}, {a})";
        }
    }

    /// <summary>
    /// The mode used to interpret the color value provided in a style.
    /// </summary>
    public enum CesiumVectorColorMode
    {
        /// <summary>
        /// The normal color mode. The color will be used directly.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The color will be chosen randomly.
        /// </summary>
        /// <remarks>
        /// The color randomization will be applied to each component, with the
        /// resulting value between 0 and the specified color component value. Alpha is
        /// always ignored. For example, if the color was (R: 0, G: 119, B: 0, A: 255),
        /// the resulting randomized value could be (R: 0, G: 65, B: 0, A: 255), or
        /// (R: 0, G: 118, B: 0, A: 255), but never (R: 0, G: 170, B: 0, A: 255).
        /// </remarks>
        Random = 1
    }

    /// <summary>
    /// The mode used to render polylines and strokes.
    /// </summary>
    public enum CesiumVectorLineWidthMode
    {
        /// <summary>
        /// The line width represents the number of pixels the line will take up,
        /// regardless of LOD.
        /// </summary>
        Pixels = 0,

        /// <summary>
        /// The line width represents the physical size of the line in meters.
        /// </summary>
        Meters = 1
    }

    /// <summary>
    /// The style used to draw polylines and strokes.
    /// </summary>
    [Serializable]
    public struct CesiumVectorLineStyle
    {
        /// <summary>
        /// The color to be used.
        /// </summary>
        [Tooltip("The color to be used.")]
        public CesiumColor32 color;

        /// <summary>
        /// The color mode to be used.
        /// </summary>
        [Tooltip("The color mode to be used.")]
        public CesiumVectorColorMode colorMode;

        /// <summary>
        /// The width of the line or stroke, with the unit specified by widthMode.
        /// </summary>
        [Tooltip("The width of the line or stroke, with the unit specified by widthMode.")]
        [Min(0)]
        public double width;

        /// <summary>
        /// The mode to use when interpreting width.
        /// </summary>
        [Tooltip("The mode to use when interpreting width.")]
        public CesiumVectorLineWidthMode widthMode;

        /// <summary>
        /// Creates a default line style with white color and 1 pixel width.
        /// </summary>
        public static CesiumVectorLineStyle Default => new CesiumVectorLineStyle
        {
            color = new CesiumColor32(255, 255, 255, 255),
            colorMode = CesiumVectorColorMode.Normal,
            width = 1.0,
            widthMode = CesiumVectorLineWidthMode.Pixels
        };
    }

    /// <summary>
    /// The style used to fill polygons.
    /// </summary>
    [Serializable]
    public struct CesiumVectorPolygonFillStyle
    {
        /// <summary>
        /// The color to be used.
        /// </summary>
        [Tooltip("The color to be used.")]
        public CesiumColor32 color;

        /// <summary>
        /// The color mode to be used.
        /// </summary>
        [Tooltip("The color mode to be used.")]
        public CesiumVectorColorMode colorMode;

        /// <summary>
        /// Creates a default polygon fill style with white color.
        /// </summary>
        public static CesiumVectorPolygonFillStyle Default => new CesiumVectorPolygonFillStyle
        {
            color = new CesiumColor32(255, 255, 255, 255),
            colorMode = CesiumVectorColorMode.Normal
        };
    }

    /// <summary>
    /// The style used to draw polygons.
    /// </summary>
    [Serializable]
    public struct CesiumVectorPolygonStyle
    {
        /// <summary>
        /// Whether the polygon should be filled.
        /// </summary>
        [Tooltip("Whether the polygon should be filled.")]
        public bool fill;

        /// <summary>
        /// If fill is true, this style will be used when filling the polygon.
        /// </summary>
        [Tooltip("If fill is true, this style will be used when filling the polygon.")]
        public CesiumVectorPolygonFillStyle fillStyle;

        /// <summary>
        /// Whether the polygon should be outlined.
        /// </summary>
        [Tooltip("Whether the polygon should be outlined.")]
        public bool outline;

        /// <summary>
        /// If outline is true, this style will be used when outlining the polygon.
        /// </summary>
        [Tooltip("If outline is true, this style will be used when outlining the polygon.")]
        public CesiumVectorLineStyle outlineStyle;

        /// <summary>
        /// Creates a default polygon style with fill enabled and outline disabled.
        /// </summary>
        public static CesiumVectorPolygonStyle Default => new CesiumVectorPolygonStyle
        {
            fill = true,
            fillStyle = CesiumVectorPolygonFillStyle.Default,
            outline = false,
            outlineStyle = CesiumVectorLineStyle.Default
        };
    }

    /// <summary>
    /// Style information to use when drawing vector data.
    /// </summary>
    [Serializable]
    public struct CesiumVectorStyle
    {
        /// <summary>
        /// Styles to use when drawing polylines and stroking shapes.
        /// </summary>
        [Tooltip("Styles to use when drawing polylines and stroking shapes.")]
        public CesiumVectorLineStyle lineStyle;

        /// <summary>
        /// Styles to use when drawing polygons.
        /// </summary>
        [Tooltip("Styles to use when drawing polygons.")]
        public CesiumVectorPolygonStyle polygonStyle;

        /// <summary>
        /// Creates a default vector style.
        /// </summary>
        public static CesiumVectorStyle Default => new CesiumVectorStyle
        {
            lineStyle = CesiumVectorLineStyle.Default,
            polygonStyle = CesiumVectorPolygonStyle.Default
        };
    }
}
