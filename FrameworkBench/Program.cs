using System;
using System.Diagnostics;

namespace FrameworkBench
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var ninject = new NinjectBench();
            ninject.TestMain();

            var dryioc= new DryIocBench();
            dryioc.TestMain();

            var splat = new SplatBench();
            splat.TestMain();

            var autofac = new AutofacBench();
            autofac.TestMain();

            var strange = new StrangeBench();
            strange.TestMain();
            Console.ReadLine();
        }
    }
}
