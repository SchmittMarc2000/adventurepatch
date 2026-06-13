using BrilliantSkies.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    public class AP_RandomSelection<T>
    {
        public delegate double FnGetLikelihood(T thing);

        private static readonly System.Random rng = new System.Random();

        public List<T> Things = new List<T>();

        public double NullChance { get; set; }

        public void Add(T thing)
        {
            Things.Add(thing);
        }

        public void Remove(T thing)
        {
            Things.Remove(thing);
        }

        public T Select(FnGetLikelihood fn)
        {
            AdvLogger.LogInfo($"[AP_RandomSelection] Selecting from {Things.Count} items");

            double[] weights = new double[Things.Count];
            double totalWeight = NullChance;

            for (int i = 0; i < Things.Count; i++)
            {
                try
                {
                    weights[i] = fn(Things[i]);

                    if (double.IsNaN(weights[i]) || double.IsInfinity(weights[i]))
                    {
                        AdvLogger.LogInfo($"[AP_RandomSelection] Invalid weight for item {i}: {weights[i]}");
                        weights[i] = 0;
                    }

                    totalWeight += weights[i];
                }
                catch (Exception ex)
                {
                    AdvLogger.LogInfo($"[AP_RandomSelection] Weight calculation failed for item {i}: {ex}");
                    weights[i] = 0;
                }
            }

            AdvLogger.LogInfo($"[AP_RandomSelection] Total weight = {totalWeight}");

            if (totalWeight <= NullChance)
            {
                AdvLogger.LogInfo("[AP_RandomSelection] No valid selection possible");
                return default(T);
            }

            double roll = rng.NextDouble() * totalWeight;

            AdvLogger.LogInfo($"[AP_RandomSelection] Roll = {roll}");

            double cumulative = NullChance;

            if (roll < cumulative)
            {
                AdvLogger.LogInfo("[AP_RandomSelection] Selected NULL");
                return default(T);
            }

            for (int i = 0; i < Things.Count; i++)
            {
                cumulative += weights[i];

                if (roll < cumulative)
                {
                    AdvLogger.LogInfo($"[AP_RandomSelection] Selected index {i}");
                    return Things[i];
                }
            }

            AdvLogger.LogInfo("[AP_RandomSelection] Fell through selection loop");
            return default(T);
        }
    }
}
