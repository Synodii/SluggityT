using UnityEngine;
using R2API;
using System;
using RoR2;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules;
using ConquerorMod.Modules.Characters;
using System.Collections.Generic;


namespace ConquerorMod.Survivors.Conqueror.Components
{
    internal class ConquerorController : MonoBehaviour
    {
        public CharacterBody characterBody;
        private SkillLocator skillLocator;
        public bool bagDeployed;
        public bool isManualRecall;
        //public string secondaryIconString;
        //public string secondaryDescString;
        //public string secondaryNameString;

        public List<RopeBackpackController> deployedBackpack;
        public RoR2.CharacterMotor characterMotor;
        public Animator animator;
        public Transform swordTip;

        public void Start()
        {
            characterBody = GetComponent<RoR2.CharacterBody>();
            characterMotor = GetComponent<RoR2.CharacterMotor>();
            animator = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            swordTip = characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("SwordTip");
        }

        public void RecallAllRopeBackpacks()
        {
            foreach (RopeBackpackController backpack in deployedBackpack)
            {
                if (backpack) backpack.StartCoroutine(backpack.FlyBack());
            }
            deployedBackpack.Clear();
        }

        public void ResetRopeSkill()
        {
            this.skillLocator.special.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.specialRecallRopeBackpack, GenericSkill.SkillOverridePriority.Upgrade);
            characterBody.GetComponent<ConquerorController>().bagDeployed = false;
        }

        //pretend this list is populated somwhere in your setup
        List<RoR2.Skills.SkillDef> AllSpells = new List<RoR2.Skills.SkillDef>();
        //would convert this to your custom skilldef or wrapper later
        RoR2.Skills.SkillDef[] spellSlots = new RoR2.Skills.SkillDef[4];

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            this.skillLocator = this.GetComponent<SkillLocator>();
        }
    }
}