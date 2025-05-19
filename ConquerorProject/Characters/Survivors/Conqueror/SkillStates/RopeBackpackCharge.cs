using EntityStates;
using UnityEngine;
using RoR2;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class RopeBackpackCharge : BaseSkillState
    {
        private float charge;
        private float baseChargeDuration = 4f;
        private float compensatedChargeDuration;
        private bool isCharged;

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
    }
}