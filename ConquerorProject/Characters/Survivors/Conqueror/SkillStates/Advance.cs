using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules.BaseStates;
using RoR2;
using System;
using EntityStates.ImpMonster;
using EntityStates.ImpBossMonster;
using EntityStates.Huntress;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using EntityStates;
using R2API;
using RoR2.Skills;
using static RoR2.BlastAttack;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class Advance : BaseSkillState
    {
        private GameObject aimSphere;
        //private GameObject aimSphere2;
        public float radius = 8f;
        private Ray aimRay;
        private float maxDistance = 45f;
        private BlastAttack bleedblast;
        private BlastAttack pullblast;
        private float pullblastDamageCoefficient = 0f;
        private float bleedblastDamageCoefficient = ConquerorStaticValues.warpDamageCoefficient;
        private float chargedbleedblastDamageCoefficient = ConquerorStaticValues.chargedwarpDamageCoefficient;
        private ChildLocator childLocator;
        private Vector3 forwardDirection;
        private Animator animator;

        private float chargeDuration;
        private float baseChargeDuration = 1f;
        public float charge;
        private bool isCharged;
        private float baseMaxSecondaryStock;
        private float secondaryStock;


        public override void OnEnter()
        {
            this.isCharged = false;

            base.OnEnter();
            this.aimSphere = UnityEngine.Object.Instantiate<GameObject>(ArrowRain.areaIndicatorPrefab);

            this.chargeDuration = this.baseChargeDuration / this.attackSpeedStat;

            //AkSoundEngine.PostEvent("Judgement", base.gameObject);
            Util.PlaySound("Play_voidDevastator_m1_stick", base.gameObject);
            Util.PlaySound("Play_imp_overlord_attack1_impact", base.gameObject);
            Util.PlaySound("Play_imp_attack_tell", base.gameObject);
            //base.PlayAnimation("FullBody, Override", BackflipState.BackflipStateHash, BackflipState.BackflipParamHash, BackflipState.duration);

        }

        public override void Update()
        {
            base.Update();
            this.UpdateAreaIndicator();
        }

        private void UpdateAreaIndicator()
        {
            if (base.isAuthority)
            {
                this.aimSphere.transform.localScale = new Vector3(this.radius, this.radius, this.radius);
                //this.aimSphere2.transform.localScale = new Vector3(this.radius, this.radius, this.radius);
            }
            this.aimRay = base.GetAimRay();
            RaycastHit raycastHit;
            bool raycast = Physics.Raycast(base.GetAimRay(), out raycastHit, this.maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask);
            if (raycast)
            {
                this.aimSphere.transform.position = raycastHit.point + Vector3.up;
                this.aimSphere.transform.up = raycastHit.normal;
                this.aimSphere.transform.forward = -this.aimRay.direction;
            }
            else
            {
                Ray ray = base.GetAimRay();
                Vector3 position = ray.origin + this.maxDistance * ray.direction;
                this.aimSphere.transform.position = position;
                this.aimSphere.transform.up = raycastHit.normal;
                this.aimSphere.transform.forward = -this.aimRay.direction;

            }
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.aimSphere.transform.position - base.characterBody.corePosition);
            effectData.origin = origin;
            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.charge = (base.fixedAge / this.chargeDuration);

            if (base.isAuthority && !base.IsKeyDownAuthority())
            {
                base.characterMotor.rootMotion += this.aimSphere.transform.position - base.characterBody.corePosition;

                base.characterMotor.velocity.y = 10f;
                this.outer.SetNextStateToMain();
            }
            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            ChargedSoundplayed();
        }

        private void ChargedSoundplayed()
        {
            if (!this.isCharged && this.charge >= this.chargeDuration)
            {
                this.isCharged = true;
                Util.PlaySound("Play_voidDevastator_step", base.gameObject);
                Util.PlaySound("Play_imp_overlord_attack1_impact", base.gameObject);
                Util.PlaySound("Play_voidman_sprint_start", base.gameObject);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            //EntityState.Destroy(this.aimSphere2.gameObject);

            if (this.charge >= this.chargeDuration)
            {
                if (NetworkServer.active && healthComponent)
                {
                    healthComponent.TakeDamage(new DamageInfo
                    {
                        damage = healthComponent.combinedHealth * .3f,
                        position = characterBody.corePosition,
                        attacker = null,
                        inflictor = null,
                        damageType = DamageType.NonLethal | DamageType.BypassArmor,
                        procCoefficient = 1f
                    });
                }
            }


            bleedblast = new BlastAttack();
            bleedblast.radius = 10f;
            bleedblast.attacker = gameObject;
            bleedblast.inflictor = gameObject;
            bleedblast.teamIndex = TeamIndex.Player;
            bleedblast.procCoefficient = 1f;
            bleedblast.baseForce = 300;
            bleedblast.canRejectForce = false;
            bleedblast.falloffModel = BlastAttack.FalloffModel.None;
            if (this.charge >= this.chargeDuration)
            {
                bleedblast.baseDamage = chargedbleedblastDamageCoefficient * damageStat;
            }
            else
            {
                bleedblast.baseDamage = bleedblastDamageCoefficient * damageStat;
            }
            bleedblast.damageType = DamageType.BleedOnHit;
            bleedblast.crit = RollCrit();
            bleedblast.position = this.aimSphere.transform.position;
            bleedblast.Fire();
            //Log.Debug("BleedBlast");

            pullblast = new BlastAttack();
            pullblast.radius = 30f;
            pullblast.attacker = gameObject;
            pullblast.inflictor = gameObject;
            pullblast.teamIndex = TeamIndex.Player;
            pullblast.procCoefficient = 0f;
            //pullblast.baseForce = -2000;
            pullblast.canRejectForce = false;
            pullblast.falloffModel = BlastAttack.FalloffModel.None;
            pullblast.baseDamage = pullblastDamageCoefficient;
            pullblast.damageType = DamageType.Stun1s;
            pullblast.crit = RollCrit();
            pullblast.position = this.aimSphere.transform.position;
            BlastAttack.Result targetsHit = pullblast.Fire();
            //Log.Debug("Targets hit:" + targetsHit.hitCount);
            for (int i = 0; i < targetsHit.hitCount; i++)
            {
                //targetsHit.hitPoints[i].hurtBox 
                Vector3 targetPosition = this.aimSphere.transform.position;
                Vector3 startPosition = targetsHit.hitPoints[i].hurtBox.transform.position;
                CharacterBody body = targetsHit.hitPoints[i].hurtBox.healthComponent.body;
                bool isFlyer = body.isFlying || (body.characterMotor && (body.characterMotor.isFlying || !body.characterMotor.isGrounded));
                Vector3 pullforce = GetWarpPullVelocity(targetPosition, startPosition, isFlyer); // (body.rigidbody.mass * .3f);
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
            //Log.Debug("PullBlast");

            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.Motor.ForceUnground(0.1f);
            SmallHop(characterMotor, 5f);

            if (NetworkServer.active)
            {
                Util.CleanseBody(base.characterBody, true, false, false, true, true, true);
            }

            Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);
            Util.PlaySound("Play_imp_attack_blink", gameObject);
            Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
            

            EntityState.Destroy(this.aimSphere.gameObject);
        }

        public Vector3 GetWarpPullVelocity(Vector3 targetPos, Vector3 startPos, bool isFlyer)
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}