using RoR2;
using UnityEngine;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorBuffs
    {
        public static BuffDef conquerorIntimidateDebuff;

        static ConquerorBuffs()
        {
            conquerorIntimidateDebuff = AddNewBuff("ConquerorIntimidatedDebuff", LegacyResourcesAPI.Load<BuffDef>(null).iconSprite, Color.green, canStack: false, isDebuff: true, isHidden: false);
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
