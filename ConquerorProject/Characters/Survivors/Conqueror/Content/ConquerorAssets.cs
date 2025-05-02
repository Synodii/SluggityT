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

            //CreateEffects();

            CreateProjectiles();

        }

        #region effects
        /*private static void CreateEffects()
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

        }*/
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateRopeBackpackProjectile();
            Content.AddProjectilePrefab(ropeBackpackProjectilePrefab);
        }

        private static void CreateRopeBackpackProjectile()
        {
            ropeBackpackProjectilePrefab = _assetBundle.LoadAndAddProjectilePrefab("ConquerorRopeWardProjectile");
            ropeBackpackProjectilePrefab.layer = LayerIndex.projectile.intVal;

            Transform thatbuffwardihate = ropeBackpackProjectilePrefab.transform.Find("SirTheresBeenASecondBuffWard");

            LineRenderer lineyboy = ropeBackpackProjectilePrefab.GetComponent<LineRenderer>();
            Rigidbody rb = ropeBackpackProjectilePrefab.GetComponent<Rigidbody>();
            ProjectileSimple ps = ropeBackpackProjectilePrefab.GetComponent<ProjectileSimple>();
            ProjectileStickOnImpact stickOnImpact = ropeBackpackProjectilePrefab.GetComponent<ProjectileStickOnImpact>();
            ProjectileController pc = ropeBackpackProjectilePrefab.GetComponent<ProjectileController>();
            CapsuleCollider collider = ropeBackpackProjectilePrefab.GetComponent<CapsuleCollider>();
            ProjectileOverlapAttack piss = ropeBackpackProjectilePrefab.GetComponent<ProjectileOverlapAttack>();
            BuffWard bW1 = ropeBackpackProjectilePrefab.GetComponent<BuffWard>();
            BuffWard bW2 = thatbuffwardihate.GetComponent<BuffWard>();

            bW1.buffDef = ConquerorBuffs.frenzyBuff;
            bW2.buffDef = ConquerorBuffs.intimidateDebuff;

            DamageTypeCombo ropeBagDmg = new DamageTypeCombo
            {
                damageType = DamageType.Stun1s,
                damageTypeExtended = DamageTypeExtended.Generic,
                damageSource = DamageSource.Secondary,
            };
            ProjectileDamage projectileDamage = ps.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = ropeBagDmg;

            RopeBackpackController ropePack = ropeBackpackProjectilePrefab.AddComponent<RopeBackpackController>();
            ropePack.rb = rb;
            ropePack.stickComponent = stickOnImpact;
            ropePack.controller = pc;
            ropePack.backpackCollider = collider;
            ropePack.projOverlap = piss;
            ropePack.projSimple = ps;
            ropePack.lineRenderer = lineyboy;

            Log.Debug("Successfully performed CreateRopeBackpackProjectile");
        }
        #endregion projectiles
    }
}