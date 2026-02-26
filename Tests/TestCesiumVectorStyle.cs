using NUnit.Framework;
using CesiumForUnity;
using UnityEngine;

public class TestCesiumVectorStyle
{
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
        style.color = new Color32(255, 0, 0, 128);
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
        style.color = new Color32(0, 255, 0, 200);
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
        style.lineStyle.color = new Color32(255, 0, 0, 255);
        style.lineStyle.width = 5.0;
        style.lineStyle.widthMode = CesiumVectorLineWidthMode.Meters;
        style.lineStyle.colorMode = CesiumVectorColorMode.Normal;

        // Configure polygon style
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new Color32(0, 255, 0, 180);
        style.polygonStyle.fillStyle.colorMode = CesiumVectorColorMode.Normal;
        style.polygonStyle.outline = true;
        style.polygonStyle.outlineStyle.color = new Color32(0, 0, 255, 255);
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
