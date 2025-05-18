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
        public Vector3 backpackTargetPos; // where the rope landed
        public Vector3 playerPos;

        private Vector3 projectilespawnPosition;

        void Awake()
        {
            Log.Debug($"AWAKE position: {transform.position}");
            Log.Debug($"Awake Rigidbody position: {rb.position}, velocity: {rb.velocity}, isKinematic: {rb.isKinematic}");
        }

        void Start()
        {
            Log.Debug($"START position: {transform.position}");
            Log.Debug($"START Rigidbody position: {rb.position}, velocity: {rb.velocity}, isKinematic: {rb.isKinematic}");

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.position = projectilespawnPosition;
            ownerTransform = controller.owner.transform;
            objTracker = ownerTransform.GetComponent<ConquerorController>();


            if (objTracker != null)
            {
                objTracker.deployedBackpack.Add(this);
            }
            backpackCollider.enabled = true;
            stickComponent.stickEvent.AddListener(OnStickEvent);
        }
        void OnStickEvent()
        {
            if (isFlying) return;

            Log.Debug("[RopeBackpack] STUCK POSITION: " + transform.position);
            

            //remove motion and collision in order to prevent enemy sliding
            backpackCollider.enabled = false;
            rb.velocity = Vector3.zero;
            //rb.drag = 0;
            //rb.angularDrag = 0;
            //rb.mass = 0; 
            rb.useGravity = false;
            projSimple.SetForwardSpeed(0);
        }
        void Update()
        {
            //lineRenderer.SetPosition(0, transform.position);
            //lineRenderer.SetPosition(1, Vector3.Lerp(transform.position, objTracker.fishingPoleTip.position, 0.5f) + (Vector3.up * 0.1f));
            //lineRenderer.SetPosition(2, objTracker.fishingPoleTip.position);
        }

        void FixedUpdate()
        {
            playerPos = objTracker.characterBody.footPosition;

            if (controller.owner == null) Destroy(gameObject);

            objTracker.distanceToOwner = Vector3.Distance(transform.position, ownerTransform.position);
            if (!isFlying)
            {
                if (objTracker.distanceToOwner > autoTriggerDistance)
                {
                    objTracker.isManualRecall = false;
                    StartCoroutine(DelayedFlyBack());
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
            yield return new WaitForSeconds(1f);

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

            //fling
            backpackTargetPos = transform.position;

            if (objTracker.distanceToOwner > 18f && objTracker.characterBody && objTracker.characterMotor && objTracker.isManualRecall)
            {
                Vector3 pullVelocity = GetPullVelocity(backpackTargetPos, playerPos, false);

                objTracker.characterMotor.velocity = pullVelocity;
                objTracker.characterMotor.Motor.ForceUnground(0.1f);
                Debug.DrawLine(playerPos, backpackTargetPos, Color.cyan, 2f);
            }

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