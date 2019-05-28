using System;
using System.Diagnostics;
using strange.extensions.context.impl;

namespace FrameworkBench
{
    public class StrangeBench
    {
        public void TestMain()
        {
            var start = DateTime.Now;
            Console.WriteLine("Starting test at : "+start);
            var context = new TestContext();
            context.Start();
            Console.WriteLine("ContextCreated test at : "+(DateTime.Now - start).TotalMilliseconds);
            start = DateTime.Now;
            for (int i = 0; i < 50; i++)
            {
                var model = context.GetTestModel();
                var model2 = context.GetTestModel();
                Console.WriteLine(model == model2);
            }

            Console.WriteLine("ContextCreated test at : "+(DateTime.Now - start).TotalMilliseconds);
        }
    }

    public class TestContext : CrossContext
    {
        protected override void mapBindings()
        {
            Console.WriteLine("Mapping bindings");
//            injectionBinder.Bind<TestModel>().To<TestModel>();
            injectionBinder.Bind<TestModel>().ToSingleton();
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