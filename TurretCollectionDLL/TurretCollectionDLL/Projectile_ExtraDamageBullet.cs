using UnityEngine;
using Verse;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_ExtraDamageBullet : Bullet
    {

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), 3f);
        }

        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                CompExtraDamage_Properties compProperties = this.def.GetCompProperties<CompExtraDamage_Properties>();
                if (hitThing.def.category == ThingCategory.Pawn)
                {
                    MoteMaker.ThrowText(new Vector3((float)this.Position.x + 1f, (float)this.Position.y, (float)this.Position.z + 1f), Translator.Translate("TC_Hit"), Color.yellow);
                }
                DamageInfo dinfo = new DamageInfo(compProperties.ExtraDamageDef, compProperties.ExtraDamageAmount, this.launcher, this.ExactRotation.eulerAngles.y, null, this.equipmentDef);
                hitThing.TakeDamage(dinfo);
                this.Explode();
            }
            else
            {
                this.Explode();
            }
        }

        protected virtual void Explode()
        {
            this.Destroy(DestroyMode.Vanish);
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            float explosionSpawnChance = this.def.projectile.explosionSpawnChance;
            GenExplosion.DoExplosion(base.Position, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, this.def.projectile.soundExplode, this.def, this.equipmentDef, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.explosionSpawnChance, 1, false, preExplosionSpawnThingDef, explosionSpawnChance, 1);
        }
    }
}
