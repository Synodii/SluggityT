using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules.BaseStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror.Components;
using System.Collections;


namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class Eye : BaseSkillState
    {
        public static float selfHealFraction = .15f;

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

        private BlastAttack secondaryeyeblast;
        private BlastAttack eyeblastinit;

        public override void OnEnter()
        {
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;

            if (characterBody.GetComponent<ConquerorController>().bagDeployed == false)
            {
                Util.PlaySound("Play_voidman_R_activate", gameObject);
                Util.PlaySound("Play_chef_skill1_return", gameObject);
                Util.PlaySound("Play_scav_backpack_open", gameObject);



                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
            }
            else
            {
                Util.PlaySound("Play_ui_obj_eradicator_open", gameObject);
                Util.PlaySound("Play_chef_skill1_return", gameObject);
                Util.PlaySound("Play_scav_backpack_open", gameObject);



                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
            }

            base.OnEnter();
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
            outer.SetNextStateToMain();
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

        private IEnumerator ExplodeEnemyAfterDelay(CharacterBody nmebody, float delay)
        {
            yield return new WaitForSeconds(delay);

            secondaryeyeblast = new BlastAttack();
            secondaryeyeblast.radius = 6.5f;
            secondaryeyeblast.attacker = gameObject;
            secondaryeyeblast.inflictor = gameObject;
            secondaryeyeblast.teamIndex = TeamIndex.Player;
            secondaryeyeblast.procCoefficient = 1f;
            //eyeblastpull.baseForce = -2000;
            secondaryeyeblast.canRejectForce = false;
            secondaryeyeblast.falloffModel = BlastAttack.FalloffModel.Linear;
            secondaryeyeblast.baseDamage = ConquerorStaticValues.eyeblastsecondaryblastDamageCoefficient * damageStat;
            secondaryeyeblast.damageType = DamageType.BleedOnHit;
            secondaryeyeblast.crit = RollCrit();
            secondaryeyeblast.position = nmebody.corePosition;
            secondaryeyeblast.Fire();

            Util.PlaySound("Play_voidDevastator_m2_secondary_explo", nmebody.gameObject);
            Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);

        }

        private void Fire()
        {
            if (NetworkServer.active)
            {
                if (characterBody.GetComponent<ConquerorController>().bagDeployed == false)
                {
                    ProcChainMask procChainMask = default(ProcChainMask);
                    Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                    Util.PlaySound("Play_gup_step", gameObject);
                    //Util.PlaySound("Play_ui_obj_eradicator_open", base.gameObject);

                    characterBody.AddTimedBuff(ConquerorBuffs.frenzyBuff, 4f);

                    if (selfHealFraction > 0f)
                    {
                        base.healthComponent.HealFraction(selfHealFraction, procChainMask);
                    }
                    hasFired = true;
                }
                else
                {
                    Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                    Util.PlaySound("Play_gup_step", gameObject);
                    Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);
                    Util.PlaySound("Play_nullifier_attack1_summon", gameObject);


                    eyeblastinit = new BlastAttack();
                    eyeblastinit.radius = 18f;
                    eyeblastinit.attacker = gameObject;

                    eyeblastinit.inflictor = gameObject;
                    eyeblastinit.teamIndex = TeamIndex.Player;
                    eyeblastinit.procCoefficient = 1f;
                    eyeblastinit.baseForce = 300;
                    eyeblastinit.canRejectForce = false;
                    eyeblastinit.falloffModel = BlastAttack.FalloffModel.None;
                    eyeblastinit.baseDamage = ConquerorStaticValues.eyeblastinitDamageCoefficient * damageStat;
                    eyeblastinit.damageType = DamageType.Generic;
                    eyeblastinit.crit = RollCrit();
                    eyeblastinit.position = this.characterBody.corePosition;
                    
                    BlastAttack.Result targetsHit = eyeblastinit.Fire();
                    Log.Debug("Targets hit:" + targetsHit.hitCount);
                    for (int i = 0; i < targetsHit.hitCount; i++)
                    {
                        HealthComponent hc = targetsHit.hitPoints[i].hurtBox.healthComponent;
                        CharacterBody nmebody = hc.body;

                        float delay = UnityEngine.Random.Range(1f, 1.5f);
                        RoR2.Run.instance.StartCoroutine(ExplodeEnemyAfterDelay(nmebody, delay));

                    }
                    hasFired = true;
                }
            }
        }

        /*public Vector3 GetEyePullVelocity(Vector3 targetPos, Vector3 startPos, bool isFlyer)
        {
            Vector3 distanceVector = (targetPos - startPos);
            Vector2 xzDistanceVec = new Vector2(distanceVector.x, distanceVector.z); // 
            float distanceToTarget = xzDistanceVec.magnitude;
            float timeToTarget = Mathf.Min(distanceToTarget * 0.07f, 1);

            Vector2 normailzedDistvec = xzDistanceVec / distanceToTarget;
            float y = isFlyer ? distanceVector.y : Mathf.Max(Trajectory.CalculateInitialYSpeed(timeToTarget, distanceVector.y), 6);
            float travelRate = distanceToTarget / timeToTarget;
            Vector3 direction = new Vector3(normailzedDistvec.x * travelRate, y, normailzedDistvec.y * travelRate);
            return direction;
        }*/
        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(base.characterBody.corePosition);
            effectData.origin = origin;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}