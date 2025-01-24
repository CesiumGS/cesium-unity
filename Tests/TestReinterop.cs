using NUnit.Framework;

public class TestReinterop
{
    [Test]
    public void TestADotNetExceptionCanBeCaughtInCpp()
    {
        CesiumForUnity.TestReinterop o = new CesiumForUnity.TestReinterop();
        Assert.IsTrue(o.CallThrowAnExceptionFromCppAndCatchIt());
    }

    [Test]
    public void TestADotNetExceptionCanPropagateThroughCpp()
    {
        CesiumForUnity.TestReinterop o = new CesiumForUnity.TestReinterop();
        try
        {
            o.CallThrowAnExceptionFromCppAndDontCatchIt();
        }
        catch (System.Exception e)
        {
            Assert.AreEqual("Test Exception!", e.Message);
        }
    }

    [Test]
    public void TestACppStdExceptionCanBeCaughtInCSharp()
    {
        CesiumForUnity.TestReinterop o = new CesiumForUnity.TestReinterop();
        try
        {
            o.ThrowCppStdException();
        }
        catch (System.Exception e)
        {
            Assert.AreEqual("An exceptional hello from C++!", e.Message);
        }
    }

    [Test]
    public void TestAGeneralCppExceptionCanBeCaughtInCSharp()
    {
        CesiumForUnity.TestReinterop o = new CesiumForUnity.TestReinterop();
        try
        {
            o.ThrowOtherCppExceptionType();
        }
        catch (System.Exception e)
        {
            Assert.AreEqual("An unknown native exception occurred.", e.Message);
        }
    }
}
