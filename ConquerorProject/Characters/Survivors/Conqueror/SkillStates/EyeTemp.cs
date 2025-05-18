
using EntityStates;
/*using ConquerorMod.Survivors.Conqueror;
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
using System.Collections;
using UnityEngine.UIElements;
using ConquerorMod.Survivors.Conqueror.Components;
using UnityEngine.AddressableAssets;
using ConquerorMod.Modules;
//using ConquerorMod.Characters.Survivors.Conqueror.Content;


namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class EyeOld : BaseSkillState
    {   
        private GameObject aimSphere;
        //private GameObject aimSphere2;
        public float radius = 1f;
        private Ray aimRay;
        private float maxDistance = 15f;
        
        private BlastAttack bleedblast;
        private BlastAttack pullblast;
        private float pullblastDamageCoefficient = 0f;
        private float bleedblastDamageCoefficient = ConquerorStaticValues.warpeyeDamageCoefficient;
        
        private ChildLocator childLocator;
        private Vector3 forwardDirection;
        private Animator animator;

        private GameObject transmitterFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/TeleportOnLowHealth/TeleportOnLowHealthVFX.prefab").WaitForCompletion(); 
        //private GameObject blinkDestination = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpBossBlinkDestination.prefab").WaitForCompletion();
        private GameObject blinkDestinationCharged = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/TeleportOnLowHealth/TeleportOnLowHealthExplosion.prefab").WaitForCompletion();



        private float chargeDuration;
        private float baseChargeDuration = 1f;
        public float charge;
        private bool isCharged;

        public override void OnEnter()
        {
            this.isCharged = false;

            base.OnEnter();
            this.aimSphere = UnityEngine.Object.Instantiate<GameObject>(blinkDestinationCharged);

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

        private void CreateBlinkEffect(Vector3 origin, Vector3 destination)
        {
            EffectData effectData = new EffectData();
            effectData.origin = origin;
            effectData.rotation = Util.QuaternionSafeLookRotation(origin - destination);
            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, false);
        }

        private void CreateExplosionEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            EffectManager.SpawnEffect(transmitterFlash, effectData, false);
        }

        public override void OnExit()
        {
            base.OnExit();

            bleedblast = new BlastAttack();
            bleedblast.radius = 10f;
            bleedblast.attacker = gameObject;
            bleedblast.inflictor = gameObject;
            bleedblast.teamIndex = TeamIndex.Player;
            bleedblast.procCoefficient = 1f;
            bleedblast.baseForce = 300;
            bleedblast.canRejectForce = false;
            bleedblast.falloffModel = BlastAttack.FalloffModel.None; 
            bleedblast.baseDamage = bleedblastDamageCoefficient * damageStat;
            bleedblast.damageType = DamageType.BleedOnHit;
            bleedblast.crit = RollCrit();
            bleedblast.position = this.aimSphere.transform.position;
            bleedblast.Fire();

            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.Motor.ForceUnground(0.1f);
            SmallHop(characterMotor, 5f);

            Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);
            Util.PlaySound("Play_imp_attack_blink", gameObject);
            Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
            Util.PlaySound("Play_nullifier_attack1_summon", gameObject);

            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject), Util.GetCorePosition(this.aimSphere.gameObject));
            this.CreateBlinkEffect(Util.GetCorePosition(this.aimSphere.gameObject), Util.GetCorePosition(base.gameObject));

            EntityState.Destroy(this.aimSphere.gameObject);
        }

        private IEnumerator TeleportEnemyAfterDelay(CharacterBody nmebody, Vector3 targetPosition, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (nmebody && nmebody.characterMotor)
            {
                nmebody.characterMotor.Motor.SetPosition(targetPosition);
                nmebody.characterMotor.velocity = Vector3.zero;
                nmebody.characterMotor.Motor.ForceUnground(0.1f);
                SmallHop(nmebody.characterMotor, 3f);
            }
            else if (nmebody && nmebody.transform)
            {
                nmebody.transform.position = targetPosition;
            }

            Util.PlaySound("Play_voidDevastator_m2_secondary_explo", nmebody.gameObject);
            this.CreateBlinkEffect(Util.GetCorePosition(nmebody.gameObject), Util.GetCorePosition(base.gameObject));
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
}*/