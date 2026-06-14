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
using R2API;
using RoR2.Skills;
using static RoR2.BlastAttack;
using System.Collections;
using UnityEngine.UIElements;
using ConquerorMod.Survivors.Conqueror.Components;
using UnityEngine.AddressableAssets;
using ConquerorMod.Modules;
using System.Linq;
using static RoR2.CameraTargetParams;
using TMPro;
using static UnityEngine.UI.Image;
using static R2API.DamageAPI;
using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Characters.Survivors.Conqueror.Components;
//using ConquerorMod.Characters.Survivors.Conqueror.Content;


namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class Eye : BaseSkillState
    {
        public static float baseDuration = .5f;
        public static float baseFireTime = .5f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;


        private float baseMaxUtilityStock;
        private float utilityStock;

        //nme teleport
        private BlastAttack bleeddetector;
        private float bleeddetectorDamageCoefficient = 0f;
        private BlastAttack bleedburst;
        private float bleedburstDamageCoefficient = 2f;

        //self teleport
        private Ray aimRay;
        private float blinkDistance = 15f;
        private float extendedRange = 30f;
        private BlastAttack bleedblast;
        private float bleedblastDamageCoefficient = ConquerorStaticValues.warpeyeDamageCoefficient;
        private ChildLocator childLocator;
        private Vector3 forwardDirection;
        private Animator animator;
        private Vector3 blinkVector = Vector3.zero;
        private Vector3 initialPosition = Vector3.zero;

        private Transform modelTransform;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;


        //vfx
        private GameObject transmitterExplode = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/TeleportOnLowHealth/TeleportOnLowHealthVFX.prefab").WaitForCompletion();
        private GameObject shatterspleenExplode = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion();
        private GameObject voidspikeExplode = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeExplosion.prefab").WaitForCompletion();
        private GameObject shatterspleenImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Impact.prefab").WaitForCompletion();
        private GameObject impbossBlink = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            if (characterBody.GetComponent<ConquerorController>().isUsingMunch)
            {
                outer.SetNextStateToMain(); // immediately cancel to base state
                return;
            }

            characterBody.GetComponent<ConquerorController>().isUsingMunch = true;

            duration = baseDuration / attackSpeedStat;
            fireTime = baseFireTime / attackSpeedStat;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;

            characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, duration);

            CreateTransmitterExploFX(Util.GetCorePosition(characterBody.gameObject));

            if (characterBody.GetComponent<ConquerorController>().distanceToOwner > 18)
            {
                Util.PlaySound("Play_voidman_R_activate", gameObject);
                Util.PlaySound("Play_chef_skill1_return", gameObject);
                Util.PlaySound("Play_imp_attack_blink", gameObject);

                this.modelTransform = base.GetModelTransform();
                if (this.modelTransform)
                {
                    this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                    this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
                }
                if (this.characterModel)
                {
                    CharacterModel characterModel = this.characterModel;
                    int num = characterModel.invisibilityCount;
                    characterModel.invisibilityCount = num + 1;
                }
                if (this.hurtboxGroup)
                {
                    HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                    int num = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                    hurtBoxGroup.hurtBoxesDeactivatorCounter = num;
                }

                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
            }
            else
            {
                Util.PlaySound("Play_ui_obj_eradicator_open", gameObject);
                Util.PlaySound("Play_chef_skill1_return", gameObject);


                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
            }
            initialPosition = Util.GetCorePosition(base.gameObject);

            base.OnEnter();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasFired)
            {
                if (fixedAge >= fireTime)
                {
                    Fire();
                    hasFired = true;
                }
                base.characterMotor.velocity = Vector3.zero;
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
                Vector3 body = Util.GetCorePosition(base.gameObject);

                if (characterBody.GetComponent<ConquerorController>().distanceToOwner > 18)
                {

                    Ray aimRay = base.GetAimRay();
                    Vector3 origin = base.characterBody.corePosition;
                    Vector3 direction = aimRay.direction.normalized;
                    float maxDistance = 24f;

                    RaycastHit hit;
                    Vector3 teleportDestination;

                    if (Physics.Raycast(origin, direction, out hit, maxDistance, LayerIndex.world.mask | LayerIndex.enemyBody.mask))
                    {
                        teleportDestination = hit.point;
                    }
                    else
                    {
                        teleportDestination = origin + direction.normalized * maxDistance;
                    }

                    CreateBlinkFX(origin, teleportDestination);

                    base.characterMotor.Motor.SetPosition(teleportDestination);

                    CreateVoidspikeExploFX(teleportDestination);
                    CreateShatterspleenImpactFX(body, 20f);

                    //ProcChainMask procChainMask = default(ProcChainMask);
                    Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                    Util.PlaySound("Play_gup_step", gameObject);
                    Util.PlaySound("Play_voidDevastator_step", gameObject);

                    bleedblast = new BlastAttack();
                    bleedblast.radius = 20f;
                    bleedblast.attacker = gameObject;
                    bleedblast.inflictor = gameObject;
                    bleedblast.teamIndex = TeamIndex.Player;
                    bleedblast.procCoefficient = 1f;
                    bleedblast.baseForce = 0;
                    bleedblast.canRejectForce = false;
                    bleedblast.falloffModel = BlastAttack.FalloffModel.None;
                    bleedblast.baseDamage = bleedblastDamageCoefficient * damageStat;
                    bleedblast.AddModdedDamageType(DamageTypes.BleedOnHitbutCooler);
                    bleedblast.crit = RollCrit();
                    bleedblast.position = teleportDestination;
                    bleedblast.Fire();

                    base.characterMotor.velocity.y = 0f;
                    base.characterMotor.Motor.ForceUnground(0.1f);
                    if (NetworkServer.active)
                    {
                        Util.CleanseBody(base.characterBody, true, false, false, true, true, true);
                    }
                }
                else
                {
                    Util.PlaySound("Play_gup_step", gameObject);
                    Util.PlaySound("Play_imp_overlord_attack1_pop", gameObject);
                    Util.PlaySound("Play_voidDevastator_step", base.gameObject);
                    Util.PlaySound("Play_nullifier_attack1_summon", gameObject);

                    CreateShatterspleenImpactFX(body, 7f);
                    float detectionRadius = 13f;

                    Vector3 origin = characterBody.corePosition;
                    Ray aimRay = base.GetAimRay();
                    Vector3 direction = aimRay.direction.normalized;


                    SphereSearch search = new SphereSearch
                    {
                        origin = origin,
                        radius = detectionRadius,
                        mask = LayerIndex.entityPrecise.mask,
                        queryTriggerInteraction = QueryTriggerInteraction.Collide
                    };

                    List<HurtBox> hurtBoxBuffer = new List<HurtBox>();
                    search.RefreshCandidates();
                    search.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(gameObject)));
                    search.OrderCandidatesByDistance();
                    search.FilterCandidatesByDistinctHurtBoxEntities();
                    search.GetHurtBoxes(hurtBoxBuffer);
                    foreach (HurtBox hurtBox in hurtBoxBuffer)
                    {
                        HealthComponent hc = hurtBox.healthComponent;
                        if (!hc) continue;

                        CharacterBody nmebody = hc.body;
                        if (!nmebody || nmebody == characterBody) continue;

                        Vector3 toTarget = (nmebody.corePosition - origin).normalized;
                        float angleToTarget = Vector3.Angle(direction, toTarget);

                        if (nmebody.isBoss)
                        {
                            nmebody.AddBuff(ConquerorBuffs.fallDamageImmune);
                            nmebody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                            if (!nmebody.GetComponent<FallDamageImmunityTracker>())
                            {
                                nmebody.gameObject.AddComponent<FallDamageImmunityTracker>();
                            }
                        }
                        bool isFlyer = nmebody.isFlying ||
                                       (nmebody.characterMotor &&
                                       (nmebody.characterMotor.isFlying || !nmebody.characterMotor.isGrounded));

                        float delay = UnityEngine.Random.Range(0.8f, 1f);
                        Vector3 targetPosition = body + toTarget * 6f;

                        if (Physics.Raycast(targetPosition + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, LayerIndex.world.mask))
                        {
                            targetPosition = hit.point + Vector3.up * 0.1f;
                        }

                        Util.PlaySound("Play_voidDevastator_m2_secondary_explo", nmebody.gameObject);
                        RoR2.Run.instance.StartCoroutine(TeleportandExplodeEnemyAfterDelay(nmebody, targetPosition, delay));

                        /*bleeddetector = new BlastAttack();
                    bleeddetector.radius = 50f;
                    bleeddetector.attacker = gameObject;
                    bleeddetector.inflictor = gameObject;
                    bleeddetector.teamIndex = TeamIndex.Player;
                    bleeddetector.procCoefficient = 0f;
                    //pullblast.baseForce = -2000;
                    bleeddetector.canRejectForce = false;
                    bleeddetector.falloffModel = BlastAttack.FalloffModel.None;
                    bleeddetector.baseDamage = 0;
                    bleeddetector.damageType = DamageType.Stun1s;
                    bleeddetector.crit = RollCrit();
                    bleeddetector.position = body;
                    BlastAttack.Result targetsHit = bleeddetector.Fire();
                    Log.Debug("Bleeddetector targets hit:" + targetsHit.hitCount);
                    for (int i = 0; i < targetsHit.hitCount; i++)
                    {
                        HealthComponent hc = targetsHit.hitPoints[i].hurtBox.healthComponent;
                        CharacterBody nmebody = hc.body;

                        /*int bleedStacks = 0;
                        DotController dotController = DotController.FindDotController(nmebody.gameObject);
                        if (dotController != null)
                        {
                            foreach (var dot in dotController.dotStackList)
                            {
                                if (dot.dotIndex == DotController.DotIndex.Bleed)
                                {
                                    bleedStacks++;
                                }
                            }
                            if (bleedStacks > 0)
                            {
                                for (int j = dotController.dotStackList.Count - 1; j >= 0; j--)
                                {
                                    var dotStack = dotController.dotStackList[j];
                                    if (dotStack.dotIndex == DotController.DotIndex.Bleed)
                                    {
                                        dotController.RemoveDotStackAtServer(j);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Debug($"[CONQUEROR EYE] DOTCONTROLLER NULL");
                        }


                        bool isFlyer = nmebody.isFlying || (nmebody.characterMotor && (nmebody.characterMotor.isFlying || !nmebody.characterMotor.isGrounded));

                        Vector3 enemyPosition = nmebody.corePosition;
                        Vector3 relativeDirection = (enemyPosition - body);
                        relativeDirection.Normalize();

                        float distance = 6f;
                        float delay = UnityEngine.Random.Range(.8f, 1f);


                        //safetp pos
                        Vector3 targetPosition = body + relativeDirection * distance;
                        RaycastHit hit;
                        if (Physics.Raycast(targetPosition + Vector3.up * 5f, Vector3.down, out hit, 10f, LayerIndex.world.mask))
                        {
                            targetPosition = hit.point + Vector3.up * 0.1f;
                        }


                        Util.PlaySound("Play_voidDevastator_m2_secondary_explo", nmebody.gameObject);

                        RoR2.Run.instance.StartCoroutine(TeleportandExplodeEnemyAfterDelay(nmebody, targetPosition, delay));

                        //Log.Debug($"Bleedstacks on target hit: {bleedStacks}");
                    }*/

                    }
                }
            }
        }

        private IEnumerator TeleportandExplodeEnemyAfterDelay(CharacterBody nmebody, Vector3 targetPosition, float delay)
        {
            //if (bleedstacks > 0)
            //{
            CreateTransmitterExploFX(Util.GetCorePosition(nmebody.gameObject));
            yield return new WaitForSeconds(delay);

            //teleport
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
            this.CreateBlinkFX(Util.GetCorePosition(nmebody.gameObject), Util.GetCorePosition(base.gameObject));
            //explode
            //yield return new WaitForSeconds(.5f);

            /*if (nmebody != null)
            {
                bleedburst = new BlastAttack();
                bleedburst.radius = 7f;
                bleedburst.attacker = gameObject;
                bleedburst.inflictor = gameObject;
                bleedburst.teamIndex = TeamIndex.Player;
                bleedburst.procCoefficient = 1f;
                //eyeblastpull.baseForce = -2000;
                bleedburst.canRejectForce = false;
                bleedburst.falloffModel = BlastAttack.FalloffModel.Linear;
                bleedburst.baseDamage = ConquerorStaticValues.specialeyeDamageCoefficient * damageStat;
                bleedburst.damageType = DamageType.Generic;
                bleedburst.crit = RollCrit();
                bleedburst.position = nmebody.corePosition;
                bleedburst.Fire();

                CreateShatterspleenExploFX(Util.GetCorePosition(nmebody));

                Util.PlaySound("Play_voidDevastator_m2_secondary_explo", nmebody.gameObject);
                Util.PlaySound("Play_imp_overlord_attack1_pop", nmebody.gameObject);
                Util.PlaySound("Play_voidDevastator_step", nmebody.gameObject);
            */
            //}
            //}
        }

        public override void OnExit()
        {
            /*if (NetworkServer.active)
            {
                baseMaxUtilityStock = (float)base.skillLocator.GetSkill(SkillSlot.Utility).maxStock;
                utilityStock = (float)base.skillLocator.GetSkill(SkillSlot.Utility).stock;
                if (utilityStock < baseMaxUtilityStock)
                {
                    GenericSkill skill = base.skillLocator.GetSkill(SkillSlot.Utility);
                    int stock = skill.stock;
                    skill.stock = stock + 1;
                }
            }*/
            if (this.characterModel)
            {
                CharacterModel characterModel = this.characterModel;
                int num = characterModel.invisibilityCount;
                characterModel.invisibilityCount = num - 1;
            }
            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int num = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = num;
            }

            characterBody.GetComponent<ConquerorController>().isUsingMunch = false;
            base.OnExit();
            outer.SetNextStateToMain();
        }

        //effects
        private void CreateBlinkFX(Vector3 origin, Vector3 destination)
        {
            EffectData effectData = new EffectData();
            effectData.origin = origin;
            effectData.rotation = Util.QuaternionSafeLookRotation(origin - destination);
            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, false);
        }
        private void CreateTransmitterExploFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 3f;
            EffectManager.SpawnEffect(transmitterExplode, effectData, false);
        }
        private void CreateShatterspleenExploFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 7f;
            EffectManager.SpawnEffect(shatterspleenExplode, effectData, false);
        }
        private void CreateShatterspleenImpactFX(Vector3 origin, float scale)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = scale;
            EffectManager.SpawnEffect(shatterspleenImpact, effectData, false);
        }
        private void CreateVoidspikeExploFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 5f;
            EffectManager.SpawnEffect(voidspikeExplode, effectData, false);
        }
        private void CreateImpOverlordDepartFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 10f;
            EffectManager.SpawnEffect(impbossBlink, effectData, false);
        }
    }
}