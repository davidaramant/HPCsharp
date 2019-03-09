using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HPCsharp;

namespace Benchmarks
{
    public class FillBenchmarks
    {
        private const int Size = 10000;
        readonly int[] _testArray = new int[Size];

        [Benchmark(Baseline = true)]
        public void FillWithLoop()
        {
            _testArray.Fill(55);
        }

        [Benchmark]
        public void FillWithCore()
        {
            Array.Fill(_testArray, 55);
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

    public class SumBenchmarks
    {
        private const int Size = 10000;
        readonly int[] _testArray = new int[Size];

        [GlobalSetup]
        public void FillArray()
        {
            var random = new Random();
            foreach (var index in Enumerable.Range(0, Size))
            {
                _testArray[index] = random.Next(int.MinValue, int.MaxValue);
            }
        }

        [Benchmark(Baseline = true)]
        public long SumHpc()=> _testArray.SumHpc();

        [Benchmark]
        public void SumHpcUnchecked() => _testArray.SumHpc2();
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<FillBenchmarks>();
        }
    }
}
