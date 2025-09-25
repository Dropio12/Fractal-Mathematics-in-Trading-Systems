using System;
using System.Collections.Generic;

namespace FractalMarketDemo
{
    /// <summary>
    /// Represents a single market data point
    /// </summary>
    public class MarketDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Returns { get; set; }
        public double Volatility { get; set; }
        
        public MarketDataPoint(DateTime timestamp, double price, double volume = 1000)
        {
            Timestamp = timestamp;
            Price = price;
            Volume = volume;
        }
    }

    /// <summary>
    /// Generates realistic market data using geometric Brownian motion with fractal noise
    /// </summary>
    public class MarketDataGenerator
    {
        private readonly Random _random;
        private readonly double _drift;
        private readonly double _volatility;
        private readonly double _fractalDimension;
        
        public MarketDataGenerator(double drift = 0.0001, double volatility = 0.02, double fractalDimension = 1.5, int seed = 42)
        {
            _random = new Random(seed);
            _drift = drift;
            _volatility = volatility;
            _fractalDimension = fractalDimension;
        }

        /// <summary>
        /// Generates market data using fractal-enhanced Brownian motion
        /// </summary>
        public List<MarketDataPoint> GenerateMarketData(int dataPoints, double initialPrice = 100.0)
        {
            var data = new List<MarketDataPoint>(dataPoints);
            var currentPrice = initialPrice;
            var startTime = DateTime.Now.AddDays(-dataPoints / 24.0); // Hourly data going back
            
            for (int i = 0; i < dataPoints; i++)
            {
                var timestamp = startTime.AddHours(i);
                
                // Generate fractal noise component
                var fractalNoise = GenerateFractalNoise(i);
                
                // Geometric Brownian motion with fractal enhancement
                var randomComponent = GenerateGaussianRandom();
                var priceChange = _drift + (_volatility * (randomComponent + fractalNoise * 0.3));
                
                // Apply price change
                currentPrice *= (1 + priceChange);
                
                // Generate volume with some correlation to price volatility
                var baseVolume = 1000;
                var volumeNoise = Math.Abs(randomComponent) * 500;
                var volume = baseVolume + volumeNoise;
                
                var dataPoint = new MarketDataPoint(timestamp, currentPrice, volume);
                
                // Calculate returns if not the first point
                if (i > 0)
                {
                    dataPoint.Returns = (currentPrice - data[i - 1].Price) / data[i - 1].Price;
                }
                
                data.Add(dataPoint);
            }
            
            // Calculate rolling volatility
            CalculateRollingVolatility(data);
            
            return data;
        }

        /// <summary>
        /// Generates fractal noise using self-similar patterns
        /// </summary>
        private double GenerateFractalNoise(int timeIndex)
        {
            double noise = 0.0;
            double amplitude = 1.0;
            double frequency = 1.0;
            
            // Generate multiple octaves of noise for fractal behavior
            for (int octave = 0; octave < 5; octave++)
            {
                var phase = (timeIndex * frequency * 0.1) % (2 * Math.PI);
                var sineWave = Math.Sin(phase) + Math.Sin(phase * 1.618) * 0.5; // Golden ratio for fractal scaling
                
                noise += amplitude * sineWave * GenerateGaussianRandom() * 0.1;
                
                // Scale for next octave using fractal dimension
                amplitude *= Math.Pow(0.5, _fractalDimension - 1);
                frequency *= 2.0;
            }
            
            return noise;
        }

        /// <summary>
        /// Generates Gaussian distributed random numbers using Box-Muller transform
        /// </summary>
        private double GenerateGaussianRandom()
        {
            double u1 = 1.0 - _random.NextDouble(); // uniform(0,1] random doubles
            double u2 = 1.0 - _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        /// <summary>
        /// Calculates rolling volatility for the market data
        /// </summary>
        private void CalculateRollingVolatility(List<MarketDataPoint> data, int window = 20)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (i < window)
                {
                    data[i].Volatility = 0.02; // Default volatility
                    continue;
                }

                // Calculate standard deviation of returns over the window
                double sumSquares = 0;
                double mean = 0;
                
                // Calculate mean
                for (int j = i - window; j < i; j++)
                {
                    mean += data[j].Returns;
                }
                mean /= window;
                
                // Calculate variance
                for (int j = i - window; j < i; j++)
                {
                    var deviation = data[j].Returns - mean;
                    sumSquares += deviation * deviation;
                }
                
                data[i].Volatility = Math.Sqrt(sumSquares / (window - 1));
            }
        }
    }

    /// <summary>
    /// Adds market events like volatility spikes and trend changes
    /// </summary>
    public class MarketEventSimulator
    {
        private readonly Random _random;
        
        public MarketEventSimulator(int seed = 42)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Adds volatility clustering events to market data
        /// </summary>
        public void AddVolatilityClustering(List<MarketDataPoint> data)
        {
            int clusterCount = data.Count / 50; // One cluster per ~50 data points
            
            for (int cluster = 0; cluster < clusterCount; cluster++)
            {
                int startIndex = _random.Next(20, data.Count - 30);
                int clusterLength = _random.Next(10, 25);
                double multiplier = 2.0 + _random.NextDouble() * 3.0; // 2x to 5x volatility
                
                for (int i = startIndex; i < Math.Min(startIndex + clusterLength, data.Count); i++)
                {
                    // Amplify price movements during volatility cluster
                    if (i > 0 && i < data.Count - 1)
                    {
                        var baseReturn = data[i].Returns;
                        var amplifiedReturn = baseReturn * multiplier;
                        data[i].Price = data[i - 1].Price * (1 + amplifiedReturn);
                        data[i].Returns = amplifiedReturn;
                        
                        // Update subsequent prices
                        if (i + 1 < data.Count)
                        {
                            data[i + 1].Returns = (data[i + 1].Price - data[i].Price) / data[i].Price;
                        }
                    }
                }
            }
        }
    }
}