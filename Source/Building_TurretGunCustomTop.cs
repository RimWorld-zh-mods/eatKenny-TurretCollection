using System;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace TurretCollection
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunCustomTop : Building_TurretGun
    {
        public Vector3 topSize;

        public Building_TurretGunCustomTop()
        {
            this.top = new TurretTopCustomSize(this);
        }

        private bool WarmingUp
        {
            get
            {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        private bool CanSetForcedTarget
        {
            get
            {
                return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        private bool CanToggleHoldFire
        {
            get
            {
                return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        private bool MannedByColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }

        private bool MannedByNonColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompTurretTopSize compTurretTopSize = base.GetComp<CompTurretTopSize>();
            if (compTurretTopSize != null)
            {
                this.topSize = compTurretTopSize.Props.topSize;
            }
            else
            {
                this.topSize = Vector3.one;
            }
        }

        private IAttackTargetSearcher TargSearcher()
        {
            if (this.mannableComp != null && this.mannableComp.MannedNow)
            {
                return this.mannableComp.ManningPawn;
            }
            return this;
        }

        private bool IsValidTarget(Thing t)
        {
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                if (this.GunCompEq.PrimaryVerb.verbProps.projectileDef.projectile.flyOverhead)
                {
                    RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                if (this.mannableComp == null)
                {
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        private void ResetForcedTarget()
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething(false);
            }
        }

        private void ResetCurrentTarget()
        {
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }

        public override void Draw()
        {
            ((TurretTopCustomSize)this.top).DrawTurret();
            base.Comps_PostDraw();
        }
    }
}
