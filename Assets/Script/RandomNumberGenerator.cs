using System;
using System.Collections.Generic;

//prng class
namespace RedGaint.Games.Core
{
    public class RandomNumberGenerator
    {
        private System.Random _random;
        private int _seed;
        private List<int> _history;

        public RandomNumberGenerator(int seed)
        {
            _seed = seed;
            _random = new System.Random(seed);
            _history = new List<int>();
        }

        public int Next(int min, int max)
        {
            int value = _random.Next(min, max);
            _history.Add(value);
            return value;
        }

        public float NextFloat01()
        {
            float value = (float)_random.NextDouble();
            _history.Add((int)(value * 1000000)); 
            return value;
        }

        public GeneratorState GetState()
        {
            return new GeneratorState
            {
                Seed = _seed,
                History = new List<int>(_history)
            };
        }

        public void LoadState(GeneratorState state)
        {
            _seed = state.Seed;
            _history = new List<int>(state.History);
            _random = new System.Random(_seed);

            // Replay RNG to reach current state
            foreach (var value in _history)
            {
                _random.Next(); 
            }
        }

        public struct GeneratorState
        {
            public int Seed;
            public List<int> History;

            public override string ToString()
            {
                return $"Seed: {Seed}, History: [{string.Join(", ", History)}]";
            }
        }
    }

}