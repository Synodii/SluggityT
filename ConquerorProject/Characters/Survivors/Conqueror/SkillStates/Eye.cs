using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules.BaseStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror.Components;


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

        private BlastAttack eyeblastpull;
        private BlastAttack eyeblastbleed;

        private float eyeblastDamageCoefficient = ConquerorStaticValues.offenceeyeDamageCoefficient;

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

                    eyeblastbleed = new BlastAttack();
                    eyeblastbleed.radius = 10f;
                    eyeblastbleed.attacker = gameObject;

                    eyeblastbleed.inflictor = gameObject;
                    eyeblastbleed.teamIndex = TeamIndex.Player;
                    eyeblastbleed.procCoefficient = 1f;
                    eyeblastbleed.baseForce = 300;
                    eyeblastbleed.canRejectForce = false;
                    eyeblastbleed.falloffModel = BlastAttack.FalloffModel.None;
                    eyeblastbleed.baseDamage = ConquerorStaticValues.offenceeyeDamageCoefficient * damageStat;
                    eyeblastbleed.damageType = DamageType.BleedOnHit;
                    eyeblastbleed.crit = RollCrit();
                    eyeblastbleed.position = this.characterBody.corePosition;
                    eyeblastbleed.Fire();
                    //Log.Debug("eyeblastbleed");

                    eyeblastpull = new BlastAttack();
                    eyeblastpull.radius = 30f;
                    eyeblastpull.attacker = gameObject;
                    eyeblastpull.inflictor = gameObject;
                    eyeblastpull.teamIndex = TeamIndex.Player;
                    eyeblastpull.procCoefficient = 0f;
                    //eyeblastpull.baseForce = -2000;
                    eyeblastpull.canRejectForce = false;
                    eyeblastpull.falloffModel = BlastAttack.FalloffModel.None;
                    eyeblastpull.baseDamage = 0;
                    eyeblastpull.damageType = DamageType.Stun1s;
                    eyeblastpull.crit = RollCrit();
                    eyeblastpull.position = this.characterBody.footPosition;
                    BlastAttack.Result targetsHit = eyeblastpull.Fire();
                    //Log.Debug("Targets hit:" + targetsHit.hitCount);
                    for (int i = 0; i < targetsHit.hitCount; i++)
                    {
                        //targetsHit.hitPoints[i].hurtBox 
                        Vector3 targetPosition = this.characterBody.footPosition;
                        Vector3 startPosition = targetsHit.hitPoints[i].hurtBox.transform.position;
                        CharacterBody body = targetsHit.hitPoints[i].hurtBox.healthComponent.body;
                        bool isFlyer = body.isFlying || (body.characterMotor && (body.characterMotor.isFlying || !body.characterMotor.isGrounded));
                        Vector3 pullforce = GetEyePullVelocity(targetPosition, startPosition, isFlyer); // (body.rigidbody.mass * .3f);
                                                                                                         //Log.Debug($"Hit: {targetsHit.hitPoints[i].hurtBox.healthComponent.body.name} Pullforce: {pullforce} IsFlyer: {isFlyer} Mass {body.rigidbody.mass}");
                        if (body.rigidbody)
                        {
                            if (body.characterMotor)
                            {
                                if (body.characterMotor.isGrounded) body.characterMotor.Motor.ForceUnground();
                                if (!isFlyer) body.characterMotor.disableAirControlUntilCollision = true;
                                body.characterMotor.velocity = Vector3.zero;
                                body.characterMotor.velocity = pullforce;
                            }
                            else
                            {
                                body.rigidbody.AddForce(pullforce, ForceMode.VelocityChange);
                            }
                        }
                    };
                    //Log.Debug("EyeBlast");

                    hasFired = true;
                }
            }
        }

        public Vector3 GetEyePullVelocity(Vector3 targetPos, Vector3 startPos, bool isFlyer)
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
        }

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