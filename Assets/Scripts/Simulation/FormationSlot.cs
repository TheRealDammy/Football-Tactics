using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public enum FormationArea
    {
        Goalkeeper,

        LeftBack,
        CentreBack,
        RightBack,

        LeftMidfield,
        CentreMidfield,
        RightMidfield,

        DefensiveMidfield,
        AttackingMidfield,

        LeftWing,
        Striker,
        RightWing
    }

    public sealed class FormationSlot
    {
        public string Id { get; }

        public FormationArea Area { get; }

        public PlayerPosition RequiredPosition { get; }

        // Position on the pitch as percentages.
        public float X { get; }

        public float Y { get; }

        public FormationSlot(
            string id,
            FormationArea area,
            PlayerPosition requiredPosition,
            float x,
            float y)
        {
            Id = id;
            Area = area;
            RequiredPosition = requiredPosition;

            X = x;
            Y = y;
        }
    }
}