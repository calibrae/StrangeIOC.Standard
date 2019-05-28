using System;
using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.framework.api;
using strange.framework.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestInjectionBinding
    {
        [Test]
        public void TestDefaultType()
        {
            const string TEST_KEY = "TEST_KEY";
            Binder.BindingResolver resolver = delegate(IBinding binding)
            {
                (binding as IInjectionBinding).type = InjectionBindingType.DEFAULT;
                Assert.That(TEST_KEY == binding.value as string);
                Assert.That((binding as InjectionBinding).type == InjectionBindingType.DEFAULT);
            };
            var defaultBinding = new InjectionBinding(resolver);
            defaultBinding.To(TEST_KEY);
        }

        [Test]
        public void TestGetSupply()
        {
            var supplied = new Type[3];
            supplied[0] = typeof(HasANamedInjection);
            supplied[1] = typeof(HasANamedInjection2);
            supplied[2] = typeof(InjectsClassToBeInjected);
            var iterator = 0;

            Binder.BindingResolver resolver = delegate(IBinding bound)
            {
                var value = (bound as IInjectionBinding).GetSupply();
                Assert.AreEqual(value[value.Length - 1], supplied[iterator]);
            };

            var binding = new InjectionBinding(resolver);

            while (iterator < 3)
            {
                binding.SupplyTo(supplied[iterator]);
                iterator++;
            }

            var supply = binding.GetSupply();
            Assert.AreEqual(3, supply.Length);

            for (var a = 0; a < supply.Length; a++)
            {
                Assert.AreEqual(supply[a], supplied[a]);
            }
        }

        [Test]
        public void TestIllegalValueBinding()
        {
            var illegalValue = new MarkerClass();

            Binder.BindingResolver resolver = delegate { };
            TestDelegate testDelegate = delegate
            {
                new InjectionBinding(resolver).Bind<InjectableSuperClass>().To<InjectableDerivedClass>()
                    .ToValue(illegalValue);
            };
            var ex =
                Assert.Throws<InjectionException>(testDelegate);
            Assert.That(ex.type == InjectionExceptionType.ILLEGAL_BINDING_VALUE);
        }

        [Test]
        public void TestSingletonChainBinding()
        {
            var a = 0;

            Binder.BindingResolver resolver = delegate(IBinding binding)
            {
                Assert.That(binding.value == typeof(InjectableDerivedClass));
                var correctType = a == 0 ? InjectionBindingType.DEFAULT : InjectionBindingType.SINGLETON;
                Assert.That((binding as InjectionBinding).type == correctType);
                a++;
            };
            new InjectionBinding(resolver).Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToSingleton();
        }

        [Test]
        public void TestSingletonType()
        {
            const string TEST_KEY = "TEST_KEY";
            Binder.BindingResolver resolver = delegate(IBinding binding)
            {
                (binding as IInjectionBinding).type = InjectionBindingType.SINGLETON;
                Assert.That(TEST_KEY == binding.value as string);
                Assert.That((binding as InjectionBinding).type == InjectionBindingType.SINGLETON);
            };
            var defaultBinding = new InjectionBinding(resolver);
            defaultBinding.To(TEST_KEY);
        }

        [Test]
        public void TestSupplyOne()
        {
            Binder.BindingResolver resolver = delegate(IBinding bound)
            {
                var value = (bound as IInjectionBinding).GetSupply();
                Assert.AreEqual(value[0], typeof(HasANamedInjection));
            };
            var binding = new InjectionBinding(resolver);
            binding.SupplyTo<HasANamedInjection>();
        }

        [Test]
        public void TestSupplySeveral()
        {
            var supplied = new Type[3];
            supplied[0] = typeof(HasANamedInjection);
            supplied[1] = typeof(HasANamedInjection2);
            supplied[2] = typeof(InjectsClassToBeInjected);
            var iterator = 0;
            var resolveIterator = 0;

            Binder.BindingResolver resolver = delegate(IBinding bound)
            {
                var value = (bound as IInjectionBinding).GetSupply();
                Assert.AreEqual(value[value.Length - 1], supplied[iterator]);
                resolveIterator++;
            };

            var binding = new InjectionBinding(resolver);

            while (iterator < 3)
            {
                binding.SupplyTo(supplied[iterator]);
                iterator++;
            }

            Assert.AreEqual(3, resolveIterator);
        }

        [Test]
        public void TestUnsupply()
        {
            Binder.BindingResolver resolver = delegate { };
            var binding = new InjectionBinding(resolver);
            binding.To<ClassToBeInjected>().SupplyTo<HasANamedInjection>();
            Assert.AreEqual(typeof(HasANamedInjection), binding.GetSupply()[0]);
            Assert.AreEqual(typeof(ClassToBeInjected), binding.value);

            binding.Unsupply<HasANamedInjection>();
            Assert.IsNull(binding.GetSupply());
        }

        [Test]
        public void TestValueChainBinding()
        {
            var a = 0;
            var testValue = new InjectableDerivedClass();

            Binder.BindingResolver resolver = delegate(IBinding binding)
            {
                if (a == 2)
                {
                    Assert.That(binding.value == testValue);
                    var correctType = a == 0 ? InjectionBindingType.DEFAULT : InjectionBindingType.VALUE;
                    Assert.That((binding as InjectionBinding).type == correctType);
                }

                a++;
            };
            new InjectionBinding(resolver).Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToValue(testValue);
        }

        [Test]
        public void TestValueType()
        {
            const string TEST_KEY = "TEST_KEY";
            Binder.BindingResolver resolver = delegate(IBinding binding)
            {
                (binding as IInjectionBinding).type = InjectionBindingType.VALUE;
                Assert.That(TEST_KEY == binding.value as string);
                Assert.That((binding as InjectionBinding).type == InjectionBindingType.VALUE);
            };
            var defaultBinding = new InjectionBinding(resolver);
            defaultBinding.To(TEST_KEY);
        }
    }
}