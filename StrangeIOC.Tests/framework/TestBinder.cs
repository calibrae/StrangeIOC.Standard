using System;
using System.Collections.Generic;
using NUnit.Framework;
using strange.framework.api;
using strange.framework.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestBinder
    {
        [SetUp]
        public void SetUp()
        {
            binder = new Binder();
        }

        private IBinder binder;

        [Test]
        public void TestBindType()
        {
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            var binding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNotNull(binding);
            Assert.That(binding.key == typeof(InjectableSuperClass));
            Assert.That(binding.key != typeof(InjectableDerivedClass));
            Assert.That(binding.value != typeof(InjectableSuperClass));
            Assert.That((binding.value as object[])[0] == typeof(InjectableDerivedClass));
        }

        [Test]
        public void TestBindValue()
        {
            object testKeyValue = new MarkerClass();
            binder.Bind(testKeyValue).To<InjectableDerivedClass>();
            var binding = binder.GetBinding(testKeyValue);
            Assert.IsNotNull(binding);
            Assert.That(binding.key == testKeyValue);
            Assert.That((binding.value as object[])[0] == typeof(InjectableDerivedClass));
        }

        [Test]
        public void TestConflictWithoutWeak()
        {
            binder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>();

            TestDelegate testDelegate = delegate
            {
                binder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementerTwo>();
                var instance = binder.GetBinding<ISimpleInterface>().value;
                Assert.IsNotNull(instance);
            };

            var ex = Assert
                .Throws<BinderException>(testDelegate); //Because we have a conflict between the two above bindings
            Assert.AreEqual(BinderExceptionType.CONFLICT_IN_BINDER, ex.type);
        }

        [Test]
        public void TestNamedBinding()
        {
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName<MarkerClass>();
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName("strange");

            //Test the unnamed binding
            var unnamedBinding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNotNull(unnamedBinding);
            Assert.That(unnamedBinding.key == typeof(InjectableSuperClass));
            var unnamedBindingValue = (unnamedBinding.value as object[])[0] as Type;
            Assert.That(unnamedBindingValue == typeof(InjectableDerivedClass));
            Assert.That(unnamedBinding.name != typeof(MarkerClass));

            //Test the binding named by type
            var namedBinding = binder.GetBinding<InjectableSuperClass>(typeof(MarkerClass));
            Assert.IsNotNull(namedBinding);
            Assert.That(namedBinding.key == typeof(InjectableSuperClass));
            var namedBindingValue = (namedBinding.value as object[])[0] as Type;
            Assert.That(namedBindingValue == typeof(InjectableDerivedClass));
            Assert.That(namedBinding.name == typeof(MarkerClass));

            //Test the binding named by string
            var namedBinding2 = binder.GetBinding<InjectableSuperClass>("strange");
            Assert.IsNotNull(namedBinding2);
            Assert.That(namedBinding2.key == typeof(InjectableSuperClass));
            var namedBinding2Value = (namedBinding2.value as object[])[0] as Type;
            Assert.That(namedBinding2Value == typeof(InjectableDerivedClass));
            Assert.That((string) namedBinding2.name == "strange");
        }

        [Test]
        public void TestNullValueInBinding()
        {
            var binding = binder.Bind<float>().To(1).To(2).To(3);
            var before = binding.value as object[];
            Assert.AreEqual(3, before.Length);

            binder.RemoveValue(binding, before);

            var after = binding.value as object[];
            Assert.IsNull(after);
        }

        [Test]
        public void TestRemoveBindingByBinding()
        {
            var unnamedBinding = binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName<MarkerClass>();

            //Remove the first binding
            binder.Unbind(unnamedBinding);
            var removedUnnamedBinding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNull(removedUnnamedBinding);

            //Ensure the named binding still exists
            var unremovedNamedBinding = binder.GetBinding<InjectableSuperClass>(typeof(MarkerClass));
            Assert.IsNotNull(unremovedNamedBinding);
        }

        [Test]
        public void TestRemoveBindingByKey()
        {
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName<MarkerClass>();

            //Test the unnamed binding
            var unnamedBinding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNotNull(unnamedBinding);
            Assert.That(unnamedBinding.key == typeof(InjectableSuperClass));
            var unnamedBindingValue = (unnamedBinding.value as object[])[0] as Type;
            Assert.That(unnamedBindingValue == typeof(InjectableDerivedClass));
            Assert.That(unnamedBinding.name != typeof(MarkerClass));

            //Test the binding named by type
            var namedBinding = binder.GetBinding<InjectableSuperClass>(typeof(MarkerClass));
            Assert.IsNotNull(namedBinding);
            Assert.That(namedBinding.key == typeof(InjectableSuperClass));
            var namedBindingValue = (namedBinding.value as object[])[0] as Type;
            Assert.That(namedBindingValue == typeof(InjectableDerivedClass));
            Assert.That(namedBinding.name == typeof(MarkerClass));

            //Remove the first binding
            binder.Unbind<InjectableSuperClass>();
            var removedUnnamedBinding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNull(removedUnnamedBinding);

            //Ensure the named binding still exists
            var unremovedNamedBinding = binder.GetBinding<InjectableSuperClass>(typeof(MarkerClass));
            Assert.IsNotNull(unremovedNamedBinding);
        }

        [Test]
        public void TestRemoveBindingByKeyAndName()
        {
            binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>();
            var namedBinding = binder.Bind<InjectableSuperClass>().To<InjectableDerivedClass>().ToName<MarkerClass>();

            //Remove the first binding
            binder.Unbind(namedBinding.key, namedBinding.name);
            var removedNamedBinding = binder.GetBinding<InjectableSuperClass>(typeof(MarkerClass));
            Assert.IsNull(removedNamedBinding);

            //Ensure the unnamed binding still exists
            var unremovedUnnamedBinding = binder.GetBinding<InjectableSuperClass>();
            Assert.IsNotNull(unremovedUnnamedBinding);
        }

        [Test]
        public void TestRemoveValueFromBinding()
        {
            var binding = binder.Bind<float>().To(1).To(2).To(3);
            var before = binding.value as object[];
            Assert.AreEqual(3, before.Length);

            binder.RemoveValue(binding, 2);

            var after = binding.value as object[];
            Assert.AreEqual(2, after.Length);
            Assert.AreEqual(1, after[0]);
            Assert.AreEqual(3, after[1]);
        }

        [Test]
        public void TestRuntimeFailedWhitelistException()
        {
            var jsonString = "[{\"Bind\":\"Han\",\"To\":\"Solo\"}, {\"Bind\":\"Darth\",\"To\":\"Vader\"}]";

            var whitelist = new List<object>();
            whitelist.Add("Solo");
            binder.WhitelistBindings(whitelist);

            TestDelegate testDelegate = delegate { binder.ConsumeBindings(jsonString); };
            var ex = Assert.Throws<BinderException>(testDelegate); //Because Vader is not whitelisted
            Assert.AreEqual(BinderExceptionType.RUNTIME_FAILED_WHITELIST_CHECK, ex.type);
        }

        [Test]
        public void TestRuntimeNoBindException()
        {
            var jsonString = "[{\"oops\":\"Han\",\"To\":\"Solo\"}]";
            TestDelegate testDelegate = delegate { binder.ConsumeBindings(jsonString); };
            var ex = Assert.Throws<BinderException>(testDelegate); //Because we have no bind key
            Assert.AreEqual(BinderExceptionType.RUNTIME_NO_BIND, ex.type);
        }

        [Test]
        public void TestRuntimeTooManyKeysException()
        {
            var jsonString = "[{\"Bind\":[\"Han\",\"Leia\"],\"To\":\"Solo\"}]";
            TestDelegate testDelegate = delegate { binder.ConsumeBindings(jsonString); };
            var ex = Assert
                .Throws<BinderException>(testDelegate); //Because we have two keys in a Binder that only supports one
            Assert.AreEqual(BinderExceptionType.RUNTIME_TOO_MANY_KEYS, ex.type);
        }

        [Test]
        public void TestRuntimeUnknownTypeException()
        {
            var jsonString = "[{\"Bind\":true,\"To\":\"Solo\"}]";
            TestDelegate testDelegate = delegate { binder.ConsumeBindings(jsonString); };
            var ex = Assert.Throws<BinderException>(testDelegate); //Because we can't bind a boolean as a key
            Assert.AreEqual(BinderExceptionType.RUNTIME_TYPE_UNKNOWN, ex.type);
        }

        [Test]
        public void TestRuntimeWeakBinding()
        {
            var jsonString =
                "[{\"Bind\":\"This\",\"To\":\"That\", \"Options\":\"Weak\"}, {\"Bind\":[\"Han\"],\"To\":\"Solo\"}]";
            binder.ConsumeBindings(jsonString);

            var binding = binder.GetBinding("This");
            Assert.NotNull(binding);
            Assert.IsTrue(binding.isWeak);

            var binding2 = binder.GetBinding("Han");
            Assert.NotNull(binding2);
            Assert.IsFalse(binding2.isWeak);
        }

        [Test]
        public void TestSimpleRuntimeBinding()
        {
            var jsonString =
                "[{\"Bind\":\"This\",\"To\":\"That\"},{\"Bind\":[\"Han\"],\"To\":\"Solo\"},{\"Bind\":\"Jedi\",\"To\":[\"Luke\",\"Yoda\",\"Ben\"]}]";
            binder.ConsumeBindings(jsonString);

            var binding = binder.GetBinding("This");
            Assert.NotNull(binding);
            Assert.AreEqual(binding.key, "This");
            Assert.AreEqual((binding.value as object[])[0], "That");

            var binding2 = binder.GetBinding("Han");
            Assert.NotNull(binding2);
            Assert.AreEqual(binding2.key, "Han");
            Assert.AreEqual((binding2.value as object[])[0], "Solo");

            var binding3 = binder.GetBinding("Jedi");
            Assert.NotNull(binding3);
            Assert.AreEqual(binding3.key, "Jedi");
            Assert.AreEqual((binding3.value as object[])[0], "Luke");
            Assert.AreEqual((binding3.value as object[])[1], "Yoda");
            Assert.AreEqual((binding3.value as object[])[2], "Ben");
        }

        [Test]
        public void TestWeakBindings()
        {
            var one = new SimpleInterfaceImplementer();
            var two = new SimpleInterfaceImplementerTwo();
            var binding = binder.Bind<ISimpleInterface>().To(one).Weak();

            binding.valueConstraint = BindingConstraintType.ONE;
            TestDelegate testDelegate = delegate
            {
                binder.Bind<ISimpleInterface>().To(two).valueConstraint = BindingConstraintType.ONE;
                var retrievedBinding = binder.GetBinding<ISimpleInterface>();
                Assert.NotNull(retrievedBinding);
                Assert.NotNull(retrievedBinding.value);
                Console.WriteLine(retrievedBinding.value);
                Assert.AreEqual(retrievedBinding.value, two);
            };

            Assert.DoesNotThrow(testDelegate); //Second binding "two" overrides weak binding "one"
        }
    }
}