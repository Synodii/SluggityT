using System.Collections.Generic;
using UnityEngine;
using ConquerorMod.Modules;
using On.RoR2.Skills;
using static EntityStates.BaseState;
using UnityEngine.Events;
using ConquerorMod.Survivors.Conqueror.Components;

namespace ConquerorMod.Survivors.Conqueror.Components
{
    public class ObjectTracker : MonoBehaviour
    {
        public List<RopeBackpackController> deployedBackpack;
        public RoR2.CharacterBody characterBody;
        public RoR2.CharacterMotor characterMotor;
        public Animator animator;
        public Transform fishingPoleTip;

        public void Start()
        {
            characterBody = GetComponent<RoR2.CharacterBody>();
            characterMotor = GetComponent<RoR2.CharacterMotor>();
            animator = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            //fishingPoleTip = characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("PoleEnd");
        }

        public void RecallAllRopeBackpacks()
        {
            foreach (RopeBackpackController backpack in deployedBackpack)
            {
                if (backpack) backpack.StartCoroutine(backpack.FlyBack());
            }
            deployedBackpack.Clear();
        }


        //pretend this list is populated somwhere in your setup
        List<RoR2.Skills.SkillDef> AllSpells = new List<RoR2.Skills.SkillDef>();
        //would convert this to your custom skilldef or wrapper later
        RoR2.Skills.SkillDef[] spellSlots = new RoR2.Skills.SkillDef[4];
    }
}