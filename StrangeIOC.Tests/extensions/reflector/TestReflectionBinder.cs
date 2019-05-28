using NUnit.Framework;
using strange.extensions.reflector.api;
using strange.extensions.reflector.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestReflectionBinder
    {
        [SetUp]
        public void SetUp()
        {
            reflector = new ReflectionBinder();
        }

        private IReflectionBinder reflector;

        [Test]
        public void TestConstructorNamedInjection()
        {
            var reflected = reflector.Get<ConstructorNamedInjection>();

            Assert.That(reflected.ConstructorParameters.Length == 2);
            Assert.That(reflected.ConstructorParameterNames.Length == 2);
        }

        [Test]
        public void TestConstructorWithMultipleParameters()
        {
            var reflected = reflector.Get<ClassWithConstructorParameters>();
            Assert.AreEqual(2, reflected.ConstructorParameters.Length);
        }

        [Test]
        public void TestConstructorWithNoParameters()
        {
            var reflected = reflector.Get<ClassToBeInjected>();
            Assert.AreEqual(0, reflected.ConstructorParameters.Length);
        }

        [Test]
        public void TestConstructorWithSingleParameter()
        {
            var reflected = reflector.Get<ClassWithConstructorParametersOnlyOneConstructor>();
            Assert.AreEqual(1, reflected.ConstructorParameters.Length);
        }

        [Test]
        public void TestFoundSoleConstructor()
        {
        }

        [Test]
        public void TestInheritedInjectionHiding()
        {
            var overrideBase = reflector.Get<BaseInheritanceOverride>();
            var overrideExtended = reflector.Get<ExtendedInheritanceOverride>();

            Assert.AreEqual(1, overrideBase.Setters.Length);
            Assert.AreEqual(1, overrideExtended.Setters.Length);
        }

        [Test]
        public void TestInjectedTypeForOverrideIsCorrect()
        {
            var overrideBase = reflector.Get<BaseInheritanceOverride>();
            var overrideExtended = reflector.Get<ExtendedInheritanceOverride>();

            Assert.AreEqual(overrideExtended.Setters[0].type, typeof(IExtendedInterface));
        }

        [Test]
        public void TestMultipleLevelsOfInheritance()
        {
            var overrideBase = reflector.Get<BaseInheritanceOverride>();
            var overrideExtended = reflector.Get<ExtendedInheritanceOverride>();
            var overrideExtendedTwo = reflector.Get<ExtendedInheritanceOverrideTwo>();

            Assert.AreEqual(1, overrideBase.Setters.Length);
            Assert.AreEqual(1, overrideExtended.Setters.Length);
            Assert.AreEqual(1, overrideExtendedTwo.Setters.Length);
        }

        [Test]
        public void TestMultiplePostConstructs()
        {
            var reflected = reflector.Get<PostConstructTwo>();
            Assert.AreEqual(2, reflected.PostConstructors.Length);
        }

        [Test]
        public void TestMultipleSetters()
        {
            var reflected = reflector.Get<HasTwoInjections>();
            Assert.AreEqual(2, reflected.Setters.Length);
            Assert.IsNull(reflected.Setters[0].name);

            var foundStringType = false;
            var foundInjectableSuperClassType = false;

            foreach (var attr in reflected.Setters)
            {
                if (attr.type == typeof(string))
                {
                    foundStringType = true;
                    Assert.AreEqual("injectionTwo", attr.propertyInfo.Name);
                }

                if (attr.type == typeof(InjectableSuperClass))
                {
                    foundInjectableSuperClassType = true;
                    Assert.AreEqual("injectionOne", attr.propertyInfo.Name);
                }
            }

            Assert.True(foundStringType);
            Assert.True(foundInjectableSuperClassType);
        }

        [Test]
        public void TestNamedSetters()
        {
            var reflected = reflector.Get<HasNamedInjections>();
            Assert.AreEqual(2, reflected.Setters.Length);

            var a = 0;
            var injectableSuperClassCount = 0;
            var foundSomeEnum = false;
            var foundMarkerClass = false;

            foreach (var attr in reflected.Setters)
            {
                if (attr.type == typeof(InjectableSuperClass))
                {
                    injectableSuperClassCount++;

                    if (attr.name.Equals(SomeEnum.ONE))
                    {
                        Assert.False(foundSomeEnum);
                        foundSomeEnum = true;
                    }

                    if (attr.name.Equals(typeof(MarkerClass)))
                    {
                        Assert.False(foundMarkerClass);
                        foundMarkerClass = true;
                    }
                }

                a++;
            }

            Assert.AreEqual(2, injectableSuperClassCount);
        }

        [Test]
        public void TestNonpublicInjectionMapping()
        {
            TestDelegate testDelegate = delegate { reflector.Get<NonpublicInjection>(); };
            var ex = Assert.Throws<ReflectionException>(testDelegate);
            Assert.That(ex.type == ReflectionExceptionType.CANNOT_INJECT_INTO_NONPUBLIC_SETTER);
        }

        [Test]
        public void TestReflectAnInterface()
        {
            TestDelegate testDelegate = delegate { reflector.Get<ISimpleInterface>(); };
            var ex = Assert.Throws<ReflectionException>(testDelegate);
            Assert.That(ex.type == ReflectionExceptionType.CANNOT_REFLECT_INTERFACE);
        }

        [Test]
        public void TestReflectionCaching()
        {
            var reflected = reflector.Get<HasNamedInjections>();
            Assert.False(reflected.PreGenerated);
            var reflected2 = reflector.Get<HasNamedInjections>();
            Assert.True(reflected2.PreGenerated);
        }

        [Test]
        public void TestSettersOnDerivedClass()
        {
            var reflected = reflector.Get<InjectableDerivedClass>();
            Assert.AreEqual(2, reflected.Setters.Length);

            var foundIntType = false;
            var foundClassToBeInjectedType = false;

            foreach (var attr in reflected.Setters)
            {
                if (attr.type == typeof(int))
                {
                    foundIntType = true;
                    Assert.AreEqual("intValue", attr.propertyInfo.Name);
                }

                if (attr.type == typeof(ClassToBeInjected))
                {
                    foundClassToBeInjectedType = true;
                    Assert.AreEqual("injected", attr.propertyInfo.Name);
                }
            }

            Assert.True(foundIntType);
            Assert.True(foundClassToBeInjectedType);
        }

        [Test]
        public void TestShortestConstructor()
        {
            var reflected = reflector.Get<MultipleConstructorsUntagged>();
            Assert.AreEqual(3, reflected.ConstructorParameters.Length);

            ISimpleInterface simple = new SimpleInterfaceImplementer();
            simple.intValue = 11001001;

            var constructor = reflected.Constructor;
            var parameters = new object[3];
            parameters[0] = simple;
            parameters[1] = 42;
            parameters[2] = "Zaphod";
            var instance = constructor.Invoke(parameters) as MultipleConstructorsUntagged;
            Assert.IsNotNull(instance);
            Assert.AreEqual(simple.intValue, instance.simple.intValue);
            Assert.AreEqual(42, instance.intValue);
            Assert.AreEqual("Zaphod", instance.stringValue);
        }

        [Test]
        public void TestSinglePostConstruct()
        {
            var reflected = reflector.Get<PostConstructClass>();
            Assert.AreEqual(1, reflected.PostConstructors.Length);
        }

        [Test]
        public void TestSingleSetter()
        {
            var reflected = reflector.Get<PostConstructTwo>();
            Assert.AreEqual(1, reflected.Setters.Length);
            Assert.IsNull(reflected.Setters[0].name);

            var attr = reflected.Setters[0];
            Assert.AreEqual(attr.type, typeof(float));
        }

        [Test]
        public void TestTaggedConstructor()
        {
            var reflected = reflector.Get<ClassWithConstructorParameters>();
            Assert.AreEqual(2, reflected.ConstructorParameters.Length);

            var constructor = reflected.Constructor;
            var parameters = new object[2];
            parameters[0] = 42;
            parameters[1] = "Zaphod";
            var instance = constructor.Invoke(parameters) as ClassWithConstructorParameters;
            Assert.IsNotNull(instance);
            Assert.AreEqual(42, instance.intValue);
            Assert.AreEqual("Zaphod", instance.stringValue);
        }

        [Test]
        public void TestVirtualOverrideInjection()
        {
            var overrideBase = reflector.Get<BaseInheritanceOverride>();
            var overrideExtended = reflector.Get<ExtendedInheritanceImplied>();

            Assert.AreEqual(1, overrideBase.Setters.Length);
            Assert.AreEqual(1, overrideExtended.Setters.Length);
        }
    }
}