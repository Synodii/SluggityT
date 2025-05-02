using UnityEngine;
using R2API;
using System;
using RoR2;
using UnityEngine.Networking;
using ConquerorMod.Survivors.Conqueror;
using ConquerorMod.Modules;
using ConquerorMod.Modules.Characters;


namespace ConquerorMod.Survivors.Conqueror.Components
{
    internal class ConquerorController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private SkillLocator skillLocator;
        public bool bagDeployed;


        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            this.skillLocator = this.GetComponent<SkillLocator>();
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
        }
    }
}