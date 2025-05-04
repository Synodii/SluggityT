using RoR2;
using UnityEngine;
using ConquerorMod.Modules;
using RoR2.Projectile;
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
            Transform rangeindicator = ropeBackpackProjectilePrefab.transform.Find("Indicator");

            LineRenderer lineyboy = ropeBackpackProjectilePrefab.GetComponent<LineRenderer>();
            Rigidbody rb = ropeBackpackProjectilePrefab.GetComponent<Rigidbody>();
            ProjectileSimple ps = ropeBackpackProjectilePrefab.GetComponent<ProjectileSimple>();
            ProjectileStickOnImpact stickOnImpact = ropeBackpackProjectilePrefab.GetComponent<ProjectileStickOnImpact>();
            ProjectileController pc = ropeBackpackProjectilePrefab.GetComponent<ProjectileController>();
            CapsuleCollider collider = ropeBackpackProjectilePrefab.GetComponent<CapsuleCollider>();
            BuffWard bW1 = ropeBackpackProjectilePrefab.AddComponent<BuffWard>();
            BuffWard bW2 = thatbuffwardihate.gameObject.AddComponent<BuffWard>();

            Log.Debug($"[Debug] frenzyBuff is null? {ConquerorBuffs.frenzyBuff == null}");

            bW1.shape = BuffWard.BuffWardShape.Sphere;
            bW1.radius = ConquerorStaticValues.autoRecallDistance;
            bW1.interval = 1f;
            bW1.buffDef = ConquerorBuffs.frenzyBuff;
            bW1.buffDuration = 1.5f;
            bW1.rangeIndicator = rangeindicator;
            bW1.floorWard = true;
            bW1.expires = false;
            bW1.invertTeamFilter = false;
            bW1.animateRadius = false;

            bW2.shape = BuffWard.BuffWardShape.Sphere;
            bW2.radius = ConquerorStaticValues.autoRecallDistance;
            bW2.interval = 1f;
            bW2.buffDef = ConquerorBuffs.intimidateDebuff;
            bW2.buffDuration = 1.5f;
            bW2.rangeIndicator = null;
            bW2.floorWard = true;
            bW2.expires = false;
            bW2.invertTeamFilter = true;
            bW2.animateRadius = false;

            RopeBackpackController ropePack = ropeBackpackProjectilePrefab.AddComponent<RopeBackpackController>();
            ropePack.rb = rb;
            ropePack.stickComponent = stickOnImpact;
            ropePack.controller = pc;
            ropePack.backpackCollider = collider;
            ropePack.projSimple = ps;
            ropePack.lineRenderer = lineyboy;
            ropePack.buffward = bW1;
            //thatbuffwardihate.gameObject.buffward = bW2;
        }
        #endregion projectiles
    }
}