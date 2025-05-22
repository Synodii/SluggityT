using System;
using System.Collections.Generic;
using System.Text;
using static ak.wwise.core;
using ConquerorMod;
using ConquerorMod.Characters.Survivors.Conqueror.Content;
using RoR2;
using UnityEngine;
using ConquerorMod.Survivors.Conqueror;

namespace ConquerorMod.Characters.Survivors.Conqueror.Components
{
    public class FallDamageImmunityTracker : MonoBehaviour
    {
        private CharacterBody body;
        private CharacterMotor motor;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            motor = GetComponent<CharacterMotor>();
        }

        private void FixedUpdate()
        {
            if (body && body.HasBuff(ConquerorBuffs.fallDamageImmune))
            {
                if (motor && motor.isGrounded)
                {
                    body.RemoveBuff(ConquerorBuffs.fallDamageImmune);
                    body.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                    Destroy(this);
                }
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
