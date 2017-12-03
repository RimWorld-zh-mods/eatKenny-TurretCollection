using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_ProjectileSmoke : CompProperties {
        public bool smokeAtFire = false;
        public float smokeAtFireSize = 3f;

        public bool smokeAtTrajectory = false;
        public float smokeAtTrajectorySize = 3f;
        public int smokeAtTrajectoryInterval = 3;

        public CompProperties_ProjectileSmoke() {
            base.compClass = typeof(CompProjectileSmoke);
        }
    }

    public class CompProjectileSmoke : ThingComp {
        public CompProperties_ProjectileSmoke Props => (CompProperties_ProjectileSmoke)base.props;
        
        private bool smokeAtTrajectory = false;
        private float smokeAtTrajectorySize = 3f;
        private int smokeAtTrajectoryInterval = 3;

        private int tickToSmoke = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            if (this.Props.smokeAtFire) {
                MoteMaker.ThrowSmoke(this.parent.Position.ToVector3Shifted(), this.parent.Map, this.Props.smokeAtFireSize);
            }
            this.smokeAtTrajectory = this.Props.smokeAtTrajectory;
            this.smokeAtTrajectorySize = this.Props.smokeAtTrajectorySize;
            this.smokeAtTrajectoryInterval = this.Props.smokeAtTrajectoryInterval;
        }

        public override void CompTick() {
            if (this.smokeAtTrajectory) {
                this.tickToSmoke--;
                if (this.tickToSmoke <= 0) {
                    MoteMaker.ThrowSmoke(this.parent.Position.ToVector3Shifted(), this.parent.Map, this.smokeAtTrajectorySize);
                    this.tickToSmoke = this.smokeAtTrajectoryInterval;
                }
            }
        }
    }
}
