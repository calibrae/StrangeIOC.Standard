using System;
using NUnit.Framework;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestEventBinding
    {
        private const int INIT_VALUE = 42;
        private int confirmationValue = 42;

        private void noArgumentCallback()
        {
            confirmationValue *= 2;
        }

        private void oneArgumentCallback(IEvent o)
        {
            confirmationValue *= (int) o.data;
        }


        private class TestEvent : IEvent
        {
            public TestEvent(object type, IEventDispatcher target, object data)
            {
                Type = type;
                Target = target;
                Data = data;
            }

            public object Type { get; set; }
            public IEventDispatcher Target { get; set; }
            public object Data { get; set; }

            public object type
            {
                get => Type;
                set => Type = value;
            }

            public IEventDispatcher target
            {
                get => Target;
                set => Target = value;
            }

            public object data
            {
                get => Data;
                set => Data = value;
            }
        }


        [Test]
        public void TestMapNoArgumentCallback()
        {
            confirmationValue = INIT_VALUE;
            IEventBinding binding = new EventBinding();
            binding.Bind(SomeEnum.ONE).To(noArgumentCallback);
            var type = binding.TypeForCallback(noArgumentCallback);
            var value = binding.value as object[];
            var extracted = value[0] as Delegate;

            Assert.AreEqual(EventCallbackType.NO_ARGUMENTS, type);

            extracted.DynamicInvoke();
            //Calling the method should change the confirmationValue
            Assert.AreNotEqual(confirmationValue, INIT_VALUE);
        }

        [Test]
        public void TestMapOneArgumentCallback()
        {
            confirmationValue = INIT_VALUE;
            IEventBinding binding = new EventBinding();
            binding.Bind(SomeEnum.ONE).To(oneArgumentCallback);
            var type = binding.TypeForCallback(oneArgumentCallback);
            var value = binding.value as object[];
            var extracted = value[0] as Delegate;

            Assert.AreEqual(EventCallbackType.ONE_ARGUMENT, type);

            var parameters = new object[1];
            parameters[0] = new TestEvent("TEST", null, INIT_VALUE);
            extracted.DynamicInvoke(parameters);
            //Calling the method should change the confirmationValue
            Assert.AreEqual(confirmationValue, INIT_VALUE * INIT_VALUE);
        }
    }
}