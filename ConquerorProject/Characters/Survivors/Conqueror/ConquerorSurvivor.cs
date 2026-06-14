using BepInEx.Configuration;
using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Characters.Survivors.Conqueror.SkillStates;
using ConquerorMod.Modules;
using ConquerorMod.Modules.Characters;
using ConquerorMod.Survivors.Conqueror.Components;
using ConquerorMod.Survivors.Conqueror.SkillStates;
using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoR2.TeleporterInteraction;

namespace ConquerorMod.Survivors.Conqueror
{
    public class ConquerorSurvivor : SurvivorBase<ConquerorSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "conqdelaassets"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "ConquerorBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "ConquerorMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlHenry";
        public override string displayPrefabName => "HenryDisplay";

        public const string CONQUEROR_PREFIX = ConquerorPlugin.DEVELOPER_PREFIX + "_CONQUEROR_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => CONQUEROR_PREFIX;

        public static SkillDef primaryAxe;

        public static SkillDef utilityEye;
        //public static SkillDef secondaryCrush;

        public static SkillDef secondaryAxe1;
        public static SkillDef secondaryAxeComboManager;

        public static SkillDef specialRecallRopeBackpack;
        public static SkillDef specialRopeBackpack;

        public static SkillDef specialRecallApportBackpack;
        public static SkillDef specialApportBackpack;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = CONQUEROR_PREFIX + "NAME",
            subtitleNameToken = CONQUEROR_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texConquerorIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Asset.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matHenry"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",
                }
        };

        public override UnlockableDef characterUnlockableDef => ConquerorUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new ConquerorItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public static BodyIndex conquerorBodyIndex;

        public override void Initialize()
        {
            ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Conqueror");

            if (!characterEnabled.Value)
                return;

            BodyCatalog.availability.CallWhenAvailable(() =>
            {
                conquerorBodyIndex = BodyCatalog.FindBodyIndex("ConquerorBody"); // use internal name
                Log.Debug($"[Conqueror] Registered body index: {conquerorBodyIndex}");
            });

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            ConquerorUnlockables.Init();

            base.InitializeCharacter();

            ConquerorConfig.Init();
            ConquerorStates.Init();
            ConquerorTokens.Init();

            ConquerorAssets.Init(assetBundle);

            DamageTypes.Init();

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();
            AddHooks();
        }

        private IEnumerator AssignBodyIndexWhenReady()
        {
            while (BodyCatalog.FindBodyIndex("ConquerorBody") == BodyIndex.None)
            {
                yield return null;
            }

            conquerorBodyIndex = BodyCatalog.FindBodyIndex("ConquerorBody");
            Log.Debug($"[Conqueror] BodyIndex assigned: {conquerorBodyIndex}");
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<ConquerorController>();
            //anything else here
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            ChildLocator childLocator = characterModelObject.GetComponent<ChildLocator>();


            Transform swordHitBoxTransform = childLocator.FindChild("SwordHitbox");


            Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", swordHitBoxTransform);
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your ConquerorStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon3");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon4");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            //AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = CONQUEROR_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ConquerorPassive",
                skillNameToken = CONQUEROR_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            ConquerorSurvivor.primaryAxe = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Overpower",
                skillNameToken = CONQUEROR_PREFIX + "PRIMARY_AXE_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "PRIMARY_AXE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(OverpowerCharge)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 0,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 0,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });
            //custom Skilldefs can have additional fields that you can set manually
            Skills.AddPrimarySkills(bodyPrefab, primaryAxe);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);


            //here's a skilldef of a typical movement skill.
            ConquerorSurvivor.secondaryAxe1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Advance",
                skillNameToken = CONQUEROR_PREFIX + "UTILITY_WARP_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "UTILITY_WARP_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(AxeCombo1)),
                activationStateMachineName = "Weapon3",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 10f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });
            ConquerorSurvivor.secondaryAxeComboManager = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Advance",
                skillNameToken = CONQUEROR_PREFIX + "UTILITY_WARP_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "UTILITY_WARP_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Idle)),
                activationStateMachineName = "Weapon3",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 10f,
                baseMaxStock = 0,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });
            ;
            Skills.AddSecondarySkills(bodyPrefab, secondaryAxe1);
        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);


            ConquerorSurvivor.utilityEye = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ConquerorMunch",
                skillNameToken = CONQUEROR_PREFIX + "SECONDARY_MUNCH_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "SECONDARY_MUNCH_DESCRIPTION",
                //keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Eye)),
                activationStateMachineName = "Weapon4",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 7f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });

            /*ConquerorSurvivor.secondaryCrush = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ConquerorCrush",
                skillNameToken = CONQUEROR_PREFIX + "SECONDARY_CRUSH_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "SECONDARY_CRUSH_DESCRIPTION",
                //keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texBazookaFireIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.EyeCrush)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = ConquerorSurvivor.secondaryMunch.baseRechargeInterval,
                baseMaxStock = 3,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });*/

            Skills.AddUtilitySkills(bodyPrefab, utilityEye);
            //Skills.AddSecondarySkills(bodyPrefab, secondaryCrush);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            //a basic skill. some fields are omitted and will just have default values
            ConquerorSurvivor.specialRopeBackpack = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ConquerorRopeBackpack",
                skillNameToken = CONQUEROR_PREFIX + "SPECIAL_ROPEBACKPACK_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "SPECIAL_ROPEBACKPACK_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.RopeBackpackFire)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 6f,

                isCombatSkill = false,
                beginSkillCooldownOnSkillEnd = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = true,
            });

            ConquerorSurvivor.specialRecallRopeBackpack = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ConquerorRecallRopeBackpack",
                skillNameToken = CONQUEROR_PREFIX + "SPECIAL_RECALLROPEBACKPACK_NAME",
                skillDescriptionToken = CONQUEROR_PREFIX + "SPECIAL_RECALLROPEBACKPACK_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texBazookaIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.RecallRopeBackpacks)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 2f,
                baseMaxStock = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            //Skills.AddSpecialSkills(bodyPrefab, specialRecallRopeBackpack);
            Skills.AddSpecialSkills(bodyPrefab, specialRopeBackpack);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshConquerorSword",
            //    "meshConquerorGun",
            //    "meshConqueror");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    ConquerorUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshConquerorSwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshConquerorAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matConquerorAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matConquerorAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matConquerorAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            ConquerorAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += ConqDamageReduction;
        }
        private void ConqDamageReduction(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!self || !self.body)
            {
                orig(self, damageInfo);
                return;
            }

            if (self.body.bodyIndex == conquerorBodyIndex)
            {
                float scalingFactor = .4f;

                float currentHP = self.health;
                float maxHP = self.fullCombinedHealth;
                float incomingDamage = damageInfo.damage;

                float predictedHP = Mathf.Max(0f, currentHP - incomingDamage);
                float missingHpFraction = 1f - (predictedHP / maxHP);

                float conqDamReduct = Mathf.Pow(missingHpFraction, 2f) * (1f * scalingFactor);
                float maxReduction = 0.8f; // safety cap
                conqDamReduct = Mathf.Min(conqDamReduct, maxReduction);

                float originalDamage = damageInfo.damage;
                damageInfo.damage *= 1f - conqDamReduct;


                /*if (self.body.master == LocalUserManager.GetFirstLocalUser().currentNetworkUser.master)
                {
                    Chat.AddMessage($"<color=#FFA500>[Conqueror]</color> Original Damage: {originalDamage}, DR: {conqDamReduct * 100f:F1}%, Final: {damageInfo.damage} Predicted HP: {predictedHP}, Missing: {missingHpFraction * 100f:F1}%");
                }*/
            }

            orig(self, damageInfo);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            /* HEALTH PASSIVE OLD
            if (sender)
            {
                Log.Debug($"Conqueror BodyIndex = {conquerorBodyIndex};");
                //if (sender.bodyIndex == conquerorBodyIndex)
                //{
                    if (sender.healthComponent != null)
                    {
                        float currentFraction = sender.healthComponent.combinedHealthFraction;

                        if (!float.IsFinite(currentFraction) || currentFraction < 0f || currentFraction > 1f)
                        {
                            Log.Warning($"[Conqueror] Invalid health fraction: {currentFraction}");
                            return;
                        }

                        float missingFraction = 1f - currentFraction;
                        Log.Debug($"[Conqueror] Missing HP Fraction: {missingFraction}");

                        args.healthMultAdd += missingFraction;
                    }
                //}
            }*/

            if (sender.HasBuff(ConquerorBuffs.conquerorIntimidateDebuff))
            {
                args.armorAdd += -25;
                args.moveSpeedReductionMultAdd += 15;
                args.baseDamageAdd += -25;
            }
        }
    }
}