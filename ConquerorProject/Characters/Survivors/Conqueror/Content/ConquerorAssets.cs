using RoR2;
using UnityEngine;
using ConquerorMod.Modules;
using RoR2.Projectile;
using ConquerorMod.Survivors.Conqueror.Components;
using UnityEngine.AddressableAssets;

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

            //Transform thatbuffwardihate = ropeBackpackProjectilePrefab.transform.Find("SirTheresBeenASecondBuffWard");
            Transform rangeindicator = ropeBackpackProjectilePrefab.transform.Find("Indicator");

            Transform ropeIndicator = ropeBackpackProjectilePrefab.transform.Find("Indicator/RopeWardIndicator");
            ropeIndicator.localScale = Vector3.zero;

            MeshRenderer meshi = ropeIndicator.GetComponent<MeshRenderer>();
            meshi.material = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Items/IncreaseDamageOnMultiKill/matTeleportOnLowHealthIndicator.mat").WaitForCompletion();

            Rigidbody rb = ropeBackpackProjectilePrefab.GetComponent<Rigidbody>();
            ProjectileSimple ps = ropeBackpackProjectilePrefab.GetComponent<ProjectileSimple>();
            ProjectileStickOnImpact stickOnImpact = ropeBackpackProjectilePrefab.GetComponent<ProjectileStickOnImpact>();
            ProjectileController pc = ropeBackpackProjectilePrefab.GetComponent<ProjectileController>();
            CapsuleCollider collider = ropeBackpackProjectilePrefab.GetComponent<CapsuleCollider>();

            BuffWard bW1 = ropeBackpackProjectilePrefab.AddComponent<BuffWard>();
            bW1.shape = BuffWard.BuffWardShape.Sphere;
            bW1.radius = 18f;
            bW1.interval = 1f;
            bW1.buffDef = ConquerorBuffs.bolsteredBuff;
            bW1.buffDuration = 1.5f;
            bW1.rangeIndicator = rangeindicator;
            bW1.floorWard = true;
            bW1.expires = false;
            bW1.invertTeamFilter = false;
            bW1.animateRadius = false;
            bW1.enabled = false;

            RopeBackpackController ropePack = ropeBackpackProjectilePrefab.AddComponent<RopeBackpackController>();
            ropePack.rb = rb;
            ropePack.stickComponent = stickOnImpact;
            ropePack.controller = pc;
            ropePack.backpackCollider = collider;
            ropePack.projSimple = ps;
            ropePack.buffward = bW1;
            ropePack.ropeIndicator = ropeIndicator;
        }
        #endregion projectiles
    }
}