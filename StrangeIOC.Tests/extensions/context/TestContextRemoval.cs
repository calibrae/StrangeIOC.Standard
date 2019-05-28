using System;
using NUnit.Framework;
using strange.extensions.context.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestContextRemoval
    {
        [SetUp]
        public void SetUp()
        {
            Context.firstContext = null;
            Parent = new CrossContext( true);
            Child = new TestCrossContextSubclass( true);
        }

        private object view;
        private CrossContext Parent;
        private TestCrossContextSubclass Child;

        [Test]
        public void TestRemoval()
        {
            Parent.AddContext(Child);

            TestDelegate testDelegate = delegate { Parent.RemoveContext(Child); };

            Assert.Throws<TestPassedException>(testDelegate);
        }
    }


    public class TestCrossContextSubclass : CrossContext
    {
        public TestCrossContextSubclass()
        {
        }

        public TestCrossContextSubclass( bool autoStartup) : base( autoStartup)
        {
        }

        public override void OnRemove()
        {
            base.OnRemove();

            throw new TestPassedException("Test Passed");
        }
    }

    internal class TestPassedException : Exception
    {
        public TestPassedException(string str) : base(str)
        {
        }
    }
}