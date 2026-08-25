namespace FootballTactics.Simulation
{
    public enum ManagerPersonality
    {
        Balanced,
        Possession,
        Gegenpress,
        CounterAttack,
        Pragmatic,
        Direct
    }

    public sealed class ManagerProfile
    {
        public ManagerPersonality Personality { get; }
        public Formation PreferredFormation { get; }
        public Mentality DefaultMentality { get; }
        public Pressing DefaultPressing { get; }
        public DefensiveLine DefaultDefensiveLine { get; }

        public ManagerProfile(
            ManagerPersonality personality,
            Formation preferredFormation,
            Mentality defaultMentality,
            Pressing defaultPressing,
            DefensiveLine defaultDefensiveLine)
        {
            Personality = personality;
            PreferredFormation = preferredFormation;
            DefaultMentality = defaultMentality;
            DefaultPressing = defaultPressing;
            DefaultDefensiveLine = defaultDefensiveLine;
        }

        public static ManagerProfile Create(ManagerPersonality personality)
        {
            switch (personality)
            {
                case ManagerPersonality.Possession:
                    return new ManagerProfile(
                        personality,
                        Formation.FourTwoThreeOne,
                        Mentality.Balanced,
                        Pressing.Medium,
                        DefensiveLine.Normal);

                case ManagerPersonality.Gegenpress:
                    return new ManagerProfile(
                        personality,
                        Formation.FourThreeThree,
                        Mentality.Attacking,
                        Pressing.High,
                        DefensiveLine.High);

                case ManagerPersonality.CounterAttack:
                    return new ManagerProfile(
                        personality,
                        Formation.FourTwoThreeOne,
                        Mentality.Balanced,
                        Pressing.Low,
                        DefensiveLine.Deep);

                case ManagerPersonality.Pragmatic:
                    return new ManagerProfile(
                        personality,
                        Formation.FourFourTwo,
                        Mentality.Balanced,
                        Pressing.Low,
                        DefensiveLine.Normal);

                case ManagerPersonality.Direct:
                    return new ManagerProfile(
                        personality,
                        Formation.FourFourTwo,
                        Mentality.Attacking,
                        Pressing.Medium,
                        DefensiveLine.Normal);

                default:
                    return new ManagerProfile(
                        ManagerPersonality.Balanced,
                        Formation.FourThreeThree,
                        Mentality.Balanced,
                        Pressing.Medium,
                        DefensiveLine.Normal);
            }
        }
    }
}