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
        try {
            o.CallThrowAnExceptionFromCppAndDontCatchIt();
        } catch (System.Exception e)
        {
            Assert.AreEqual("Test Exception!", e.Message);
        }
    }
}
