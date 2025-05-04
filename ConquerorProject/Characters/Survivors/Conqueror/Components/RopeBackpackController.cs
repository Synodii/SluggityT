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
        public LineRenderer lineRenderer;
        public BuffWard buffward;

        ConquerorController objTracker;

        bool isFlying = false;
        float distanceToOwner;
        float autoTriggerDistance = ConquerorStaticValues.autoRecallDistance;
        float homeToBodyDistance = 50;
        float homingForce = 5f;
        float homingDeceleration = 0.33f;
        float returnForceBase = 1;
        Transform ownerTransform;
        float timeFlying = 0;
        float minTimeBeforeReturning = 0.25f;
        float maxFlyTime = 2;
        private bool isWaitingToRecall;

        void Awake()
        {
        }

        /*void Start()
        {
            ownerTransform = controller.owner.transform;
            objTracker = ownerTransform.GetComponent<ConquerorController>();
            objTracker.deployedBackpack.Add(this);
            stickComponent.stickEvent.AddListener(OnStickEvent);

            TeamFilter tf = GetComponent<TeamFilter>();
            if (tf && controller.teamFilter)
            {
                tf.teamIndex = controller.teamFilter.teamIndex;
                Debug.Log($"TeamFilter set to: {tf.teamIndex}");
            }

            if (buffward)
            {
                buffward.teamFilter = tf;
                Debug.Log($"BuffWard team set to: {buffward.teamFilter?.teamIndex}");
            }

            // If you have a child buffward, repeat the assignment there
            foreach (BuffWard childBuffWard in GetComponentsInChildren<BuffWard>())
            {
                if (childBuffWard != buffward)
                {
                    childBuffWard.teamFilter = tf;
                }
            }

            Debug.Log($"BuffWard initialized. Buff: {buffward?.buffDef}, Radius: {buffward?.radius}, TeamIndex: {buffward.teamFilter?.teamIndex}");

        }*/
        void Start()
        {
            ownerTransform = controller.owner?.transform;
            objTracker = ownerTransform?.GetComponent<ConquerorController>();

            if (objTracker != null)
            {
                objTracker.deployedBackpack.Add(this);
            }

            stickComponent.stickEvent.AddListener(OnStickEvent);

            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
            }
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
                objTracker.isManualRecall = false;
                StartCoroutine(DelayedFlyBack());
            }
            if (isFlying)
            {
                timeFlying += Time.fixedDeltaTime;
                if ((distanceToOwner <= homeToBodyDistance && timeFlying >= minTimeBeforeReturning) || timeFlying >= maxFlyTime)
                {
                    Vector3 vel = (ownerTransform.position - transform.position).normalized * rb.mass * Mathf.Max(homingForce - distanceToOwner, 1) * timeFlying;
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
                if (distanceToOwner <= 3)
                {
                    projSimple.lifetime = 0.0001f;
                    if (!objTracker.isManualRecall)
                    {
                        objTracker.ResetRopeSkill();
                    }
                }
            }
        }
        private IEnumerator DelayedFlyBack()
        {
            isWaitingToRecall = true;
            yield return new WaitForSeconds(.5f);

            distanceToOwner = Vector3.Distance(transform.position, ownerTransform.position);
            if (!isFlying && distanceToOwner > autoTriggerDistance)
            {
                StartCoroutine(FlyBack());
            }

            isWaitingToRecall = false;
        }

        public IEnumerator FlyBack()
        {
            Log.Debug("[BP] Flyback Start");
            Util.PlaySound("Play_scav_backpack_open", gameObject);


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
    }
}