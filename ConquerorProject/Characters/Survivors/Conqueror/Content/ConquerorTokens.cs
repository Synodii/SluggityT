using System;
using ConquerorMod.Modules;
using ConquerorMod.Survivors.Conqueror.Achievements;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorTokens
    {
        public static void Init()
        {
            AddConquerorTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Conqueror.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddConquerorTokens()
        {
            string prefix = ConquerorSurvivor.CONQUEROR_PREFIX;

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", "Conqueror");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Displaced Hero");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Superbia");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Gain damage reduction exponentially the lower your health is up to 40% at 0 HP.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_AXE_NAME", "Invidia");
            Language.Add(prefix + "PRIMARY_AXE_DESCRIPTION", $"Hold to charge a horizontal sweep for 200-500% damage.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_AXECOMBO_NAME", "Ira");
            Language.Add(prefix + "SECONDARY_MUNCH_DESCRIPTION", Tokens.agilePrefix + $"Displacing. Slayer. Slam your axe down for 500%. Successfully hitting an enemy makes your next primary fully charged. Can be reactivated 3 times.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_WARP_NAME", "Gula");
            Language.Add(prefix + "UTILITY_WARP_DESCRIPTION", $"Bleeding. Crush an imp eye, blinking forward a short distance and dealing 120% damage to all nearby enemies. Hold up to 3. Becomes Luxuria while in range of Special.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_ROPEBACKPACK_NAME", "Acedia");
            Language.Add(prefix + "SPECIAL_ROPEBACKPACK_DESCRIPTION", $"Hold to charge. Throw your backpack, creating an encampment and pulling yourself to where it lands. While inside the radius of your encampment, gain 30 armor.");

            Language.Add(prefix + "SPECIAL_RECALLROPEBACKPACK_NAME", "Acedia");
            Language.Add(prefix + "SPECIAL_RECALLROPEBACKPACK_DESCRIPTION", Tokens.stunningPrefix +  $"Recall your backpack.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(ConquerorMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(ConquerorMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
