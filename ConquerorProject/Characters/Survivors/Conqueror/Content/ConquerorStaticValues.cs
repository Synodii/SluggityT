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

        public const float swingDamageCoefficient = 2.4f;

        public const float warpDamageCoefficient = 2.5f;

        public const float chargedwarpDamageCoefficient = 5f;

        public const float offenceeyeDamageCoefficient = 4f;

        public const float gigalaserDamageCoefficient = 18f;

        public const float ropeduffelretrieveDamageCoefficient = 2f;

        public const float apportduffelretrieveDamageCoefficient = 2f;
    }
}