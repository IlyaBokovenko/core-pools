using System;
using CW.Extensions.Pooling;
using NUnit.Framework;

namespace CW.Extensions.Pooling.Tests
{
    [TestFixture]
    public class TestDisposeBlock
    {
        class Foo : IDisposable
        {
            public static readonly StaticMemoryPool<string, Foo> Pool =
                new StaticMemoryPool<string, Foo>(OnSpawned, OnDespawned);

            public void Dispose()
            {
                Pool.Despawn(this);
            }

            static void OnDespawned(Foo that)
            {
                that.Value = null;
            }

            static void OnSpawned(string value, Foo that)
            {
                that.Value = value;
            }

            public string Value
            {
                get; private set;
            }
        }

        public class Bar : IDisposable
        {
            readonly Pool _pool;

            public Bar(Pool pool)
            {
                _pool = pool;
            }

            public void Dispose()
            {
                _pool.Despawn(this);
            }

            public class Pool : MemoryPool<Bar>
            {
                public Pool(MemoryPoolSettings settings = null)
                {
                    Init(new Factory(this), settings);
                }

                private class Factory : IFactory<Bar>
                {
                    private readonly Pool _pool;

                    public Factory(Pool pool)
                    {
                        _pool = pool;
                    }

                    public Bar Create()
                    {
                        return new Bar(_pool);
                    }
                }
            }
        }

        public class Qux : IDisposable
        {
            public bool WasDisposed
            {
                get; private set;
            }

            public void Dispose()
            {
                WasDisposed = true;
            }
        }

        [Test]
        public void TestExceptions()
        {
            var qux1 = new Qux();
            var qux2 = new Qux();

            try
            {
                using (var block = DisposeBlock.Spawn())
                {
                    block.Add(qux1);
                    block.Add(qux2);
                    throw new Exception();
                }
            }
            catch
            {
            }

            Assert.That(qux1.WasDisposed);
            Assert.That(qux2.WasDisposed);
        }

        [Test]
        public void TestWithStaticMemoryPool()
        {
            var pool = Foo.Pool;

            pool.Clear();

            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            using (var block = DisposeBlock.Spawn())
            {
                block.Spawn(pool, "asdf");

                Assert.That(pool.NumTotal, Is.EqualTo(1));
                Assert.That(pool.NumActive, Is.EqualTo(1));
                Assert.That(pool.NumInactive, Is.EqualTo(0));
            }

            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
        }

        [Test]
        public void TestWithNormalMemoryPool()
        {
            var pool = new Bar.Pool();

            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            using (var block = DisposeBlock.Spawn())
            {
                block.Spawn(pool);

                Assert.That(pool.NumTotal, Is.EqualTo(1));
                Assert.That(pool.NumActive, Is.EqualTo(1));
                Assert.That(pool.NumInactive, Is.EqualTo(0));
            }

            Assert.That(pool.NumTotal, Is.EqualTo(1));
            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
        }
    }
}
