using System;
using Verse;

namespace TurretCollection
{
	public class CompExtraDamage_Properties : CompProperties
	{
		public int ExtraDamageAmount;
        public DamageDef ExtraDamageDef;

        public CompExtraDamage_Properties()
		{
			this.compClass = typeof(CompExtraDamage);
		}
	}
}