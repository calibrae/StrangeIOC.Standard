using NUnit.Framework;
using strange.extensions.context.api;
using strange.extensions.context.impl;

namespace strange.unittests
{
    /**
     * Test the startup routine for a basic ViewedContext.
     **/
    [TestFixture]
    public class TestContext
    {
        [SetUp]
        public void SetUp()
        {
            Context.firstContext = null;
        }

        [Test]
        public void TestAutoStartup()
        {
            var context = new TestContextSubclass();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestContextIsFirstContext()
        {
            var context = new Context();
            Assert.AreEqual(context, Context.firstContext);
        }



        [Test]
        public void TestInterruptAll()
        {
            var context = new TestContextSubclass(
                ContextStartupFlags.MANUAL_MAPPING | ContextStartupFlags.MANUAL_LAUNCH);
            Assert.AreEqual(TestContextSubclass.INIT_VALUE, context.testValue);
            context.Start();
            Assert.AreEqual(TestContextSubclass.MAPPING_VALUE, context.testValue);
            context.Launch();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestInterruptLaunch()
        {
            var context = new TestContextSubclass( ContextStartupFlags.MANUAL_LAUNCH);
            Assert.AreEqual(TestContextSubclass.MAPPING_VALUE, context.testValue);
            context.Launch();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestInterruptMapping()
        {
            var context = new TestContextSubclass( ContextStartupFlags.MANUAL_MAPPING);
            Assert.AreEqual(TestContextSubclass.INIT_VALUE, context.testValue);
            context.Start();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestOldStyleAutoStartup()
        {
            var context = new TestContextSubclass( true);
            Assert.AreEqual(TestContextSubclass.INIT_VALUE, context.testValue);
            context.Start();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestOldStyleInterruptLaunch()
        {
            var context = new TestContextSubclass( false);
            Assert.AreEqual(TestContextSubclass.INIT_VALUE, context.testValue);
            context.Start();
            Assert.AreEqual(TestContextSubclass.MAPPING_VALUE, context.testValue);
            context.Launch();
            Assert.AreEqual(TestContextSubclass.LAUNCH_VALUE, context.testValue);
        }

        [Test]
        public void TestContextCannotBeStartedTwice()
        {
            var context = new TestContextSubclass();
            Assert.Throws<ContextException>(delegate { context.Start(); });
        }
    }

    internal class TestContextSubclass : Context
    {
        public static string INIT_VALUE = "Zaphod";
        public static string MAPPING_VALUE = "Ford Prefect";
        public static string LAUNCH_VALUE = "Arthur Dent";

        public TestContextSubclass() : base()
        {
        }

        public TestContextSubclass( bool autoMapping) : base( autoMapping)
        {
        }

        public TestContextSubclass(ContextStartupFlags flags) : base( flags)
        {
        }

        public string testValue { get; private set; } = INIT_VALUE;


        protected override void mapBindings()
        {
            base.mapBindings();
            testValue = MAPPING_VALUE;
        }

        public override void Launch()
        {
            base.Launch();
            testValue = LAUNCH_VALUE;
        }
    }
}