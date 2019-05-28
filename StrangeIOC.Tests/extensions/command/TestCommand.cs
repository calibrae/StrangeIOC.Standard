using NUnit.Framework;
using strange.extensions.command.api;

namespace strange.unittests
{
    [TestFixture]
    public class TestCommand
    {
        [Test]
        public void TestRetainRelease()
        {
            ICommand command = new CommandWithExecute();
            Assert.IsFalse(command.retain);
            command.Retain();
            Assert.IsTrue(command.retain);
            command.Release();
            Assert.IsFalse(command.retain);
        }

        [Test]
        public void TestSuccessfulExecute()
        {
            ICommand command = new CommandWithExecute();
            TestDelegate testDelegate = delegate { command.Execute(); };
            Assert.DoesNotThrow(testDelegate);
        }
    }
}