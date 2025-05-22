using EntityStates;
using UnityEngine;
using RoR2;
using UnityEngine.AddressableAssets;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class RopeBackpackCharge : BaseSkillState
    {
        private float charge;
        private float baseChargeDuration = 4f;
        private float compensatedChargeDuration;
        private bool isCharged;

        private GameObject shatterspleenImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Impact.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();
            charge = 0f;
            this.compensatedChargeDuration = this.baseChargeDuration / this.attackSpeedStat;
            //PlayAnimation("LeftArm, Override", "ChargeGun"); // Optional animation
            isCharged = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (charge < 1f)
            {
                charge += Time.fixedDeltaTime / compensatedChargeDuration;
            }
            if (charge  > compensatedChargeDuration && !isCharged) 
            {
                Util.PlaySound("Play_voidman_sprint_start", base.gameObject);
                CreateShatterspleenImpactFX(Util.GetCorePosition(this.characterBody.gameObject));
                isCharged = true;
            }

            if (!inputBank.skill4.down)
            {
                Fire();
            }
        }

        private void Fire()
        {
            var fireState = new RopeBackpackFire();
            fireState.charge = Mathf.Clamp01(charge); // ensure it's capped at 1
            outer.SetNextState(fireState);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }

        private void CreateShatterspleenImpactFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 4f;
            EffectManager.SpawnEffect(shatterspleenImpact, effectData, false);
        }
    }
}