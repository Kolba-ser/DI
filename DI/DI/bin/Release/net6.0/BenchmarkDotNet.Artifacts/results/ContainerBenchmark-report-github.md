``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19042.746/20H2/October2020Update)
AMD Ryzen 7 5800X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.401
  [Host]     : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2


```
|     Method |       Mean |      Error |    StdDev |     Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------- |-----------:|-----------:|----------:|-----------:|------:|--------:|-------:|----------:|------------:|
|     Create |   8.989 ns |  1.2988 ns |  3.830 ns |   6.478 ns |  1.00 |    0.00 | 0.0029 |      48 B |        1.00 |
| Reflection | 216.481 ns |  6.4277 ns | 18.952 ns | 211.708 ns | 28.57 |   11.15 | 0.0138 |     232 B |        4.83 |
|     Lambda |  79.452 ns |  1.6185 ns |  2.704 ns |  79.965 ns | 11.29 |    4.13 | 0.0105 |     176 B |        3.67 |
|    Autofac | 593.639 ns | 14.4608 ns | 42.638 ns | 594.347 ns | 77.58 |   28.13 | 0.0801 |    1344 B |       28.00 |
|       MSDI |  31.374 ns |  0.8430 ns |  2.486 ns |  31.204 ns |  4.14 |    1.60 | 0.0029 |      48 B |        1.00 |
