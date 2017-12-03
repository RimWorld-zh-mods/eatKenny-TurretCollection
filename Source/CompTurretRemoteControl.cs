using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_TurretRemoteContol : CompProperties {
        public CompProperties_TurretRemoteContol() {
            base.compClass = typeof(CompTurretRemoteControl);
        }
    }

    public class CompTurretRemoteControl : ThingComp {
        CompProperties_TurretRemoteContol Props => (CompProperties_TurretRemoteContol)base.props;
    }
}
