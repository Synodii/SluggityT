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
            Language.Add(prefix + "PASSIVE_NAME", "Scavenge");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "While Special is active, kills while within its radius refresh stocks of your secondary.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_AXE_NAME", "Overpower");
            Language.Add(prefix + "PRIMARY_AXE_DESCRIPTION", $"Swing forward for <style=cIsDamage>{100f * ConquerorStaticValues.swingDamageCoefficient}% damage</style>. Every 3rd hit does <style=cIsDamage>{100f * ConquerorStaticValues.thirdswingDamageCoefficient}% damage</style> and is <style=cIsDamage>Slayer</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_MUNCH_NAME", "Imbibe");
            Language.Add(prefix + "SECONDARY_MUNCH_DESCRIPTION", Tokens.agilePrefix + $" Consume an Imp eye to <style=cIsHealing>heal for 15% max health</style>.  Refreshes 1 Utility stock. Hold up to 3. <style=cIsUtility>Becomes Smother while Special is active.</style>. REMOVE FROM THIS DESC AND MAKE ITS OWN BUT IM TOO LAZY: Crush an imp eye. exploding an area for <style=cIsDamage>{100f * ConquerorStaticValues.eyeblastinitDamageCoefficient}% damage</style>. After a delay, each enemy hit will explode again in a smaller radius for <style=cIsDamage>{100f * ConquerorStaticValues.eyeblastsecondaryblastDamageCoefficient}% damage</style>. Refreshes 1 Utility stock. Hold up to 3.");

            Language.Add(prefix + "SECONDARY_CRUSH_NAME", "Smother");
            Language.Add(prefix + "SECONDARY_CRUSH_DESCRIPTION", Tokens.agilePrefix + $"insert token here");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_WARP_NAME", "Advance");
            Language.Add(prefix + "UTILITY_WARP_DESCRIPTION", $"Charge a teleport. On arrival, deal <style=cIsDamage>{100f * ConquerorStaticValues.warpeyeDamageCoefficient}% damage</style>, <style=cIsHealth>bleeding</style> enemies. At full charge, incur a <style=cIsHealth>25% health cost</style>, <style=cIsUtility>cleanse debuffs</style>, and  <style=cIsHealth>teleport</style> enemies to you. Always charged when Special is active.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_ROPEBACKPACK_NAME", "Conquest");
            Language.Add(prefix + "SPECIAL_ROPEBACKPACK_DESCRIPTION", $"Deploy your backpack. Allies near it are Bolstered, while enemies are Disheartened. Leave its radius or reactivate to reclaim it.");

            Language.Add(prefix + "SPECIAL_RECALLROPEBACKPACK_NAME", "Conquest");
            Language.Add(prefix + "SPECIAL_RECALLROPEBACKPACK_DESCRIPTION", Tokens.stunningPrefix +  $"Recall your backpack.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(ConquerorMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(ConquerorMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
