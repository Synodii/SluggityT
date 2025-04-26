using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules.BaseStates;
using RoR2;
using System;
using EntityStates.Huntress;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using EntityStates;
using R2API;
using RoR2.Skills;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class Advance : BaseConquerorSkillState
    {
        private GameObject aimSphere;
        //private GameObject aimSphere2;
        public float radius = 8f;
        private Ray aimRay;
        private float maxDistance = 45f;
        private BlastAttack bleedblast;
        private BlastAttack pullblast;
        private float pullblastDamageCoefficient = 0.01f;
        private float bleedblastDamageCoefficient = 4.5f;
        private ChildLocator childLocator;
        private Vector3 forwardDirection;
        private Animator animator;
        public static string beginSoundString = "Play_imp_attack_blink";

        private float chargeDuration;
        private float baseChargeDuration = 1.2f;
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
            Util.PlaySound("Play_imp_attack_blink", base.gameObject);
            //base.PlayAnimation("FullBody, Override", BackflipState.BackflipStateHash, BackflipState.BackflipParamHash, BackflipState.duration);

        }

        public override void Update()
        {
            base.Update();
            this.UpdateAreaIndicator();
        }

        private void UpdateAreaIndicator()
        {
            bool isAuthority = base.isAuthority;
            bool flag = isAuthority;
            if (flag)
            {
                this.aimSphere.transform.localScale = new Vector3(this.radius, this.radius, this.radius);
                //this.aimSphere2.transform.localScale = new Vector3(this.radius, this.radius, this.radius);
            }
            this.aimRay = base.GetAimRay();
            RaycastHit raycastHit;
            bool flag2 = Physics.Raycast(base.GetAimRay(), out raycastHit, this.maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask);
            bool flag3 = flag2;
            if (flag3)
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
            EffectManager.SpawnEffect(BlinkState.blinkPrefab, effectData, false);
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
                Util.PlaySound("Play_imp_attack_tell", base.gameObject);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(this.aimSphere.gameObject);
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
                if (NetworkServer.active)
                {
                    baseMaxSecondaryStock = (float)base.skillLocator.GetSkill(SkillSlot.Secondary).maxStock;
                    secondaryStock = (float)base.skillLocator.GetSkill(SkillSlot.Secondary).stock;
                    if (secondaryStock < baseMaxSecondaryStock)
                    {
                        GenericSkill skill = base.skillLocator.GetSkill(SkillSlot.Secondary);
                        int stock = skill.stock;
                        skill.stock = stock + 1;
                    }
                }
            }


                bleedblast = new BlastAttack();
            bleedblast.radius = 8f;
            bleedblast.attacker = gameObject;
            bleedblast.inflictor = gameObject;
            bleedblast.teamIndex = TeamIndex.Player;
            bleedblast.procCoefficient = 1f;
            bleedblast.baseForce = 300;
            bleedblast.canRejectForce = false;
            bleedblast.falloffModel = BlastAttack.FalloffModel.None;
            bleedblast.baseDamage = bleedblastDamageCoefficient;
            bleedblast.damageType = DamageType.BleedOnHit;
            bleedblast.crit = RollCrit();
            bleedblast.position = this.aimSphere.transform.position;
            bleedblast.Fire();
            Log.Debug("BleedBlast");

            pullblast = new BlastAttack();
            pullblast.radius = 30f;
            pullblast.attacker = gameObject;
            pullblast.inflictor = gameObject;
            pullblast.teamIndex = TeamIndex.Player;
            pullblast.procCoefficient = 0f;
            pullblast.baseForce = -2000;
            pullblast.canRejectForce = false;
            pullblast.falloffModel = BlastAttack.FalloffModel.None;
            pullblast.baseDamage = pullblastDamageCoefficient;
            pullblast.damageType = DamageType.Stun1s;
            pullblast.crit = RollCrit();
            pullblast.position = this.aimSphere.transform.position;
            pullblast.Fire();
            Log.Debug("PullBlast");
            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.Motor.ForceUnground(0.1f);
            SmallHop(characterMotor, 5f);

            if (NetworkServer.active)
            {
                Util.CleanseBody(base.characterBody, true, false, false, true, true, true);
            }

            Util.PlaySound(beginSoundString, gameObject);
            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}