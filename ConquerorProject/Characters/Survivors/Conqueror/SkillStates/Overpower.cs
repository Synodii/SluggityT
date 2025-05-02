using ConquerorMod.Modules.BaseStates;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class Overpower : BaseMeleeAttack
    {

        public override void OnEnter()
        {
            hitboxGroupName = "SwordGroup";

            damageType = DamageTypeCombo.GenericPrimary;
            damageCoefficient = ConquerorStaticValues.swingDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.3f;
            attackEndPercentTime = 0.7f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 1f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            muzzleString = swingIndex % 2 == 0 ? "SwingMuzzle1" : "SwingMuzzle2";
            playbackRateParam = "Swing.playbackRate";

            base.OnEnter();
        }

        protected override void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.ModifyOverlapAttack(overlapAttack);
            
            if (swingIndex == 2)
            {
                attack.damageType = RoR2.DamageType.BonusToLowHealth;
                //attack.damageType = RoR2.DamageType.BleedOnHit
            }

            overlapAttack.damageType.damageSource = DamageSource.Primary;
        }

        protected override void PlayAttackAnimation()
        {
            PlayCrossfade("Gesture, Override", "Slash" + (1 + swingIndex), playbackRateParam, duration, 0.1f * duration);
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

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (swingIndex == 2)
            {
                return InterruptPriority.Skill;
            }
            else return InterruptPriority.PrioritySkill;
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}