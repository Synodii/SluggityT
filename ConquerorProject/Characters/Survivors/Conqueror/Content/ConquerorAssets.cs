using RoR2;
using UnityEngine;
using ConquerorMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using ConquerorMod.Survivors.Conqueror.Components;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        public static GameObject nemGasGrenade;
        public static GameObject ropeBackpackZone;

        //projectiles
        public static GameObject ropeBackpackProjectilePrefab;
        public static GameObject originBackpackProjectilePrefab;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            CreateEffects();

            CreateProjectiles();

            //CreateBuffWard();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateRopeBackpackProjectile();
            Content.AddProjectilePrefab(ropeBackpackProjectilePrefab);
        }

        private static void CreateRopeBackpackProjectile()
        {
            ropeBackpackProjectilePrefab = _assetBundle.LoadAndAddProjectilePrefab("HenryBombProjectile");
            ropeBackpackProjectilePrefab.layer = LayerIndex.projectile.intVal;

            Rigidbody rb = ropeBackpackProjectilePrefab.GetComponent<Rigidbody>();

            ProjectileSimple ps = ropeBackpackProjectilePrefab.GetComponent<ProjectileSimple>();

            ProjectileStickOnImpact stickOnImpact = ropeBackpackProjectilePrefab.GetComponent<ProjectileStickOnImpact>();

            ProjectileController pc = ropeBackpackProjectilePrefab.GetComponent<ProjectileController>();

            CapsuleCollider collider = ropeBackpackProjectilePrefab.GetComponent<CapsuleCollider>();



            ProjectileOverlapAttack piss = ropeBackpackProjectilePrefab.GetComponent<ProjectileOverlapAttack>();

            ProjectileDamage projectileDamage = ps.GetComponent<ProjectileDamage>();
            DamageTypeCombo hookDmg = new DamageTypeCombo
            {
                damageType = DamageType.NonLethal,
                damageTypeExtended = DamageTypeExtended.Generic,
                damageSource = DamageSource.Secondary,
            };
            //hookDmg.


            RopeBackpackController fishHook = ropeBackpackProjectilePrefab.AddComponent<RopeBackpackController>();
            fishHook.rb = rb;
            fishHook.stickComponent = stickOnImpact;
            fishHook.controller = pc;
            fishHook.projectileDamage = projectileDamage;
            fishHook.backpackCollider = collider;
            fishHook.projOverlap = piss;
            fishHook.projSimple = ps;
            fishHook.lineRenderer = ropeBackpackProjectilePrefab.GetComponent<LineRenderer>();


            //item grabber
            GameObject ItemInteractor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ItemInteractor.transform.parent = ropeBackpackProjectilePrefab.transform;
            UnityEngine.Object.Destroy(ItemInteractor.GetComponent<MeshRenderer>());
            UnityEngine.Object.Destroy(ItemInteractor.GetComponent<MeshFilter>());
            ItemInteractor.GetComponent<SphereCollider>().isTrigger = true;
            ItemInteractor.transform.localPosition = Vector3.zero;
            ItemInteractor.transform.localScale = Vector3.one * 6;
            ItemInteractor.layer = 15;

            BuffWard buffWard = ropeBackpackProjectilePrefab.AddComponent<BuffWard>();
            buffWard.radius = 18;
            buffWard.interval = 1;
            buffWard.rangeIndicator = null;
            buffWard.buffDef = ConquerorBuffs.intimidateDebuff;
            buffWard.buffDuration = 1.5f;
            buffWard.floorWard = true;
            buffWard.expires = false;
            buffWard.invertTeamFilter = true;
            buffWard.expireDuration = 0;
            buffWard.animateRadius = false;
        }
        #endregion projectiles
    }
}