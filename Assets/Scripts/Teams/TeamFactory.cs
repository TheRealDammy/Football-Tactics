using System.Collections.Generic;

namespace FootballTactics.Teams
{
    public static class TeamFactory
    {
        public static Team CreateHomeTeam()
        {
            return new Team(
                "Riverside FC",

                new List<Player>
                {
                    new("James", PlayerPosition.Goalkeeper, 10, 75, 60, 55, 90, PlayerRole.Sweeper),

                    new("Cole", PlayerPosition.Defender, 30, 78, 60, 72, 92, PlayerRole.FullBack),
                    new("Davies", PlayerPosition.Defender, 28, 82, 61, 68, 94, PlayerRole.CentreBack),
                    new("Morgan", PlayerPosition.Defender, 25, 80, 65, 70, 91, PlayerRole.CentreBack),
                    new("Williams", PlayerPosition.Defender, 35, 74, 67, 81, 95, PlayerRole.FullBack),

                    new("Lewis", PlayerPosition.Midfielder, 60, 65, 82, 72, 93,PlayerRole.Playmaker),
                    new("Thomas", PlayerPosition.Midfielder, 64, 70, 78, 69, 92, PlayerRole.BoxToBox),
                    new("Ali", PlayerPosition.Midfielder, 74, 57, 84, 80, 90, PlayerRole.BoxToBox),

                    new("Johnson", PlayerPosition.Attacker, 82, 30, 70, 88, 91, PlayerRole.Winger),
                    new("Brown", PlayerPosition.Attacker, 86, 24, 73, 83, 89, PlayerRole.Striker),
                    new("Williams Jr", PlayerPosition.Attacker, 79, 28, 76, 92, 94, PlayerRole.Winger)
                },

                new List<Player>
                {
                    new("Mills", PlayerPosition.Goalkeeper, 12, 72, 58, 61, 95, PlayerRole.Goalkeeper),

                    new("Clarke", PlayerPosition.Defender, 32, 76, 65, 80, 96, PlayerRole.FullBack),
                    new("Hughes", PlayerPosition.Defender, 29, 74, 69, 75, 94, PlayerRole.CentreBack),

                    new("Young", PlayerPosition.Midfielder, 70, 61, 79, 85, 97, PlayerRole.BoxToBox),
                    new("King", PlayerPosition.Midfielder, 58, 73, 75, 70, 98, PlayerRole.BoxToBox),

                    new("Green", PlayerPosition.Attacker, 84, 25, 71, 94, 96, PlayerRole.Winger)
                });
        }

        public static Team CreateAwayTeam()
        {
            return new Team(
                "City United",
                new List<Player>
                {
                    new("Taylor", PlayerPosition.Goalkeeper, 12, 78, 62, 57, 92, PlayerRole.LineHolding),

                    new("White", PlayerPosition.Defender, 28, 80, 61, 74, 90, PlayerRole.FullBack),
                    new("Hall", PlayerPosition.Defender, 22, 84, 63, 66, 93, PlayerRole.CentreBack),
                    new("Green", PlayerPosition.Defender, 29, 77, 67, 71, 92, PlayerRole.CentreBack),
                    new("Scott", PlayerPosition.Defender, 34, 73, 70, 84, 95, PlayerRole.FullBack),

                    new("Adams", PlayerPosition.Midfielder, 58, 69, 80, 70, 91, PlayerRole.CentralMidfielder),
                    new("Baker", PlayerPosition.Midfielder, 68, 71, 81, 68, 89, PlayerRole.Playmaker),
                    new("King", PlayerPosition.Midfielder, 76, 54, 87, 82, 93, PlayerRole.BoxToBox),

                    new("Clarke", PlayerPosition.Attacker, 81, 29, 74, 89, 92, PlayerRole.Striker),
                    new("Young", PlayerPosition.Attacker, 84, 27, 78, 86, 91, PlayerRole.Winger),
                    new("Walker", PlayerPosition.Attacker, 77, 32, 75, 90, 94, PlayerRole.Winger)
                },

                new List<Player>
                {
                    new("Jerry", PlayerPosition.Goalkeeper, 12, 72, 58, 61, 95, PlayerRole.Goalkeeper),

                    new("Kevin", PlayerPosition.Defender, 32, 76, 65, 80, 96, PlayerRole.CentreBack),
                    new("Mark", PlayerPosition.Defender, 29, 74, 69, 75, 94, PlayerRole.CentreBack),

                    new("Oldham", PlayerPosition.Midfielder, 70, 61, 79, 85, 97, PlayerRole.BoxToBox),
                    new("Ray", PlayerPosition.Midfielder, 58, 73, 75, 70, 98, PlayerRole.BoxToBox),

                    new("Brown", PlayerPosition.Attacker, 84, 25, 71, 94, 96, PlayerRole.Winger)
                });
        }
    }
}