using System;
using Autofac;
using DryIoc;

namespace FrameworkBench
{
    public class DryIocBench
    {
        public void TestMain()
        {
            Console.WriteLine("////////////////////////////////////// Dryioc : ");
            var now = DateTime.Now;
            Console.WriteLine("Starting DryIoc : " + now);
            var container = new Container();
            container.Register<TestModel>();

            Console.WriteLine(" Dryioc built : " + (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;
            for (int i = 0; i < BenchConstants.ITERATIONS; i++)
            {
                var model = container.Resolve<TestModel>();
            }

            Console.WriteLine(" DryIoc EndTest : " + (DateTime.Now - now).TotalMilliseconds);
        }
        
    }
}