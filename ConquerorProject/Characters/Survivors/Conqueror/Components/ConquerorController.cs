using UnityEngine;
using R2API;
using System;
using RoR2;
using EntityStates;
using On.RoR2;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules;
using ConquerorMod.Modules.Characters;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using ConquerorMod.Characters.Survivors.Conqueror.Content;
using static RoR2.Skills.ComboSkillDef;
using ConquerorMod.Survivors.Conqueror.SkillStates;


namespace ConquerorMod.Survivors.Conqueror.Components
{
    public class ConquerorController : MonoBehaviour
    {
        public RoR2.CharacterBody characterBody;
        private RoR2.SkillLocator skillLocator;
        public bool bagDeployed;
        public float distanceToOwner;
        public bool isManualRecall;
        public bool isUsingMunch = false;
        public float passiveCooldownReduction = 1f;
        //public string secondaryIconString;
        //public string secondaryDescString;
        //public string secondaryNameString;

        public List<RopeBackpackController> deployedBackpack;
        public RoR2.CharacterMotor characterMotor;
        public Animator animator;
        public Transform swordTip;

        public int comboCount = 1;
        public bool isInCombo = false;
        public int maxStep = 4;
        public float comboStopwatch;

        //private RoR2.BlastAttack bleedblast;
        private GameObject shatterspleenExplode = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion();


        public void Start()
        {
            characterBody = GetComponent<RoR2.CharacterBody>();
            characterMotor = GetComponent<RoR2.CharacterMotor>();
            animator = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            swordTip = characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("SwordTip");
            AddHooks();

        }

        public void RecallAllRopeBackpacks()
        {
            foreach (RopeBackpackController backpack in deployedBackpack)
            {
                if (backpack) backpack.StartCoroutine(backpack.FlyBack());
            }
            deployedBackpack.Clear();
        }

        private void AddHooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        public void FixedUpdate()
        {
            if (!bagDeployed && distanceToOwner != 8008135f)
            {
                distanceToOwner = 8008135f;
            }
        }

        //In bag range Passive
        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            //Log.Debug("Triggering OnCharacterDeath");
            if (!damageReport.attacker || damageReport.attacker != gameObject) return;

            if (distanceToOwner >= 18) return;

            //Log.Debug("Triggering InBag Passive");
            InRangeReduceCooldownsOnKill();
            //TriggerBleedBlast();
        }
        private void InRangeReduceCooldownsOnKill()
        {
            if (characterBody?.skillLocator == null) return;

            foreach (var skill in new[] {characterBody.skillLocator.primary, characterBody.skillLocator.secondary, characterBody.skillLocator.utility})
            {
                if (skill != null)
                skill.rechargeStopwatch += passiveCooldownReduction;
            }
        }
        /*private void TriggerBleedBlast()
        {
            float radius = 6;
            bleedblast = new RoR2.BlastAttack();
            bleedblast.radius = radius;
            bleedblast.attacker = gameObject;
            bleedblast.inflictor = gameObject;
            bleedblast.teamIndex = TeamIndex.Player;
            bleedblast.procCoefficient = 1f;
            bleedblast.baseForce = 0;
            bleedblast.canRejectForce = false;
            bleedblast.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            bleedblast.baseDamage = 0;
            bleedblast.AddModdedDamageType(DamageTypes.BleedOnHitbutCooler);
            bleedblast.crit = false;
            bleedblast.position = characterBody.corePosition;
            bleedblast.Fire();

            CreateShatterspleenExplodeFX(characterBody.corePosition, radius);
        }*/
        private void CreateShatterspleenExplodeFX(Vector3 origin, float scale)
        {
            RoR2.EffectData effectData = new RoR2.EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = scale;
            RoR2.EffectManager.SpawnEffect(shatterspleenExplode, effectData, false);
        }


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

        public void IncrementCombo()
        {
            this.comboCount = comboCount + 1;
        }
        public void ResetCombo()
        {
            comboStopwatch = 0f;
            comboCount = 1;
            isInCombo = false;

            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe2, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe3, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe4, RoR2.GenericSkill.SkillOverridePriority.Upgrade);


            RoR2.GenericSkill utilitySkill = this.skillLocator.utility;
            if (utilitySkill != null)
            {
                if (utilitySkill.stock > 0)
                {
                    utilitySkill.DeductStock(1);
                    utilitySkill.rechargeStopwatch = 0f;
                }
            }
        }
    }
}