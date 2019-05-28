using System;
using System.Diagnostics;
using strange.extensions.context.impl;

namespace FrameworkBench
{
    public class StrangeBench
    {
        public void TestMain()
        {

            Console.WriteLine("////////////////////////////////////// Strange : ");
            var start = DateTime.Now;
            Console.WriteLine("Starting test at : "+start);
            var context = new TestContext();
            Console.WriteLine(" Strange built : "+(DateTime.Now - start).TotalMilliseconds);
            start = DateTime.Now;
            for (int i = 0; i < BenchConstants.ITERATIONS; i++)
            {
                var model = context.GetTestModel();
            }

            Console.WriteLine("Strange Endtest : "+(DateTime.Now - start).TotalMilliseconds);
        }
    }

    public class TestContext : CrossContext
    {
        protected override void mapBindings()
        {
            Console.WriteLine("Mapping bindings");
            injectionBinder.Bind<TestModel>().To<TestModel>();
        }

        public TestModel GetTestModel()
        {
            return injectionBinder.GetInstance<TestModel>();
        }
    }

    public class TestModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}