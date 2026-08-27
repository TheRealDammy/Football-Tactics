using System.Collections.Generic;
using UnityEngine;

namespace FootballTactics.Simulation
{
    public static class OpponentTacticalAI
    {
        private static readonly Dictionary<MatchEngine, ManagerProfile> profiles =
            new Dictionary<MatchEngine, ManagerProfile>();

        public static ManagerProfile GetProfile(MatchEngine engine)
        {
            if (engine == null)
                return ManagerProfile.Create(ManagerPersonality.Balanced);

            if (!profiles.TryGetValue(engine, out ManagerProfile profile))
            {
                ManagerPersonality personality =
                    (ManagerPersonality)Random.Range(
                        0,
                        System.Enum.GetValues(typeof(ManagerPersonality)).Length);

                profile = ManagerProfile.Create(personality);
                profiles.Add(engine, profile);
            }

            return profile;
        }

        public static void Update(MatchEngine engine)
        {
            if (engine == null || engine.State.Minute < 10)
                return;

            if (engine.State.Minute % 5 != 0)
                return;

            ManagerProfile profile = GetProfile(engine);

            int goalDifference =
                engine.State.AwayGoals - engine.State.HomeGoals;

            float fitness =
                engine.AwayTeam.GetAverageFitness(engine.AwayLineup);

            if (goalDifference <= -2)
            {
                ApplyLosingByTwo(engine, fitness, profile);
                return;
            }

            if (goalDifference == -1)
            {
                ApplyLosing(engine, fitness, profile);
                return;
            }

            if (goalDifference >= 2 && engine.State.Minute >= 60)
            {
                ApplyWinningByTwo(engine, fitness, profile);
                return;
            }

            if (goalDifference == 1 && engine.State.Minute >= 70)
            {
                ApplyWinningLate(engine, fitness, profile);
                return;
            }

            if (goalDifference == 0 && engine.State.Minute >= 45)
            {
                ApplyDrawing(engine, fitness, profile);
            }
        }

        private static void ApplyLosingByTwo(
            MatchEngine engine,
            float fitness,
            ManagerProfile profile)
        {
            engine.SetAwayMentality(
                profile.Personality == ManagerPersonality.Pragmatic
                    ? Mentality.Balanced
                    : Mentality.Attacking,
                false);

            engine.SetAwayPressing(
                profile.Personality == ManagerPersonality.Gegenpress && fitness > 45f
                    ? Pressing.High
                    : fitness > 55f
                        ? Pressing.High
                        : Pressing.Medium,
                false);

            engine.SetAwayDefensiveLine(
                profile.Personality == ManagerPersonality.Pragmatic
                    ? DefensiveLine.Normal
                    : DefensiveLine.High,
                false);

            engine.SetAwayFormation(
                profile.Personality == ManagerPersonality.Direct
                    ? Formation.FourFourTwo
                    : Formation.FourThreeThree,
                false);
        }

        private static void ApplyLosing(
            MatchEngine engine,
            float fitness,
            ManagerProfile profile)
        {
            engine.SetAwayMentality(
                profile.Personality == ManagerPersonality.Pragmatic
                    ? Mentality.Balanced
                    : Mentality.Attacking,
                false);

            Pressing pressing = fitness > 55f
                ? Pressing.High
                : Pressing.Medium;

            if (profile.Personality == ManagerPersonality.CounterAttack ||
                profile.Personality == ManagerPersonality.Pragmatic)
            {
                pressing = Pressing.Medium;
            }

            engine.SetAwayPressing(pressing, false);

            engine.SetAwayDefensiveLine(
                profile.Personality == ManagerPersonality.CounterAttack
                    ? DefensiveLine.Deep
                    : DefensiveLine.Normal,
                false);
        }

        private static void ApplyWinningByTwo(
            MatchEngine engine,
            float fitness,
            ManagerProfile profile)
        {
            engine.SetAwayMentality(
                profile.Personality == ManagerPersonality.Gegenpress
                    ? Mentality.Balanced
                    : Mentality.Defensive,
                false);

            engine.SetAwayPressing(
                profile.Personality == ManagerPersonality.Gegenpress && fitness > 65f
                    ? Pressing.Medium
                    : Pressing.Low,
                false);

            engine.SetAwayDefensiveLine(
                profile.Personality == ManagerPersonality.CounterAttack
                    ? DefensiveLine.Normal
                    : DefensiveLine.Deep,
                false);

            engine.SetAwayFormation(
                profile.Personality == ManagerPersonality.Possession
                    ? Formation.FourTwoThreeOne
                    : Formation.FourFourTwo,
                false);
        }

        private static void ApplyWinningLate(
            MatchEngine engine,
            float fitness,
            ManagerProfile profile)
        {
            engine.SetAwayMentality(
                profile.Personality == ManagerPersonality.Gegenpress && fitness > 60f
                    ? Mentality.Balanced
                    : Mentality.Defensive,
                false);

            Pressing pressing = fitness > 45f
                ? Pressing.Medium
                : Pressing.Low;

            if (profile.Personality == ManagerPersonality.Pragmatic ||
                profile.Personality == ManagerPersonality.CounterAttack)
            {
                pressing = Pressing.Low;
            }

            engine.SetAwayPressing(pressing, false);
            engine.SetAwayDefensiveLine(DefensiveLine.Deep, false);
        }

        private static void ApplyDrawing(
            MatchEngine engine,
            float fitness,
            ManagerProfile profile)
        {
            switch (profile.Personality)
            {
                case ManagerPersonality.Gegenpress:
                    engine.SetAwayMentality(Mentality.Attacking, false);
                    engine.SetAwayPressing(
                        fitness > 55f ? Pressing.High : Pressing.Medium,
                        false);
                    engine.SetAwayDefensiveLine(DefensiveLine.High, false);
                    break;

                case ManagerPersonality.Possession:
                    engine.SetAwayMentality(Mentality.Balanced, false);
                    engine.SetAwayPressing(Pressing.Medium, false);
                    engine.SetAwayDefensiveLine(DefensiveLine.Normal, false);
                    engine.SetAwayFormation(Formation.FourTwoThreeOne, false);
                    break;

                case ManagerPersonality.CounterAttack:
                    engine.SetAwayMentality(Mentality.Balanced, false);
                    engine.SetAwayPressing(Pressing.Medium, false);
                    engine.SetAwayDefensiveLine(DefensiveLine.Deep, false);
                    break;

                case ManagerPersonality.Pragmatic:
                    engine.SetAwayMentality(Mentality.Balanced, false);
                    engine.SetAwayPressing(Pressing.Low, false);
                    engine.SetAwayDefensiveLine(DefensiveLine.Deep, false);
                    break;

                case ManagerPersonality.Direct:
                    engine.SetAwayMentality(Mentality.Attacking, false);
                    engine.SetAwayPressing(Pressing.Medium, false);
                    engine.SetAwayDefensiveLine(DefensiveLine.Normal, false);
                    engine.SetAwayFormation(Formation.FourFourTwo, false);
                    break;

                default:
                    engine.SetAwayMentality(Mentality.Balanced, false);
                    engine.SetAwayPressing(
                        fitness > 55f ? Pressing.Medium : Pressing.Low,
                        false);
                    engine.SetAwayDefensiveLine(DefensiveLine.Normal, false);
                    break;
            }
        }
    }
}
