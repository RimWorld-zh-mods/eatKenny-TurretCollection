using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection {
    public class Projectile_Custom : Projectile {

        // usually for armor-piercing projectile
        protected CompProjectileExtraDamage extraDamageComp;

        private int ticksToDetonation;

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.extraDamageComp = base.GetComp<CompProjectileExtraDamage>();
        }

        public override void Tick() {
            base.Tick();
            if (this.ticksToDetonation > 0) {
                this.ticksToDetonation--;
                if (this.ticksToDetonation <= 0) {
                    this.Explode();
                }
            }
        }

        protected override void Impact(Thing hitThing) {
            Map map = base.Map;
            if (map == null) {
                Log.Error("map is null!");
            }
            if (this.def.projectile.explosionRadius > 0) {
                if (this.extraDamageComp != null) {
                    this.ImpactExtra(hitThing, map);
                }
                this.ImpactExplode(hitThing);
            } else {
                this.ImpactDirectly(hitThing, map);
                if (this.extraDamageComp != null) {
                    this.ImpactExtra(hitThing, map);
                }
            }
        }

        protected virtual void ImpactExtra(Thing hitThing, Map map) {
            if (hitThing != null) {
                int damageAmountBase = this.extraDamageComp.Props.damageAmountBase;
                DamageDef damageDef = this.extraDamageComp.Props.damageDef;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = base.launcher;
                ThingDef equipmentDef = base.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmountBase, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo);
                if (hitThing.def.category == ThingCategory.Pawn) {
                    MoteMaker.ThrowText(new Vector3((float)this.Position.x + 1f, (float)this.Position.y, (float)this.Position.z + 1f),
                        map,
                        this.extraDamageComp.Props.hitText.Translate(),
                        this.extraDamageComp.Props.hitTextColor);
                }
            }
        }

        protected virtual void ImpactDirectly(Thing hitThing, Map map) {
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(base.launcher, hitThing, base.intendedTarget, base.equipmentDef, base.def);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null) {
                int damageAmountBase = base.def.projectile.damageAmountBase;
                DamageDef damageDef = base.def.projectile.damageDef;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = base.launcher;
                ThingDef equipmentDef = base.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmountBase, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_RangedImpact);
            } else {
                SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, map, false));
                MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes) {
                    MoteMaker.MakeWaterSplash(this.ExactPosition, map, (float)(Mathf.Sqrt((float)base.def.projectile.damageAmountBase) * 1.0), 4f);
                }
            }
        }

        protected virtual void ImpactExplode(Thing hitThing) {
            if (base.def.projectile.explosionDelay == 0) {
                this.Explode();
            } else {
                base.landed = true;
                this.ticksToDetonation = base.def.projectile.explosionDelay;
                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, base.def.projectile.damageDef, base.launcher.Faction);
            }
        }

        protected virtual void Explode() {
            Map map = base.Map;
            this.Destroy(DestroyMode.Vanish);
            if (base.def.projectile.explosionEffect != null) {
                Effecter effecter = base.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = base.Position;
            Map map2 = map;
            float explosionRadius = base.def.projectile.explosionRadius;
            DamageDef damageDef = base.def.projectile.damageDef;
            Thing launcher = base.launcher;
            int damageAmountBase = base.def.projectile.damageAmountBase;
            SoundDef soundExplode = base.def.projectile.soundExplode;
            ThingDef equipmentDef = base.equipmentDef;
            ThingDef def = base.def;
            ThingDef postExplosionSpawnThingDef = base.def.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = base.def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = base.def.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = base.def.projectile.preExplosionSpawnThingDef;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmountBase, soundExplode, equipmentDef, def, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, base.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, base.def.projectile.preExplosionSpawnChance, base.def.projectile.preExplosionSpawnThingCount, base.def.projectile.explosionChanceToStartFire, base.def.projectile.explosionDealMoreDamageAtCenter);
        }
    }
}
