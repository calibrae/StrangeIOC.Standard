using System;
using NUnit.Framework;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.extensions.pool.api;
using strange.framework.api;

namespace strange.unittests
{
    [TestFixture]
    public class TestCommandBinderWithoutPools
    {
        [SetUp]
        public void SetUp()
        {
            injectionBinder = new InjectionBinder();
            injectionBinder.Bind<IInjectionBinder>().Bind<IInstanceProvider>().ToValue(injectionBinder);
            injectionBinder.Bind<ICommandBinder>().To<CommandBinder>().ToSingleton();

            commandBinder = injectionBinder.GetInstance<ICommandBinder>();
            (commandBinder as IPooledCommandBinder).usePooling = false;
        }

        private ICommandBinder commandBinder;
        private IInjectionBinder injectionBinder;

        [Test]
        public void TestCommandsNotReused()
        {
            commandBinder.Bind(SomeEnum.ONE).To<MarkablePoolCommand>();
            IPool<MarkablePoolCommand> pool = (commandBinder as IPooledCommandBinder).GetPool<MarkablePoolCommand>();
            Assert.IsNull(pool);
        }

        [Test]
        public void TestInterruptedSequence()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>().To<FailCommand>().To<CommandThatThrows>()
                .InSequence();

            TestDelegate testDelegate = delegate { commandBinder.ReactTo(SomeEnum.ONE); };

            //That the exception is not thrown demonstrates that the last command was interrupted
            Assert.DoesNotThrow(testDelegate);

            //That the value is 100 demonstrates that the first command ran
            var instance = injectionBinder.GetInstance<ISimpleInterface>();
            Assert.AreEqual(100, instance.intValue);
        }

        [Test]
        public void TestNotOnce()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>();

            var binding = commandBinder.GetBinding(SomeEnum.ONE) as ICommandBinding;
            Assert.IsNotNull(binding);

            commandBinder.ReactTo(SomeEnum.ONE);

            var binding2 = commandBinder.GetBinding(SomeEnum.ONE) as ICommandBinding;
            Assert.IsNotNull(binding2);
        }

        [Test]
        public void TestOnce()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>().Once();

            var binding = commandBinder.GetBinding(SomeEnum.ONE) as ICommandBinding;
            Assert.IsNotNull(binding);

            commandBinder.ReactTo(SomeEnum.ONE);

            var binding2 = commandBinder.GetBinding(SomeEnum.ONE) as ICommandBinding;
            Assert.IsNull(binding2);
        }

        [Test]
        public void TestSequence()
        {
            //CommandWithInjection requires an ISimpleInterface
            injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer>().ToSingleton();

            //Bind the trigger to the command
            commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>().To<CommandWithExecute>().To<CommandThatThrows>()
                .InSequence();

            TestDelegate testDelegate = delegate { commandBinder.ReactTo(SomeEnum.ONE); };

            //That the exception is thrown demonstrates that the last command ran
            var ex = Assert.Throws<NotImplementedException>(testDelegate);
            Assert.NotNull(ex);

            //That the value is 100 demonstrates that the first command ran
            var instance = injectionBinder.GetInstance<ISimpleInterface>();
            Assert.AreEqual(100, instance.intValue);
        }
    }
}