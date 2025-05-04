/*using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using ConquerorMod;
using RoR2.Projectile;
using ConquerorMod.Modules;
using ConquerorMod.Modules.BaseStates;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using EntityStates;
using static RoR2.DotController;
using ConquerorMod.Modules.Characters;
using ConquerorMod.Survivors.Conqueror;

namespace ConquerorMod.Characters.Survivors.Conqueror.Content
{
    internal class DamageTypes
    {
        public static DamageAPI.ModdedDamageType MarkForScrounge;
        public static DamageAPI.ModdedDamageType Default;


        internal void Init()
        {
            MarkForScrounge = DamageAPI.ReserveDamageType();
            Default = DamageAPI.ReserveDamageType();

            Hook();
        }

        private void Hook()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_ServerDamageDealt;
        }

        private void GlobalEventManager_ServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport == null || damageReport.damageInfo == null) return;

            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, MarkForScrounge))
            {
                if (damageReport.victimBody)
                {
                    damageReport.victimBody.AddTimedBuff(ConquerorBuffs.conqExecutionMark, 2f);
                }
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            var damageInfo = damageReport?.damageInfo;
            if (damageInfo == null || !DamageAPI.HasModdedDamageType(damageInfo, MarkForScrounge))
                return;

            if (!damageReport.attacker || !damageReport.attackerBody)
                return;

            if (damageReport.attackerBody.bodyIndex != ConquerorSurvivor.conquerorBodyIndex)
                return;

            var skillLocator = damageReport.attackerBody.skillLocator;
            if (skillLocator && skillLocator.secondary)
            {
                skillLocator.secondary.rechargeStopwatch += 1f;

                if (skillLocator.secondary.rechargeStopwatch > skillLocator.secondary.finalRechargeInterval)
                {
                    skillLocator.secondary.rechargeStopwatch = skillLocator.secondary.finalRechargeInterval;
                }
            }
        }
    }
}*/