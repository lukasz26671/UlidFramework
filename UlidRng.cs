using System;
using System.Security.Cryptography;

namespace UlidFramework
{
    internal static class UlidRng
    {
        private const int MaxRand = 0x7fff;
        private static int _lastValue;
        private static int _generations;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly Random _random = new Random(); // Default Random
        public static int GetMonotonicRandomInteger(int minValue, int maxValue = MaxRand)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            // Calculate the range
            int range = maxValue - minValue + 1;

            // Generate a random integer
            byte[] randomBytes = new byte[4];
            _rng.GetBytes(randomBytes);
            int randomValue = BitConverter.ToInt32(randomBytes, 0) & 0x7FFFFFFF; // Ensure non-negative

            // Scale the random value to the range
            int scaledValue = minValue + (randomValue % range);

            // Ensure the value is monotonic
            int result = Math.Max(scaledValue, _lastValue + 1);
            result = Math.Min(result, maxValue);

            // Update the last value
            _lastValue = result;

            return result;
        }
        public static int GetRandomInteger(int minValue, int maxValue = MaxRand)
        {
            if (_lastValue > MaxRand) _lastValue = 0;
            
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            // Generate a random integer within the range
            int result = _random.Next(minValue, maxValue + 1);
            result += _generations++;

            return result;
        }
    }
}