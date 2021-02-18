using System;
using System.Collections.Generic;
using NUnit.Framework;
using CW.Extensions.Pooling;

namespace CW.Extensions.Pooling.Tests
{
    public class TestStaticMemoryPool
    {

        [Test]
        public void RunTest()
        {
            var pool = Foo.Pool;

            pool.Clear();
            pool.ClearActiveCount();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));

            var foo = pool.Spawn("asdf");

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            Assert.That(foo.Value, Is.EqualTo("asdf"));
            pool.Despawn(foo);
            Assert.IsNull(foo.Value);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var foo2 = pool.Spawn("zxcv");
            Assert.That(ReferenceEquals(foo, foo2));
            Assert.That(foo2.Value, Is.EqualTo("zxcv"));

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var foo3 = pool.Spawn("bar");
            Assert.That(!ReferenceEquals(foo2, foo3));

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            pool.Despawn(foo3);
            pool.Despawn(foo2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            // too expensive
            //Assert.Throws<PoolingException>(() => pool.Despawn(foo3));
        }

        [Test]
        public void TestListPool()
        {
            var pool = ListPool<string>.Instance;

            pool.Clear();
            pool.ClearActiveCount();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));

            var list = pool.Spawn();

            list.Add("asdf");
            list.Add("zbx");

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            pool.Despawn(list);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var list2 = pool.Spawn();

            Assert.That(list2.Count, Is.EqualTo(0));
            Assert.That(list2, Is.SameAs(list));

            Assert.That(pool.NumActive, Is.EqualTo(1));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(1));

            var list3 = pool.Spawn();

            Assert.That(list2, Is.Not.SameAs(list3));

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            pool.Despawn(list3);
            pool.Despawn(list2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));

            // too expensive 
            // Assert.Throws<PoolingException>(() => pool.Despawn(list3));
        }

        [Test]
        public void TestPoolWrapper()
        {
            var pool = Foo.Pool;

            pool.Clear();
            pool.ClearActiveCount();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));

            using (var block = DisposeBlock.Spawn())
            {
                block.Spawn(pool, "asdf");

                Assert.That(pool.NumActive, Is.EqualTo(1));
                Assert.That(pool.NumInactive, Is.EqualTo(0));
                Assert.That(pool.NumTotal, Is.EqualTo(1));
            }

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(1));
            Assert.That(pool.NumTotal, Is.EqualTo(1));
        }

        [Test]
        public void TestResize()
        {
            var pool = Bar.Pool;

            pool.Clear();
            pool.ClearActiveCount();

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(0));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.Resize(2);

            Assert.That(pool.NumActive, Is.EqualTo(0));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            var bars = new List<Bar>();

            bars.Add(pool.Spawn());
            bars.Add(pool.Spawn());
            bars.Add(pool.Spawn());
            bars.Add(pool.Spawn());
            bars.Add(pool.Spawn());

            Assert.That(pool.NumActive, Is.EqualTo(5));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            pool.Despawn(bars[0]);
            pool.Despawn(bars[1]);
            pool.Despawn(bars[2]);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            pool.ShrinkBy(1);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(4));
            Assert.That(pool.NumInactive, Is.EqualTo(2));

            pool.ExpandBy(1);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(5));
            Assert.That(pool.NumInactive, Is.EqualTo(3));

            pool.Resize(1);

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(3));
            Assert.That(pool.NumInactive, Is.EqualTo(1));

            pool.Clear();

            Assert.That(pool.NumActive, Is.EqualTo(2));
            Assert.That(pool.NumTotal, Is.EqualTo(2));
            Assert.That(pool.NumInactive, Is.EqualTo(0));

            Assert.Throws<PoolingException>(() => pool.Resize(-1));
            Assert.Throws<PoolingException>(() => pool.ShrinkBy(1));
        }

        public class Bar
        {
            public static readonly StaticMemoryPool<Bar> Pool =
                new StaticMemoryPool<Bar>(OnSpawned, OnDespawned);

            static void OnSpawned(Bar that)
            {
            }

            static void OnDespawned(Bar that)
            {
            }
        }

        public class Foo : IDisposable
        {
            public static readonly StaticMemoryPool<string, Foo> Pool =
                new StaticMemoryPool<string, Foo>(OnSpawned, OnDespawned);

            public string Value
            {
                get; private set;
            }

            public void Dispose()
            {
                Pool.Despawn(this);
            }

            static void OnSpawned(string value, Foo that)
            {
                that.Value = value;
            }

            static void OnDespawned(Foo that)
            {
                that.Value = null;
            }
        }
    }
}
