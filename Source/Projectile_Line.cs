using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace TurretCollection {
    public class Projectile_Line : Projectile_Custom {
        public const float stunChance = 0.3f;

        protected CompProjectileLineImpact impactComp;

        public Matrix4x4 lineMatrix = default(Matrix4x4);
        public Vector3 lineScale = new Vector3(1f, 1f, 1f);

        private int age;

        protected float LineBrightness {
            get {
                if (this.age <= 3) {
                    return (float)(this.age / 3f);
                }
                return (1f - this.age / (float)Rand.Range(15, 60));
            }
        }

        protected void ComputeDrawingParameters() {
            //this.origin;
            //this.destination;
            Vector3 position = (base.origin + base.destination) / 2f;
            float z = (base.destination - base.origin).MagnitudeHorizontal();
            //Quaternion quaternion = Quaternion.LookRotation(base.origin - base.destination);
            Vector3 scale = new Vector3(1f, 1f, z);
            this.lineMatrix.SetTRS(position, this.ExactRotation, scale);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.impactComp = base.GetComp<CompProjectileLineImpact>();
        }

        public override void Tick() {
            if (this.age == 0) {
                this.ComputeDrawingParameters();
            }
            base.Tick();
            this.age++;
        }

        protected override void Impact(Thing hitThing) {
            Map map = base.Map;
            if (hitThing != null) {
                if (this.impactComp != null) {
                    CompProperties_ProjectileLineImpact impactProps = this.impactComp.Props;
                    if (impactProps.smokeAtImpact) {
                        MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), base.Map, 1f);
                    }
                    string throwAtImpact = impactProps.throwAtImpact;
                    if (throwAtImpact == "Lightning") {
                        MoteMaker.ThrowLightningGlow(base.destination, base.Map, 1.5f);
                    } else if (throwAtImpact == "Fire") {
                        MoteMaker.ThrowFireGlow(base.destination.ToIntVec3(), map, 1.5f);
                    } else if (!string.IsNullOrEmpty(throwAtImpact)) {
                        Log.Error($"Projectile {base.def.defName} 'CompProperties_ProjectileLineImpact' setting error: Value '{throwAtImpact}' of throwAtImpact is invalid. Available value: empty string, 'Lightning', 'Fire'.");
                    }
                    if (impactProps.stunTarget) {
                        Pawn pawn = hitThing as Pawn;
                        if (pawn != null && !pawn.Downed && Rand.Value < Projectile_Line.stunChance) {
                            hitThing.TakeDamage(new DamageInfo(DamageDefOf.Stun, 10, -1, this.launcher, null, null));
                        }
                    }
                }
            } else {
                MoteMaker.ThrowMicroSparks(this.destination, map);
            }
            base.Impact(hitThing);
        }

        public override void Draw() {
            //Graphics.DrawMesh(MeshPool.plane10, this.lineMatrix, this.def.DrawMatSingle, 0);
            Graphics.DrawMesh(MeshPool.plane10, this.lineMatrix, FadedMaterialPool.FadedVersionOf(this.def.DrawMatSingle, this.LineBrightness), 0);
            base.Comps_PostDraw();
        }
    }
}
