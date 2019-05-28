using System;
using Splat;

namespace FrameworkBench
{
    public class SplatBench
    {
        public void TestMain()
        {
            Console.WriteLine("////////////////////////////////////// Splat : ");
            var now = DateTime.Now;
            Console.WriteLine("Starting Splat : " + now);
            

            Locator.CurrentMutable.Register(() => new TestModel(), typeof(TestModel));

            Console.WriteLine(" Splat built : " + (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;
            for (int i = 0; i < BenchConstants.ITERATIONS; i++)
            {
                var model = Locator.Current.GetService<TestModel>();
            }

            Console.WriteLine(" Splat EndTest : " + (DateTime.Now - now).TotalMilliseconds);
        }
    }
}