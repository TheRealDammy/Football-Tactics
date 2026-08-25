using UnityEngine;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Applies a manager profile to the home side and makes personality-specific,
    /// score-aware tactical adjustments during a simulated match.
    /// </summary>
    public sealed class ManagerPersonalityController
    {
        private readonly ManagerProfile profile;
        private int lastChangeMinute = -99;

        public ManagerPersonality Personality => profile.Personality;
        public int TotalTacticalChanges { get; private set; }
        public int MentalityChanges { get; private set; }
        public int PressingChanges { get; private set; }
        public int DefensiveLineChanges { get; private set; }
        public int FormationChanges { get; private set; }
        public int Decisions { get; private set; }
        public int EarlyDecisions { get; private set; }
        public int MidDecisions { get; private set; }
        public int LateDecisions { get; private set; }

        public ManagerPersonalityController(ManagerPersonality personality)
        {
            profile = ManagerProfile.Create(personality);
        }

        public void ApplyInitialTactics(MatchEngine engine)
        {
            engine.SetHomeFormation(profile.PreferredFormation);
            engine.SetHomeMentality(profile.DefaultMentality);
            engine.SetHomePressing(profile.DefaultPressing);
            engine.SetHomeDefensiveLine(profile.DefaultDefensiveLine);
        }

        public void Update(MatchEngine engine)
        {
            int minute = engine.State.Minute;

            if (minute < 10 || minute >= 88)
                return;

            // Keep decisions spread through the match instead of clustering them.
            if (minute - lastChangeMinute < 8)
                return;

            float chance = minute < 30 ? 0.050f : minute < 60 ? 0.050f : 0.060f;
            bool trailing = engine.State.HomeGoals < engine.State.AwayGoals;
            bool leading = engine.State.HomeGoals > engine.State.AwayGoals;

            if (trailing)
                chance += 0.030f;
            else if (leading)
                chance += 0.015f;

            if (Random.value > chance)
                return;

            Decisions++;
            if (minute <= 30) EarlyDecisions++;
            else if (minute <= 60) MidDecisions++;
            else LateDecisions++;

            bool changed = ApplyPersonalityDecision(engine, trailing, leading);

            if (changed)
                lastChangeMinute = minute;
        }

        private bool ApplyPersonalityDecision(MatchEngine engine, bool trailing, bool leading)
        {
            switch (profile.Personality)
            {
                case ManagerPersonality.Possession:
                    // Possession managers prioritise control: pressing/shape first,
                    // with mentality changes used when the score requires them.
                    if (trailing)
                        return TryChanges(engine,
                            () => ChangeMentality(engine, Mentality.Attacking),
                            () => ChangePressing(engine, Pressing.High),
                            () => ChangeFormation(engine, Formation.FourThreeThree));

                    if (leading)
                        return TryChanges(engine,
                            () => ChangePressing(engine, Pressing.Low),
                            () => ChangeMentality(engine, Mentality.Balanced),
                            () => ChangeFormation(engine, Formation.FourTwoThreeOne));

                    return TryChanges(engine,
                        () => ChangePressing(engine, Pressing.Medium),
                        () => ChangeFormation(engine, Formation.FourTwoThreeOne),
                        () => ChangeMentality(engine, Mentality.Balanced));

                case ManagerPersonality.Gegenpress:
                    // Gegenpress managers actively manipulate pressing and the line.
                    if (trailing)
                        return TryChanges(engine,
                            () => ChangePressing(engine, Pressing.High),
                            () => ChangeDefensiveLine(engine, DefensiveLine.High),
                            () => ChangeMentality(engine, Mentality.Attacking));

                    if (leading)
                        return TryChanges(engine,
                            () => ChangePressing(engine, Pressing.Medium),
                            () => ChangeDefensiveLine(engine, DefensiveLine.Normal),
                            () => ChangeMentality(engine, Mentality.Balanced));

                    return TryChanges(engine,
                        () => ChangePressing(engine, Pressing.High),
                        () => ChangeDefensiveLine(engine, DefensiveLine.High),
                        () => ChangeFormation(engine, Formation.FourThreeThree));

                case ManagerPersonality.CounterAttack:
                    if (trailing)
                        return TryChanges(engine,
                            () => ChangeMentality(engine, Mentality.Attacking),
                            () => ChangePressing(engine, Pressing.Medium),
                            () => ChangeDefensiveLine(engine, DefensiveLine.Normal));

                    if (leading)
                        return TryChanges(engine,
                            () => ChangeDefensiveLine(engine, DefensiveLine.Deep),
                            () => ChangePressing(engine, Pressing.Low),
                            () => ChangeMentality(engine, Mentality.Balanced));

                    return TryChanges(engine,
                        () => ChangeDefensiveLine(engine, DefensiveLine.Deep),
                        () => ChangePressing(engine, Pressing.Medium),
                        () => ChangeFormation(engine, Formation.FourTwoThreeOne));

                case ManagerPersonality.Pragmatic:
                    if (leading)
                        return TryChanges(engine,
                            () => ChangeDefensiveLine(engine, DefensiveLine.Deep),
                            () => ChangePressing(engine, Pressing.Low),
                            () => ChangeMentality(engine, Mentality.Defensive));

                    if (trailing && engine.State.Minute >= 55)
                        return TryChanges(engine,
                            () => ChangeMentality(engine, Mentality.Attacking),
                            () => ChangePressing(engine, Pressing.Medium),
                            () => ChangeDefensiveLine(engine, DefensiveLine.Normal));

                    return TryChanges(engine,
                        () => ChangePressing(engine, Pressing.Low),
                        () => ChangeDefensiveLine(engine, DefensiveLine.Deep),
                        () => ChangeMentality(engine, Mentality.Balanced));

                case ManagerPersonality.Direct:
                    if (trailing)
                        return TryChanges(engine,
                            () => ChangeMentality(engine, Mentality.Attacking),
                            () => ChangeFormation(engine, Formation.FourFourTwo),
                            () => ChangePressing(engine, Pressing.High));

                    if (leading)
                        return TryChanges(engine,
                            () => ChangeMentality(engine, Mentality.Balanced),
                            () => ChangePressing(engine, Pressing.Medium),
                            () => ChangeDefensiveLine(engine, DefensiveLine.Normal));

                    return TryChanges(engine,
                        () => ChangeFormation(engine, Formation.FourFourTwo),
                        () => ChangeMentality(engine, Mentality.Attacking),
                        () => ChangePressing(engine, Pressing.Medium));

                default:
                    return TryChanges(engine,
                        () => ChangeMentality(engine, trailing ? Mentality.Attacking : leading ? Mentality.Defensive : Mentality.Balanced),
                        () => ChangePressing(engine, trailing ? Pressing.High : Pressing.Medium),
                        () => ChangeDefensiveLine(engine, leading ? DefensiveLine.Deep : DefensiveLine.Normal));
            }
        }

        private bool TryChanges(MatchEngine engine, params System.Func<bool>[] changes)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                if (changes[i]())
                    return true;
            }

            return false;
        }

        private bool ChangeMentality(MatchEngine engine, Mentality value)
        {
            if (engine.HomeTactics.Mentality == value)
                return false;

            engine.SetHomeMentality(value);
            MentalityChanges++;
            TotalTacticalChanges++;
            return true;
        }

        private bool ChangePressing(MatchEngine engine, Pressing value)
        {
            if (engine.HomeTactics.Pressing == value)
                return false;

            engine.SetHomePressing(value);
            PressingChanges++;
            TotalTacticalChanges++;
            return true;
        }

        private bool ChangeDefensiveLine(MatchEngine engine, DefensiveLine value)
        {
            if (engine.HomeTactics.DefensiveLine == value)
                return false;

            engine.SetHomeDefensiveLine(value);
            DefensiveLineChanges++;
            TotalTacticalChanges++;
            return true;
        }

        private bool ChangeFormation(MatchEngine engine, Formation value)
        {
            if (engine.HomeTactics.Formation == value)
                return false;

            if (!engine.ChangeFormation(value))
                return false;

            FormationChanges++;
            TotalTacticalChanges++;
            return true;
        }
    }
}
