using System;
using Verse;

namespace TurretCollection
{
	public class CompExtraDamage : ThingComp
	{
        public CompProperties_ExtraDamage Props
        {
            get
            {
                return (CompProperties_ExtraDamage)this.props;
            }
        }
	}
}