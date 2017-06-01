using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_ExtraDamageTrailBullet : Projectile
    {
        private int Burnticks = 3;

        private Map map;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.map = map;
            MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), this.map, 3f);
        }

        public override void Tick()
        {
            base.Tick();
            if (--Burnticks == 0)
            {
                MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), this.map, 1f);
                Burnticks = 3;
            }
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, this.def.projectile.damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef);
                hitThing.TakeDamage(dinfo);
                CompExtraDamage compExtraDamage = base.GetComp<CompExtraDamage>();
                if (compExtraDamage != null)
                {
                    CompProperties_ExtraDamage cpExtraDamage = compExtraDamage.Props;
                    int extraDamagaAmount = cpExtraDamage.ExtraDamageAmount;
                    DamageDef extraDamagaDef = cpExtraDamage.ExtraDamageDef;
                    DamageInfo edinfo = new DamageInfo(extraDamagaDef, extraDamagaAmount, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef);
                    if (hitThing.def.category == ThingCategory.Pawn)
                    {
                        MoteMaker.ThrowText(new Vector3((float)this.Position.x + 1f, (float)this.Position.y, (float)this.Position.z + 1f), this.map, "TC_Hit".Translate(), Color.yellow);
                    }
                    hitThing.TakeDamage(edinfo);
                }
            }
            else
            {
                SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, this.map, false));
                MoteMaker.MakeStaticMote(this.ExactPosition, this.map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
            }
            this.Explode();
        }

        protected virtual void Explode()
        {
            //this.Destroy(DestroyMode.Vanish);
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            float explosionSpawnChance = this.def.projectile.explosionSpawnChance;
            GenExplosion.DoExplosion(base.Position, this.map, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, this.def.projectile.soundExplode, this.def, this.equipmentDef, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.explosionSpawnChance, 1, false, preExplosionSpawnThingDef, explosionSpawnChance, 1);
        }
    }
}