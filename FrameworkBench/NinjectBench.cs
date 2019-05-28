using System;
using DryIoc;
using Ninject;

namespace FrameworkBench
{
    public class NinjectBench
    {
        public void TestMain()
        {
            Console.WriteLine("////////////////////////////////////// NInject : ");
            var now = DateTime.Now;
            Console.WriteLine("Starting NInject : " + now);
            var container = new StandardKernel();
            container.Bind<TestModel>().ToSelf();


            Console.WriteLine(" NInject built : " + (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;
            for (int i = 0; i < BenchConstants.ITERATIONS; i++)
            {
                var model = container.Get<TestModel>();
            }

            Console.WriteLine(" NInject EndTest : " + (DateTime.Now - now).TotalMilliseconds);
        }
    }
}