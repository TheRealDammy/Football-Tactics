using FootballTactics.Teams;

namespace FootballTactics.UI
{
    public sealed class PlayerButtonData
    {
        public Player Player { get; }

        public PlayerButtonData(Player player)
        {
            Player = player;
        }
    }
}