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

            UpdateButtonText();
        }

        public void UpdateButtonText()
        {
            if (Player == null)
            {
                Button.text =
                    $"{Slot.Id}\nEMPTY";

                return;
            }

            Button.text =
                $"{Player.Name}\n" +
                $"{FormatRole(Player.Role)}\n" +
                $"FIT {Player.Fitness}%";
        }

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                Button.AddToClassList(
                    "selected-lineup-player");
            }
            else
            {
                Button.RemoveFromClassList(
                    "selected-lineup-player");
            }
        }

        private static string FormatRole(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Goalkeeper => "GK",
                PlayerRole.Sweeper => "SW",
                PlayerRole.LineHolding => "LH",

                PlayerRole.CentreBack => "CB",
                PlayerRole.FullBack => "FB",

                PlayerRole.CentralMidfielder => "CM",
                PlayerRole.Playmaker => "PM",
                PlayerRole.DefensiveMidfielder => "DM",
                PlayerRole.BoxToBox => "B2B",

                PlayerRole.Striker => "ST",
                PlayerRole.Winger => "WG",

                _ => "?"
            };
        }
    }
}