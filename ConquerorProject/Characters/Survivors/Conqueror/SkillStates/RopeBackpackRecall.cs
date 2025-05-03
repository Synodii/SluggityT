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
                var controller = characterBody.GetComponent<ConquerorController>();
                if (controller) controller.ResetRopeSkill();

                characterBody.GetComponent<ConquerorController>().isManualRecall = true;

                base.skillLocator.special.DeductStock(1);
                ConquerorController objt = characterBody.GetComponent<ConquerorController>();
                if (objt)
                {
                    objt.animator = base.GetModelAnimator();
                    objt.RecallAllRopeBackpacks();
                }
                PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1f);
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