using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorBuffs
    {
        public static BuffDef frenzyBuff;
        public static BuffDef intimidateDebuff;
        public static BuffDef eyecountTracker;
        public static BuffDef eyecooldownTracker;


        public static void Init(AssetBundle assetBundle)
        {
            frenzyBuff = Modules.Content.CreateAndAddBuff("ConquerorFrenziedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.magenta,
                false,
                false);

            intimidateDebuff = Modules.Content.CreateAndAddBuff("ConquerorIntimidatedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.green,
                false,
                true);

            eyecountTracker = Modules.Content.CreateAndAddBuff("ConquerorEyeCountTracker",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.red,
                true,
                false);

            eyecooldownTracker = Modules.Content.CreateAndAddBuff("ConquerorEyeCDTracker",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.gray,
                false,
                false);
        }
    }
}
