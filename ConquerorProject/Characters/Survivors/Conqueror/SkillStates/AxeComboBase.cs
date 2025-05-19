using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Characters.Survivors.Conqueror.SkillStates;
using ConquerorMod.Modules.BaseStates;
using ConquerorMod.Survivors.Conqueror.Components;
using EntityStates;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class AxeComboBase : BaseSkillState
    {
        public bool isAirborneAttack = false;
        public BlastAttack groundBlast;
        public BlastAttack airBlast;
        public bool heheFallinTime = false;
        public bool hasFired = false;
        public float delay = .6f;
        public float realDelay;
        public float stopwatch;
        public Vector3 aimDirection;
        public Vector3 flatDirection;
        public Vector3 playerPos;
        public GameObject magmaWormMeatballExplo = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaOrbExplosion.prefab").WaitForCompletion();
        public ConquerorController combo;
        public float safeguardStopwatch;

        public override void OnEnter()
        {
            combo = GetComponent<ConquerorController>();

            combo.isInCombo = true;

            base.OnEnter();

            PlayAnimation();

            ComboPrep();

            if (!isAirborneAttack)
            {
                DoGroundedAttack();
            }
            else
            {
                DoAirborneAttack();
            }
        }

        protected virtual void ComboPrep()
        {
            combo.comboStopwatch = 0f;
            realDelay = delay / attackSpeedStat;
            isAirborneAttack = !characterMotor.isGrounded;
            aimDirection = GetAimRay().direction;
        }

        protected virtual void DoGroundedAttack()
        {
            flatDirection = new Vector3(aimDirection.x, 0f, aimDirection.z);

            Util.PlaySound("Play_falseson_skill1_swing", base.gameObject);
            Util.PlaySound("Play_acrid_m1_slash", base.gameObject);

            characterMotor.Motor.ForceUnground(realDelay * .5f);
            characterMotor.velocity = flatDirection * 25f * attackSpeedStat;
            RoR2.Run.instance.StartCoroutine(GroundSlam());
        }

        protected virtual IEnumerator GroundSlam()
        {
            yield return new WaitForSeconds(realDelay);
            Vector3 origin = base.characterBody.corePosition;
            CreateShockwaveFX(origin, 7f);

            BlastAttack groundBlast = new BlastAttack();
            groundBlast.attacker = gameObject;
            groundBlast.baseDamage = damageStat * ConquerorStaticValues.axecomboDamageCoefficient;
            groundBlast.falloffModel = BlastAttack.FalloffModel.None;
            groundBlast.baseForce = 0;
            groundBlast.position = transform.position + characterDirection.forward * 3f;
            groundBlast.radius = 7f;
            groundBlast.teamIndex = TeamComponent.GetObjectTeam(gameObject);
            groundBlast.crit = RollCrit();
            groundBlast.AddModdedDamageType(DamageTypes.BleedOnHitbutCooler);
            groundBlast.AddModdedDamageType(DamageTypes.ConquerorKnockup);

            groundBlast.Fire();
            characterMotor.velocity = new Vector3(0, 0, 0);
            hasFired = true;
        }

        protected virtual void DoAirborneAttack()
        {
            flatDirection = new Vector3(aimDirection.x * 20f, 10f, aimDirection.z * 20f);

            Util.PlaySound("Play_falseson_skill1_swing", base.gameObject);
            Util.PlaySound("Play_acrid_m1_slash", base.gameObject);

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            characterMotor.velocity = flatDirection;

            RoR2.Run.instance.StartCoroutine(DownwardSlam());
        }

        protected virtual IEnumerator DownwardSlam()
        {
            yield return new WaitForSeconds(realDelay * .5f);
            characterMotor.velocity.y = -20f;
            heheFallinTime = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (heheFallinTime)
            {
                safeguardStopwatch += Time.fixedDeltaTime;
                characterMotor.velocity.y = characterMotor.velocity.y + -50f * base.GetDeltaTime();
                characterMotor.velocity.x = aimDirection.x * 10f;
                characterMotor.velocity.z = aimDirection.z * 10f;
            }
            if (safeguardStopwatch > 5.5f)
            {
                hasFired = true;
            }
            if (isAirborneAttack && characterMotor.isGrounded)
            {
                FireAirSlam();
                heheFallinTime = false;
            }
            if (hasFired)
            {
                Util.PlaySound("Play_loader_R_variant_slam", base.gameObject);
                
                var graceState = new AxeComboGrace();
                this.skillLocator.utility.SetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxeGrace, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
            }
        }

        protected virtual void FireAirSlam()
        {
            Vector3 origin = base.characterBody.corePosition;
            CreateShockwaveFX(origin, 8f);

            BlastAttack airBlast = new BlastAttack();
            airBlast.attacker = gameObject;
            airBlast.baseDamage = damageStat * ConquerorStaticValues.axecomboDamageCoefficient;
            airBlast.falloffModel = BlastAttack.FalloffModel.None;
            airBlast.baseForce = 0;
            airBlast.position = transform.position + characterDirection.forward * 2f;
            airBlast.radius = 8f;
            airBlast.teamIndex = TeamComponent.GetObjectTeam(gameObject);
            airBlast.crit = RollCrit();
            airBlast.AddModdedDamageType(DamageTypes.ConquerorKnockup);
            airBlast.Fire();
            hasFired = true;
        }

        public override void OnExit()
        {
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            combo.IncrementCombo();
            if (combo.comboCount > combo.maxStep)
            {
                combo.ResetCombo();
            }
            base.OnExit();
        }

        protected virtual void PlayAnimation()
        {
        }

        protected virtual void CreateShockwaveFX(Vector3 origin, float scale)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = scale;
            EffectManager.SpawnEffect(magmaWormMeatballExplo, effectData, false);
        }
    }

    public class AxeCombo1 : AxeComboBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //PlayAnimation("Gesture, Override", "AxeComboVariant1Animation", "AxeCombo.playbackRate", 1f);
        }
        protected override void PlayAnimation()
        {
        }
    }

    public class AxeCombo2 : AxeComboBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //PlayAnimation("Gesture, Override", "AxeComboVariant1Animation", "AxeCombo.playbackRate", 1f);
        }
        protected override void PlayAnimation()
        {
        }
    }

    public class AxeCombo3 : AxeComboBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //PlayAnimation("Gesture, Override", "AxeComboVariant1Animation", "AxeCombo.playbackRate", 1f);
        }
        protected override void PlayAnimation()
        {
        }
    }

    public class AxeCombo4 : AxeComboBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //PlayAnimation("Gesture, Override", "AxeComboVariant1Animation", "AxeCombo.playbackRate", 1f);
        }
        protected override void PlayAnimation()
        {
        }
    }
}