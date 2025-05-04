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
            if (projectilePrefab != null)
            {
                Log.Debug($"[Test] Firing projectile: {projectilePrefab.name}");
                if (!projectilePrefab.GetComponent<RopeBackpackController>())
                {
                    Log.Debug("[Test] RopeBackpackController is NOT on the prefab!");
                }
                else
                {
                    Log.Debug("[Test] RopeBackpackController is present on the prefab.");
                }
            }

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



        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            base.ModifyProjectileInfo(ref fireProjectileInfo);
            fireProjectileInfo.damageTypeOverride = DamageTypeCombo.GenericSpecial;
            fireProjectileInfo.rotation = Quaternion.LookRotation(Vector3.down);
            fireProjectileInfo.position = characterBody.corePosition + Vector3.up * 0.2f;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}