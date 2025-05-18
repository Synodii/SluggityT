using System;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorStaticValues
    {
        public const float baseBackpackHitStop = 0.04f;
        public static float hitStopMod = 1f;
        public static float CurHitStop
        {
            get { return baseBackpackHitStop * hitStopMod; }
        }

        public const float swingDamageCoefficient = 3.5f;

        public const float thirdswingDamageCoefficient = 4f;

        public const float warpeyeDamageCoefficient = 1.2f;

        //public const float chargedwarpDamageCoefficient = 5f;

        public const float eyeblastsecondaryblastDamageCoefficient = 2.5f;

        public const float eyeblastinitDamageCoefficient = .8f;

        public const float gigalaserDamageCoefficient = 18f;

        public const float ropeduffelretrieveDamageCoefficient = 2f;

        public const float apportduffelretrieveDamageCoefficient = 2f;

        public const float chargeWarpHealthCost = .25f;
    }
}