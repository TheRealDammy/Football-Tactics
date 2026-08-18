using System.Collections.Generic;

namespace FootballTactics.Simulation
{
    public class MatchState
    {
        private readonly List<MatchEvent> events = new();

        public int Minute { get; private set; }

        public int HomeGoals { get; private set; }
        public int AwayGoals { get; private set; }

        public int HomeShots { get; private set; }
        public int AwayShots { get; private set; }

        public float HomeXG { get; private set; }
        public float AwayXG { get; private set; }

        public float HomePossession { get; private set; }
        public float AwayPossession => 100f - HomePossession;

        public IReadOnlyList<MatchEvent> Events => events;

        public MatchState()
        {
            Reset();
        }

        public void Reset()
        {
            Minute = 0;

            HomeGoals = 0;
            AwayGoals = 0;

            HomeShots = 0;
            AwayShots = 0;

            HomeXG = 0f;
            AwayXG = 0f;

            HomePossession = 50f;

            events.Clear();
        }

        public void AdvanceMinute()
        {
            if (Minute < 90)
            {
                Minute++;
            }
        }

        public void AddHomeShot(float xG)
        {
            HomeShots++;
            HomeXG += xG;
        }

        public void AddAwayShot(float xG)
        {
            AwayShots++;
            AwayXG += xG;
        }

        public void HomeScores()
        {
            HomeGoals++;
        }

        public void AwayScores()
        {
            AwayGoals++;
        }

        public void SetPossession(float homePossession)
        {
            HomePossession = UnityEngine.Mathf.Clamp(
                homePossession,
                0f,
                100f);
        }

        public void AddEvent(string description)
        {
            events.Add(
                new MatchEvent(
                    Minute,
                    description));
        }
    }
}