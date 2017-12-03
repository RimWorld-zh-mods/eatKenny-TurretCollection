using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_TurretDestroySelfAfterFire : CompProperties {
        public CompProperties_TurretDestroySelfAfterFire() {
            base.compClass = typeof(CompTurretDestroySelfAfterFire);
        }
    }

    public class CompTurretDestroySelfAfterFire : ThingComp {
        public CompProperties_TurretDestroySelfAfterFire Props => (CompProperties_TurretDestroySelfAfterFire)base.props;
    }
}
