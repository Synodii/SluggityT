using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static RoR2.CameraTargetParams;
using IL.RoR2.Skills;
using ConquerorMod.Survivors.Conqueror.Components;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class RopeBackpackFire : GenericProjectileBaseState
    {
        public float charge;
        public override void OnEnter()
        {
            projectilePrefab = ConquerorAssets.ropeBackpackProjectilePrefab;

            baseDuration = 0.4f;
            force = 0f;
            recoilAmplitude = 0.1f;
            baseDelayBeforeFiringProjectile = 0.4f;

            Util.PlaySound("Play_scav_backpack_open", gameObject);


            base.skillLocator.special.SetSkillOverride(gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);

            //base.skillLocator.secondary.SetSkillOverride(gameObject, ConquerorSurvivor.secondaryCrush, RoR2.GenericSkill.SkillOverridePriority.Replacement);
            base.skillLocator.special.DeductStock(1);
            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);

            characterBody.GetComponent<ConquerorController>().bagDeployed = true;
            characterBody.GetComponent<ConquerorController>().isManualRecall = false;

            base.OnEnter();

            FireProjectile();

            outer.SetNextStateToMain();
        }


        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            base.ModifyProjectileInfo(ref fireProjectileInfo);


            //Ray aimRay;


            Vector3 aimDirection = GetAimRay().direction;
            Vector3 spawnPos = characterBody.corePosition + aimDirection * 1.5f;

            fireProjectileInfo.position = spawnPos;
            fireProjectileInfo.rotation = Quaternion.LookRotation(aimDirection);
            fireProjectileInfo.speedOverride = 30;
            fireProjectileInfo.damageTypeOverride = DamageTypeCombo.GenericSpecial;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}