using System.Collections.Generic;

namespace FootballTactics.Simulation
{
    public enum TacticalSituationType
    {
        OppositionPressing,
        TiredPlayer,
        SpaceBehindDefence,
        OpponentDeepBlock,
        ProtectLead
    }

    public sealed class TacticalSituationOption
    {
        public string Id { get; }

        public string Title { get; }

        public string Description { get; }

        public float PossessionModifier { get; }

        public float ChanceModifier { get; }

        public float FatigueModifier { get; }

        public float CounterAttackModifier { get; }

        public TacticalSituationOption(
            string id,
            string title,
            string description,
            float possessionModifier,
            float chanceModifier,
            float fatigueModifier,
            float counterAttackModifier)
        {
            Id = id;
            Title = title;
            Description = description;

            PossessionModifier = possessionModifier;
            ChanceModifier = chanceModifier;
            FatigueModifier = fatigueModifier;
            CounterAttackModifier = counterAttackModifier;
        }
    }

    public sealed class TacticalSituation
    {
        public TacticalSituationType Type { get; }

        public string Title { get; }

        public string Description { get; }

        public IReadOnlyList<TacticalSituationOption> Options { get; }

        public TacticalSituation(
            TacticalSituationType type,
            string title,
            string description,
            IReadOnlyList<TacticalSituationOption> options)
        {
            Type = type;
            Title = title;
            Description = description;
            Options = options;
        }
    }
}