using RoR2;
using ConquerorMod.Modules.Achievements;

namespace ConquerorMod.Survivors.Conqueror.Achievements
{
    
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class ConquerorMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = ConquerorSurvivor.CONQUEROR_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = ConquerorSurvivor.CONQUEROR_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => ConquerorSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}