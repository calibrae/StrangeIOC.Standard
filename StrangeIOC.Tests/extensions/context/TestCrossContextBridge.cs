using NUnit.Framework;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;

namespace strange.unittests
{
    public class TestCrossContextBridge
    {
        private CrossContext ChildOne;
        private CrossContext ChildTwo;
        private CrossContext Parent;

        private int testValue;
        private object view;

        [SetUp]
        public void SetUp()
        {
            testValue = 0;

            Context.firstContext = null;
            view = new object();
            Parent = new CrossContextTestClass(view, true);
            Parent.Start();

            ChildOne = new CrossContextTestClass(view, true);
            ChildOne.Start();

            ChildTwo = new CrossContextTestClass(view, true);
            ChildTwo.Start();
        }

        [Test]
        public void TestBridgeMapping()
        {
            Assert.IsNotNull(Parent.crossContextBridge);
            Assert.IsNotNull(ChildOne.crossContextBridge);
            Assert.IsNotNull(ChildTwo.crossContextBridge);

            Assert.AreSame(Parent.crossContextBridge, ChildOne.crossContextBridge);
            Assert.AreSame(ChildOne.crossContextBridge, ChildTwo.crossContextBridge);
            Assert.AreSame(ChildTwo.crossContextBridge, Parent.crossContextBridge);
        }

        [Test]
        public void TestBridgeParentToChild()
        {
            Parent.crossContextBridge.Bind(SomeEnum.ONE);
            var parentDispatcher = Parent.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);

            var childDispatcher =
                ChildOne.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);
            childDispatcher.AddListener(SomeEnum.ONE, testCallback);

            var sentValue1 = 42;
            var sentValue2 = 43;

            parentDispatcher.Dispatch(SomeEnum.ONE, sentValue1);
            Assert.AreEqual(sentValue1, testValue);

            Parent.crossContextBridge.Unbind(SomeEnum.ONE);

            parentDispatcher.Dispatch(SomeEnum.ONE, sentValue2);
            Assert.AreEqual(sentValue1, testValue); //didn't change

            //Unit-test wise, this is a bit of a cheat, but it assures me that
            //all Events are returned to the EventDispatcher pool
            Assert.AreEqual(0, EventDispatcher.eventPool.instanceCount - EventDispatcher.eventPool.available);
        }

        [Test]
        public void TestBridgeChildToParent()
        {
            ChildOne.crossContextBridge.Bind(SomeEnum.ONE);
            var childDispatcher =
                ChildOne.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);

            var parentDispatcher = Parent.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);
            parentDispatcher.AddListener(SomeEnum.ONE, testCallback);

            var sentValue1 = 42;
            var sentValue2 = 43;

            childDispatcher.Dispatch(SomeEnum.ONE, sentValue1);
            Assert.AreEqual(sentValue1, testValue);

            ChildOne.crossContextBridge.Unbind(SomeEnum.ONE);

            childDispatcher.Dispatch(SomeEnum.ONE, sentValue2);
            Assert.AreEqual(sentValue1, testValue);

            //Unit-test wise, this is a bit of a cheat, but it assures me that
            //all Events are returned to the EventDispatcher pool
            Assert.AreEqual(0, EventDispatcher.eventPool.instanceCount - EventDispatcher.eventPool.available);
        }

        [Test]
        public void TestBridgeChildToChild()
        {
            ChildTwo.crossContextBridge.Bind(SomeEnum.ONE); //Note: binding in one ViewedContext...
            var childOneDispatcher =
                ChildOne.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);

            var childTwoDispatcher =
                ChildTwo.injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);
            childTwoDispatcher.AddListener(SomeEnum.ONE, testCallback);

            var sentValue1 = 42;
            var sentValue2 = 43;

            childOneDispatcher.Dispatch(SomeEnum.ONE, sentValue1);
            Assert.AreEqual(sentValue1, testValue);

            ChildOne.crossContextBridge.Unbind(SomeEnum.ONE); //...unbinding in another

            childOneDispatcher.Dispatch(SomeEnum.ONE, sentValue2);
            Assert.AreEqual(sentValue1, testValue);

            //Unit-test wise, this is a bit of a cheat, but it assures me that
            //all Events are returned to the EventDispatcher pool
            Assert.AreEqual(0, EventDispatcher.eventPool.instanceCount - EventDispatcher.eventPool.available);
        }

        private void testCallback(IEvent evt)
        {
            testValue = (int) evt.data;
        }
    }

    internal class CrossContextTestClass : CrossContext
    {
        public CrossContextTestClass()
        {
        }

        public CrossContextTestClass(object view, bool autoStartup) : base( autoStartup)
        {
        }

        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>().ToSingleton()
                .ToName(ContextKeys.CONTEXT_DISPATCHER);
        }
    }
}