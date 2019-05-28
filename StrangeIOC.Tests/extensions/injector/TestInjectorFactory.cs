using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using strange.framework.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestInjectorFactory
    {
        [SetUp]
        public void SetUp()
        {
            factory = new InjectorFactory();
            resolver = delegate { };
        }

        private IInjectorFactory factory;
        private Binder.BindingResolver resolver;

        // NOTE: Due to a limitation in the version of C# used by Unity,
        // IT IS NOT POSSIBLE TO MAP GENERICS ABSTRACTLY!!!!!
        // Therefore, pools must be mapped to concrete instance types. (Yeah, this blows.)
        [Test]
        public void TestGetFromPool()
        {
            IPool<ClassToBeInjected> pool = new Pool<ClassToBeInjected>();
            // Format the pool
            pool.size = 4;
            pool.instanceProvider = new TestInstanceProvider();

            IInjectionBinding binding = new InjectionBinding(resolver);
            binding.Bind<IPool<ClassToBeInjected>>().To<Pool<ClassToBeInjected>>().ToValue(pool);

            IPool<ClassToBeInjected> myPool = factory.Get(binding) as Pool<ClassToBeInjected>;
            Assert.NotNull(myPool);

            var instance1 = myPool.GetInstance();
            Assert.NotNull(instance1);

            var instance2 = myPool.GetInstance();
            Assert.NotNull(instance2);

            Assert.AreNotSame(instance1, instance2);
        }

        [Test]
        public void TestImplicitBinding()
        {
            //This succeeds if no error
            var binding = new InjectionBinding(resolver).Bind<InjectableSuperClass>();
            factory.Get(binding);

            //Succeeds if throws error
            var binding2 = new InjectionBinding(resolver).Bind<ISimpleInterface>();
            TestDelegate testDelegate = delegate { factory.Get(binding2); };
            var ex = Assert.Throws<InjectionException>(testDelegate);
            Assert.That(ex.type == InjectionExceptionType.NOT_INSTANTIABLE);

            //Succeeds if throws error
            var binding3 = new InjectionBinding(resolver).Bind<AbstractClass>();
            TestDelegate testDelegate2 = delegate { factory.Get(binding3); };
            var ex2 = Assert.Throws<InjectionException>(testDelegate2);
            Assert.That(ex2.type == InjectionExceptionType.NOT_INSTANTIABLE);
        }

        [Test]
        public void TestImplicitToSingleton()
        {
            //This succeeds if no error
            var binding = new InjectionBinding(resolver).Bind<InjectableSuperClass>().ToSingleton();
            factory.Get(binding);

            //Succeeds if throws error
            var binding2 = new InjectionBinding(resolver).Bind<ISimpleInterface>().ToSingleton();
            TestDelegate testDelegate = delegate { factory.Get(binding2); };
            var ex = Assert.Throws<InjectionException>(testDelegate);
            Assert.That(ex.type == InjectionExceptionType.NOT_INSTANTIABLE);

            //Succeeds if throws error
            var binding3 = new InjectionBinding(resolver).Bind<AbstractClass>().ToSingleton();
            TestDelegate testDelegate2 = delegate { factory.Get(binding3); };
            var ex2 = Assert.Throws<InjectionException>(testDelegate2);
            Assert.That(ex2.type == InjectionExceptionType.NOT_INSTANTIABLE);
        }

        [Test]
        public void TestInstantiateSingleton()
        {
            var defaultBinding = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>().ToSingleton();
            var testResult = factory.Get(defaultBinding) as InjectableDerivedClass;
            Assert.IsNotNull(testResult);
            //Set a value
            testResult.intValue = 42;
            //Now get an instance again and ensure it's the same instance
            var testResult2 = factory.Get(defaultBinding) as InjectableDerivedClass;
            Assert.That(testResult2.intValue == 42);
        }

        [Test]
        public void TestInstantiation()
        {
            var defaultBinding = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>();
            var testResult = factory.Get(defaultBinding) as InjectableDerivedClass;
            Assert.IsNotNull(testResult);
        }

        [Test]
        public void TestInstantiationFactory()
        {
            var defaultBinding = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>();
            var testResult = factory.Get(defaultBinding) as InjectableDerivedClass;
            Assert.IsNotNull(testResult);
            var defaultValue = testResult.intValue;
            //Set a value
            testResult.intValue = 42;
            //Now get an instance again and ensure it's a different instance
            var testResult2 = factory.Get(defaultBinding) as InjectableDerivedClass;
            Assert.That(testResult2.intValue == defaultValue);
        }

        [Test]
        public void TestNamedInstances()
        {
            //Create two named instances
            var defaultBinding = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>().ToName(SomeEnum.ONE);
            var defaultBinding2 = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>().ToName(SomeEnum.TWO);

            var testResult = factory.Get(defaultBinding) as InjectableDerivedClass;
            var defaultValue = testResult.intValue;
            Assert.IsNotNull(testResult);
            //Set a value
            testResult.intValue = 42;

            //Now get an instance again and ensure it's a different instance
            var testResult2 = factory.Get(defaultBinding2) as InjectableDerivedClass;
            Assert.IsNotNull(testResult2);
            Assert.That(testResult2.intValue == defaultValue);
        }

        //NOTE: Technically this test is redundant with the test above, since a named instance
        //is a de-facto Singleton
        [Test]
        public void TestNamedSingletons()
        {
            //Create two named singletons
            var defaultBinding = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>().ToName(SomeEnum.ONE).ToSingleton();
            var defaultBinding2 = new InjectionBinding(resolver).Bind<InjectableSuperClass>()
                .To<InjectableDerivedClass>().ToName(SomeEnum.TWO).ToSingleton();

            var testResult = factory.Get(defaultBinding) as InjectableDerivedClass;
            var defaultValue = testResult.intValue;
            Assert.IsNotNull(testResult);
            //Set a value
            testResult.intValue = 42;

            //Now get an instance again and ensure it's a different instance
            var testResult2 = factory.Get(defaultBinding2) as InjectableDerivedClass;
            Assert.IsNotNull(testResult2);
            Assert.That(testResult2.intValue == defaultValue);
        }

        [Test]
        public void TestValueMap()
        {
            var testvalue = new InjectableDerivedClass();
            testvalue.intValue = 42;
            var binding = new InjectionBinding(resolver).Bind<InjectableSuperClass>().To<InjectableDerivedClass>()
                .ToValue(testvalue);
            var testResult = factory.Get(binding) as InjectableDerivedClass;
            Assert.IsNotNull(testResult);
            Assert.That(testResult.intValue == testvalue.intValue);
            Assert.That(testResult.intValue == 42);
        }
    }
}