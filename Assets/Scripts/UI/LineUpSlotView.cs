using FootballTactics.Simulation;
using FootballTactics.Teams;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    public sealed class LineupSlotView
    {
        public FormationSlot Slot { get; }
        public Button Button { get; }
        public Player Player { get; private set; }

        public LineupSlotView(
            FormationSlot slot,
            Button button)
        {
            Slot = slot;
            Button = button;
        }

        public void SetPlayer(Player player)
        {
            Player = player;

            if (player == null)
            {
                Button.text =
                    $"{Slot.Id}\nEMPTY";

                return;
            }

            Button.text =
                $"{Slot.Id}\n" +
                $"{player.Name}\n" +
                $"{player.Role}";
        }
    }
}