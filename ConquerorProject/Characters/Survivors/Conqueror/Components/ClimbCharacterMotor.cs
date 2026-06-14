using UnityEngine;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using KinematicCharacterController;
using RoR2.ConVar;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace ConquerorMod.Survivors.Conqueror.Components
{
    public class ClimbCharacterMotor : CharacterMotor
    {
        [Tooltip("How sharply the character should snap to a given surface. Smaller values will result in more precise but less smooth movements.")]
        public float orientationSharpness = 10f;

        [Header("Climbing Camera Settings")]
        [Tooltip("How sharply the camera should snap to the character's downward direction. Smaller values will result in more precise but less smooth movements.")]
        public float cameraOrientationSharpness = 10f;
        [Tooltip("In degrees per second")]
        public float cameraMaxAdjustmentSpeed = 160f;

        public bool isClimbing => isGrounded && Vector3.Angle(Vector3.up, estimatedGroundNormal) >= Motor.MaxStableSlopeAngle;
        public new void Awake()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            body = GetComponent<CharacterBody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            previousPosition = transform.position;
            //Motor.MaxStableSlopeAngle = 70f;
            //Motor.MaxStableDenivelationAngle = 55f;
            Motor.RebuildCollidableLayers();
            if (generateParametersOnAwake)
                GenerateParameters();
            useGravity = gravityParameters.CheckShouldUseGravity();
            isFlying = flightParameters.CheckShouldUseFlight();
        }

        public new void GenerateParameters()
        {
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 currentUp = currentRotation * Vector3.up;
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-orientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
            }
            else
            {
                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-orientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
            }
        }

        public new void PreMove(float deltaTime)
        {
            if (hasEffectiveAuthority)
            {
                float control = acceleration;
                if (!isGrounded)
                    control *= (disableAirControlUntilCollision ? 0f : airControl);

                Vector3 moveDir = moveDirection;
                if (!isFlying && !isClimbing)
                    moveDir.y = 0f;

                if (body.isSprinting)
                {
                    float magnitude = moveDir.magnitude;
                    if (magnitude < 1f && magnitude > 0f)
                        moveDir *= 1f / magnitude;
                }

                Vector3 walkMovement = moveDir * walkSpeed;
                if (!isFlying && !isClimbing)
                    walkMovement.y = velocity.y;

                velocity = Vector3.MoveTowards(velocity, walkMovement, control * deltaTime);

                if (useGravity && !isClimbing)
                {
                    ref float yVelocity = ref velocity.y;
                    yVelocity += Physics.gravity.y * deltaTime;
                    if (isGrounded)
                        yVelocity = Mathf.Max(yVelocity, 0f);
                }
            }
        }
    }
}