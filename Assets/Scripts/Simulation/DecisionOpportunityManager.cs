using System.Collections.Generic;
using UnityEngine;

namespace FootballTactics.Simulation
{
    public class DecisionOpportunityManager
    {
        private readonly Queue<int> plannedMinutes = new();

        private int cooldownUntil;

        public DecisionOpportunityManager()
        {
            GeneratePlan();
        }

        private void GeneratePlan()
        {
            plannedMinutes.Clear();

            // Every match gets 2–5 decisions.
            int count = Random.Range(2, 6);

            List<int> pool = new()
            {
                Random.Range(10,20),
                Random.Range(22,32),
                Random.Range(36,45),
                Random.Range(50,60),
                Random.Range(62,74),
                Random.Range(76,89)
            };

            Shuffle(pool);

            pool.Sort();

            for (int i = 0; i < count; i++)
                plannedMinutes.Enqueue(pool[i]);
        }

        public bool ShouldOfferDecision(MatchEngine engine)
        {
            if (engine == null)
                return false;

            int minute = engine.State.Minute;

            if (minute < cooldownUntil)
                return false;

            if (plannedMinutes.Count == 0)
                return false;

            if (minute < plannedMinutes.Peek())
                return false;

            plannedMinutes.Dequeue();

            cooldownUntil = minute + Random.Range(4, 8);

            return true;
        }

        private void Shuffle(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count);

                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}