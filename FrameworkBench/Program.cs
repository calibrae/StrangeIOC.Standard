using System;
using System.Diagnostics;

namespace FrameworkBench
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var strange = new StrangeBench();


            strange.TestMain();

            Console.ReadLine();
        }
    }
}
