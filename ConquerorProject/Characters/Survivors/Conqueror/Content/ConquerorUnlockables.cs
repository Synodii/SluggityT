using ConquerorMod.Survivors.Conqueror.Achievements;
using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
            ConquerorMasteryAchievement.unlockableIdentifier,
            Modules.Tokens.GetAchievementNameToken(ConquerorMasteryAchievement.identifier),
            ConquerorSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
