using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Modules.BaseStates;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using ConquerorMod.Survivors.Conqueror.Components;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class OverpowerFire : BaseMeleeAttack
    {
        private ConquerorController controller;

        int step;

        public override void OnEnter()
        {
            controller = GetComponent<ConquerorController>();

            baseDuration = 0.75f;
            attackStartPercentTime = 0f;
            attackEndPercentTime = 0.7f;
            earlyExitPercentTime = .7f;

            swingIndex = charge >= 1f ? 1 : 0;
            damageType = charge >= 1f ? DamageType.Generic//DamageType.BonusToLowHealth
                                        : DamageType.Generic;

            base.OnEnter();
        }

        //protected override void PlaySwingEffect()
        //{
            //base.PlaySwingEffect();
        //}

        protected override void ModifyOverlapAttack(OverlapAttack attack)
        {
            base.ModifyOverlapAttack(attack);

            float min = ConquerorStaticValues.minPrimaryDamageCoefficient;
            float max = ConquerorStaticValues.maxPrimaryDamageCoefficient;
            damageCoefficient = Mathf.Lerp(min, max, charge);
            attack.damage = damageCoefficient * damageStat;

            attack.damageType.damageSource = DamageSource.Primary;
        }

        protected override void FireAttack()
        {
            Transform swingPivot = base.FindModelChild("SwingPivot");
            Ray aimRay = base.GetAimRay();
            swingPivot.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
            if (base.isAuthority)
            {
                base.FireAttack();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void PlayAttackAnimation()
        {
            PlayCrossfade("Gesture, Override", "Slash" + (1 + controller.stepCount), playbackRateParam, duration, 0.1f * duration);
        }

        public override void OnExit()
        {
            if (isMaxCharge)
            {
                controller.IncreasePrimaryStepCount();
            }

            isMaxCharge = false;
            controller.isPreCharged = false;
            controller.preChargeEffectComplete = false;
            
            base.OnExit();
        }
    }
}