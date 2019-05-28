using strange.extensions.context.impl;

namespace FrameworkBench
{
    public class StrangeBench
    {
        public void TestMain()
        {
            var context = new TestContext();
        }
    }

    public class TestContext : Context
    {
    }

    public class TestModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}