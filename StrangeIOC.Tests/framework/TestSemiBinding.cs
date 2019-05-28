using NUnit.Framework;
using strange.framework.api;
using strange.framework.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestSemiBinding
    {
        [SetUp]
        public void Setup()
        {
            semibinding = new SemiBinding();
        }

        [TearDown]
        public void TearDown()
        {
            semibinding = null;
        }

        private ISemiBinding semibinding;

        [Test]
        public void TestAddList()
        {
            semibinding.constraint = BindingConstraintType.MANY;

            var o = new ClassWithConstructorParameters(42, "abc");
            var o1 = new ClassWithConstructorParameters(43, "def");
            var o2 = new ClassWithConstructorParameters(44, "ghi");

            var list = new ClassWithConstructorParameters[3] {o, o1, o2};
            semibinding.Add(list);

            var values = semibinding.value as object[];
            Assert.AreEqual(3, values.Length);
            var value = values[2] as ClassWithConstructorParameters;
            Assert.AreEqual(o2, value);
            Assert.AreEqual(44, value.intValue);
        }

        [Test]
        public void TestIntType()
        {
            semibinding.Add(typeof(int));
            var typeOfInt = typeof(int);
            Assert.AreEqual(typeOfInt, semibinding.value);
        }

        [Test]
        public void TestMultiSemibinding()
        {
            semibinding.constraint = BindingConstraintType.MANY;

            var o = new ClassWithConstructorParameters(42, "abc");
            semibinding.Add(o);
            var o1 = new ClassWithConstructorParameters(43, "def");
            semibinding.Add(o1);
            var o2 = new ClassWithConstructorParameters(44, "ghi");
            semibinding.Add(o2);

            var values = semibinding.value as object[];
            Assert.AreEqual(3, values.Length);
            var value = values[2] as ClassWithConstructorParameters;
            Assert.AreEqual(o2, value);
            Assert.AreEqual(44, value.intValue);
        }

        [Test]
        public void TestObject()
        {
            var o = new ClassWithConstructorParameters(42, "abc");
            semibinding.Add(o);
            Assert.AreEqual(o, semibinding.value);
            Assert.AreEqual(42, o.intValue);
        }

        [Test]
        public void TestOverwriteSingleSemibinding()
        {
            var o = new ClassWithConstructorParameters(42, "abc");
            semibinding.Add(o);
            var o1 = new ClassWithConstructorParameters(43, "def");
            semibinding.Add(o1);
            var o2 = new ClassWithConstructorParameters(44, "ghi");
            semibinding.Add(o2);
            Assert.AreNotEqual(o, semibinding.value);
            Assert.AreEqual(o2, semibinding.value);
            Assert.AreEqual(44, o2.intValue);
        }

        [Test]
        public void TestRemoveFromMultiSemibinding()
        {
            semibinding.constraint = BindingConstraintType.MANY;

            var o = new ClassWithConstructorParameters(42, "abc");
            semibinding.Add(o);
            var o1 = new ClassWithConstructorParameters(43, "def");
            semibinding.Add(o1);
            var o2 = new ClassWithConstructorParameters(44, "ghi");
            semibinding.Add(o2);

            var before = semibinding.value as object[];
            Assert.AreEqual(3, before.Length);
            var beforeValue = before[2] as ClassWithConstructorParameters;
            Assert.AreEqual(o2, beforeValue);
            Assert.AreEqual(44, beforeValue.intValue);

            semibinding.Remove(o1);

            var after = semibinding.value as object[];
            Assert.AreEqual(2, after.Length);
            var afterValue = after[1] as ClassWithConstructorParameters;
            Assert.AreEqual(o2, afterValue);
            Assert.AreEqual(44, afterValue.intValue);
        }

        [Test]
        public void TestRemoveFromSingleSemibinding()
        {
            semibinding.constraint = BindingConstraintType.ONE;

            var o = new ClassWithConstructorParameters(42, "abc");
            semibinding.Add(o);

            var value = semibinding.value as ClassWithConstructorParameters;

            Assert.AreEqual(o, value);
            Assert.AreEqual(42, value.intValue);

            semibinding.Remove(o);

            Assert.IsNull(semibinding.value);
        }

        [Test]
        public void TestRemoveList()
        {
            semibinding.constraint = BindingConstraintType.MANY;

            var o = new ClassWithConstructorParameters(42, "abc");
            var o1 = new ClassWithConstructorParameters(43, "def");
            var o2 = new ClassWithConstructorParameters(44, "ghi");
            var list = new ClassWithConstructorParameters[3] {o, o1, o2};
            semibinding.Add(list);

            var before = semibinding.value as object[];
            Assert.AreEqual(3, before.Length);
            var beforeValue = before[2] as ClassWithConstructorParameters;
            Assert.AreEqual(o2, beforeValue);
            Assert.AreEqual(44, beforeValue.intValue);

            var removalList = new ClassWithConstructorParameters[2] {o, o2};
            semibinding.Remove(removalList);

            var after = semibinding.value as object[];
            Assert.AreEqual(1, after.Length);
            var afterValue = after[0] as ClassWithConstructorParameters;
            Assert.AreEqual(o1, afterValue);
            Assert.AreEqual(43, afterValue.intValue);
        }

        [Test]
        public void TestType()
        {
            semibinding.Add(typeof(TestSemiBinding));
            Assert.AreEqual(typeof(TestSemiBinding), semibinding.value);
        }
    }
}