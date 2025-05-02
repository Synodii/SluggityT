using EntityStates;
using ConquerorMod.Survivors.Conqueror.Components;
using ConquerorMod.Modules;
using ConquerorMod.Survivors.Conqueror;
using IL.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    //TODO: Correct overide behavior so it isnt creating a new overide each time
    public class RecallRopeBackpacks : BaseSkillState
    {
        public override void OnEnter()
        {
            if (base.isAuthority) 
            {
                base.skillLocator.special.UnsetSkillOverride(gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                base.skillLocator.special.DeductStock(1);
                ObjectTracker objt = characterBody.GetComponent<ObjectTracker>();
                if (objt)
                {
                    objt.animator = base.GetModelAnimator();
                    objt.RecallAllRopeBackpacks();
                }
                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);

                characterBody.GetComponent<ConquerorController>().bagDeployed = false;
            }

            outer.SetNextStateToMain();
            base.OnEnter();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}