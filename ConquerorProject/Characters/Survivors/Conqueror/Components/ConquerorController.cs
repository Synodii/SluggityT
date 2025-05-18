using UnityEngine;
using R2API;
using System;
using RoR2;
using On.RoR2;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules;
using ConquerorMod.Modules.Characters;
using System.Collections.Generic;


namespace ConquerorMod.Survivors.Conqueror.Components
{
    internal class ConquerorController : MonoBehaviour
    {
        public RoR2.CharacterBody characterBody;
        private RoR2.SkillLocator skillLocator;
        public bool bagDeployed;
        public float distanceToOwner;
        public bool isManualRecall;
        public bool isUsingMunch = false;
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

        public void FixedUpdate()
        {
            if (!bagDeployed && distanceToOwner != 8008135f)
            {
                distanceToOwner = 8008135f;
            }
        }
        /*private float prevHealth;
        public void FixedUpdate()
        {
            float currentFullHealth = characterBody.healthComponent.fullCombinedHealth;
            if (!Mathf.Approximately(prevHealth, currentFullHealth)) 
            {
            prevHealth = characterBody.healthComponent.fullCombinedHealth;
            characterBody.RecalculateStats();
            Log.Debug("[Conqueror Controller] Attempting to RecalculateStaties");
            }
            else return;
        }*/

        public void ResetRopeSkill()
        {
            this.skillLocator.special.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.specialRecallRopeBackpack, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
            characterBody.GetComponent<ConquerorController>().bagDeployed = false;
            if (!isManualRecall)
            {
                RoR2.GenericSkill specialSkill = this.skillLocator.special;
                if (specialSkill != null)
                {
                    if (specialSkill.stock > 0)
                    {
                        specialSkill.DeductStock(1);
                        specialSkill.rechargeStopwatch = 0f;
                    }
                }
            }
        }

        //pretend this list is populated somwhere in your setup
        List<RoR2.Skills.SkillDef> AllSpells = new List<RoR2.Skills.SkillDef>();
        //would convert this to your custom skilldef or wrapper later
        RoR2.Skills.SkillDef[] spellSlots = new RoR2.Skills.SkillDef[4];



        private void Awake()
        {
            this.characterBody = this.GetComponent<RoR2.CharacterBody>();
            this.skillLocator = this.GetComponent<RoR2.SkillLocator>();
        }
    }
}