using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_TurretTopSize : CompProperties {
        public Vector3 topSize = Vector3.one;

        public CompProperties_TurretTopSize() {
            base.compClass = typeof(CompTurretTopSize);
        }
    }

    public class CompTurretTopSize : ThingComp {
        public CompProperties_TurretTopSize Props => (CompProperties_TurretTopSize)base.props;
    }
}
