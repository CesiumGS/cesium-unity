using NUnit.Framework;
using CesiumForUnity;
using UnityEngine;

public class TestCesiumVectorStyle
{
    #region CesiumColor32 Tests

    [Test]
    public void CesiumColor32ConstructorSetsAllComponents()
    {
        CesiumColor32 color = new CesiumColor32(100, 150, 200, 250);

        Assert.AreEqual(100, color.r);
        Assert.AreEqual(150, color.g);
        Assert.AreEqual(200, color.b);
        Assert.AreEqual(250, color.a);
    }

    [Test]
    public void CesiumColor32ImplicitConversionFromColor32()
    {
        Color32 unityColor = new Color32(50, 100, 150, 200);
        CesiumColor32 cesiumColor = unityColor;

        Assert.AreEqual(50, cesiumColor.r);
        Assert.AreEqual(100, cesiumColor.g);
        Assert.AreEqual(150, cesiumColor.b);
        Assert.AreEqual(200, cesiumColor.a);
    }

    [Test]
    public void CesiumColor32ImplicitConversionToColor32()
    {
        CesiumColor32 cesiumColor = new CesiumColor32(25, 75, 125, 175);
        Color32 unityColor = cesiumColor;

        Assert.AreEqual(25, unityColor.r);
        Assert.AreEqual(75, unityColor.g);
        Assert.AreEqual(125, unityColor.b);
        Assert.AreEqual(175, unityColor.a);
    }

    [Test]
    public void CesiumColor32ToStringReturnsFormattedString()
    {
        CesiumColor32 color = new CesiumColor32(255, 128, 64, 32);

        string result = color.ToString();

        Assert.AreEqual("RGBA(255, 128, 64, 32)", result);
    }

    [Test]
    public void CesiumColor32HandlesMinMaxValues()
    {
        CesiumColor32 minColor = new CesiumColor32(0, 0, 0, 0);
        CesiumColor32 maxColor = new CesiumColor32(255, 255, 255, 255);

        Assert.AreEqual(0, minColor.r);
        Assert.AreEqual(0, minColor.a);
        Assert.AreEqual(255, maxColor.r);
        Assert.AreEqual(255, maxColor.a);
    }

    #endregion

    #region CesiumVectorLineStyle Tests

    [Test]
    public void LineStyleDefaultHasCorrectValues()
    {
        CesiumVectorLineStyle style = CesiumVectorLineStyle.Default;

        Assert.AreEqual(255, style.color.r);
        Assert.AreEqual(255, style.color.g);
        Assert.AreEqual(255, style.color.b);
        Assert.AreEqual(255, style.color.a);
        Assert.AreEqual(CesiumVectorColorMode.Normal, style.colorMode);
        Assert.AreEqual(1.0, style.width, 0.001);
        Assert.AreEqual(CesiumVectorLineWidthMode.Pixels, style.widthMode);
    }

    [Test]
    public void LineStyleCanBeModified()
    {
        CesiumVectorLineStyle style = new CesiumVectorLineStyle();
        style.color = new CesiumColor32(255, 0, 0, 128);
        style.colorMode = CesiumVectorColorMode.Random;
        style.width = 10.5;
        style.widthMode = CesiumVectorLineWidthMode.Meters;

        Assert.AreEqual(255, style.color.r);
        Assert.AreEqual(0, style.color.g);
        Assert.AreEqual(128, style.color.a);
        Assert.AreEqual(CesiumVectorColorMode.Random, style.colorMode);
        Assert.AreEqual(10.5, style.width, 0.001);
        Assert.AreEqual(CesiumVectorLineWidthMode.Meters, style.widthMode);
    }

    #endregion

    #region CesiumVectorPolygonFillStyle Tests

    [Test]
    public void PolygonFillStyleDefaultHasCorrectValues()
    {
        CesiumVectorPolygonFillStyle style = CesiumVectorPolygonFillStyle.Default;

        Assert.AreEqual(255, style.color.r);
        Assert.AreEqual(255, style.color.g);
        Assert.AreEqual(255, style.color.b);
        Assert.AreEqual(255, style.color.a);
        Assert.AreEqual(CesiumVectorColorMode.Normal, style.colorMode);
    }

    [Test]
    public void PolygonFillStyleCanBeModified()
    {
        CesiumVectorPolygonFillStyle style = new CesiumVectorPolygonFillStyle();
        style.color = new CesiumColor32(0, 255, 0, 200);
        style.colorMode = CesiumVectorColorMode.Random;

        Assert.AreEqual(0, style.color.r);
        Assert.AreEqual(255, style.color.g);
        Assert.AreEqual(0, style.color.b);
        Assert.AreEqual(200, style.color.a);
        Assert.AreEqual(CesiumVectorColorMode.Random, style.colorMode);
    }

    #endregion

    #region CesiumVectorPolygonStyle Tests

    [Test]
    public void PolygonStyleDefaultHasFillEnabledOutlineDisabled()
    {
        CesiumVectorPolygonStyle style = CesiumVectorPolygonStyle.Default;

        Assert.IsTrue(style.fill);
        Assert.IsFalse(style.outline);
    }

    [Test]
    public void PolygonStyleCanEnableBothFillAndOutline()
    {
        CesiumVectorPolygonStyle style = new CesiumVectorPolygonStyle();
        style.fill = true;
        style.fillStyle = CesiumVectorPolygonFillStyle.Default;
        style.outline = true;
        style.outlineStyle = CesiumVectorLineStyle.Default;

        Assert.IsTrue(style.fill);
        Assert.IsTrue(style.outline);
    }

    [Test]
    public void PolygonStyleCanDisableBothFillAndOutline()
    {
        CesiumVectorPolygonStyle style = new CesiumVectorPolygonStyle();
        style.fill = false;
        style.outline = false;

        Assert.IsFalse(style.fill);
        Assert.IsFalse(style.outline);
    }

    #endregion

    #region CesiumVectorStyle Tests

    [Test]
    public void VectorStyleDefaultHasCorrectSubStyles()
    {
        CesiumVectorStyle style = CesiumVectorStyle.Default;

        // Line style defaults
        Assert.AreEqual(255, style.lineStyle.color.r);
        Assert.AreEqual(1.0, style.lineStyle.width, 0.001);

        // Polygon style defaults
        Assert.IsTrue(style.polygonStyle.fill);
        Assert.IsFalse(style.polygonStyle.outline);
    }

    [Test]
    public void VectorStyleCanBeFullyConfigured()
    {
        CesiumVectorStyle style = new CesiumVectorStyle();

        // Configure line style
        style.lineStyle.color = new CesiumColor32(255, 0, 0, 255);
        style.lineStyle.width = 5.0;
        style.lineStyle.widthMode = CesiumVectorLineWidthMode.Meters;
        style.lineStyle.colorMode = CesiumVectorColorMode.Normal;

        // Configure polygon style
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new CesiumColor32(0, 255, 0, 180);
        style.polygonStyle.fillStyle.colorMode = CesiumVectorColorMode.Normal;
        style.polygonStyle.outline = true;
        style.polygonStyle.outlineStyle.color = new CesiumColor32(0, 0, 255, 255);
        style.polygonStyle.outlineStyle.width = 2.0;

        // Verify line style
        Assert.AreEqual(255, style.lineStyle.color.r);
        Assert.AreEqual(5.0, style.lineStyle.width, 0.001);
        Assert.AreEqual(CesiumVectorLineWidthMode.Meters, style.lineStyle.widthMode);

        // Verify polygon style
        Assert.IsTrue(style.polygonStyle.fill);
        Assert.AreEqual(0, style.polygonStyle.fillStyle.color.r);
        Assert.AreEqual(255, style.polygonStyle.fillStyle.color.g);
        Assert.AreEqual(180, style.polygonStyle.fillStyle.color.a);
        Assert.IsTrue(style.polygonStyle.outline);
        Assert.AreEqual(0, style.polygonStyle.outlineStyle.color.r);
        Assert.AreEqual(0, style.polygonStyle.outlineStyle.color.g);
        Assert.AreEqual(255, style.polygonStyle.outlineStyle.color.b);
        Assert.AreEqual(2.0, style.polygonStyle.outlineStyle.width, 0.001);
    }

    #endregion

    #region Color Mode Enum Tests

    [Test]
    public void ColorModeNormalHasCorrectValue()
    {
        Assert.AreEqual(0, (int)CesiumVectorColorMode.Normal);
    }

    [Test]
    public void ColorModeRandomHasCorrectValue()
    {
        Assert.AreEqual(1, (int)CesiumVectorColorMode.Random);
    }

    #endregion

    #region Line Width Mode Enum Tests

    [Test]
    public void LineWidthModePixelsHasCorrectValue()
    {
        Assert.AreEqual(0, (int)CesiumVectorLineWidthMode.Pixels);
    }

    [Test]
    public void LineWidthModeMetersHasCorrectValue()
    {
        Assert.AreEqual(1, (int)CesiumVectorLineWidthMode.Meters);
    }

    #endregion
}
