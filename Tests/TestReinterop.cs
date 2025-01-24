using NUnit.Framework;

public class TestReinterop
{
    [Test]
    public void TestADotNetExceptionCanBeCaughtInCpp()
    {
        CesiumForUnity.TestReinterop o = new CesiumForUnity.TestReinterop();
        Assert.IsTrue(o.CallThrowAnExceptionFromCpp());
    }
}
