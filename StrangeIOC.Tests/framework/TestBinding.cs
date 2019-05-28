using NUnit.Framework;
using strange.framework.api;
using strange.framework.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestBinding
    {
        [SetUp]
        public void Setup()
        {
            binding = new Binding();
        }

        [TearDown]
        public void TearDown()
        {
            binding = null;
        }

        private IBinding binding;

        [Test]
        public void TestIntToValue()
        {
            const int TEST_VALUE = 42;
            binding.Bind<int>().To(TEST_VALUE);
            Assert.That(typeof(int) == binding.key);
            var values = binding.value as object[];
            Assert.That(TEST_VALUE == (int) values[0]);
        }

        [Test]
        public void TestKeyAsIntType()
        {
            binding.Bind<int>();
            var typeOfInt = typeof(int);
            Assert.That(binding.key == typeOfInt);
        }

        [Test]
        public void TestKeyAsObject()
        {
            var value = new MarkerClass();
            binding.Bind(value);

            Assert.That(binding.key == value);
        }

        [Test]
        public void TestKeyAsType()
        {
            binding.Bind<MarkerClass>();
            Assert.That(binding.key == typeof(MarkerClass));
        }

        [Test]
        public void TestKeyToAsStrings()
        {
            const string TEST_KEY = "Test Key";
            const string TEST_VALUE = "Test result value";
            binding.Bind(TEST_KEY).To(TEST_VALUE);
            Assert.That(TEST_KEY == binding.key as string);
            var values = binding.value as object[];
            Assert.That(TEST_VALUE == values[0] as string);
        }

        [Test]
        public void TestKeyToAsTypes()
        {
            binding.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            Assert.That(binding.key == typeof(InjectableSuperClass));
            var values = binding.value as object[];
            Assert.That(values[0] == typeof(InjectableDerivedClass));
        }

        [Test]
        public void TestKeyToWithMultipleChainedValues()
        {
            var test1 = new ClassWithConstructorParameters(1, "abc");
            var test2 = new ClassWithConstructorParameters(2, "def");
            var test3 = new ClassWithConstructorParameters(3, "ghi");

            binding.Bind<ISimpleInterface>().To(test1).To(test2).To(test3);
            Assert.That(binding.key == typeof(ISimpleInterface));

            var values = binding.value as object[];
            Assert.IsNotNull(values);
            Assert.That(values.Length == 3);
            for (var a = 0; a < values.Length; a++)
            {
                var value = values[a] as ISimpleInterface;
                Assert.IsNotNull(value);
                Assert.That(value.intValue == a + 1);
            }
        }

        [Test]
        public void TestNameAsType()
        {
            binding.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName<MarkerClass>();
            Assert.That(binding.name == typeof(MarkerClass));
        }

        [Test]
        public void TestNameToValue()
        {
            binding.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName(SomeEnum.FOUR);
            Assert.That((SomeEnum) binding.name == SomeEnum.FOUR);
        }

        [Test]
        public void TestOneToOneConstrainedBinding()
        {
            var test1 = new ClassWithConstructorParameters(1, "abc");
            var test2 = new ClassWithConstructorParameters(2, "def");
            var test3 = new ClassWithConstructorParameters(3, "ghi");

            binding.valueConstraint = BindingConstraintType.ONE;
            binding.Bind<ISimpleInterface>().To(test1).To(test2).To(test3);
            Assert.That(binding.key == typeof(ISimpleInterface));
            Assert.That(binding.value is ClassWithConstructorParameters);
            Assert.That((binding.value as ClassWithConstructorParameters).intValue == 3);

            //Clean up
            binding.valueConstraint = BindingConstraintType.MANY;
        }

        [Test]
        public void TestWeakBinding()
        {
            binding.Bind<int>().To(42);
            Assert.False(binding.isWeak);
            binding.Weak();
            Assert.True(binding.isWeak);
        }
    }
}