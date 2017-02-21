using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;

namespace TurretCollection
{
    class CompTurretTopSize : ThingComp
    {
        public CompProperties_TurretTopSize Props
        {
            get
            {
                return (CompProperties_TurretTopSize)this.props;
            }
        }
    }
}
