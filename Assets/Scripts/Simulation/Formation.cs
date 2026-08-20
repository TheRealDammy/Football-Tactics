using System.Collections.Generic;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public enum Formation
    {
        FourFourTwo,
        FourThreeThree,
        FourTwoThreeOne
    }

    public sealed class FormationDefinition
    {
        public Formation Formation { get; }

        public IReadOnlyList<FormationSlot> Slots { get; }

        public FormationDefinition(
            Formation formation,
            IReadOnlyList<FormationSlot> slots)
        {
            Formation = formation;
            Slots = slots;
        }
    }

    public static class FormationExtensions
    {
        // =========================================================
        // FORMATION DEFINITIONS
        // =========================================================

        public static FormationDefinition GetDefinition(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo =>
                    CreateFourFourTwo(),

                Formation.FourThreeThree =>
                    CreateFourThreeThree(),

                Formation.FourTwoThreeOne =>
                    CreateFourTwoThreeOne(),

                _ =>
                    CreateFourThreeThree()
            };
        }

        // =========================================================
        // MATCH ENGINE MODIFIERS
        // =========================================================

        public static float GetMidfieldModifier(this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.00f,
                Formation.FourThreeThree => 1.02f,
                Formation.FourTwoThreeOne => 1.04f,

                _ => 1.00f
            };
        }

        public static float GetAttackModifier( this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.01f,
                Formation.FourThreeThree => 1.03f,
                Formation.FourTwoThreeOne => 1.02f,

                _ => 1.00f
            };
        }

        public static float GetDefenceModifier(this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.03f,
                Formation.FourThreeThree => 0.99f,
                Formation.FourTwoThreeOne => 1.01f,

                _ => 1.00f
            };
        }

        public static float GetWidthModifier(this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.00f,
                Formation.FourThreeThree => 1.04f,
                Formation.FourTwoThreeOne => 1.01f,

                _ => 1.00f
            };
        }

        public static float GetCounterModifier(this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.04f,
                Formation.FourThreeThree => 1.01f,
                Formation.FourTwoThreeOne => 1.02f,

                _ => 1.00f
            };
        }

        // =========================================================
        // 4-4-2
        // =========================================================

        private static FormationDefinition CreateFourFourTwo()
        {
            return new FormationDefinition(
                Formation.FourFourTwo,
                new List<FormationSlot>
                {
            new(
                "GK",
                FormationArea.Goalkeeper,
                PlayerPosition.Goalkeeper,
                50,
                90),

            new(
                "LB",
                FormationArea.LeftBack,
                PlayerPosition.Defender,
                12,
                72),

            new(
                "LCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                38,
                72),

            new(
                "RCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                62,
                72),

            new(
                "RB",
                FormationArea.RightBack,
                PlayerPosition.Defender,
                88,
                72),

            new(
                "LM",
                FormationArea.LeftMidfield,
                PlayerPosition.Midfielder,
                10,
                48),

            new(
                "LCM",
                FormationArea.CentreMidfield,
                PlayerPosition.Midfielder,
                38,
                48),

            new(
                "RCM",
                FormationArea.CentreMidfield,
                PlayerPosition.Midfielder,
                62,
                48),

            new(
                "RM",
                FormationArea.RightMidfield,
                PlayerPosition.Midfielder,
                90,
                48),

            new(
                "LS",
                FormationArea.LeftWing,
                PlayerPosition.Attacker,
                38,
                24),

            new(
                "RS",
                FormationArea.RightWing,
                PlayerPosition.Attacker,
                62,
                24)
                });
        }

        // =========================================================
        // 4-3-3
        // =========================================================

        private static FormationDefinition CreateFourThreeThree()
        {
            return new FormationDefinition(
                Formation.FourThreeThree,
                new List<FormationSlot>
                {
            new(
                "GK",
                FormationArea.Goalkeeper,
                PlayerPosition.Goalkeeper,
                50,
                90),

            new(
                "LB",
                FormationArea.LeftBack,
                PlayerPosition.Defender,
                12,
                72),

            new(
                "LCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                38,
                72),

            new(
                "RCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                62,
                72),

            new(
                "RB",
                FormationArea.RightBack,
                PlayerPosition.Defender,
                88,
                72),

            new(
                "LCM",
                FormationArea.CentreMidfield,
                PlayerPosition.Midfielder,
                28,
                50),

            new(
                "CM",
                FormationArea.CentreMidfield,
                PlayerPosition.Midfielder,
                50,
                46),

            new(
                "RCM",
                FormationArea.CentreMidfield,
                PlayerPosition.Midfielder,
                72,
                50),

            new(
                "LW",
                FormationArea.LeftWing,
                PlayerPosition.Attacker,
                15,
                25),

            new(
                "ST",
                FormationArea.Striker,
                PlayerPosition.Attacker,
                50,
                17),

            new(
                "RW",
                FormationArea.RightWing,
                PlayerPosition.Attacker,
                85,
                25)
                });
        }

        // =========================================================
        // 4-2-3-1
        // =========================================================

        private static FormationDefinition CreateFourTwoThreeOne()
        {
            return new FormationDefinition(
                Formation.FourTwoThreeOne,
                new List<FormationSlot>
                {
            new(
                "GK",
                FormationArea.Goalkeeper,
                PlayerPosition.Goalkeeper,
                50,
                90),

            new(
                "LB",
                FormationArea.LeftBack,
                PlayerPosition.Defender,
                12,
                72),

            new(
                "LCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                38,
                72),

            new(
                "RCB",
                FormationArea.CentreBack,
                PlayerPosition.Defender,
                62,
                72),

            new(
                "RB",
                FormationArea.RightBack,
                PlayerPosition.Defender,
                88,
                72),

            new(
                "LDM",
                FormationArea.DefensiveMidfield,
                PlayerPosition.Midfielder,
                40,
                53),

            new(
                "RDM",
                FormationArea.DefensiveMidfield,
                PlayerPosition.Midfielder,
                60,
                53),

            new(
                "LW",
                FormationArea.LeftWing,
                PlayerPosition.Attacker,
                15,
                27),

            new(
                "CAM",
                FormationArea.AttackingMidfield,
                PlayerPosition.Midfielder,
                50,
                34),

            new(
                "RW",
                FormationArea.RightWing,
                PlayerPosition.Attacker,
                85,
                27),

            new(
                "ST",
                FormationArea.Striker,
                PlayerPosition.Attacker,
                50,
                16)
                });
        }
    }
}