using System;
using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestInjector
    {
        [SetUp]
        public void SetUp()
        {
            binder = new InjectionBinder();
        }

        private IInjectionBinder binder;

        [Test]
        public void TestCircularDependencies()
        {
            binder.Bind<CircularDependencyOne>().To<CircularDependencyOne>();
            binder.Bind<CircularDependencyTwo>().To<CircularDependencyTwo>();

            TestDelegate testDelegate = delegate { binder.GetInstance<CircularDependencyOne>(); };
            var ex = Assert.Throws<InjectionException>(testDelegate);
            Assert.That(ex.type == InjectionExceptionType.CIRCULAR_DEPENDENCY);
        }

        // ISSUE #45. This is a rather complex test which essentially validates that a Singleton is a Singleton,
        // whether it's injected via constructor injection or property injection.
        [Test]
        public void TestConstructorAndSetterSingletonsAreSame()
        {
            binder.Bind<IMapConfig>().ToValue(new MapConfig());
            binder.Bind<IMap>().To<Map>().ToSingleton();
            binder.Bind<IRenderer>().To<Renderer>().ToSingleton();
            binder.Bind<Phred>().ToSingleton();

            var m = binder.GetInstance<IMap>();
            var m2 = binder.GetInstance<IMap>();
            var r = binder.GetInstance<IRenderer>();
            var p = binder.GetInstance<Phred>();

            Assert.AreSame(m, p.map);
            Assert.AreSame(m, m2);
            Assert.AreSame(m, r.map);
        }

        [Test]
        public void TestConstructorNamedInjection()
        {
            var class1 = new ClassToBeInjected();
            var class2 = new ClassToBeInjected();

            binder.Bind<ClassToBeInjected>().To(class1).ToName("First");
            binder.Bind<ClassToBeInjected>().To(class2).ToName("Second");
            binder.Bind<ConstructorNamedInjection>().To<ConstructorNamedInjection>();
            var instance = binder.GetInstance<ConstructorNamedInjection>();

            Assert.That(instance.first.GetType() == typeof(ClassToBeInjected));
            Assert.That(instance.second.GetType() == typeof(ClassToBeInjected));
            Assert.That(instance.first != instance.second);
        }

        [Test]
        public void TestEmptyUntaggedConstructorNeverInvoked()
        {
            binder.Bind<int>().ToValue(42);
            binder.Bind<string>().ToValue("Zaphod");
            binder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>();
            binder.Bind<MultipleConstructorsOneThreeFour>().ToSingleton();

            TestDelegate testDelegate = delegate { binder.GetInstance<MultipleConstructorsOneThreeFour>(); };

            Assert.DoesNotThrow(testDelegate);
        }

        [Test]
        public void TestForcedParameterConstruction()
        {
            binder.Bind<ClassWithConstructorParametersOnlyOneConstructor>()
                .To<ClassWithConstructorParametersOnlyOneConstructor>();
            binder.Bind<string>().ToValue("Zaphod");
            var instance =
                binder.GetInstance<ClassWithConstructorParametersOnlyOneConstructor>();
            Assert.IsNotNull(instance);
            Assert.That(instance.stringVal == "Zaphod");
        }

        [Test]
        public void TestMultiplePostConstructs()
        {
            binder.Bind<PostConstructTwo>().To<PostConstructTwo>();
            binder.Bind<float>().ToValue((float) Math.PI);
            var instance = binder.GetInstance<PostConstructTwo>();
            Assert.IsNotNull(instance);
            Assert.That(instance.floatVal == (float) Math.PI * 4f);
        }

        [Test]
        public void TestNamedFactories()
        {
            binder.Bind<int>().ToValue(20);
            binder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToName(SomeEnum.ONE);
            binder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementerTwo>().ToName(SomeEnum.TWO);

            var valueOneOne = binder.GetInstance<ISimpleInterface>(SomeEnum.ONE);
            var valueOneTwo = binder.GetInstance<ISimpleInterface>(SomeEnum.ONE);

            var valueTwoOne = binder.GetInstance<ISimpleInterface>(SomeEnum.TWO);
            var valueTwoTwo = binder.GetInstance<ISimpleInterface>(SomeEnum.TWO);

            //Of course nothing should return null
            Assert.NotNull(valueOneOne);
            Assert.NotNull(valueOneTwo);
            Assert.NotNull(valueTwoOne);
            Assert.NotNull(valueTwoTwo);

            //All four instances should be unique.
            Assert.AreNotSame(valueOneOne, valueOneTwo);
            Assert.AreNotSame(valueOneTwo, valueTwoOne);
            Assert.AreNotSame(valueTwoOne, valueTwoTwo);
            Assert.AreNotSame(valueOneOne, valueTwoTwo);
            //First pair should be of type SimpleInterfaceImplementer.
            Assert.IsInstanceOf<SimpleInterfaceImplementer>(valueOneOne);
            Assert.IsInstanceOf<SimpleInterfaceImplementer>(valueOneTwo);
            //Second pair should be of type SimpleInterfaceImplementerTwo.
            Assert.IsInstanceOf<SimpleInterfaceImplementerTwo>(valueTwoOne);
            Assert.IsInstanceOf<SimpleInterfaceImplementerTwo>(valueTwoTwo);
        }

        [Test]
        public void TestNamedInstances()
        {
            var testValue = new InjectableSuperClass();
            var defaultFloatValue = testValue.floatValue;
            testValue.floatValue = 3.14f;

            binder.Bind<int>().ToValue(20);
            binder.Bind<InjectableSuperClass>().ToSingleton().ToName(SomeEnum.ONE);
            binder.Bind<InjectableSuperClass>().ToName<MarkerClass>().ToValue(testValue);
            binder.Bind<HasNamedInjections>().To<HasNamedInjections>();
            var instance = binder.GetInstance<HasNamedInjections>();
            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.injectionOne);
            Assert.IsNotNull(instance.injectionTwo);
            Assert.AreEqual(20, instance.injectionOne.intValue);
            Assert.AreEqual(20, instance.injectionTwo.intValue);
            Assert.That(instance.injectionOne.floatValue == defaultFloatValue);
            Assert.That(instance.injectionTwo.floatValue == 3.14f);
        }

        [Test]
        public void TestPostConstruct()
        {
            binder.Bind<PostConstructClass>().To<PostConstructClass>();
            binder.Bind<float>().ToValue((float) Math.PI);
            var instance = binder.GetInstance<PostConstructClass>();
            Assert.IsNotNull(instance);
            Assert.That(instance.floatVal == (float) Math.PI * 2f);
        }

        [Test]
        public void TestPostConstructOrdering()
        {
            binder.Bind<PostConstructSeveralOrdered>().To<PostConstructSeveralOrdered>();
            var instance = binder.GetInstance<PostConstructSeveralOrdered>();
            Assert.IsNotNull(instance);
            //Post-Constructs are ordered to spell this word
            Assert.AreEqual("ZAPHOD", instance.stringVal);
        }

        [Test]
        public void TestTaggedConstructor()
        {
            binder.Bind<ClassWithConstructorParameters>().To<ClassWithConstructorParameters>();
            binder.Bind<int>().ToValue(42);
            binder.Bind<string>().ToValue("Liberator");
            var instance =
                binder.GetInstance<ClassWithConstructorParameters>();
            Assert.IsNotNull(instance);
            Assert.AreEqual(42, instance.intValue);
            Assert.AreEqual("Liberator", instance.stringValue);
        }

        //RE: Issue #23. A value-mapped object must never attempt to re-instantiate
        [Test]
        public void TestValueMappingWithConstructorArguments()
        {
            var stringVal = "Ender Wiggin";
            var instance = new ClassWithConstructorParametersOnlyOneConstructor(stringVal);
            binder.Bind<ClassWithConstructorParametersOnlyOneConstructor>().ToValue(instance);
            //If this class attempts to construct, with no string mapped, there'll be an error
            var instance2 = binder.GetInstance<ClassWithConstructorParametersOnlyOneConstructor>();
            Assert.AreSame(instance, instance2);
            Assert.AreEqual(stringVal, instance2.stringVal);
        }
    }

    public interface IMapConfig
    {
    }

    public class MapConfig : IMapConfig
    {
    }

    public interface IMap
    {
    }

    public class Map : IMap
    {
        public Map(IMapConfig config)
        {
            Console.WriteLine("Test map " + GetHashCode());
        }
    }

    public interface IRenderer
    {
        IMap map { get; set; }
    }

    public class Renderer : IRenderer
    {
        public Renderer(IMap map)
        {
            this.map = map;
        }

        public IMap map { get; set; }
    }

    public class Phred
    {
        [Inject] public IMap map { get; set; }
    }
}