using System;
using Verse;

namespace TurretCollection
{
	public class CompExtraDamage : ThingComp
	{
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
            CompExtraDamage_Properties compAPI_Properties = props as CompExtraDamage_Properties;
			if (compAPI_Properties != null)
			{
				this.props = compAPI_Properties;
			}
		}
	}
}