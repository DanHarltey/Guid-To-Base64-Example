using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace EfficientGuids
{
    [MemoryDiagnoser]
    public class Program
    {
        const int Iterations = 1000000;

        private Guid _guid;

        private static void Main(string[] args) => BenchmarkRunner.Run<Program>();

        [GlobalSetup]
        public void Setup() => _guid = Guid.NewGuid();

        [Benchmark]
        public void Base64EncodedGuidOriginal()
        {
            for (int i = 0; i < Iterations; i++)
            {
                Convert.ToBase64String(_guid.ToByteArray())
                    .Replace("/", "-")
                    .Replace("+", "_")
                    .Replace("=", "");
            }
        }

        [Benchmark]
        public void Base64EncodedGuid()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _guid.EncodeBase64String();
            }
        }

        [Benchmark(Baseline = true)]
        public void Base64EncodedGuidImproved()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _guid.EncodeBase64StringImproved();
            }
        }

        [Benchmark]
        public void ToBase64()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _guid.ToBase64();
            }
        }

        [Benchmark]
        public void ToBase64Unrolled()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _guid.ToBase64Unrolled();
            }
        }
    }
}
