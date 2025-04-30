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

        HashSet<GameObject> objectsHooked = new HashSet<GameObject>();

        UnityEvent<GameObject> onHookEvent = new UnityEvent<GameObject>();

        void Awake()
        {
            /* UI ELEMENTS NOOOO I SHALL NOT
            
            //Log.Debug("[HOOK] New Hook Created ------------------------------------------------------------------------------------------------------------");
            GameObject hi = Instantiate(ConquerorAssets.hookIndicator, transform);
            hi.GetComponent<PositionIndicator>().targetTransform = transform;
            hi.transform.position = Vector3.zero; */
        }

        void Start()
        {
            ownerTransform = controller.owner.transform;
            objTracker = ownerTransform.GetComponent<ObjectTracker>();
            objTracker.deployedBackpack.Add(this);
            backpackCollider.enabled = true;
            stickComponent.stickEvent.AddListener(OnStickEvent);
            projectileDamage.force = 0;
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
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, Vector3.Lerp(transform.position, objTracker.fishingPoleTip.position, 0.5f) + (Vector3.up * 0.1f));
            lineRenderer.SetPosition(2, objTracker.fishingPoleTip.position);
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
            }
        }
        public IEnumerator FlyBack()
        {
            //Log.Debug("[HOOK] Flyback Start");

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
    }
}