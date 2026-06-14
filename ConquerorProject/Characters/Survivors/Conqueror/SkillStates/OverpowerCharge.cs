using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Modules.BaseStates;
using ConquerorMod.Survivors.Conqueror.Components;
using EntityStates;
using EntityStates.Bison;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ConquerorMod.Survivors.Conqueror.SkillStates
{
    public class OverpowerCharge : BaseSkillState
    {
        private ConquerorController controller;

        private float overpowerCharge = 0f;
        private float minChargeDuration = .3f;
        private float baseChargeDuration = .75f;
        private float compensatedMinDuration;
        private float compensatedChargeDuration;
        private bool isCharged;
        private float fullChargeHoldPeriod;
        private float stopwatch;

        private GameObject shatterspleenImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Impact.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();
            controller = GetComponent<ConquerorController>();


            this.compensatedChargeDuration = this.baseChargeDuration / this.attackSpeedStat;
            this.compensatedMinDuration = this.minChargeDuration / this.attackSpeedStat;
            fullChargeHoldPeriod = (compensatedChargeDuration + 9999f) / this.attackSpeedStat;
            //PlayCrossfade("Gesture, Override", "ChargeLoop", "Swing.playbackRate", 1f, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= compensatedMinDuration)
            {
                if (!controller.isPreCharged)
                {
                    if (overpowerCharge < 1f)
                    {
                        overpowerCharge += Time.fixedDeltaTime / compensatedChargeDuration;
                    }
                }
                else
                {
                    overpowerCharge = 1f;
                }

                if (overpowerCharge >= 1f && !isCharged && !controller.isPreCharged)
                {
                    Util.PlaySound("Play_voidman_sprint_start", base.gameObject);
                    isCharged = true;
                    Log.Debug($"isCharged {isCharged}");
                    CreateShatterspleenImpactFX(Util.GetCorePosition(this.characterBody.gameObject));

                }

                bool releasedEarly = !inputBank.skill1.down && overpowerCharge >= 0;

                if (isCharged && stopwatch > fullChargeHoldPeriod || releasedEarly)
                {
                    outer.SetNextState(new OverpowerFire(overpowerCharge));
                }
            }
        }
        private void CreateShatterspleenImpactFX(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            //effectData.rotation = Util.QuaternionSafeLookRotation(forwardDirection);
            effectData.origin = origin;
            effectData.scale = 4f;
            EffectManager.SpawnEffect(shatterspleenImpact, effectData, false);
        }
    }
}
