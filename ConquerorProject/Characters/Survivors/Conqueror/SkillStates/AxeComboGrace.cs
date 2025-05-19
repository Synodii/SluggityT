using ConquerorMod.Characters.Survivors.Conqueror.Content;
using ConquerorMod.Modules.BaseStates;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Survivors.Conqueror.Components;
using ConquerorMod.Survivors.Conqueror.SkillStates;
using EntityStates;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ConquerorMod.Characters.Survivors.Conqueror.SkillStates
{
    public class AxeComboGrace : BaseSkillState
    {
        private ConquerorController combo;
        private float graceTimer;
        private float graceDuration = 1.5f;
        private bool inputReceived;

        public override void OnEnter()
        {
            base.OnEnter();
            combo = characterBody.GetComponent<ConquerorController>();
            graceTimer = 0f;
            inputReceived = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            graceTimer += Time.fixedDeltaTime;

            if (!inputReceived && inputBank.skill3.down)
            {
                inputReceived = true;
                AdvanceCombo();
            }

            if (graceTimer > graceDuration && !inputReceived)
            {
                combo.ResetCombo();
                outer.SetNextStateToMain();
            }
        }

        private void AdvanceCombo()
        {
            switch (combo.comboCount)
            {
                case 2:
                    this.skillLocator.utility.SetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe2, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxeGrace, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    break;
                case 3:
                    this.skillLocator.utility.SetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe3, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxeGrace, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    break;
                case 4:
                    this.skillLocator.utility.SetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxe4, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    this.skillLocator.utility.UnsetSkillOverride(this.gameObject, ConquerorSurvivor.utilityAxeGrace, RoR2.GenericSkill.SkillOverridePriority.Upgrade);
                    break;
                default:
                    combo.ResetCombo();
                    outer.SetNextStateToMain();
                    break;
            }
        }
    }
}

