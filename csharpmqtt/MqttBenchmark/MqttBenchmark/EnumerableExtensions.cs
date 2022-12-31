namespace MqttBenchmark;

public static class EnumerableExtensions
{
    public static double Median<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        var list = sources.Select(selector).ToList();
        
        var i = (int)Math.Ceiling((double)(list.Count - 1) / 2);
        if (i >= 0)
        {
            var values = list.ToList();
            values.Sort();
            return values[i];
        }

        return default(double);
    }
    
    public static double StandardDeviation<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        var list = sources.Select(selector).ToList();

        double average = sources.Average(selector);
        double sum = list.Sum(d => Math.Pow(d - average, 2));
        return Math.Sqrt((sum) / sources.Count());
    }

    public static double GetQuantileOne<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        return GetQuantile(sources, selector, 25);
    }
    public static double GetQuantileTwo<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        return GetQuantile(sources, selector, 50);
    }
    public static double GetQuantileThree<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        return GetQuantile(sources, selector, 75);
    }
    public static double GetQuantileFour<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector)
    {
        return GetQuantile(sources, selector, 100);
    }
    
    public static double GetQuantile<TSource>(this IEnumerable<TSource> sources, Func<TSource, long> selector, int percent)
    {
        var list = sources.Select(selector).ToList();
        list.Sort();

        var q = list.Skip(list.Count * (percent / 100)).Take(1);
        if (!q.Any())
        {
            return list.Last();
        }
        return q.First();
    }
}