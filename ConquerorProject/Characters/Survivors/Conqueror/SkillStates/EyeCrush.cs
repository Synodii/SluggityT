/*using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;


namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class EyeCrush : BaseSkillState
    {
        public static float baseDuration = 1f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        // I KNOW WHAT IM DOING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static float firePercentTime = 1f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        private float baseMaxUtilityStock;
        private float utilityStock;

        private BlastAttack eyeblast;
        private float eyeblastDamageCoefficient = ConquerorStaticValues.offenceeyeDamageCoefficient;


        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;

            Util.PlaySound("Play_ui_obj_eradicator_open", gameObject);
            Util.PlaySound("Play_chef_skill1_return", gameObject);
            Util.PlaySound("Play_scav_backpack_open", gameObject);



            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                baseMaxUtilityStock = (float)base.skillLocator.GetSkill(SkillSlot.Utility).maxStock;
                utilityStock = (float)base.skillLocator.GetSkill(SkillSlot.Utility).stock;
                if (utilityStock < baseMaxUtilityStock)
                {
                    GenericSkill skill = base.skillLocator.GetSkill(SkillSlot.Utility);
                    int stock = skill.stock;
                    skill.stock = stock + 1;
                }
            }
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
                Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                Util.PlaySound("Play_gup_step", gameObject);
                Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);

                eyeblast = new BlastAttack();
                eyeblast.radius = 13f;
                eyeblast.attacker = gameObject;
                eyeblast.inflictor = gameObject;
                eyeblast.teamIndex = TeamIndex.Player;
                eyeblast.procCoefficient = 1f;
                eyeblast.baseForce = 300;
                eyeblast.canRejectForce = false;
                eyeblast.falloffModel = BlastAttack.FalloffModel.None;
                eyeblast.baseDamage = eyeblastDamageCoefficient * damageStat;
                eyeblast.damageType = DamageType.BleedOnHit;
                eyeblast.crit = RollCrit();
                eyeblast.position = this.characterBody.transform.position;
                eyeblast.Fire();
                Log.Debug("EyeBlast");

                hasFired = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}*/