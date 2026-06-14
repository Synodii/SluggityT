using System;
using System.Collections.Generic;
using HG;
using Rewired;
using RoR2.Networking;
using RoR2.UI;
using UnityEngine;
using RoR2.CameraModes;
using RoR2;
using KinematicCharacterController;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;


namespace ConquerorMod.Survivors.Conqueror.Components
{
    public class CameraModeWallClimb : CameraModePlayerBasic
    {
        static CameraModeWallClimb()
        {
            IL.RoR2.PlayerCharacterMasterController.Update += PatchMovement;
        }

        private static void PatchMovement(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int locCameraRig = 0;
            int locMovementInputs = 0;
            int locMoveVector = 0;

            bool flag = c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<NetworkUser>("get_networkUser"),
                x => x.MatchLdloca(out var locLocalUser),
                x => x.MatchLdloca(out var locPlayer),
                x => x.MatchLdloca(out locCameraRig),
                x => x.MatchCallOrCallvirt<bool>(nameof(PlayerCharacterMasterController.CanSendBodyInput)),
                x => x.MatchBrfalse(out var junkLabel));

            flag = flag && c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Quaternion>(nameof(Quaternion.Euler)),
                x => x.MatchLdloc(out locMovementInputs),
                x => x.MatchLdfld<Vector2>(nameof(Vector2.x)),
                x => x.MatchLdcR4(0),
                x => x.MatchLdloc(locMovementInputs),
                x => x.MatchLdfld<Vector2>(nameof(Vector2.y)),
                x => x.MatchNewobj<Vector3>(),
                x => x.MatchCallOrCallvirt<Quaternion>("op_Multiply"),
                x => x.MatchStloc(out locMoveVector));

            if (!flag)
            {
                // Use the project's own logger instead of the missing ModLogger
                Log.Warning("CameraModeWallClimb: Failed to find IL injection point in PlayerCharacterMasterController.Update!");
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, locCameraRig);
            c.Emit(OpCodes.Ldloc, locMovementInputs);
            c.Emit(OpCodes.Ldloc, locMoveVector);
            c.EmitDelegate<Func<PlayerCharacterMasterController, CameraRigController, Vector2, Vector3, Vector3>>(
                (that, cameraRigController, movementInput, moveVector) =>
                {
                    if (that.bodyMotor is ClimbCharacterMotor && cameraRigController.cameraMode is CameraModeWallClimb)
                    {
                        var cameraMode = cameraRigController.cameraMode as CameraModeWallClimb;
                        if (cameraMode.camToRawInstanceData.TryGetValue(cameraRigController, out var rawInstanceData))
                            cameraMode.RewriteMovementInput(rawInstanceData, movementInput, out moveVector);
                    }
                    return moveVector;
                });
            c.Emit(OpCodes.Stloc, locMoveVector);
        }

        private void RewriteMovementInput(object rawInstancedata, in Vector2 movementInput, out Vector3 moveVector)
        {
            var instanceData = (ClimbInstanceData)rawInstancedata;
            var pitchYaw = instanceData.pitchYaw;
            moveVector = instanceData.cameraRotationOffset * (Quaternion.Euler(pitchYaw.pitch, pitchYaw.yaw, 0f) * new Vector3(movementInput.x, 0f, movementInput.y));
        }

        public override void OnInstallInternal(object rawInstancedata, CameraRigController cameraRigController)
        {
            base.OnInstallInternal(rawInstancedata, cameraRigController);
            var cid = (ClimbInstanceData)rawInstancedata;
            cid.cameraRotationOffset = Quaternion.identity;
            cid.cameraRotationVelocity = 0f;
        }

        public override void CollectLookInputInternal(object rawInstanceData, in CameraModeContext context, out CollectLookInputResult output)
        {
            var instanceData = (ClimbInstanceData)rawInstanceData;
            CameraInfo cameraInfo = context.cameraInfo;
            ViewerInfo viewerInfo = context.viewerInfo;

            Player inputPlayer = viewerInfo.inputPlayer;
            UserProfile userProfile = viewerInfo.userProfile;
            ICameraStateProvider overrideCam = cameraInfo.overrideCam;

            output.lookInput = Vector3.zero;
            if (!viewerInfo.hasCursor && viewerInfo.inputPlayer != null && userProfile != null && !viewerInfo.isUIFocused &&
                (!(UnityEngine.Object)overrideCam || overrideCam.IsUserLookAllowed(cameraInfo.cameraRigController)))
            {
                Vector2 mouseInput = new Vector2(inputPlayer.GetAxisRaw(2), inputPlayer.GetAxisRaw(3));
                Vector2 stickInput = new Vector2(inputPlayer.GetAxisRaw(16), inputPlayer.GetAxisRaw(17));

                mouseInput.x = userProfile.mouseLookInvertX ? -mouseInput.x : mouseInput.x;
                mouseInput.y = userProfile.mouseLookInvertY ? -mouseInput.y : mouseInput.y;

                stickInput.x = userProfile.stickLookInvertX ? -stickInput.x : stickInput.x;
                stickInput.y = userProfile.stickLookInvertY ? -stickInput.y : stickInput.y;

                PerformStickPostProcessing(instanceData, context, ref stickInput);

                float mouseLookSensitivity = userProfile.mouseLookSensitivity;
                float stickLookSensitivity = userProfile.stickLookSensitivity * CameraRigController.aimStickGlobalScale.value * 45f;

                Vector2 mouseLookScale = new Vector2(userProfile.mouseLookScaleX, userProfile.mouseLookScaleY);
                Vector2 stickLookScale = new Vector2(userProfile.stickLookScaleX, userProfile.stickLookScaleY);

                mouseInput *= mouseLookScale * mouseLookSensitivity;
                stickInput *= stickLookScale * stickLookSensitivity;

                PerformAimAssist(context, ref stickInput);

                stickInput *= Time.deltaTime;
                output.lookInput = mouseInput + stickInput;
            }
            output.lookInput *= cameraInfo.previousCameraState.fov / cameraInfo.baseFov;
            if (context.targetInfo.isSprinting && CameraRigController.enableSprintSensitivitySlowdown.value)
            {
                output.lookInput *= 0.5f;
            }
        }

        public override void ApplyLookInputInternal(object rawInstanceData, in CameraModeContext context, in ApplyLookInputArgs input)
        {
            var instanceData = (ClimbInstanceData)rawInstanceData;
            TargetInfo targetInfo = context.targetInfo;
            if (targetInfo.isViewerControlled)
            {
                instanceData.pitchYaw.pitch = Mathf.Clamp(instanceData.pitchYaw.pitch - input.lookInput.y, instanceData.minPitch, instanceData.maxPitch);
                instanceData.pitchYaw.yaw += input.lookInput.x;
                if (targetInfo.networkedViewAngles && targetInfo.networkedViewAngles.hasEffectiveAuthority)
                {
                    targetInfo.networkedViewAngles.viewAngles = instanceData.pitchYaw;
                    return;
                }
            }
            else
            {
                if (targetInfo.networkedViewAngles)
                {
                    instanceData.pitchYaw = targetInfo.networkedViewAngles.viewAngles;
                    return;
                }
                if (targetInfo.inputBank)
                {
                    instanceData.SetPitchYawFromLookVector(targetInfo.inputBank.aimDirection);
                }
            }
        }

        public override void UpdateInternal(object rawInstanceData, in CameraModeContext context, out UpdateResult result)
        {
            // Must assign result before any early return — CS0177
            result = default;

            var instanceData = (ClimbInstanceData)rawInstanceData;
            CameraRigController cameraRigController = context.cameraInfo.cameraRigController;
            CameraTargetParams targetParams = context.targetInfo.targetParams;
            float fov = context.cameraInfo.baseFov;
            Quaternion cameraRotation = context.cameraInfo.previousCameraState.rotation;
            Vector3 position = context.cameraInfo.previousCameraState.position;
            GameObject firstPersonTarget = null;

            float num = cameraRigController.baseFov;
            if (context.targetInfo.isSprinting)
                num *= 1.3f;
            instanceData.neutralFov = Mathf.SmoothDamp(instanceData.neutralFov, num, ref instanceData.neutralFovVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);

            var cameraParams = CharacterCameraParamsData.basic;
            cameraParams.fov = instanceData.neutralFov;
            Vector2 recoil = Vector2.zero;
            if (targetParams)
            {
                CharacterCameraParamsData.Blend(targetParams.currentCameraParamsData, ref cameraParams, 1f);
                fov = cameraParams.fov.value;
                recoil = targetParams.recoil;
            }
            if (cameraParams.isFirstPerson.value)
                firstPersonTarget = context.targetInfo.target;

            instanceData.minPitch = cameraParams.minPitch.value;
            instanceData.maxPitch = cameraParams.maxPitch.value;
            var pitch = Mathf.Clamp(instanceData.pitchYaw.pitch + recoil.y, instanceData.minPitch, instanceData.maxPitch);
            var yaw = Mathf.Repeat(instanceData.pitchYaw.yaw + recoil.x, 360f);
            Vector3 desiredCamPosition = CalculateTargetPivotPosition(context);

            if (context.targetInfo.target)
            {
                ClimbCharacterMotor climbMotor = context.targetInfo.body.characterMotor as ClimbCharacterMotor;

                // Guard: if body doesn't have ClimbCharacterMotor (e.g. spectating another character), fall back gracefully
                if (climbMotor == null)
                {
                    cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
                }
                else
                {
                    Quaternion rotationOffset = instanceData.cameraRotationOffset;
                    Vector3 currentUp = rotationOffset * Vector3.up;

                    Vector3 groundNormal = !climbMotor.isClimbing
                        ? Vector3.up
                        : Vector3.Slerp(climbMotor.Motor.CharacterUp, climbMotor.Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-climbMotor.cameraOrientationSharpness * Time.deltaTime));

                    rotationOffset = Util.SmoothDampQuaternion(rotationOffset,
                        Quaternion.FromToRotation(currentUp, groundNormal) * rotationOffset,
                        ref instanceData.cameraRotationVelocity, 0.75f, climbMotor.cameraMaxAdjustmentSpeed, Time.deltaTime);
                    instanceData.cameraRotationOffset = rotationOffset;

                    cameraRotation = Quaternion.Euler(pitch, yaw, 0) * rotationOffset;
                }

                Vector3 direction = desiredCamPosition + cameraRotation * cameraParams.idealLocalCameraPos.value - desiredCamPosition;
                float dirMagnitude = direction.magnitude;

                float num5 = (1f + pitch / -90f) * 0.5f;
                dirMagnitude *= Mathf.Sqrt(1f - num5);
                if (dirMagnitude < 0.25f)
                    dirMagnitude = 0.25f;

                float wallRay = cameraRigController.Raycast(new Ray(desiredCamPosition, direction), dirMagnitude, cameraParams.wallCushion.value - 0.01f);
                Debug.DrawRay(desiredCamPosition, direction.normalized * dirMagnitude, Color.yellow, Time.deltaTime);
                Debug.DrawRay(desiredCamPosition, direction.normalized * wallRay, Color.red, Time.deltaTime);

                if (instanceData.currentCameraDistance >= wallRay)
                {
                    instanceData.currentCameraDistance = wallRay;
                    instanceData.cameraDistanceVelocity = 0f;
                }
                else
                {
                    instanceData.currentCameraDistance = Mathf.SmoothDamp(instanceData.currentCameraDistance, wallRay, ref instanceData.cameraDistanceVelocity, 0.5f);
                }
                position = desiredCamPosition + direction.normalized * instanceData.currentCameraDistance;
            }

            result.cameraState.position = position;
            result.cameraState.rotation = cameraRotation;
            result.cameraState.fov = fov;
            result.showSprintParticles = context.targetInfo.isSprinting;
            result.firstPersonTarget = firstPersonTarget;
            UpdateCrosshair(rawInstanceData, context, result.cameraState, desiredCamPosition, out result.crosshairWorldPosition);
        }

        // 'new' suppresses the CS0108 hiding warnings — intentional shadowing of base statics
        public new static CameraModeWallClimb playerBasic = new CameraModeWallClimb { isSpectatorMode = false };
        public new static CameraModeWallClimb spectator = new CameraModeWallClimb { isSpectatorMode = true };

        public class ClimbInstanceData : InstanceData
        {
            public Quaternion cameraRotationOffset;
            public float cameraRotationVelocity;
        }
    }
}