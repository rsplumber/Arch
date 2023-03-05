// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Core.EndpointDefinitions;

BenchmarkRunner.Run<TreeBenchmark>();

Console.WriteLine("Hello, World!");

[MemoryDiagnoser()]
[RPlotExporter]
public class TreeBenchmark
{
    private static readonly EndpointNode BaseNode = EndpointNode.CreateRoot();
    private const string Url = "api/v2/service6/myget35";
    private const string AppendUrl = "api/v10/service6/myget35";

    [GlobalSetup]
    public void Setup()
    {
        foreach (var i in Enumerable.Range(1, 3))
        {
            foreach (var j in Enumerable.Range(1, 10))
            {
                foreach (var k in Enumerable.Range(1, 50))
                {
                    BaseNode.Append($"api/v{i}/service{j}/myget{k}");
                }
            }
        }
    }

    [Benchmark]
    public void Append()
    {
        BaseNode.Append(AppendUrl);
    }

    [Benchmark(OperationsPerInvoke = 10)]
    public string Find()
    {
        return BaseNode.Find(Url);
    }
}