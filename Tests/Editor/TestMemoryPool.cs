using System.Collections.Generic;
using CW.Extensions.Pooling;
using NUnit.Framework;

#pragma warning disable 219

namespace CW.Extensions.Pooling.Tests
{
    [TestFixture]
    public class TestMemoryPool
    {
        [Test]
        public void TestFactoryProperties()
        {
            var pool = new Foo.Pool();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(foo.ResetCount, Is.EqualTo(1));

            pool.Despawn(foo);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(foo.ResetCount, Is.EqualTo(1));

            foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(foo.ResetCount, Is.EqualTo(2));

            var foo2 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(foo2.ResetCount, Is.EqualTo(1));

            pool.Despawn(foo);

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(foo.ResetCount, Is.EqualTo(2));

            pool.Despawn(foo2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));
        }

        [Test]
        public void TestFactoryPropertiesDefault()
        {
            var pool = new Foo.Pool();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.Despawn(foo);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo2 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.Despawn(foo);

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            pool.Despawn(foo2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));
        }

        [Test]
        public void TestExpandDouble()
        {
            var pool = new Foo.Pool(new MemoryPoolSettings{ExpandMethod = PoolExpandMethods.Double});

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo2 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            var foo3 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(4));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            pool.Despawn(foo2);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(4));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            var foo4 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(4));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
        }

        [Test]
        public void TestFixedSize()
        {
            var pool = new Foo.Pool(new MemoryPoolSettings{InitialSize = 2, ExpandMethod = PoolExpandMethods.Disabled});

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            var foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            var foo2 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            Assert.Throws<PoolingException>(() => pool.Spawn());
        }

        [Test]
        public void TestInitialSize()
        {
            var pool = new Foo.Pool(new MemoryPoolSettings{InitialSize = 5});

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(5));
        }

        [Test]
        public void TestExpandAndShrinkManually()
        {
            var pool = new Foo.Pool();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.ExpandBy(2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            var foo = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            pool.ExpandBy(3);

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(4));

            var foo2 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            var foo3 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            pool.ExpandBy(1);

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(6));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            pool.Despawn(foo2);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(6));
            Assert.That(pool.NumInactive, Is.EqualTo(4));

            var foo4 = pool.Spawn();

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(6));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            pool.ShrinkBy(1);

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            pool.Resize(6);

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(9));
            Assert.That(pool.NumInactive, Is.EqualTo(6));

            pool.Clear();

            Assert.That(pool.NumActive, Is.EqualTo(3));
            Assert.That(pool.NumTotal, Is.EqualTo(3));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            Assert.Throws<PoolingException>(() => pool.Resize(-1));
            Assert.Throws<PoolingException>(() => pool.ShrinkBy(1));
        }

        [Test]
        public void TestMaxSize()
        {
            var pool = new Foo.Pool(new MemoryPoolSettings{InitialSize = 2, MaxSize = 4});

            var foos = new List<Foo>();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            foos.Add(pool.Spawn());
            foos.Add(pool.Spawn());
            foos.Add(pool.Spawn());
            foos.Add(pool.Spawn());
            foos.Add(pool.Spawn());

            Assert.That(pool.NumActive, Is.EqualTo(5));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.Despawn(foos[0]);
            pool.Despawn(foos[1]);
            pool.Despawn(foos[2]);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            pool.Despawn(foos[3]);
            pool.Despawn(foos[4]);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(4));
            Assert.That(pool.NumInactive, Is.EqualTo(4));
        }

        class Foo
        {
            public int ResetCount
            {
                get; private set;
            }

            public class Pool : MemoryPool<Foo>
            {
                public Pool(MemoryPoolSettings settings = null) : base(new Factory(), settings)
                {
                }

                protected override void OnSpawned(Foo foo)
                {
                    foo.ResetCount++;
                }

                private class Factory : IFactory<Foo>
                {
                    public Foo Create()
                    {
                        return new Foo();
                    }
                }
            }
        }
    }
}

