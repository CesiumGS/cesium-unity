using NUnit.Framework;
using CesiumForUnity;
using System.Collections.Generic;

public class TestCesiumObjectPool
{
    private class TestObject
    {
        public bool isDestroyed = false;
        public bool isReleased = false;
    }

    [Test]
    public void ObjectReleasedIntoPoolCanBeRetrieved()
    {
        var pool = new CesiumObjectPool<TestObject>(
            () => new TestObject(),
            (to) => to.isReleased = true,
            (to) => to.isDestroyed = true);

        TestObject obj = pool.Get();
        pool.Release(obj);
        TestObject newObj = pool.Get();
        Assert.AreSame(obj, newObj);
    }

    [Test]
    public void IfPoolIsFullObjectIsReleasedAndDestroyed()
    {
        int maxSize = 5;
        var pool = new CesiumObjectPool<TestObject>(
            () => new TestObject(),
            (to) => to.isReleased = true,
            (to) => to.isDestroyed = true,
            maxSize);

        List<TestObject> gotten = new List<TestObject>();
        for (int i = 0; i < maxSize; ++i)
        {
            gotten.Add(pool.Get());
        }

        TestObject oneMore = pool.Get();

        foreach (TestObject to in gotten)
        {
            pool.Release(to);
        }

        pool.Release(oneMore);

        Assert.IsTrue(oneMore.isReleased);
        Assert.IsTrue(oneMore.isDestroyed);

        foreach (TestObject to in gotten)
        {
            Assert.IsTrue(to.isReleased);
            Assert.IsFalse(to.isDestroyed);
        }
    }

    [Test]
    public void AfterPoolIsDisposedReleasedObjectsAreDestroyed()
    {
        var pool = new CesiumObjectPool<TestObject>(
            () => new TestObject(),
            (to) => to.isReleased = true,
            (to) => to.isDestroyed = true);

        TestObject to = pool.Get();
        pool.Dispose();
        pool.Release(to);
        Assert.IsTrue(to.isReleased);
        Assert.IsTrue(to.isDestroyed);
    }
}