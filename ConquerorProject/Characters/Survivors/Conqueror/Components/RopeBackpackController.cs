using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using static EntityStates.BaseState;
using EntityStates;
using UnityEngine.Events;
using ConquerorMod.Survivors.Conqueror.Components;
using ConquerorMod.Survivors.Conqueror.SkillStates;

namespace ConquerorMod.Survivors.Conqueror.Components
{
    public class RopeBackpackController : MonoBehaviour
    {
        public Rigidbody rb;
        public ProjectileStickOnImpact stickComponent;
        public ProjectileController controller;
        public ProjectileDamage projectileDamage;
        public CapsuleCollider backpackCollider;
        public ProjectileOverlapAttack projOverlap;
        public ProjectileSimple projSimple;
        public BuffWard buffward;
        public Transform ropeIndicator;
        ConquerorController objTracker;

        bool isFlying = false;
        //float distanceToOwner; 
        float autoTriggerDistance = 75;
        float autoDropDistance = 65;
        float homeToBodyDistance = 50;
        float homingForce = 5f;
        float homingDeceleration = 0.33f;
        float returnForceBase = 1;
        Transform ownerTransform;
        float timeFlying = 0;
        float minTimeBeforeReturning = 0.25f;
        float maxFlyTime = 1.5f;

        private bool isWaitingToRecall;
        bool hasStuck = false;
        bool isImmunetoFallDamage = false;
        public Vector3 backpackTargetPos; // where the rope landed
        public Vector3 playerPos;

        private Vector3 projectilespawnPosition;

        void Awake()
        {
        }

        void Start()
        {
            ownerTransform = controller.owner.transform;

            objTracker = ownerTransform.GetComponent<ConquerorController>();

            if (objTracker != null)
            {
                objTracker.deployedBackpack.Add(this);
            }
            backpackCollider.enabled = true;
            stickComponent.stickEvent.AddListener(OnStickEvent);
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;

        }
        void OnStickEvent()
        {
            if (isFlying) return;
            Log.Debug("Has Stuck");

            hasStuck = true;

            ropeIndicator.localScale = Vector3.one * 2;
            buffward.enabled = true;
            objTracker.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            isImmunetoFallDamage = true;

            backpackTargetPos = transform.position;

            if (objTracker.distanceToOwner > 10f && objTracker.characterBody && objTracker.characterMotor)
            {
                Vector3 pullVelocity = GetPullVelocity(backpackTargetPos, playerPos, false);
                Log.Debug($"Applying PullVelocity: {pullVelocity}");

                objTracker.characterMotor.Motor.ForceUnground(0.5f);
                objTracker.characterMotor.Motor.GroundingStatus = default; // optional

                objTracker.characterMotor.velocity = pullVelocity;
                objTracker.characterMotor.disableAirControlUntilCollision = true;
            }
        }
        void Update()
        {
            if (isFlying)
            {
                buffward.radius = Mathf.Lerp(18f, 0f, Time.deltaTime);
            }
        }


        void FixedUpdate()
        {
            playerPos = objTracker.characterBody.footPosition;
             
            if (controller.owner == null) Destroy(gameObject);

            if (objTracker.characterMotor.isGrounded && isImmunetoFallDamage)
            {
                objTracker.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                objTracker.characterMotor.velocity = Vector3.zero;
                isImmunetoFallDamage = false;
            }

            objTracker.distanceToOwner = Vector3.Distance(transform.position, ownerTransform.position);
            if (!isFlying)
            {
                if (objTracker.distanceToOwner > autoTriggerDistance)
                {
                    objTracker.isManualRecall = false;
                    StartCoroutine(FlyBack());
                }
                else if (objTracker.distanceToOwner > autoDropDistance)
                {
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    rb.velocity = Vector3.down * 30f;
                    rb.angularVelocity = Vector3.zero;

                    if (projSimple)
                    {
                        projSimple.desiredForwardSpeed = 0;
                    }
                }
            }
            if (isFlying)
            {
                timeFlying += Time.fixedDeltaTime;
                if ((objTracker.distanceToOwner <= homeToBodyDistance && timeFlying >= minTimeBeforeReturning) || timeFlying >= maxFlyTime)
                {
                    Vector3 vel = (ownerTransform.position - transform.position).normalized * rb.mass * Mathf.Max(homingForce - objTracker.distanceToOwner, 1) * timeFlying;
                    rb.AddForce(vel, ForceMode.VelocityChange);
                    if (rb.velocity.magnitude > 1)
                    {
                        rb.velocity *= homingDeceleration;
                    }
                    else
                    {
                        projSimple.lifetime = 0.0001f;
                    }
                }
                if (objTracker.distanceToOwner <= 3)
                {
                    projSimple.lifetime = 0.0001f;
                    if (!objTracker.isManualRecall)
                    {
                        objTracker.ResetRopeSkill();
                    }
                }
            }
            
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimBody == objTracker.characterBody)
            {
                projSimple.lifetime = 0.0001f;
            }
        }

        private void OnDestroy()
        {
            objTracker.ResetRopeSkill();
            objTracker.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            isImmunetoFallDamage = false;
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
        }

        private Vector3 GetPullVelocity(Vector3 targetPos, Vector3 startPos, bool isFlyer)
        {
            Vector3 toTarget = targetPos - startPos;
            Vector2 xz = new Vector2(toTarget.x, toTarget.z);
            float distanceXZ = xz.magnitude;

            //distanceXZ * x, y, z
            float timeToTarget = Mathf.Clamp(distanceXZ * 1f, 1.2f, 1f);

            float ySpeed = isFlyer
                ? toTarget.y / timeToTarget
                : Mathf.Max(Trajectory.CalculateInitialYSpeed(timeToTarget, toTarget.y), 6f);

            return new Vector3(
                xz.x / timeToTarget,
                ySpeed,
                xz.y / timeToTarget
            );
        }


        private IEnumerator DelayedFlyBack()
        {
            isWaitingToRecall = true;
            yield return new WaitForSeconds(0);

            if (!isFlying && objTracker.distanceToOwner > autoTriggerDistance)
            {
                StartCoroutine(FlyBack());
            }

            isWaitingToRecall = false;
        }

        public IEnumerator FlyBack()
        {
            Log.Debug("[BP] Flyback Start");
            Util.PlaySound("Play_scav_backpack_open", gameObject);

            transform.localScale = new Vector3(0F, 0F, 0F);

            isFlying = true; //aka is being recalled

            backpackCollider.enabled = true;
            backpackCollider.gameObject.layer = LayerIndex.noCollision.intVal;

            //reset rigid body for motion (previously frozen on impact)
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            stickComponent.Detach();
            rb.detectCollisions = true;
            stickComponent.ignoreCharacters = true;
            stickComponent.ignoreWorld = true;
            rb.isKinematic = false;

            yield return new WaitForFixedUpdate();


            //apply return force to hook 
            projSimple.desiredForwardSpeed = 0;
            Vector3 vel = (ownerTransform.position - transform.position).normalized * rb.mass;
            rb.AddForce(vel, ForceMode.VelocityChange);

            /*
            float startWidth = lineRenderer.startWidth;
            float endWidth = lineRenderer.endWidth;

            lineRenderer.startWidth *= 10f;
            lineRenderer.endWidth *= 10f;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;*/
        }

        //bool inHitPause;
        //float hitStopDuration = 0.05f;
        //Vector3 storedVelocity;
        //HitStopCachedState hitStopCachedState;
        //float hitPauseTimer;
        //string playbackRateParam = "SecondaryCast.playbackRate";
    }
}