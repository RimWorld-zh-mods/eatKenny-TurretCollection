using System;
using RimWorld;
using Verse;

namespace TurretCollection
{
	public class CompProperties_ExtraDamage : CompProperties
	{
		public int ExtraDamageAmount = 1;
        public DamageDef ExtraDamageDef = DamageDefOf.Bomb;

        public CompProperties_ExtraDamage()
		{
			this.compClass = typeof(CompExtraDamage);
		}
	}
}