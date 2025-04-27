using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorBuffs
    {
        public static BuffDef frenzyBuff;
        public static BuffDef intimidateDebuff;
        public static BuffDef backpackTrackerHiddenBuff;


        public static void Init(AssetBundle assetBundle)
        {
            frenzyBuff = Modules.Content.CreateAndAddBuff("ConquerorFrenziedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.magenta,
                true,
                false);

            intimidateDebuff = Modules.Content.CreateAndAddBuff("ConquerorIntimidatedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.green,
                false,
                true);

            backpackTrackerHiddenBuff = Modules.Content.CreateAndAddBuff("ConquerorIntimidatedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.green,
                false,
                true);
        }
    }
}
