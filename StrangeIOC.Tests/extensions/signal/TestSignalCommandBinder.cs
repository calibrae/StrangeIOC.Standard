using System;
using NUnit.Framework;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.extensions.signal.api;
using strange.extensions.signal.impl;
using strange.framework.api;

namespace strange.unittests
{
    [TestFixture]
    public class TestSignalCommandBinder
    {
        [SetUp]
        public void SetUp()
        {
            injectionBinder = new InjectionBinder();
            injectionBinder.Bind<IInjectionBinder>().Bind<IInstanceProvider>().ToValue(injectionBinder);
            injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
            commandBinder = injectionBinder.GetInstance<ICommandBinder>();
            injectionBinder.Bind<TestModel>().ToSingleton();
        }

        private IInjectionBinder injectionBinder;
        private ICommandBinder commandBinder;


        private class TestModel
        {
            public int SecondaryValue;
            public int StoredValue;
        }

        private class NoArgSignal : Signal
        {
        }

        private class OneArgSignal : Signal<int>
        {
        }

        private class TwoArgSignal : Signal<int, bool>
        {
        }

        private class TwoArgSameTypeSignal : Signal<int, int>
        {
        }


        private class NoArgSignalCommand : Command
        {
            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                TestModel.StoredValue++;
            }
        }

        private class NoArgSignalCommandTwo : Command
        {
            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                TestModel.SecondaryValue += 2;
            }
        }

        private class OneArgSignalCommand : Command
        {
            [Inject] public int injectedValue { get; set; }

            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                TestModel.StoredValue += injectedValue;
            }
        }

        private class TwoArgSignalCommand : Command
        {
            [Inject] public int injectedValue { get; set; }

            [Inject] public bool injectedBool { get; set; }

            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                if (injectedBool)
                    TestModel.StoredValue += injectedValue;
                else
                    TestModel.StoredValue -= injectedValue;
            }
        }

        private class TwoArgSignalCommandTwo : Command
        {
            [Inject] public int injectedValue { get; set; }

            [Inject] public bool injectedBool { get; set; }

            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                if (injectedBool)
                    TestModel.StoredValue += injectedValue;
                else
                    TestModel.StoredValue -= injectedValue;
            }
        }

        private class TwoArgSignalCommandThree : Command
        {
            [Inject] public int injectedValue { get; set; }

            [Inject] public bool injectedBool { get; set; }

            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                throw new TestPassedException("Test Passed");
            }
        }

        private class TwoArgSameTypeSignalCommand : Command
        {
            [Inject] public int injectedValue { get; set; }

            [Inject] public int secondInjectedValue { get; set; }

            [Inject] public TestModel TestModel { get; set; }

            public override void Execute()
            {
                //This should never be run
                throw new Exception("This should not be reached");
            }
        }

        private class TestPassedException : Exception
        {
            public TestPassedException(string str) : base(str)
            {
            }
        }

        [Test]
        public void TestInterruptedSequence()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind<NoArgSignal>().To<CommandWithInjection>().To<FailCommand>().To<CommandThatThrows>()
                .InSequence();

            TestDelegate testDelegate = delegate
            {
                var signal = injectionBinder.GetInstance<NoArgSignal>();
                signal.Dispatch();
            };

            //That the exception is not thrown demonstrates that the last command was interrupted
            Assert.DoesNotThrow(testDelegate);

            //That the value is 100 demonstrates that the first command ran
            var instance = injectionBinder.GetInstance<ISimpleInterface>();
            Assert.AreEqual(100, instance.intValue);
        }

        [Test]
        public void TestMultiple()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>().To<NoArgSignalCommandTwo>();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<NoArgSignal>();

            signal.Dispatch();
            Assert.AreEqual(1, testModel.StoredValue); //first command gives 1, second gives 2
            Assert.AreEqual(2, testModel.SecondaryValue); //first command gives 1, second gives 2
        }

        [Test]
        public void TestMultipleOfSame()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>().To<NoArgSignalCommand>();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<NoArgSignal>();

            signal.Dispatch();
            Assert.AreEqual(2, testModel.StoredValue); //first command gives 1, second gives 2
        }

        [Test]
        public void TestNoArgs()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>();

            var testModel = injectionBinder.GetInstance<TestModel>();
            Assert.AreEqual(testModel.StoredValue, 0);

            var signal = injectionBinder.GetInstance<NoArgSignal>();
            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 1);
        }


        [Test]
        public void TestOnce()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>().Once();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(testModel.StoredValue, 0);


            var signal = injectionBinder.GetInstance<NoArgSignal>();
            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 1);

            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 1); //Should do nothing
        }


        [Test]
        public void TestOneArg()
        {
            commandBinder.Bind<OneArgSignal>().To<OneArgSignalCommand>();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<OneArgSignal>();

            var injectedValue = 100;
            signal.Dispatch(injectedValue);
            Assert.AreEqual(injectedValue, testModel.StoredValue);
        }


        [Test]
        public void TestSequence()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind<NoArgSignal>().To<CommandWithInjection>().To<CommandWithExecute>()
                .To<CommandThatThrows>().InSequence();

            TestDelegate testDelegate = delegate
            {
                var signal = injectionBinder.GetInstance<NoArgSignal>();
                signal.Dispatch();
            };

            //That the exception is thrown demonstrates that the last command ran
            var ex = Assert.Throws<NotImplementedException>(testDelegate);
            Assert.NotNull(ex);

            //That the value is 100 demonstrates that the first command ran
            var instance = injectionBinder.GetInstance<ISimpleInterface>();
            Assert.AreEqual(100, instance.intValue);
        }


        [Test]
        public void TestSequenceTwo()
        {
            //Slightly different test to be thorough

            //Bind the trigger to the command
            commandBinder.Bind<TwoArgSignal>().To<TwoArgSignalCommand>().To<TwoArgSignalCommandTwo>()
                .To<TwoArgSignalCommandThree>().InSequence();
            var testModel = injectionBinder.GetInstance<TestModel>();
            var intValue = 1;
            var boolValue = true;

            TestDelegate testDelegate = delegate
            {
                var signal = injectionBinder.GetInstance<TwoArgSignal>();
                signal.Dispatch(intValue, boolValue);
            };

            Assert.Throws<TestPassedException>(testDelegate);
            var intendedValue = 2; //intValue twice (addition because of bool == true)
            Assert.AreEqual(intendedValue, testModel.StoredValue);
        }


        [Test]
        public void TestSimpleRuntimeSignalCommandBinding()
        {
            injectionBinder.Bind<ExposedTestModel>().ToSingleton();

            var jsonCommandString =
                "[{\"Bind\":\"strange.unittests.ExposedOneArgSignal, StrangeIOC.Tests\",\"To\":[\"strange.unittests.ExposedOneArgSignalCommand, StrangeIOC.Tests\"]}]";
            commandBinder.ConsumeBindings(jsonCommandString);

            var testModel = injectionBinder.GetInstance<ExposedTestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<ExposedOneArgSignal>();

            var injectedValue = 100;
            signal.Dispatch(injectedValue);
            Assert.AreEqual(injectedValue, testModel.StoredValue);
        }

        [Test]
        public void TestTwoArgs()
        {
            commandBinder.Bind<TwoArgSignal>().To<TwoArgSignalCommand>();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<TwoArgSignal>();

            var injectedValue = 100;
            var injectedBool = true;
            signal.Dispatch(injectedValue, injectedBool); //true should be adding
            Assert.AreEqual(injectedValue, testModel.StoredValue);

            injectedBool = false;
            signal.Dispatch(injectedValue, injectedBool); //false should be subtracting
            Assert.AreEqual(0, testModel.StoredValue); //first command gives 1, second gives 2
        }

        [Test]
        public void TestTwoArgsSameType()
        {
            commandBinder.Bind<TwoArgSameTypeSignal>().To<TwoArgSameTypeSignalCommand>();

            var testModel = injectionBinder.GetInstance<TestModel>();

            Assert.AreEqual(0, testModel.StoredValue);
            var signal = injectionBinder.GetInstance<TwoArgSameTypeSignal>();

            var injectedValue = 100;
            var secondInjectedValue = 200;
            TestDelegate testDelegate = delegate { signal.Dispatch(injectedValue, secondInjectedValue); };
            var ex = Assert.Throws<SignalException>(testDelegate);
            Assert.AreEqual(ex.type, SignalExceptionType.COMMAND_VALUE_CONFLICT);
        }

        [Test]
        public void TestUnbind()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>();
            var testModel = injectionBinder.GetInstance<TestModel>();
            Assert.AreEqual(testModel.StoredValue, 0);

            var signal = injectionBinder.GetInstance<NoArgSignal>();
            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 1);

            commandBinder.Unbind<NoArgSignal>();
            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 1); //Should do nothing
        }

        [Test]
        public void TestUnbindNonexistentThrows()
        {
            TestDelegate testDelegate = delegate { commandBinder.Unbind<NoArgSignal>(); };
            var ex = Assert.Throws<InjectionException>(testDelegate);
            Assert.AreEqual(ex.type, InjectionExceptionType.NULL_BINDING);
        }

        [Test]
        public void TestUnbindWithoutUsage()
        {
            commandBinder.Bind<NoArgSignal>().To<NoArgSignalCommand>();
            var testModel = injectionBinder.GetInstance<TestModel>();
            Assert.AreEqual(testModel.StoredValue, 0);

            commandBinder.Unbind<NoArgSignal>();

            var signal = injectionBinder.GetInstance<NoArgSignal>();
            signal.Dispatch();
            Assert.AreEqual(testModel.StoredValue, 0); //Should do nothing
        }
    }

    public class ExposedTestModel
    {
        public int SecondaryValue = 0;
        public int StoredValue;
    }

    public class ExposedOneArgSignal : Signal<int>
    {
    }

    public class ExposedOneArgSignalCommand : Command
    {
        [Inject] public int injectedValue { get; set; }

        [Inject] public ExposedTestModel TestModel { get; set; }

        public override void Execute()
        {
            TestModel.StoredValue += injectedValue;
        }
    }
}