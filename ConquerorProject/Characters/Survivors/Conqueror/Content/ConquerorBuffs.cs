using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorBuffs
    {
        //public static BuffDef satiatedBuff;
        //public static BuffDef disheartenedDebuff;
        public static BuffDef bolsteredBuff;
        //public static BuffDef conqExecutionMark;

        static ConquerorBuffs()
        {
            //satiatedBuff = AddNewBuff("ConquerorSatiatedBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.magenta, canStack: false, isDebuff: false);
            bolsteredBuff = AddNewBuff("ConquerorBolsteredBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.blue, canStack: false, isDebuff: false);
            //disheartenedDebuff = AddNewBuff("ConquerorDishearteneddBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.green, canStack: false, isDebuff: true);
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
