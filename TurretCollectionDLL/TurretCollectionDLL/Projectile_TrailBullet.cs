using Verse;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_TrailBullet : Projectile
    {
        private int ticksToDetonation;
        private int Burnticks = 3;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 5f);
        }
        public override void Tick()
        {
            base.Tick();
            if (--Burnticks == 0)
            {
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 1f);
                Burnticks = 3;
            }
            if (this.ticksToDetonation > 0)
            {
                this.ticksToDetonation--;
                if (this.ticksToDetonation <= 0)
                {
                    this.Explode();
                }
            }
        }
        protected override void Impact(Thing hitThing)
        {
            if (this.def.projectile.explosionDelay == 0)
            {
                this.Explode();
                return;
            }
            this.landed = true;
            this.ticksToDetonation = this.def.projectile.explosionDelay;
        }

        protected virtual void Explode()
        {
            this.Destroy(DestroyMode.Vanish);
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            GenExplosion.DoExplosion(base.Position, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, this.def.projectile.soundExplode, this.def, this.equipmentDef, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.explosionSpawnChance, 1, false, preExplosionSpawnThingDef, explosionSpawnChance, 1);
        }
    }
}
