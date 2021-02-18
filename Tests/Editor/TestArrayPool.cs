using CW.Extensions.Pooling;
using NUnit.Framework;

namespace CW.Extensions.Pooling.Tests
{
    [TestFixture]
    public class TestArrayPool
    {
        [Test]
        public void RunTest()
        {
            var pool = ArrayPool<string>.GetPool(2);

            pool.Clear();
            pool.ClearActiveCount();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));

            var arr1 = pool.Spawn();

            Assert.That(arr1.Length, Is.EqualTo(2));

            arr1[0] = "asdf";
            arr1[1] = "zbx";

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            pool.Despawn(arr1);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var arr2 = pool.Spawn();

            Assert.That(arr2.Length, Is.EqualTo(2));
            Assert.IsNull(arr2[0]);
            Assert.IsNull(arr2[1]);

            Assert.That(arr2.Length, Is.EqualTo(2));
            Assert.That(arr2, Is.SameAs(arr1));

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var arr3 = pool.Spawn();

            Assert.That(arr2, Is.Not.SameAs(arr3));

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            pool.Despawn(arr3);
            pool.Despawn(arr2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            // TODO too expensive
            //Assert.Throws<PoolingException>(() => pool.Despawn(arr3));
        }
    }
}

