using HG;
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
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType BleedOnHitbutCooler;
        public static DamageAPI.ModdedDamageType ConquerorKnockup;
        public static DamageAPI.ModdedDamageType Default;


        internal static void Init()
        {
            BleedOnHitbutCooler = DamageAPI.ReserveDamageType();
            ConquerorKnockup = DamageAPI.ReserveDamageType();
            Default = DamageAPI.ReserveDamageType();

            Hook();
        }

        private static void Hook()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport == null || damageReport.damageInfo == null) return;

            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, BleedOnHitbutCooler))
            {
                if (damageReport.victimBody)
                {
                    DotController.InflictDot(damageReport.victim.gameObject, damageReport.attacker, DotController.DotIndex.Bleed, 8f, 1f);
                }
            }

            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, ConquerorKnockup))
            {
                if (!damageReport.victim.body) return;
                //apply knockup scaled with mass if victim has rigidbody. Do not apply knockup if victim is airborne.
                if (damageReport.victim.body && damageReport.victim.body.characterMotor)
                {
                    damageReport.damageInfo.force = damageReport.victimBody.characterMotor.isGrounded ? (damageReport.victimBody.rigidbody && damageReport.victimBody.rigidbody.mass < 700 ? damageReport.victimBody.rigidbody.mass : 0.1f) * new Vector3(0, 15f, 0) : Vector3.zero;
                    
                    if (damageReport.victim.body.characterMotor.isGrounded) damageReport.victim.body.characterMotor.Motor.ForceUnground();
                    damageReport.victim?.TakeDamageForce(damageReport.damageInfo.force);

                    if (damageReport.victim.body.isFlying || (damageReport.victim.body.characterMotor && (damageReport.victim.body.characterMotor.isFlying || !damageReport.victim.body.characterMotor.isGrounded)))
                    {
                        damageReport.victim?.TakeDamageForce(-damageReport.damageInfo.force * 2.5f);
                    }
                }
            }
        }
    }
}