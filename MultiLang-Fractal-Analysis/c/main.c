/* Pure C fractal market analysis with 10,000 candles */
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <time.h>
#include <string.h>

#ifdef _WIN32
    #include <direct.h>
    #define mkdir(path, mode) _mkdir(path)
#else
    #include <sys/stat.h>
    #include <sys/types.h>
#endif

#define N_CANDLES 10000
#define MAX_BOXES 100000
#define MAX_BOX_SIZES 20

typedef struct {
    time_t timestamp;
    double price;
    double volume;
    double returns;
    double volatility;
} MarketCandle;

typedef struct {
    char key[32];
    int used;
} Box;

/* Simple hash table for box counting */
static Box boxes[MAX_BOXES];
static int box_count;

/* Random number generation */
static unsigned long rng_state = 42;

double simple_random(void) {
    rng_state = (rng_state * 1103515245 + 12345) & 0x7fffffff;
    return (double)rng_state / 0x7fffffff;
}

double gaussian_random(void) {
    static int has_spare = 0;
    static double spare;
    
    if (has_spare) {
        has_spare = 0;
        return spare;
    }
    
    has_spare = 1;
    double u1 = 1.0 - simple_random();
    double u2 = 1.0 - simple_random();
    double mag = sqrt(-2.0 * log(u1));
    spare = mag * cos(2.0 * M_PI * u2);
    return mag * sin(2.0 * M_PI * u2);
}

void generate_series(MarketCandle *data, int n, double initial) {
    double price = initial;
    time_t start_time = time(NULL) - (n * 3600); /* n hours ago */
    
    for (int i = 0; i < n; i++) {
        /* Multi-octave fractal noise */
        double noise = 0.0;
        double amp = 1.0, freq = 1.0;
        
        for (int o = 0; o < 5; o++) {
            double phase = fmod(i * freq * 0.07, 2.0 * M_PI);
            double sine = sin(phase) + 0.5 * sin(phase * 1.618);
            noise += amp * sine * gaussian_random() * 0.08;
            amp *= 0.55;
            freq *= 2.0;
        }
        
        double drift = 0.00005;
        double vol = 0.015;
        double rnd = gaussian_random();
        double dP = drift + vol * (rnd + 0.3 * noise);
        price *= (1.0 + dP);
        
        double volume = 1000.0 + fabs(rnd) * 400.0;
        
        data[i].timestamp = start_time + (i * 3600);
        data[i].price = price;
        data[i].volume = volume;
        data[i].returns = 0.0;
        data[i].volatility = 0.0;
    }
}

void compute_returns_and_volatility(MarketCandle *data, int n, int window) {
    /* Compute returns */
    for (int i = 1; i < n; i++) {
        data[i].returns = (data[i].price - data[i-1].price) / data[i-1].price;
    }
    
    /* Compute rolling volatility */
    for (int i = 0; i < n; i++) {
        if (i < window) {
            data[i].volatility = 0.0;
            continue;
        }
        
        double mean = 0.0;
        for (int j = i - window; j < i; j++) {
            mean += data[j].returns;
        }
        mean /= window;
        
        double ss = 0.0;
        for (int j = i - window; j < i; j++) {
            double dev = data[j].returns - mean;
            ss += dev * dev;
        }
        
        data[i].volatility = sqrt(ss / (window - 1));
    }
}

void reset_boxes(void) {
    for (int i = 0; i < MAX_BOXES; i++) {
        boxes[i].used = 0;
    }
    box_count = 0;
}

int hash_key(const char *key) {
    int hash = 0;
    while (*key) {
        hash = (hash * 31 + *key) % MAX_BOXES;
        key++;
    }
    return hash;
}

void add_box(int x, int y) {
    char key[32];
    sprintf(key, "%d,%d", x, y);
    
    int hash = hash_key(key);
    int orig_hash = hash;
    
    while (boxes[hash].used && strcmp(boxes[hash].key, key) != 0) {
        hash = (hash + 1) % MAX_BOXES;
        if (hash == orig_hash) return; /* Table full */
    }
    
    if (!boxes[hash].used) {
        strcpy(boxes[hash].key, key);
        boxes[hash].used = 1;
        box_count++;
    }
}

int count_boxes(double *norm_prices, int n, int box_size) {
    reset_boxes();
    
    for (int i = 0; i < n - 1; i++) {
        int x = i / box_size;
        int y = (int)(norm_prices[i] * box_size);
        add_box(x, y);
    }
    
    return box_count;
}

double linear_slope(double *x, double *y, int n) {
    double sx = 0, sy = 0, sxx = 0, sxy = 0;
    
    for (int i = 0; i < n; i++) {
        sx += x[i];
        sy += y[i];
        sxx += x[i] * x[i];
        sxy += x[i] * y[i];
    }
    
    double d = n * sxx - sx * sx;
    if (fabs(d) < 1e-12) return 1.0;
    
    return (n * sxy - sx * sy) / d;
}

double box_counting_fractal_dimension(double *prices, int n) {
    if (n < 4) return 1.0;
    
    /* Find min/max */
    double min_val = prices[0], max_val = prices[0];
    for (int i = 1; i < n; i++) {
        if (prices[i] < min_val) min_val = prices[i];
        if (prices[i] > max_val) max_val = prices[i];
    }
    
    double range = max_val - min_val;
    if (range <= 0) return 1.0;
    
    /* Normalize prices */
    double *norm = malloc(n * sizeof(double));
    for (int i = 0; i < n; i++) {
        norm[i] = (prices[i] - min_val) / range;
    }
    
    int box_sizes[] = {1, 2, 3, 4, 5, 8, 10, 16, 20, 25, 32};
    int n_sizes = sizeof(box_sizes) / sizeof(box_sizes[0]);
    
    double log_inv[MAX_BOX_SIZES], log_count[MAX_BOX_SIZES];
    int valid_sizes = 0;
    
    for (int i = 0; i < n_sizes && valid_sizes < MAX_BOX_SIZES; i++) {
        int bs = box_sizes[i];
        if (bs >= n / 2) break;
        
        int count = count_boxes(norm, n, bs);
        if (count > 0) {
            log_inv[valid_sizes] = log(1.0 / bs);
            log_count[valid_sizes] = log((double)count);
            valid_sizes++;
        }
    }
    
    free(norm);
    
    if (valid_sizes < 3) return 1.0;
    
    return linear_slope(log_inv, log_count, valid_sizes);
}

void write_market_csv(MarketCandle *data, int n, const char *filename) {
    FILE *file = fopen(filename, "w");
    if (!file) return;
    
    fprintf(file, "Timestamp,Price,Volume,Returns,Volatility\n");
    
    for (int i = 0; i < n; i++) {
        struct tm *tm_info = localtime(&data[i].timestamp);
        char time_str[32];
        strftime(time_str, sizeof(time_str), "%Y-%m-%d %H:%M:%S", tm_info);
        
        fprintf(file, "%s,%.6f,%.2f,%.6f,%.6f\n",
                time_str, data[i].price, data[i].volume,
                data[i].returns, data[i].volatility);
    }
    
    fclose(file);
}

void write_summary(MarketCandle *data, int n, double fd_all, double fd_last1k, 
                  double fd_last500, const char *filename) {
    FILE *file = fopen(filename, "w");
    if (!file) return;
    
    fprintf(file, "Metric,Value\n");
    fprintf(file, "Points,%d\n", n);
    fprintf(file, "StartPrice,%.6f\n", data[0].price);
    fprintf(file, "EndPrice,%.6f\n", data[n-1].price);
    
    double total_return = (data[n-1].price - data[0].price) / data[0].price;
    fprintf(file, "TotalReturn,%.6f\n", total_return);
    fprintf(file, "FD_All,%.6f\n", fd_all);
    fprintf(file, "FD_Last1000,%.6f\n", fd_last1k);
    fprintf(file, "FD_Last500,%.6f\n", fd_last500);
    
    fclose(file);
}

void extract_prices(MarketCandle *data, int start, int len, double *prices) {
    for (int i = 0; i < len; i++) {
        prices[i] = data[start + i].price;
    }
}

int main(void) {
    printf("C: Allocating memory for 10,000 candles...\n");
    
    MarketCandle *data = malloc(N_CANDLES * sizeof(MarketCandle));
    if (!data) {
        printf("C: Memory allocation failed\n");
        return 1;
    }
    
    printf("C: Generating 10,000 candles...\n");
    generate_series(data, N_CANDLES, 100.0);
    compute_returns_and_volatility(data, N_CANDLES, 30);
    
    printf("C: Computing fractal dimensions...\n");
    
    /* Extract price arrays for different windows */
    double *all_prices = malloc(N_CANDLES * sizeof(double));
    double *last1k_prices = malloc(1000 * sizeof(double));
    double *last500_prices = malloc(500 * sizeof(double));
    
    extract_prices(data, 0, N_CANDLES, all_prices);
    extract_prices(data, N_CANDLES - 1000, 1000, last1k_prices);
    extract_prices(data, N_CANDLES - 500, 500, last500_prices);
    
    double fd_all = box_counting_fractal_dimension(all_prices, N_CANDLES);
    double fd_last1k = box_counting_fractal_dimension(last1k_prices, 1000);
    double fd_last500 = box_counting_fractal_dimension(last500_prices, 500);
    
    /* Create output directory */
    mkdir("out-c", 0755);
    
    /* Write output files */
    write_market_csv(data, N_CANDLES, "out-c/market_data.csv");
    write_summary(data, N_CANDLES, fd_all, fd_last1k, fd_last500, "out-c/session_summary.csv");
    
    printf("C: Fractal analysis complete. Results:\n");
    printf("C: FD (all):     %.3f\n", fd_all);
    printf("C: FD (last1k):  %.3f\n", fd_last1k);
    printf("C: FD (last500): %.3f\n", fd_last500);
    printf("C: CSV written to ./out-c/\n");
    
    /* Cleanup */
    free(data);
    free(all_prices);
    free(last1k_prices);
    free(last500_prices);
    
    return 0;
}