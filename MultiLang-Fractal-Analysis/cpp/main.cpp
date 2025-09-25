// C++ fractal market analysis with 10,000 candles using STL
#include <iostream>
#include <vector>
#include <cmath>
#include <random>
#include <fstream>
#include <sstream>
#include <chrono>
#include <iomanip>
#include <unordered_set>
#include <algorithm>
#include <filesystem>

struct MarketCandle {
    std::chrono::system_clock::time_point timestamp;
    double price;
    double volume;
    double returns;
    double volatility;
    
    MarketCandle(std::chrono::system_clock::time_point t, double p, double v)
        : timestamp(t), price(p), volume(v), returns(0.0), volatility(0.0) {}
};

class FractalAnalyzer {
private:
    std::mt19937 rng;
    std::normal_distribution<double> normal_dist;
    
public:
    FractalAnalyzer(unsigned seed = 42) : rng(seed), normal_dist(0.0, 1.0) {}
    
    double gaussian() {
        return normal_dist(rng);
    }
    
    std::vector<MarketCandle> generateSeries(int n, double initial) {
        std::vector<MarketCandle> data;
        data.reserve(n);
        
        double price = initial;
        auto start = std::chrono::system_clock::now() - std::chrono::hours(n);
        
        for (int i = 0; i < n; ++i) {
            // Multi-octave fractal noise
            double noise = 0.0;
            double amp = 1.0, freq = 1.0;
            
            for (int o = 0; o < 5; ++o) {
                double phase = fmod(i * freq * 0.07, 2.0 * M_PI);
                double sine = sin(phase) + 0.5 * sin(phase * 1.618);
                noise += amp * sine * gaussian() * 0.08;
                amp *= 0.55;
                freq *= 2.0;
            }
            
            double drift = 0.00005;
            double vol = 0.015;
            double rnd = gaussian();
            double dP = drift + vol * (rnd + 0.3 * noise);
            price *= (1.0 + dP);
            
            double volume = 1000.0 + std::abs(rnd) * 400.0;
            
            auto timestamp = start + std::chrono::hours(i);
            data.emplace_back(timestamp, price, volume);
        }
        
        return data;
    }
    
    void computeReturnsAndVolatility(std::vector<MarketCandle>& data, int window = 30) {
        // Compute returns
        for (size_t i = 1; i < data.size(); ++i) {
            data[i].returns = (data[i].price - data[i-1].price) / data[i-1].price;
        }
        
        // Compute rolling volatility
        for (size_t i = 0; i < data.size(); ++i) {
            if (i < static_cast<size_t>(window)) {
                data[i].volatility = 0.0;
                continue;
            }
            
            double mean = 0.0;
            for (int j = i - window; j < static_cast<int>(i); ++j) {
                mean += data[j].returns;
            }
            mean /= window;
            
            double ss = 0.0;
            for (int j = i - window; j < static_cast<int>(i); ++j) {
                double dev = data[j].returns - mean;
                ss += dev * dev;
            }
            
            data[i].volatility = sqrt(ss / (window - 1));
        }
    }
    
    double boxCountingFractalDimension(const std::vector<double>& prices) {
        if (prices.size() < 4) return 1.0;
        
        auto minmax = std::minmax_element(prices.begin(), prices.end());
        double min_val = *minmax.first;
        double max_val = *minmax.second;
        double range = max_val - min_val;
        
        if (range <= 0) return 1.0;
        
        // Normalize prices
        std::vector<double> norm(prices.size());
        for (size_t i = 0; i < prices.size(); ++i) {
            norm[i] = (prices[i] - min_val) / range;
        }
        
        std::vector<int> boxSizes = {1, 2, 3, 4, 5, 8, 10, 16, 20, 25, 32, 40};
        std::vector<double> logInv, logCount;
        
        for (int bs : boxSizes) {
            if (bs >= static_cast<int>(prices.size()) / 2) break;
            
            std::unordered_set<std::string> boxes;
            
            for (size_t i = 0; i < norm.size() - 1; ++i) {
                int x = static_cast<int>(i) / bs;
                int y = static_cast<int>(norm[i] * bs);
                boxes.insert(std::to_string(x) + "," + std::to_string(y));
            }
            
            if (!boxes.empty()) {
                logInv.push_back(log(1.0 / bs));
                logCount.push_back(log(static_cast<double>(boxes.size())));
            }
        }
        
        if (logInv.size() < 3) return 1.0;
        
        return linearSlope(logInv, logCount);
    }
    
private:
    double linearSlope(const std::vector<double>& x, const std::vector<double>& y) {
        double n = static_cast<double>(x.size());
        double sx = 0, sy = 0, sxx = 0, sxy = 0;
        
        for (size_t i = 0; i < x.size(); ++i) {
            sx += x[i];
            sy += y[i];
            sxx += x[i] * x[i];
            sxy += x[i] * y[i];
        }
        
        double d = n * sxx - sx * sx;
        if (std::abs(d) < 1e-12) return 1.0;
        
        return (n * sxy - sx * sy) / d;
    }
};

void writeMarketCSV(const std::vector<MarketCandle>& data, const std::string& filename) {
    std::ofstream file(filename);
    file << "Timestamp,Price,Volume,Returns,Volatility\n";
    
    for (const auto& candle : data) {
        auto time_t = std::chrono::system_clock::to_time_t(candle.timestamp);
        auto tm = *std::localtime(&time_t);
        
        file << std::put_time(&tm, "%Y-%m-%d %H:%M:%S") << ","
             << std::fixed << std::setprecision(6) << candle.price << ","
             << std::fixed << std::setprecision(2) << candle.volume << ","
             << std::fixed << std::setprecision(6) << candle.returns << ","
             << std::fixed << std::setprecision(6) << candle.volatility << "\n";
    }
}

void writeSummary(const std::vector<MarketCandle>& data, 
                 const std::vector<std::pair<std::string, double>>& fractalResults,
                 const std::string& filename) {
    std::ofstream file(filename);
    file << "Metric,Value\n";
    file << "Points," << data.size() << "\n";
    file << "StartPrice," << std::fixed << std::setprecision(6) << data.front().price << "\n";
    file << "EndPrice," << std::fixed << std::setprecision(6) << data.back().price << "\n";
    
    double totalReturn = (data.back().price - data.front().price) / data.front().price;
    file << "TotalReturn," << std::fixed << std::setprecision(6) << totalReturn << "\n";
    
    for (const auto& result : fractalResults) {
        file << result.first << "," << std::fixed << std::setprecision(6) << result.second << "\n";
    }
}

void writeFractalCSV(const std::vector<std::pair<std::string, double>>& results, 
                    const std::string& filename) {
    std::ofstream file(filename);
    file << "WindowName,FractalDimension\n";
    
    for (const auto& result : results) {
        file << result.first << "," << std::fixed << std::setprecision(6) << result.second << "\n";
    }
}

int main() {
    constexpr int n = 10000;
    constexpr double initial = 100.0;
    
    std::cout << "C++: Generating 10,000 candles..." << std::endl;
    
    FractalAnalyzer analyzer;
    auto data = analyzer.generateSeries(n, initial);
    analyzer.computeReturnsAndVolatility(data, 30);
    
    std::cout << "C++: Computing fractal dimensions..." << std::endl;
    
    // Extract prices for different windows
    std::vector<double> allPrices, last1000, last500, first2000, mid2000;
    
    allPrices.reserve(n);
    for (const auto& candle : data) {
        allPrices.push_back(candle.price);
    }
    
    last1000.assign(allPrices.end() - 1000, allPrices.end());
    last500.assign(allPrices.end() - 500, allPrices.end());
    first2000.assign(allPrices.begin(), allPrices.begin() + 2000);
    mid2000.assign(allPrices.begin() + 2000, allPrices.begin() + 4000);
    
    // Compute fractal dimensions
    std::vector<std::pair<std::string, double>> fractalResults = {
        {"FD_All", analyzer.boxCountingFractalDimension(allPrices)},
        {"FD_Last1000", analyzer.boxCountingFractalDimension(last1000)},
        {"FD_Last500", analyzer.boxCountingFractalDimension(last500)},
        {"FD_First2000", analyzer.boxCountingFractalDimension(first2000)},
        {"FD_Mid2000", analyzer.boxCountingFractalDimension(mid2000)}
    };
    
    // Create output directory
    std::filesystem::create_directories("out-cpp");
    
    // Write output files
    writeMarketCSV(data, "out-cpp/market_data.csv");
    writeFractalCSV(fractalResults, "out-cpp/fractal_patterns.csv");
    writeSummary(data, fractalResults, "out-cpp/session_summary.csv");
    
    std::cout << "C++: Fractal analysis complete. Results:" << std::endl;
    for (const auto& result : fractalResults) {
        std::cout << "C++: " << result.first << ": " 
                 << std::fixed << std::setprecision(3) << result.second << std::endl;
    }
    
    std::cout << "C++: CSV written to ./out-cpp/" << std::endl;
    
    return 0;
}