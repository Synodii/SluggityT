using EntityStates;
using EntityStates.Toolbot;
using ConquerorMod.Survivors.Conqueror.Components;
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
                PlayAnimation("Gesture, Override", "SecondaryCastRecall", "SecondaryCast.playbackRate", 0.65f);
                base.skillLocator.special.UnsetSkillOverride(gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                base.skillLocator.special.DeductStock(1); // may change this to deduct all stocks if all hooks are fired at once.
                ObjectTracker objt = characterBody.GetComponent<ObjectTracker>();
                if (objt)
                {
                    //objt.animator = base.GetModelAnimator();
                    objt.RecallAllRopeBackpacks();
                }
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