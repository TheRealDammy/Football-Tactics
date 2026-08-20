using System.Collections.Generic;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public static class TestSquadFactory
    {
        public static Team Create(
            SquadArchetype archetype,
            string teamName)
        {
            return archetype switch
            {
                SquadArchetype.Possession =>
                    CreatePossessionSquad(teamName),

                SquadArchetype.WideAttack =>
                    CreateWideAttackSquad(teamName),

                SquadArchetype.Direct =>
                    CreateDirectSquad(teamName),

                _ =>
                    TeamFactory.CreateHomeTeam()
            };
        }

        private static Team CreatePossessionSquad(
            string teamName)
        {
            return new Team(
                teamName,
                new List<Player>
                {
                    new(
                        "Keeper",
                        PlayerPosition.Goalkeeper,
                        12, 76, 74, 58, 94,
                        PlayerRole.Goalkeeper),

                    new(
                        "Left Back",
                        PlayerPosition.Defender,
                        42, 76, 78, 80, 94,
                        PlayerRole.FullBack),

                    new(
                        "Centre Back",
                        PlayerPosition.Defender,
                        30, 84, 80, 68, 94,
                        PlayerRole.CentreBack),

                    new(
                        "Centre Back 2",
                        PlayerPosition.Defender,
                        28, 83, 82, 69, 94,
                        PlayerRole.Sweeper),

                    new(
                        "Right Back",
                        PlayerPosition.Defender,
                        41, 75, 79, 81, 94,
                        PlayerRole.FullBack),

                    new(
                        "DM",
                        PlayerPosition.Midfielder,
                        54, 75, 86, 70, 93,
                        PlayerRole.DefensiveMidfielder),

                    new(
                        "CM",
                        PlayerPosition.Midfielder,
                        67, 66, 90, 72, 93,
                        PlayerRole.CentralMidfielder),

                    new(
                        "Playmaker",
                        PlayerPosition.Midfielder,
                        79, 55, 94, 78, 92,
                        PlayerRole.Playmaker),

                    new(
                        "Left Wing",
                        PlayerPosition.Attacker,
                        78, 35, 82, 89, 92,
                        PlayerRole.Winger),

                    new(
                        "Striker",
                        PlayerPosition.Attacker,
                        88, 29, 76, 82, 91,
                        PlayerRole.Striker),

                    new(
                        "Right Wing",
                        PlayerPosition.Attacker,
                        80, 34, 84, 90, 92,
                        PlayerRole.Winger)
                },
                new List<Player>());
        }

        private static Team CreateWideAttackSquad(
            string teamName)
        {
            return new Team(
                teamName,
                new List<Player>
                {
                    new(
                        "Keeper",
                        PlayerPosition.Goalkeeper,
                        10, 75, 65, 60, 94,
                        PlayerRole.Goalkeeper),

                    new(
                        "Left Back",
                        PlayerPosition.Defender,
                        55, 72, 74, 88, 94,
                        PlayerRole.FullBack),

                    new(
                        "LCB",
                        PlayerPosition.Defender,
                        30, 80, 64, 72, 94,
                        PlayerRole.CentreBack),

                    new(
                        "RCB",
                        PlayerPosition.Defender,
                        29, 81, 65, 70, 94,
                        PlayerRole.CentreBack),

                    new(
                        "Right Back",
                        PlayerPosition.Defender,
                        56, 71, 75, 89, 94,
                        PlayerRole.FullBack),

                    new(
                        "Box Runner",
                        PlayerPosition.Midfielder,
                        78, 59, 72, 86, 91,
                        PlayerRole.BoxToBox),

                    new(
                        "CM",
                        PlayerPosition.Midfielder,
                        69, 64, 75, 78, 92,
                        PlayerRole.CentralMidfielder),

                    new(
                        "Winger Mid",
                        PlayerPosition.Midfielder,
                        72, 55, 78, 83, 91,
                        PlayerRole.BoxToBox),

                    new(
                        "Left Winger",
                        PlayerPosition.Attacker,
                        91, 25, 75, 96, 91,
                        PlayerRole.Winger),

                    new(
                        "Striker",
                        PlayerPosition.Attacker,
                        89, 28, 71, 88, 91,
                        PlayerRole.Striker),

                    new(
                        "Right Winger",
                        PlayerPosition.Attacker,
                        90, 26, 76, 95, 91,
                        PlayerRole.Winger)
                },
                new List<Player>());
        }

        private static Team CreateDirectSquad(
            string teamName)
        {
            return new Team(
                teamName,
                new List<Player>
                {
                    new(
                        "Keeper",
                        PlayerPosition.Goalkeeper,
                        10, 79, 58, 57, 96,
                        PlayerRole.Goalkeeper),

                    new(
                        "Left Back",
                        PlayerPosition.Defender,
                        32, 82, 58, 74, 95,
                        PlayerRole.LineHolding),

                    new(
                        "LCB",
                        PlayerPosition.Defender,
                        25, 88, 55, 64, 95,
                        PlayerRole.LineHolding),

                    new(
                        "RCB",
                        PlayerPosition.Defender,
                        24, 87, 54, 65, 95,
                        PlayerRole.CentreBack),

                    new(
                        "Right Back",
                        PlayerPosition.Defender,
                        34, 81, 58, 76, 95,
                        PlayerRole.LineHolding),

                    new(
                        "CM",
                        PlayerPosition.Midfielder,
                        62, 71, 67, 76, 94,
                        PlayerRole.BoxToBox),

                    new(
                        "DM",
                        PlayerPosition.Midfielder,
                        48, 79, 70, 67, 94,
                        PlayerRole.DefensiveMidfielder),

                    new(
                        "CM 2",
                        PlayerPosition.Midfielder,
                        68, 69, 65, 74, 94,
                        PlayerRole.CentralMidfielder),

                    new(
                        "Left Winger",
                        PlayerPosition.Attacker,
                        78, 38, 68, 91, 92,
                        PlayerRole.Winger),

                    new(
                        "Striker",
                        PlayerPosition.Attacker,
                        92, 27, 66, 83, 91,
                        PlayerRole.Striker),

                    new(
                        "Striker 2",
                        PlayerPosition.Attacker,
                        89, 31, 62, 80, 91,
                        PlayerRole.Striker)
                },
                new List<Player>());
        }
    }
}