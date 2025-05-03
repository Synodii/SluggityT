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
    public class RopeBackpack : GenericProjectileBaseState
    {
        public override void OnEnter()
        {
            projectilePrefab = ConquerorAssets.ropeBackpackProjectilePrefab;
            damageCoefficient = 0;
            baseDuration = 0;
            force = 10f;
            recoilAmplitude = 0.1f;

            base.skillLocator.special.SetSkillOverride(gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
            //base.skillLocator.secondary.SetSkillOverride(gameObject, ConquerorSurvivor.secondaryCrush, RoR2.GenericSkill.SkillOverridePriority.Replacement);
            base.skillLocator.special.DeductStock(1);
            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);

            characterBody.GetComponent<ConquerorController>().bagDeployed = true;

            base.OnEnter();

            FireProjectile();

            outer.SetNextStateToMain();
        }

        public override void FixedUpdate()
        {
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            base.ModifyProjectileInfo(ref fireProjectileInfo);
            fireProjectileInfo.damageTypeOverride = DamageTypeCombo.GenericSpecial;
        }

        public override void FireProjectile()
        {
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.crit = RollCrit();
            fireProjectileInfo.owner = base.gameObject;
            fireProjectileInfo.projectilePrefab = projectilePrefab;
            fireProjectileInfo.rotation = Quaternion.identity;
            fireProjectileInfo.damage = damageCoefficient * damageStat;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
    }
}