// Go fractal market analysis with 10,000 candles and goroutines
package main

import (
	"encoding/csv"
	"fmt"
	"math"
	"math/rand"
	"os"
	"strconv"
	"sync"
	"time"
)

type MarketCandle struct {
	Timestamp  time.Time
	Price      float64
	Volume     float64
	Returns    float64
	Volatility float64
}

type FractalResult struct {
	WindowStart int
	WindowEnd   int
	Dimension   float64
}

func main() {
	rand.Seed(42)
	n := 10000
	initial := 100.0

	fmt.Println("Go: Generating 10,000 candles...")
	data := generateSeries(n, initial)
	computeReturnsAndVol(data, 30)

	fmt.Println("Go: Computing fractal dimensions in parallel...")
	
	// Parallel computation of fractal dimensions for different windows
	var wg sync.WaitGroup
	results := make(chan FractalResult, 10)
	
	// Multiple window sizes for fractal analysis
	windows := []struct{ start, size int }{
		{0, n},           // Full series
		{n - 1000, 1000}, // Last 1000
		{n - 500, 500},   // Last 500
		{0, 2000},        // First 2000
		{2000, 2000},     // Middle 2000
		{6000, 2000},     // Another 2000
	}

	for i, w := range windows {
		wg.Add(1)
		go func(idx int, start, size int) {
			defer wg.Done()
			if start+size > len(data) {
				size = len(data) - start
			}
			prices := make([]float64, size)
			for j := 0; j < size; j++ {
				prices[j] = data[start+j].Price
			}
			fd := boxCountingFractalDimension(prices)
			results <- FractalResult{start, start + size - 1, fd}
		}(i, w.start, w.size)
	}

	go func() {
		wg.Wait()
		close(results)
	}()

	// Collect results
	var fractalResults []FractalResult
	for result := range results {
		fractalResults = append(fractalResults, result)
	}

	// Create output directory
	os.MkdirAll("out-go", 0755)

	// Write CSV files
	writeMarketCSV(data, "out-go/market_data.csv")
	writeFractalCSV(fractalResults, "out-go/fractal_patterns.csv")
	writeSummary(data, fractalResults, "out-go/session_summary.csv")

	fmt.Printf("Go: Fractal analysis complete. Results:\n")
	for _, r := range fractalResults {
		windowName := "unknown"
		switch r.WindowStart {
		case 0:
			if r.WindowEnd == n-1 {
				windowName = "full"
			} else {
				windowName = "first2k"
			}
		case n - 1000:
			windowName = "last1k"
		case n - 500:
			windowName = "last500"
		case 2000:
			windowName = "mid2k"
		case 6000:
			windowName = "late2k"
		}
		fmt.Printf("Go: FD (%s): %.3f\n", windowName, r.Dimension)
	}
	fmt.Println("Go: CSV written to ./out-go/")
}

func generateSeries(n int, initial float64) []MarketCandle {
	data := make([]MarketCandle, n)
	price := initial
	start := time.Now().Add(-time.Duration(n) * time.Hour)

	for i := 0; i < n; i++ {
		// Multi-octave fractal noise
		noise := 0.0
		amp, freq := 1.0, 1.0
		for o := 0; o < 5; o++ {
			phase := math.Mod(float64(i)*freq*0.07, 2*math.Pi)
			sine := math.Sin(phase) + 0.5*math.Sin(phase*1.618)
			noise += amp * sine * gaussian() * 0.08
			amp *= 0.55
			freq *= 2
		}

		drift := 0.00005
		vol := 0.015
		rnd := gaussian()
		dP := drift + vol*(rnd+0.3*noise)
		price *= (1 + dP)

		volume := 1000 + math.Abs(rnd)*400

		data[i] = MarketCandle{
			Timestamp: start.Add(time.Duration(i) * time.Hour),
			Price:     price,
			Volume:    volume,
		}
	}
	return data
}

func computeReturnsAndVol(data []MarketCandle, window int) {
	// Compute returns
	for i := 1; i < len(data); i++ {
		data[i].Returns = (data[i].Price - data[i-1].Price) / data[i-1].Price
	}

	// Compute rolling volatility
	for i := 0; i < len(data); i++ {
		if i < window {
			data[i].Volatility = 0
			continue
		}

		mean := 0.0
		for j := i - window; j < i; j++ {
			mean += data[j].Returns
		}
		mean /= float64(window)

		ss := 0.0
		for j := i - window; j < i; j++ {
			dev := data[j].Returns - mean
			ss += dev * dev
		}
		data[i].Volatility = math.Sqrt(ss / float64(window-1))
	}
}

func boxCountingFractalDimension(prices []float64) float64 {
	if len(prices) < 4 {
		return 1.0
	}

	// Normalize prices
	min, max := prices[0], prices[0]
	for _, p := range prices {
		if p < min {
			min = p
		}
		if p > max {
			max = p
		}
	}
	
	rang := max - min
	if rang <= 0 {
		return 1.0
	}

	norm := make([]float64, len(prices))
	for i, p := range prices {
		norm[i] = (p - min) / rang
	}

	boxSizes := []int{1, 2, 3, 4, 5, 8, 10, 16, 20, 25, 32}
	var logInv, logCount []float64

	for _, bs := range boxSizes {
		if bs >= len(prices)/2 {
			break
		}

		boxes := make(map[string]bool)
		for i := 0; i < len(norm)-1; i++ {
			x := i / bs
			y := int(norm[i] * float64(bs))
			key := fmt.Sprintf("%d,%d", x, y)
			boxes[key] = true
		}

		if len(boxes) > 0 {
			logInv = append(logInv, math.Log(1.0/float64(bs)))
			logCount = append(logCount, math.Log(float64(len(boxes))))
		}
	}

	if len(logInv) < 3 {
		return 1.0
	}

	return linearSlope(logInv, logCount)
}

func linearSlope(x, y []float64) float64 {
	n := float64(len(x))
	var sx, sy, sxx, sxy float64

	for i := 0; i < len(x); i++ {
		sx += x[i]
		sy += y[i]
		sxx += x[i] * x[i]
		sxy += x[i] * y[i]
	}

	d := n*sxx - sx*sx
	if math.Abs(d) < 1e-12 {
		return 1.0
	}

	return (n*sxy - sx*sy) / d
}

func gaussian() float64 {
	u1 := 1.0 - rand.Float64()
	u2 := 1.0 - rand.Float64()
	return math.Sqrt(-2.0*math.Log(u1)) * math.Sin(2.0*math.Pi*u2)
}

func writeMarketCSV(data []MarketCandle, filename string) error {
	file, err := os.Create(filename)
	if err != nil {
		return err
	}
	defer file.Close()

	writer := csv.NewWriter(file)
	defer writer.Flush()

	// Header
	writer.Write([]string{"Timestamp", "Price", "Volume", "Returns", "Volatility"})

	// Data
	for _, candle := range data {
		record := []string{
			candle.Timestamp.Format("2006-01-02 15:04:05"),
			fmt.Sprintf("%.6f", candle.Price),
			fmt.Sprintf("%.2f", candle.Volume),
			fmt.Sprintf("%.6f", candle.Returns),
			fmt.Sprintf("%.6f", candle.Volatility),
		}
		writer.Write(record)
	}

	return nil
}

func writeFractalCSV(results []FractalResult, filename string) error {
	file, err := os.Create(filename)
	if err != nil {
		return err
	}
	defer file.Close()

	writer := csv.NewWriter(file)
	defer writer.Flush()

	writer.Write([]string{"WindowStart", "WindowEnd", "WindowSize", "FractalDimension"})

	for _, r := range results {
		record := []string{
			strconv.Itoa(r.WindowStart),
			strconv.Itoa(r.WindowEnd),
			strconv.Itoa(r.WindowEnd - r.WindowStart + 1),
			fmt.Sprintf("%.6f", r.Dimension),
		}
		writer.Write(record)
	}

	return nil
}

func writeSummary(data []MarketCandle, results []FractalResult, filename string) error {
	file, err := os.Create(filename)
	if err != nil {
		return err
	}
	defer file.Close()

	writer := csv.NewWriter(file)
	defer writer.Flush()

	writer.Write([]string{"Metric", "Value"})

	writer.Write([]string{"Points", strconv.Itoa(len(data))})
	writer.Write([]string{"StartPrice", fmt.Sprintf("%.6f", data[0].Price)})
	writer.Write([]string{"EndPrice", fmt.Sprintf("%.6f", data[len(data)-1].Price)})
	totalReturn := (data[len(data)-1].Price - data[0].Price) / data[0].Price
	writer.Write([]string{"TotalReturn", fmt.Sprintf("%.6f", totalReturn)})

	for i, r := range results {
		writer.Write([]string{fmt.Sprintf("FD_Window_%d", i), fmt.Sprintf("%.6f", r.Dimension)})
	}

	return nil
}