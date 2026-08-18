namespace FootballTactics.Teams
{
    public enum PlayerPosition
    {
        Goalkeeper,
        Defender,
        Midfielder,
        Attacker
    }

    public enum PlayerRole
    {
        Goalkeeper,
        Sweeper,
        LineHolding,

        CentreBack,
        FullBack,

        CentralMidfielder,
        Playmaker,
        DefensiveMidfielder,
        BoxToBox,

        Striker,
        Winger
    }

    public class Player
    {
        public string Name { get; }

        public PlayerPosition Position { get; }

        public PlayerRole Role { get; private set; }

        public int Attack { get; }
        public int Defence { get; }
        public int Passing { get; }
        public int Pace { get; }

        public int Fitness { get; private set; }

        public Player(
            string name,
            PlayerPosition position,
            int attack,
            int defence,
            int passing,
            int pace,
            int fitness,
            PlayerRole role)
        {
            Name = name;
            Position = position;

            Attack = attack;
            Defence = defence;
            Passing = passing;
            Pace = pace;

            Fitness = fitness;
            Role = role;
        }

        public void SetRole(PlayerRole role)
        {
            Role = role;
        }

        public void ReduceFitness(int amount)
        {
            Fitness = System.Math.Max(
                0,
                Fitness - amount);
        }
    }
}