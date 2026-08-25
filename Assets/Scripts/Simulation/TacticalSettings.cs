using UnityEngine;

namespace FootballTactics.Simulation
{
    public enum Mentality
    {
        Defensive,
        Balanced,
        Attacking
    }

    public enum Pressing
    {
        Low,
        Medium,
        High
    }

    public enum DefensiveLine
    {
        Deep,
        Normal,
        High
    }

    public class TacticalSettings
    {
        public Formation Formation { get; set; } = Formation.FourThreeThree;
        public Mentality Mentality { get; set; } = Mentality.Balanced;
        public Pressing Pressing { get; set; } = Pressing.Medium;
        public DefensiveLine DefensiveLine { get; set; } = DefensiveLine.Normal;

        public float GetAttackModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 0.90f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 1.12f,
                _ => 1.00f
            };
        }

        public float GetDefenceModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 1.12f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 0.90f,
                _ => 1.00f
            };
        }

        public float GetPossessionModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 0.95f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 1.03f,
                _ => 1.00f
            };
        }

        public float GetPressingModifier()
        {
            return Pressing switch
            {
                Pressing.Low => 0.90f,
                Pressing.Medium => 1.00f,
                Pressing.High => 1.12f,
                _ => 1.00f
            };
        }

        public float GetFitnessDrain()
        {
            return Pressing switch
            {
                Pressing.Low => 0.6f,
                Pressing.Medium => 1.0f,
                Pressing.High => 1.5f,
                _ => 1.0f
            };
        }

        public float GetDefensiveLineRisk()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.85f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.18f,
                _ => 1.00f
            };
        }

        public float GetCounterAttackVulnerability()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.75f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.20f,
                _ => 1.00f
            };
        }

        public float GetTerritoryModifier()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.94f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.06f,
                _ => 1.00f
            };
        }
    }

    public sealed class ManagerPersonalityController
    {
        public ManagerPersonality Personality { get; }
        private int lastChangeMinute = -10;

        public ManagerPersonalityController(ManagerPersonality personality)
        {
            Personality = personality;
        }

        public void ApplyInitialTactics(MatchEngine engine)
        {
            ManagerProfile profile = ManagerProfile.Create(Personality);
            ApplyDirect(
                engine,
                profile.PreferredFormation,
                profile.DefaultMentality,
                profile.DefaultPressing,
                profile.DefaultDefensiveLine,
                false);
        }

        public void Update(MatchEngine engine)
        {
            int minute = engine.State.Minute;
            if (minute - lastChangeMinute < 8)
                return;

            bool losing = engine.State.AwayGoals < engine.State.HomeGoals;
            bool winning = engine.State.AwayGoals > engine.State.HomeGoals;

            switch (Personality)
            {
                case ManagerPersonality.Gegenpress:
                    if (losing && minute >= 55)
                        Apply(engine, Mentality.Attacking, Pressing.High, DefensiveLine.High);
                    else if (winning && minute >= 75)
                        Apply(engine, Mentality.Attacking, Pressing.High, DefensiveLine.Normal);
                    break;
                case ManagerPersonality.CounterAttack:
                    if (losing && minute >= 60)
                        Apply(engine, Mentality.Attacking, Pressing.Medium, DefensiveLine.Normal);
                    else if (winning && minute >= 70)
                        Apply(engine, Mentality.Defensive, Pressing.Low, DefensiveLine.Deep);
                    break;
                case ManagerPersonality.Pragmatic:
                    if (winning && minute >= 55)
                        Apply(engine, Mentality.Defensive, Pressing.Low, DefensiveLine.Deep);
                    else if (losing && minute >= 70)
                        Apply(engine, Mentality.Attacking, Pressing.Medium, DefensiveLine.Normal);
                    break;
                case ManagerPersonality.Possession:
                    if (losing && minute >= 55)
                        Apply(engine, Mentality.Attacking, Pressing.Medium, DefensiveLine.Normal);
                    else if (winning && minute >= 70)
                        Apply(engine, Mentality.Balanced, Pressing.Medium, DefensiveLine.Normal);
                    break;
                case ManagerPersonality.Direct:
                    if (losing && minute >= 55)
                        Apply(engine, Mentality.Attacking, Pressing.High, DefensiveLine.Normal);
                    else if (winning && minute >= 75)
                        Apply(engine, Mentality.Balanced, Pressing.Medium, DefensiveLine.Normal);
                    break;
                default:
                    if (losing && minute >= 65)
                        Apply(engine, Mentality.Attacking, Pressing.Medium, DefensiveLine.High);
                    else if (winning && minute >= 75)
                        Apply(engine, Mentality.Defensive, Pressing.Low, DefensiveLine.Deep);
                    break;
            }
        }

        private void Apply(MatchEngine engine, Mentality mentality, Pressing pressing, DefensiveLine line)
        {
            if (engine.AwayTactics.Mentality == mentality &&
                engine.AwayTactics.Pressing == pressing &&
                engine.AwayTactics.DefensiveLine == line)
                return;

            engine.SetAwayMentality(mentality);
            engine.SetAwayPressing(pressing);
            engine.SetAwayDefensiveLine(line);
            lastChangeMinute = engine.State.Minute;
        }

        private void ApplyDirect(MatchEngine engine, Formation formation, Mentality mentality, Pressing pressing, DefensiveLine line, bool createEvent)
        {
            engine.SetAwayFormation(formation, createEvent);
            engine.SetAwayMentality(mentality, createEvent);
            engine.SetAwayPressing(pressing, createEvent);
            engine.SetAwayDefensiveLine(line, createEvent);
        }
    }
}