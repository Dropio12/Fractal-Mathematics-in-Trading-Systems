// Simple C# fractal market analysis with 10,000 candles
// Builds with .NET Framework C# compiler (csc.exe) available on Windows
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiLangFractal.CSharp
{
    public class MarketDataPoint
    {
        public DateTime Timestamp;
        public double Price;
        public double Returns;
        public double Volatility;
        public double Volume;
        public MarketDataPoint(DateTime t, double p, double v)
        {
            Timestamp = t; Price = p; Volume = v;
        }
    }

    public static class Fractal
    {
        // Box-counting fractal dimension on a window of prices
        public static double BoxCountingFractalDimension(double[] prices)
        {
            if (prices.Length < 4) return 1.0;
            double min = prices.Min();
            double max = prices.Max();
            double range = max - min;
            if (range <= 0) return 1.0;
            double[] norm = new double[prices.Length];
            for (int i = 0; i < prices.Length; i++) norm[i] = (prices[i] - min) / range;

            int[] boxSizes = new int[] { 1, 2, 3, 4, 5, 8, 10, 16, 20, 25 };
            var logInv = new List<double>();
            var logCount = new List<double>();
            foreach (var bs in boxSizes)
            {
                if (bs >= prices.Length / 2) break;
                var boxes = new HashSet<string>();
                for (int i = 0; i < norm.Length - 1; i++)
                {
                    int x = i / bs;
                    int y = (int)(norm[i] * bs);
                    boxes.Add(x+","+y);
                }
                if (boxes.Count > 0)
                {
                    logInv.Add(Math.Log(1.0/bs));
                    logCount.Add(Math.Log(boxes.Count));
                }
            }
            if (logInv.Count < 3) return 1.0;
            return LinearSlope(logInv.ToArray(), logCount.ToArray());
        }

        private static double LinearSlope(double[] x, double[] y)
        {
            int n = x.Length; double sx=0, sy=0, sxx=0, sxy=0;
            for (int i=0;i<n;i++){ sx+=x[i]; sy+=y[i]; sxx+=x[i]*x[i]; sxy+=x[i]*y[i]; }
            double d = n*sxx - sx*sx; if (Math.Abs(d) < 1e-12) return 1.0;
            return (n*sxy - sx*sy)/d;
        }
    }

    public class Program
    {
        static readonly Random Rng = new Random(42);

        public static void Main(string[] args)
        {
            int n = 10000; // 10k candles
            double initial = 100.0;
            var data = GenerateSeries(n, initial);
            ComputeReturnsAndVol(data);

            // Compute fractal dimension on the whole series and a few windows
            double fdAll = Fractal.BoxCountingFractalDimension(data.Select(d=>d.Price).ToArray());
            double fdLast1000 = Fractal.BoxCountingFractalDimension(data.Skip(n-1000).Select(d=>d.Price).ToArray());
            double fdLast500 = Fractal.BoxCountingFractalDimension(data.Skip(n-500).Select(d=>d.Price).ToArray());

            Directory.CreateDirectory("out-csharp");
            WriteMarketCsv(data, Path.Combine("out-csharp", "market_data.csv"));
            WriteSummary(fdAll, fdLast1000, fdLast500, data, Path.Combine("out-csharp", "session_summary.csv"));

            Console.WriteLine("C#: Generated 10,000 candles.");
            Console.WriteLine("C#: Fractal Dimension (all):    {0:F3}", fdAll);
            Console.WriteLine("C#: Fractal Dimension (last1k): {0:F3}", fdLast1000);
            Console.WriteLine("C#: Fractal Dimension (last500):{0:F3}", fdLast500);
            Console.WriteLine("C#: CSV written to .\\out-csharp\\");
        }

        static List<MarketDataPoint> GenerateSeries(int n, double initial)
        {
            var list = new List<MarketDataPoint>(n);
            double price = initial;
            DateTime start = DateTime.Now.AddHours(-n);
            for (int i=0;i<n;i++)
            {
                // Fractal-like noise via multi-octave sines scaled by gaussian noise
                double noise = 0; double amp=1, freq=1;
                for (int o=0;o<5;o++)
                {
                    double phase = (i*freq*0.07) % (2*Math.PI);
                    double sine = Math.Sin(phase) + 0.5*Math.Sin(phase*1.618);
                    noise += amp * sine * Gaussian() * 0.08;
                    amp *= 0.55; freq *= 2;
                }
                double drift = 0.00005;
                double vol = 0.015;
                double rnd = Gaussian();
                double dP = drift + vol*(rnd + 0.3*noise);
                price *= (1 + dP);
                double volume = 1000 + Math.Abs(rnd)*400;
                list.Add(new MarketDataPoint(start.AddHours(i), price, volume));
            }
            return list;
        }

        static void ComputeReturnsAndVol(List<MarketDataPoint> d, int window=30)
        {
            for (int i=1;i<d.Count;i++) d[i].Returns = (d[i].Price - d[i-1].Price)/d[i-1].Price;
            for (int i=0;i<d.Count;i++)
            {
                if (i<window){ d[i].Volatility=0; continue; }
                double mean=0; for (int j=i-window; j<i; j++) mean += d[j].Returns; mean/=window;
                double ss=0; for (int j=i-window; j<i; j++){ double dev=d[j].Returns-mean; ss+=dev*dev; }
                d[i].Volatility = Math.Sqrt(ss/(window-1));
            }
        }

        static void WriteMarketCsv(List<MarketDataPoint> d, string path)
        {
            using (var w = new StreamWriter(path))
            {
                w.WriteLine("Timestamp,Price,Volume,Returns,Volatility");
                foreach (var p in d)
                    w.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss},{1:F6},{2:F2},{3:F6},{4:F6}", p.Timestamp, p.Price, p.Volume, p.Returns, p.Volatility));
            }
        }

        static void WriteSummary(double fdAll, double fd1k, double fd500, List<MarketDataPoint> d, string path)
        {
            using (var w = new StreamWriter(path))
            {
                w.WriteLine("Metric,Value");
                w.WriteLine("Points,{0}", d.Count);
                w.WriteLine("StartPrice,{0:F6}", d[0].Price);
                w.WriteLine("EndPrice,{0:F6}", d[d.Count-1].Price);
                w.WriteLine("TotalReturn,{0:F6}", (d[d.Count-1].Price - d[0].Price)/d[0].Price);
                w.WriteLine("FD_All,{0:F6}", fdAll);
                w.WriteLine("FD_Last1000,{0:F6}", fd1k);
                w.WriteLine("FD_Last500,{0:F6}", fd500);
            }
        }

        static double Gaussian()
        {
            double u1 = 1.0 - Rng.NextDouble();
            double u2 = 1.0 - Rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0*Math.PI*u2);
        }
    }
}
