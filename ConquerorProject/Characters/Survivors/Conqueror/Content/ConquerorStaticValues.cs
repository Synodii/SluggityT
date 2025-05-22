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

        public const float minPrimaryDamageCoefficient = 2f;

        public const float maxPrimaryDamageCoefficient = 5f;

        public const float warpeyeDamageCoefficient = 1.2f;

        //public const float chargedwarpDamageCoefficient = 5f;

        public const float specialeyeDamageCoefficient = 2.5f;

        public const float gigalaserDamageCoefficient = 18f;

        public const float axecomboDamageCoefficient = 5f;
    }
}