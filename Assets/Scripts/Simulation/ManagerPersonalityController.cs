using UnityEngine;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Applies a manager profile to the home side and makes restrained,
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

            if (minute < 12 || minute >= 88)
                return;

            // Avoid tactical thrashing. A manager can only make one
            // personality-driven adjustment every eight minutes.
            if (minute - lastChangeMinute < 8)
                return;

            // Decisions become slightly more likely after half time and when
            // the score state gives the manager a reason to react.
            float chance = minute < 45 ? 0.035f : 0.055f;
            bool trailing = engine.State.HomeGoals < engine.State.AwayGoals;
            bool leading = engine.State.HomeGoals > engine.State.AwayGoals;

            if (trailing)
                chance += 0.035f;
            else if (leading)
                chance += 0.015f;

            if (Random.value > chance)
                return;

            Decisions++;
            if (minute <= 30) EarlyDecisions++;
            else if (minute <= 60) MidDecisions++;
            else LateDecisions++;

            bool changed = false;

            switch (profile.Personality)
            {
                case ManagerPersonality.Possession:
                    changed = ApplyPossession(engine, trailing, leading);
                    break;
                case ManagerPersonality.Gegenpress:
                    changed = ApplyGegenpress(engine, trailing, leading);
                    break;
                case ManagerPersonality.CounterAttack:
                    changed = ApplyCounterAttack(engine, trailing, leading);
                    break;
                case ManagerPersonality.Pragmatic:
                    changed = ApplyPragmatic(engine, trailing, leading);
                    break;
                case ManagerPersonality.Direct:
                    changed = ApplyDirect(engine, trailing, leading);
                    break;
                default:
                    changed = ApplyBalanced(engine, trailing, leading);
                    break;
            }

            if (changed)
                lastChangeMinute = minute;
        }

        private bool ApplyPossession(MatchEngine engine, bool trailing, bool leading)
        {
            if (trailing)
                return ChangeMentality(engine, Mentality.Attacking);

            if (leading)
                return ChangePressing(engine, Pressing.Low);

            return ChangeFormation(engine, Formation.FourTwoThreeOne);
        }

        private bool ApplyGegenpress(MatchEngine engine, bool trailing, bool leading)
        {
            if (trailing)
                return ChangePressing(engine, Pressing.High);

            if (leading)
                return ChangeMentality(engine, Mentality.Balanced);

            return ChangeDefensiveLine(engine, DefensiveLine.High);
        }

        private bool ApplyCounterAttack(MatchEngine engine, bool trailing, bool leading)
        {
            if (trailing)
                return ChangeMentality(engine, Mentality.Attacking);

            if (leading)
                return ChangeDefensiveLine(engine, DefensiveLine.Deep);

            return ChangePressing(engine, Pressing.Medium);
        }

        private bool ApplyPragmatic(MatchEngine engine, bool trailing, bool leading)
        {
            if (leading)
                return ChangeDefensiveLine(engine, DefensiveLine.Deep);

            if (trailing && engine.State.Minute >= 60)
                return ChangeMentality(engine, Mentality.Attacking);

            return ChangePressing(engine, Pressing.Medium);
        }

        private bool ApplyDirect(MatchEngine engine, bool trailing, bool leading)
        {
            if (trailing)
                return ChangeMentality(engine, Mentality.Attacking);

            if (leading)
                return ChangeMentality(engine, Mentality.Balanced);

            return ChangeFormation(engine, Formation.FourFourTwo);
        }

        private bool ApplyBalanced(MatchEngine engine, bool trailing, bool leading)
        {
            if (trailing && engine.State.Minute >= 60)
                return ChangeMentality(engine, Mentality.Attacking);

            if (leading && engine.State.Minute >= 70)
                return ChangeMentality(engine, Mentality.Defensive);

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
