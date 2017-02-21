using System;
using Verse;
using UnityEngine;

namespace TurretCollection
{
    class CompProperties_TurretTopSize : CompProperties
    {
        public Vector3 topSize = Vector3.one;

        public CompProperties_TurretTopSize()
        {
            this.compClass = typeof(CompTurretTopSize);
        }
    }
}
