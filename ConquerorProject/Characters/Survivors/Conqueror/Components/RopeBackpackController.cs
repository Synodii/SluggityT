using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using static EntityStates.BaseState;
using EntityStates;
using UnityEngine.Events;
using ConquerorMod.Survivors.Conqueror.Components;


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
        public LineRenderer lineRenderer;
        public BuffWard buffward;

        ObjectTracker objTracker;

        bool isFlying = false;
        float distanceToOwner;
        float autoTriggerDistance = 20;
        float homeToBodyDistance = 50;
        float homingForce = 5f;
        float homingDeceleration = 0.33f;
        float returnForceBase = 1;
        Transform ownerTransform;
        float timeFlying = 0;
        float minTimeBeforeReturning = 0.25f;
        float maxFlyTime = 2;
        float BackpackTossVelocity = 30f;

        void Awake()
        {
        }

        void Start()
        {
            ownerTransform = controller.owner.transform;
            objTracker = ownerTransform.GetComponent<ObjectTracker>();
            objTracker.deployedBackpack.Add(this);
            backpackCollider.enabled = true;
            stickComponent.stickEvent.AddListener(OnStickEvent);
            projectileDamage.force = 0;
            projOverlap.onServerHit.AddListener(() => ApplyHitStop(null));
        }
        void OnStickEvent()
        {
            if (isFlying) return;
            //Log.Debug($"[HOOK] STICK EVENT");

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
            if (controller.owner == null) Destroy(gameObject);
            distanceToOwner = Vector3.Distance(transform.position, ownerTransform.position);
            if (!isFlying && distanceToOwner > autoTriggerDistance)
            {
                StartCoroutine(FlyBack());
            }
            if (isFlying)
            {
                timeFlying += Time.fixedDeltaTime;
                // if hook is near player or has been flying for a long time, engange homing to force the hook to quickly return
                if ((distanceToOwner <= homeToBodyDistance && timeFlying >= minTimeBeforeReturning) || timeFlying >= maxFlyTime)
                {
                    Vector3 vel = (ownerTransform.position - transform.position).normalized * rb.mass * Mathf.Max(homingForce - distanceToOwner, 1) * timeFlying;
                    rb.AddForce(vel, ForceMode.VelocityChange);
                    //rb.MovePosition(Vector3.Lerp(rb.position, objTracker.fishingPoleTip.position, homingForce * distanceToOwner));
                    if (rb.velocity.magnitude > 1)
                    {
                        rb.velocity *= homingDeceleration;
                    }
                    else
                    {
                        //TODO refund stock if nothing grabbed.
                        projSimple.lifetime = 0.0001f;
                    }
                }
                if (distanceToOwner <= 3)
                {
                    projSimple.lifetime = 0.0001f;

                }
                UpdateHitStop();
            }
        }
        public IEnumerator FlyBack()
        {
            Log.Debug("[BP] Flyback Start");

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


            // Enable Hitboxes
            projOverlap.enabled = true;
            projectileDamage.damage = objTracker.characterBody.damage * ConquerorStaticValues.ropeduffelretrieveDamageCoefficient;
            projOverlap.damageCoefficient = 1;

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
            lineRenderer.endWidth = endWidth;

        }

        bool inHitPause;
        float hitStopDuration = 0.05f;
        Vector3 storedVelocity;
        HitStopCachedState hitStopCachedState;
        float hitPauseTimer;
        string playbackRateParam = "SecondaryCast.playbackRate";

        void ApplyHitStop(GameObject gameObject)
        {
            if (!inHitPause && ConquerorStaticValues.CurHitStop > 0f)
            {
                storedVelocity = objTracker.characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(objTracker.characterMotor, objTracker.animator, playbackRateParam);
                hitPauseTimer = ConquerorStaticValues.CurHitStop / objTracker.characterBody.attackSpeed;
                inHitPause = true;
            }
        }

        protected void UpdateHitStop()
        {
            hitPauseTimer -= Time.fixedDeltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }

            if (inHitPause)
            {
                objTracker.characterMotor.velocity = Vector3.zero;
                objTracker.animator.SetFloat(playbackRateParam, 0f);
            }
        }

        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, objTracker.characterMotor, objTracker.animator);
            inHitPause = false;
            objTracker.characterMotor.velocity = storedVelocity;
            ConquerorStaticValues.hitStopMod = 1;
        }

        protected HitStopCachedState CreateHitStopCachedState(CharacterMotor characterMotor, Animator animator, string playbackRateAnimationParameter)
        {
            HitStopCachedState result = default(HitStopCachedState);
            result.characterVelocity = new Vector3(characterMotor.velocity.x, Mathf.Max(0f, characterMotor.velocity.y), characterMotor.velocity.z);
            result.playbackName = playbackRateAnimationParameter;
            result.playbackRate = animator.GetFloat(result.playbackName);
            return result;
        }

        protected void ConsumeHitStopCachedState(HitStopCachedState hitStopCachedState, CharacterMotor characterMotor, Animator animator)
        {
            characterMotor.velocity = hitStopCachedState.characterVelocity;
            animator.SetFloat(hitStopCachedState.playbackName, hitStopCachedState.playbackRate);
        }
    }
}