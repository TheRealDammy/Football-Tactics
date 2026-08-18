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

        public static float GetMidfieldModifier(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.00f,
                Formation.FourThreeThree => 1.08f,
                Formation.FourTwoThreeOne => 1.12f,

                _ => 1.00f
            };
        }

        public static float GetAttackModifier(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.04f,
                Formation.FourThreeThree => 1.10f,
                Formation.FourTwoThreeOne => 1.07f,

                _ => 1.00f
            };
        }

        public static float GetDefenceModifier(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.05f,
                Formation.FourThreeThree => 0.98f,
                Formation.FourTwoThreeOne => 1.02f,

                _ => 1.00f
            };
        }

        public static float GetWidthModifier(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 0.98f,
                Formation.FourThreeThree => 1.10f,
                Formation.FourTwoThreeOne => 1.02f,

                _ => 1.00f
            };
        }

        public static float GetCounterModifier(
            this Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => 1.08f,
                Formation.FourThreeThree => 1.00f,
                Formation.FourTwoThreeOne => 1.04f,

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
                        PlayerPosition.Goalkeeper),

                    new(
                        "LB",
                        FormationArea.LeftBack,
                        PlayerPosition.Defender),

                    new(
                        "LCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RB",
                        FormationArea.RightBack,
                        PlayerPosition.Defender),

                    new(
                        "LM",
                        FormationArea.LeftMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "LCM",
                        FormationArea.CentreMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "RCM",
                        FormationArea.CentreMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "RM",
                        FormationArea.RightMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "LS",
                        FormationArea.LeftWing,
                        PlayerPosition.Attacker),

                    new(
                        "RS",
                        FormationArea.RightWing,
                        PlayerPosition.Attacker)
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
                        PlayerPosition.Goalkeeper),

                    new(
                        "LB",
                        FormationArea.LeftBack,
                        PlayerPosition.Defender),

                    new(
                        "LCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RB",
                        FormationArea.RightBack,
                        PlayerPosition.Defender),

                    new(
                        "LCM",
                        FormationArea.CentreMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "CM",
                        FormationArea.CentreMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "RCM",
                        FormationArea.CentreMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "LW",
                        FormationArea.LeftWing,
                        PlayerPosition.Attacker),

                    new(
                        "ST",
                        FormationArea.Striker,
                        PlayerPosition.Attacker),

                    new(
                        "RW",
                        FormationArea.RightWing,
                        PlayerPosition.Attacker)
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
                        PlayerPosition.Goalkeeper),

                    new(
                        "LB",
                        FormationArea.LeftBack,
                        PlayerPosition.Defender),

                    new(
                        "LCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RCB",
                        FormationArea.CentreBack,
                        PlayerPosition.Defender),

                    new(
                        "RB",
                        FormationArea.RightBack,
                        PlayerPosition.Defender),

                    new(
                        "LDM",
                        FormationArea.DefensiveMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "RDM",
                        FormationArea.DefensiveMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "LW",
                        FormationArea.LeftWing,
                        PlayerPosition.Attacker),

                    new(
                        "CAM",
                        FormationArea.AttackingMidfield,
                        PlayerPosition.Midfielder),

                    new(
                        "RW",
                        FormationArea.RightWing,
                        PlayerPosition.Attacker),

                    new(
                        "ST",
                        FormationArea.Striker,
                        PlayerPosition.Attacker)
                });
        }
    }
}