using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorBuffs
    {
        public static BuffDef frenzyBuff;
        public static BuffDef intimidateDebuff;
        public static BuffDef bunkeredBuff;
        //public static BuffDef conqExecutionMark;

        static ConquerorBuffs()
        {
            frenzyBuff = AddNewBuff("ConquerorFrenziedBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.magenta, canStack: false, isDebuff: false);
            bunkeredBuff = AddNewBuff("ConquerorFrenziedBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.magenta, canStack: false, isDebuff: false);
            intimidateDebuff = AddNewBuff("ConquerorIntimidatedBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.green, canStack: false, isDebuff: true);
            //conqExecutionMark = AddNewBuff("ConquerorExecuteHiddenBuff", LegacyResourcesAPI.Load<BuffDef>(null).iconSprite, Color.green, canStack: false, isDebuff: false, isHidden: true);
        }

        private static BuffDef AddNewBuff(string buffName, Sprite icon, Color color, bool canStack, bool isDebuff, bool isHidden = false, bool isCooldown = false)
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = buffName;
            buff.buffColor = color;
            buff.canStack = canStack;
            buff.isDebuff = isDebuff;
            buff.isHidden = isHidden;
            buff.isCooldown = isCooldown;
            buff.iconSprite = icon;

            Modules.Content.AddBuffDef(buff);
            return buff;
        }
    }
}
