using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HPCsharp;

namespace Benchmarks
{
    public class ArrayFillBenchmarks
    {
        private const int Size = 10000;
        readonly int[] _testArray = new int[Size];

        [Benchmark(Baseline = true)]
        public void FillWithLoop()
        {
            _testArray.Fill(55);
        }

        [Benchmark]
        public void FillWithBlockCopy()
        {
            _testArray.FillUsingBlockCopy(44);
        }

        [Benchmark]
        public void FillWithArrayCopy()
        {
            _testArray.FillUsingArrayCopy(66);
        }

        [Benchmark]
        public void FillWithArrayCopy_Mine()
        {
            _testArray.FillUsingArrayCopy2(66);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ArrayFillBenchmarks>();
        }
    }
}
