using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;


namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class EyeMunch : BaseSkillState
    {
        public static float selfHealFraction = .15f;

        public static float baseDuration = 1f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 1f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;

            //Util.PlaySound("Play_voidman_R_activate", gameObject);
            Util.PlaySound("Play_chef_skill1_return", gameObject);
            Util.PlaySound("Play_scav_backpack_open", gameObject);



            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hasFired)
            {
                if (fixedAge >= fireTime)
                {
                    Fire();
                }
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (NetworkServer.active)
            {
                ProcChainMask procChainMask = default(ProcChainMask);
                procChainMask.AddProc(ProcType.VoidSurvivorCrush);
                Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                Util.PlaySound("Play_gup_step", gameObject);
                //Util.PlaySound("Play_ui_obj_eradicator_open", base.gameObject);

                characterBody.AddTimedBuff(ConquerorBuffs.frenzyBuff, 5f);

                if (selfHealFraction > 0f)
                {
                    base.healthComponent.HealFraction(selfHealFraction, procChainMask);
                }
                hasFired = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}