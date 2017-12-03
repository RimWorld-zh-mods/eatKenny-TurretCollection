using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_ProjectileLineImpact : CompProperties {
        public bool stunTarget = false;
        public bool smokeAtImpact = true;
        public string throwAtImpact = "Lightning";

        public CompProperties_ProjectileLineImpact() {
            base.compClass = typeof(CompProjectileLineImpact);
        }
    }

    public class CompProjectileLineImpact : ThingComp {
        public CompProperties_ProjectileLineImpact Props => (CompProperties_ProjectileLineImpact)base.props;
    }
}
