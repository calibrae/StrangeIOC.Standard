using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using strange.extensions.context.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.unittests.annotated.multipleInterfaces;
using strange.unittests.annotated.namespaceTest.one;
using strange.unittests.annotated.namespaceTest.three.even.farther;
using strange.unittests.annotated.namespaceTest.two.far;
using strange.unittests.annotated.testConcrete;
using strange.unittests.annotated.testConcreteNamed;
using strange.unittests.annotated.testCrossContextInterface;
using strange.unittests.annotated.testImplBy;
using strange.unittests.annotated.testImplements;
using strange.unittests.annotated.testImplTwo;
using strange.unittests.testimplicitbindingnamespace;

namespace strange.unittests
{
    [TestFixture]
    public class TestImplicitBinding
    {
        [SetUp]
        public void setup()
        {
            Context.firstContext = null;
            contextView = new object();
            context = new MockContext( true);
            context.Assembly = Assembly.GetExecutingAssembly();
        }

        private MockContext context;
        private object contextView;


        /// <summary>
        ///     Test [CrossContextComponent] tag.
        ///     Child contexts should be able to 'override' Cross-ViewedContext bindings with local bindings
        /// </summary>
        [Test]
        public void TestCrossContextAllowsOverrides()
        {
            var Parent = new MockContext( true);
            Parent.Assembly = Assembly.GetExecutingAssembly();
            var ChildOne =
                new MockContext(
                    true); //Ctr will automatically add to ViewedContext.firstcontext. No need to call it manually (and you should not).
            ChildOne.Assembly = Assembly.GetExecutingAssembly();
            var ChildTwo = new MockContext( true);
            ChildTwo.Assembly = Assembly.GetExecutingAssembly();


            Parent.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testCrossContext"
            };

            ChildOne.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testCrossOverride"
            };
            Parent.Start();
            ChildOne.Start();
            ChildTwo.Start();

            var parentModel = Parent.injectionBinder.GetInstance<TestCrossContextInterface>();
            //Get the instance from the parent injector (The cross context binding)

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestCrossContextInterface>();
            Assert.AreNotSame(childOneModel,
                parentModel); //The value from getinstance is NOT the same as the cross context value. We have overidden the cross context value locally

            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestCrossContextInterface>();
            Assert.IsNotNull(childTwoModel);
            Assert.AreNotSame(childOneModel,
                childTwoModel); //These two are different objects, the childTwoModel being cross context, and childone being the override
            Assert.AreSame(parentModel, childTwoModel); //Both cross context models are the same


            parentModel.Value++;
            Assert.AreEqual(1, childTwoModel.Value); //cross context model should be changed

            parentModel.Value++;
            Assert.AreEqual(1000, childOneModel.Value); //local model is not changed


            Assert.AreEqual(2, parentModel.Value); //cross context model is changed
        }

        /// <summary>
        ///     Test [CrossContextComponent] tag.
        ///     This is not meant to be a test of all crosscontext functionality, just the tag
        ///     The CrossContextComponent tag really just tells a binding to call .CrossContext()
        ///     See TestCrossContext for tests of CrossContext
        /// </summary>
        [Test]
        public void TestCrossContextImplicit()
        {
            var Parent = new MockContext( true);
            Parent.Assembly = Assembly.GetExecutingAssembly();
            var ChildOne =
                new MockContext(
                    true); //Ctr will automatically add to ViewedContext.firstcontext. No need to call it manually (and you should not).
            ChildOne.Assembly = Assembly.GetExecutingAssembly();
            var ChildTwo = new MockContext( true);
            ChildTwo.Assembly = Assembly.GetExecutingAssembly();


            Parent.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testCrossContext"
            };

            Parent.Start();
            ChildOne.Start();
            ChildTwo.Start();

            var parentModel = Parent.injectionBinder.GetInstance<TestCrossContextInterface>();

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestCrossContextInterface>();
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestCrossContextInterface>();
            Assert.IsNotNull(childTwoModel);
            Assert.AreSame(childOneModel, childTwoModel); //These two should be the same object

            Assert.AreEqual(0, parentModel.Value); //start at 0, might as well verify.

            parentModel.Value++;
            Assert.AreEqual(1, childOneModel.Value); //child one is updated

            parentModel.Value++;
            Assert.AreEqual(2, childTwoModel.Value); //child two is updated
        }

        /// <summary>
        ///     Attempt to bind an ImplementedBy annotation pointing to a Type which does not implement the interface
        /// </summary>
        [Test]
        public void TestDoesNotImplement()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testImplementedByDoesntImplement"
            };

            TestDelegate testDelegate = delegate { context.Start(); };

            //We should be getting an exception here because the interface is not implemented
            var ex = Assert.Throws<InjectionException>(testDelegate);

            //make sure it's the right exception
            Assert.AreEqual(InjectionExceptionType.IMPLICIT_BINDING_IMPLEMENTOR_DOES_NOT_IMPLEMENT_INTERFACE, ex.type);
        }


        /// <summary>
        ///     Attempt to bind an Implements annotation pointing to an interface it does not implement
        /// </summary>
        [Test]
        public void TestDoesNotImplementTwo()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testUnimplementedImplementsTag"
            };

            TestDelegate testDelegate = delegate { context.Start(); };

            //We should be getting an exception here because the interface is not implemented
            var ex = Assert.Throws<InjectionException>(testDelegate);

            //make sure it's the right exception
            Assert.AreEqual(InjectionExceptionType.IMPLICIT_BINDING_TYPE_DOES_NOT_IMPLEMENT_DESIGNATED_INTERFACE,
                ex.type);
        }

        /// <summary>
        ///     Bind implicitly and then overwrite with an explicit binding
        /// </summary>
        [Test]
        public void TestExplicitBindingOverrides()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testImplements"
            };

            context.Start();


            var testInterfacePre = context.injectionBinder.GetInstance<TestInterface>();
            Assert.True(testInterfacePre is TestImpl);
            //Confirm the previous binding is the implicit binding as expected

            context.injectionBinder.Bind<TestInterface>().To<TestImplTwo>();
            var testInterfacePost = context.injectionBinder.GetInstance<TestInterface>();
            Assert.True(testInterfacePost is TestImplTwo);
            //Confirm the new binding is the one we just wrote
        }

        /// <summary>
        ///     Test binding a default concrete class to an interface using the ImplementedBy tag (on the interface)
        /// </summary>
        [Test]
        public void TestImplementedBy()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testImplBy" //Namespace is the only true difference. Same tests as above for the same action done by a different method
            };
            context.Start();

            var testImpl = context.injectionBinder.GetInstance<TestInterface>();
            Assert.IsNotNull(testImpl);
            Assert.IsTrue(
                typeof(TestInterface)
                    .IsAssignableFrom(testImpl.GetType())); //Check that this objects type implements test interface.
            Assert.AreEqual(testImpl.GetType(), typeof(TestImpl)); //Check that its the type we added below
        }

        /// <summary>
        ///     Tests our Implements default case, which is a concrete singleton binding
        /// </summary>
        [Test]
        public void TestImplementsConcrete()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testConcrete"
            };
            context.Start();

            var testConcreteClass = context.injectionBinder.GetInstance<TestConcreteClass>();
            Assert.IsNotNull(testConcreteClass);
        }

        [Test]
        public void TestImplementsNamedConcrete()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testConcreteNamed"
            };
            context.Start();

            var testConcreteClass = context.injectionBinder.GetInstance<TestConcreteNamedClass>("NAME");
            Assert.IsNotNull(testConcreteClass);
        }

        /// <summary>
        ///     Bind via an ImplementedBy tag, followed by an Implements from a different class.
        ///     Implements should override the ImplementedBy tag
        /// </summary>
        [Test]
        public void TestImplementsOverridesImplementedBy()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testImplTwo",
                "strange.unittests.annotated.testImplBy"
            };

            context.Start();


            var testInterface = context.injectionBinder.GetInstance<TestInterface>();
            Assert.True(testInterface is TestImplTwo);
        }

        /// <summary>
        ///     Test binding a concrete class to an interface using the Implements tag
        /// </summary>
        [Test]
        public void TestImplementsToInterface()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.testImplements"
            };
            context.Start();

            var testImpl = context.injectionBinder.GetInstance<TestInterface>();
            Assert.IsNotNull(testImpl);
            Assert.IsTrue(
                typeof(TestInterface)
                    .IsAssignableFrom(testImpl.GetType())); //Check that this objects type implements test interface.
            Assert.AreEqual(testImpl.GetType(), typeof(TestImpl)); //Check that its the type we added below
        }

        //This monster "unit" test confirms that implicit bindings
        //correctly maintain integrity across ViewedContext boundaries.
        [Test]
        public void TestMultipleCrossContextImplicitBindings()
        {
            TestImplicitBindingClass.instantiationCount = 0;

            var contextsBeforeInstancing = 3;
            var contextsAfterInstancing = 4;
            var contextsToCreate = contextsBeforeInstancing + contextsAfterInstancing;
            var getInstanceCallsPerContext = 3;
            var injectsPerContext = 3;
            var contexts = new List<Context>();
            IInjectionBinding bindingBeforeContextCreation = null;
            object bindingValueBeforeContextCreation = null;

            //We create several Contexts.
            //note contextNumber is 1-based
            for (var contextNumber = 1; contextNumber <= contextsToCreate; contextNumber++)
            {
                //The first batch of Contexts don't actually create instances, just the implicit bindings
                var toInstance = contextNumber > contextsBeforeInstancing;
                //Specifically call out the ViewedContext that is first to create actual instances
                var isFirstContextToCallGetInstance = contextNumber == contextsBeforeInstancing + 1;

                var context = new TestImplicitBindingContext();
                context.Assembly = Assembly.GetExecutingAssembly();
                contexts.Add(context);

                //For each ViewedContext, check that the TestImplicitBindingClass BINDING exists (no instance created yet)
                var bindingAfterContextCreation = context.injectionBinder.GetBinding<TestImplicitBindingClass>();
                var bindingValueAfterContextCreation = bindingAfterContextCreation.value;

                var bindingChangedDueToContextCreation =
                    !bindingAfterContextCreation.Equals(bindingBeforeContextCreation);
                var bindingValueChangedDueToContextCreation =
                    bindingValueAfterContextCreation != bindingValueBeforeContextCreation;


                //due to the weak binding replacement rules, the binding should change every time we scan until we instance
                Assert.IsFalse(bindingChangedDueToContextCreation && toInstance && !isFirstContextToCallGetInstance);

                //after creating a new context, the value of the binding should only change on the first context
                //(it was null before that)
                Assert.IsFalse(bindingValueChangedDueToContextCreation && contextNumber != 1);


                if (toInstance)
                {
                    //For the Contexts that actually create instances...
                    for (var a = 0; a < getInstanceCallsPerContext; a++)
                    {
                        //...create some instances (well, duh) of the TestImplicitBindingClass...
                        var instance = context.injectionBinder.GetInstance<TestImplicitBindingClass>();
                        Assert.IsNotNull(instance);
                    }

                    for (var b = 0; b < injectsPerContext; b++)
                    {
                        //...and some instances of the class that gets injected with TestImplicitBindingClass.
                        var instance = context.injectionBinder.GetInstance<TestImplicitBindingInjectionReceiver>();
                        Assert.IsNotNull(instance);
                        Assert.IsNotNull(instance.testImplicitBindingClass);
                    }
                }

                //We inspect the binding and its value after all this mapping/instantiation
                var bindingAfterGetInstanceCalls = context.injectionBinder.GetBinding<TestImplicitBindingClass>();
                var bindingValueAfterGetInstanceCalls = bindingAfterGetInstanceCalls.value;

                var bindingChangedDueToGetInstanceCalls = bindingAfterGetInstanceCalls != bindingAfterContextCreation;
                var bindingValueChangedDueToGetInstanceCalls =
                    bindingValueAfterGetInstanceCalls != bindingValueAfterContextCreation;

                //the binding itself should only change during the scan
                Assert.IsFalse(bindingChangedDueToGetInstanceCalls);

                //if the weak binding replacement rules are working, the only time the value should
                //change is the first time we call GetInstance
                Assert.IsFalse(bindingValueChangedDueToGetInstanceCalls && !isFirstContextToCallGetInstance);

                //reset values for the next pass
                bindingBeforeContextCreation = bindingAfterGetInstanceCalls;
                bindingValueBeforeContextCreation = bindingValueAfterGetInstanceCalls;
            }

            //This is a Cross-ViewedContext Singleton.
            //The primary purpose of this test is to ensure (that under the circumstances of this test),
            //TestImplicitBindingClass should only get instantiated once
            Assert.AreEqual(1, TestImplicitBindingClass.instantiationCount);
        }

        [Test]
        public void TestMultipleImplements()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.multipleInterfaces"
            };
            context.Start();

            var one = context.injectionBinder.GetInstance<TestInterfaceOne>();
            Assert.NotNull(one);

            var two = context.injectionBinder.GetInstance<TestInterfaceTwo>();
            Assert.NotNull(two);

            var three = context.injectionBinder.GetInstance<TestInterfaceThree>();
            Assert.NotNull(three);

            Assert.AreEqual(one, two);
            Assert.AreEqual(one, three);
        }

        /// <summary>
        ///     Test that our assumptions regarding namespace scoping are correct
        ///     (e.g. company.project.feature will include company.project.feature.signal)
        /// </summary>
        [Test]
        public void TestNamespaces()
        {
            context.ScannedPackages = new[]
            {
                "strange.unittests.annotated.namespaceTest"
            };
            context.Start();

            //Should bind 3 classes concretely in the 
            var one = context.injectionBinder.GetInstance<TestNamespaceOne>();
            Assert.NotNull(one);

            var two = context.injectionBinder.GetInstance<TestNamespaceTwo>();
            Assert.NotNull(two);

            var three = context.injectionBinder.GetInstance<TestNamespaceThree>();
            Assert.NotNull(three);
        }

        [Test]
        public void TestParamsScannedPackages()
        {
            context.Start();
            context.ScanForAnnotatedClasses("strange.unittests.annotated.testConcrete",
                "strange.unittests.annotated.testImplTwo", "strange.unittests.annotated.testImplBy");

            var testConcrete = context.injectionBinder.GetInstance<TestConcreteClass>();
            Assert.IsNotNull(testConcrete);

            var testInterface = context.injectionBinder.GetInstance<TestInterface>();
            Assert.True(testInterface is TestImplTwo);
        }

        /// <summary>
        ///     Test that rescanning the same binding does not override a value
        ///     Smaller piece of below test.
        /// </summary>
        [Test]
        public void TestRescanDoesNotOverrideCrossContextValue()
        {
            var context = new TestImplicitBindingContext();

            //Get our binding. It should have value of Runtime Type. It hasn't been instantiated yet.
            var binding = context.injectionBinder.GetBinding<TestImplicitBindingClass>();
            Assert.IsTrue(binding.value is Type);

            //GetInstance. This should set the value to the instantiated class
            var instanceValue = context.injectionBinder.GetInstance<TestImplicitBindingClass>();
            Assert.IsNotNull(instanceValue);
            var bindingAfterGetInstance = context.injectionBinder.GetBinding<TestImplicitBindingClass>();
            Assert.IsTrue(bindingAfterGetInstance.value is TestImplicitBindingClass);
            Assert.AreSame(bindingAfterGetInstance, binding);

            //Rescan our implicit bindings
            //Our binding value should remain
            context.Scan();
            var bindingAfterRescan = context.injectionBinder.GetBinding<TestImplicitBindingClass>();
            Assert.AreSame(bindingAfterRescan, binding); //Should be the same binding, and not override it

            Assert.IsTrue(bindingAfterRescan.value is TestImplicitBindingClass);
            Assert.AreSame(bindingAfterRescan.value, instanceValue);
        }
    }
}

namespace strange.unittests.annotated.testConcrete
{
    [Implements]
    public class TestConcreteClass
    {
    }
}

namespace strange.unittests.annotated.testConcreteNamed
{
    [Implements(InjectionBindingScope.SINGLE_CONTEXT, "NAME")]
    public class TestConcreteNamedClass
    {
    }
}

namespace strange.unittests.annotated.testImplBy
{
    [ImplementedBy(typeof(TestImpl))]
    public interface TestInterface
    {
    }
}

namespace strange.unittests.annotated.testImplements
{
    [Implements(typeof(TestInterface))]
    public class TestImpl : TestInterface
    {
    }
}

namespace strange.unittests.annotated.testImplTwo
{
    [Implements(typeof(TestInterface))]
    public class TestImplTwo : TestInterface
    {
    }
}

namespace strange.unittests.annotated.testImplementedByDoesntImplement
{
    [ImplementedBy(typeof(TestClassDoesntImplement))]
    public interface TestInterfaceDoesntImplement
    {
    }

    public class TestClassDoesntImplement
    {
    }
}

namespace strange.unittests.annotated.testUnimplementedImplementsTag
{
    public interface TestInterfaceDoesntImplement
    {
    }

    [Implements(typeof(TestInterfaceDoesntImplement))]
    public class TestClassDoesntImplement
    {
    }
}

namespace strange.unittests.annotated.testCrossContextInterface
{
    public interface TestCrossContextInterface
    {
        int Value { get; set; }
    }
}

namespace strange.unittests.annotated.testCrossContext
{
    [Implements(typeof(TestCrossContextInterface), InjectionBindingScope.CROSS_CONTEXT)]
    public class TestConcreteCrossContextClass : TestCrossContextInterface
    {
        public TestConcreteCrossContextClass()
        {
            Value = 0;
        }

        public int Value { get; set; }
    }
}

namespace strange.unittests.annotated.testCrossOverride
{
    [Implements(typeof(TestCrossContextInterface))]
    public class TestConcreteCrossContextClassOverride : TestCrossContextInterface
    {
        public TestConcreteCrossContextClassOverride()
        {
            Value = 1000;
        }

        public int Value { get; set; }
    }
}

namespace strange.unittests.annotated.namespaceTest.one
{
    [Implements]
    public class TestNamespaceOne
    {
    }
}

namespace strange.unittests.annotated.namespaceTest.two.far
{
    [Implements]
    public class TestNamespaceTwo
    {
    }
}

namespace strange.unittests.annotated.namespaceTest.three.even.farther
{
    [Implements]
    public class TestNamespaceThree
    {
    }
}

namespace strange.unittests.annotated.multipleInterfaces
{
    public interface TestInterfaceOne
    {
    }

    public interface TestInterfaceTwo
    {
    }

    public interface TestInterfaceThree
    {
    }

    [Implements(typeof(TestInterfaceOne))]
    [Implements(typeof(TestInterfaceTwo))]
    [Implements(typeof(TestInterfaceThree))]
    public class TestMultipleImplementer : TestInterfaceOne, TestInterfaceTwo, TestInterfaceThree
    {
    }
}

namespace strange.unittests.testimplicitbindingnamespace
{
    public class TestImplicitBindingContext : MockContext
    {
        public TestImplicitBindingContext() : base()
        {
            Assembly = Assembly.GetExecutingAssembly();
        }

        protected override void mapBindings()
        {
            implicitBinder.Assembly = Assembly.GetExecutingAssembly();
            base.mapBindings();

            Scan();
            injectionBinder.Bind<TestImplicitBindingInjectionReceiver>().ToSingleton();
        }

        public void Scan()
        {
            implicitBinder.ScanForAnnotatedClasses(new[] {"strange.unittests.testimplicitbindingnamespace"});
        }
    }


    public class TestImplicitBindingInjectionReceiver
    {
        [Inject] public TestImplicitBindingClass testImplicitBindingClass { get; set; }
    }

    [Implements(InjectionBindingScope.CROSS_CONTEXT)]
    public class TestImplicitBindingClass
    {
        public static int instantiationCount;

        public TestImplicitBindingClass()
        {
            ++instantiationCount;
        }
    }
}