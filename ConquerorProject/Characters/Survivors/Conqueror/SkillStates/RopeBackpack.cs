using EntityStates;
using ConquerorMod.Survivors.Conqueror;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static RoR2.CameraTargetParams;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class RopeBackpack : GenericProjectileBaseState
    {
        public class AimB : AimThrowableBase
        {
            float hookRangeGrowRate = 0.5f;
            float hookRangeMax = 70;
            Transform spine;
            RoR2.CameraTargetParams.AimRequest aimRequest;
            public override void OnEnter()
            {
                maxDistance = 4;
                rayRadius = 1;
                projectilePrefab = ConquerorAssets.ropeBackpackProjectilePrefab;
                endpointVisualizerRadiusScale = 1f;
                setFuse = false;
                damageCoefficient = 0;
                useGravity = true;

                spine = GetModelChildLocator().FindChild("PoleEnd");
                base.OnEnter();
                if (base.isAuthority && !KeyIsDown() && !base.IsKeyDownAuthority()) { }
                PlayAnimation("Gesture, Override", "SecondaryCastStart", "SecondaryCast.playbackRate", 0.65f);

                /*if (cameraTargetParams)
                {
                    aimRequest = BuildAimRequest();
                }*/

            }


            /*private RoR2.CameraTargetParams.AimRequest BuildAimRequest()
            {
                RoR2.CharacterCameraParamsData camData = cameraTargetParams.cameraParams.data;
                camData.idealLocalCameraPos.value += new Vector3(1f, -0.1f, -1f);
                CameraParamsOverrideHandle overrideHandle = cameraTargetParams.AddParamsOverride(new CameraParamsOverrideRequest
                {
                    cameraParamsData = camData,
                    priority = 0.1f
                }, 0.5f);
                AimRequest newAimRequest = new AimRequest(AimType.OverTheShoulder, delegate (AimRequest aimRequest)
                {
                    cameraTargetParams.RemoveRequest(aimRequest);
                    cameraTargetParams.RemoveParamsOverride(overrideHandle, 0.5f);
                });
                cameraTargetParams.aimRequestStack.Add(newAimRequest);
                return newAimRequest;
            }*/
            public override void OnExit()
            {
                base.OnExit();

                if (base.isAuthority && !KeyIsDown() && !base.IsKeyDownAuthority())
                    PlayAnimation("Gesture, Override", "SecondaryCastEnd", "SecondaryCast.playbackRate", 0.65f);
                base.skillLocator.special.SetSkillOverride(gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                base.skillLocator.special.DeductStock(1);
                aimRequest.Dispose();
            }
            public override void FixedUpdate()
            {

                base.FixedUpdate();
                if (maxDistance < hookRangeMax)
                {
                    maxDistance += hookRangeGrowRate;
                }
            }
            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.PrioritySkill;
            }

            public override void UpdateTrajectoryInfo(out TrajectoryInfo dest)
            {

                base.UpdateTrajectoryInfo(out dest);
                //dest.finalRay.origin = poletip.position;
                //dest.hitPoint = arcVisualizerLineRenderer.GetPosition(arcVisualizerLineRenderer.positionCount-1);
                StartAimMode();
                // have non centered aiming??
                // could also alter aim origin?
            }

            public override void FireProjectile()
            {
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.crit = RollCrit();
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.position = currentTrajectoryInfo.finalRay.origin;
                fireProjectileInfo.projectilePrefab = projectilePrefab;
                fireProjectileInfo.rotation = Quaternion.LookRotation(currentTrajectoryInfo.finalRay.direction, Vector3.up);
                fireProjectileInfo.speedOverride = currentTrajectoryInfo.speedOverride;
                fireProjectileInfo.damage = damageCoefficient * damageStat;
                FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
                if (setFuse)
                {
                    fireProjectileInfo2.fuseOverride = currentTrajectoryInfo.travelTime;
                }
                ModifyProjectile(ref fireProjectileInfo2);
                ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
            }
        }
    }
}