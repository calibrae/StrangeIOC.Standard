using System;
using Autofac;

namespace FrameworkBench
{
    public class AutofacBench
    {
        public void TestMain()
        {
            Console.WriteLine("////////////////////////////////////// Autofac : ");
            var now = DateTime.Now;
            Console.WriteLine("Starting Autofac : "+now);
            var builder = new ContainerBuilder();
            builder.RegisterType<TestModel>();

            var container = builder.Build();

            Console.WriteLine(" Autofac built : "+(DateTime.Now-now).TotalMilliseconds);
            now = DateTime.Now;
            for (int i = 0; i < BenchConstants.ITERATIONS; i++)
            {
                var model = container.Resolve<TestModel>();
            }

            Console.WriteLine(" Autofac EndTest : "+(DateTime.Now-now).TotalMilliseconds);
        }
    }
}