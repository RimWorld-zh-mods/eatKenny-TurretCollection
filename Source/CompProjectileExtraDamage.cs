using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace TurretCollection {
    public class CompProperties_ProjectileExtraDamage : CompProperties {
        public string hitText = "TC_Hit";
        public Color hitTextColor = new Color32(255, 153, 102, 255);
        public int damageAmountBase = 1;
        public DamageDef damageDef = DamageDefOf.Bullet;

        public CompProperties_ProjectileExtraDamage() {
            base.compClass = typeof(CompProjectileExtraDamage);
        }
    }

    public class CompProjectileExtraDamage : ThingComp {
        public CompProperties_ProjectileExtraDamage Props => (CompProperties_ProjectileExtraDamage)base.props;
    }
}