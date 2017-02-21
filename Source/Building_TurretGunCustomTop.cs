using RimWorld;
using System;
using Verse;
using UnityEngine;

namespace TurretCollection
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunCustomTop : Building_TurretGun
    {
        protected TurretTopCustomSize topXL;

        protected Vector3 topSize;

        private bool holdFire;

        public Building_TurretGunCustomTop()
        {
            this.topXL = new TurretTopCustomSize(this);
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
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
                return this.MannedByColonist;
            }
        }

        private bool MannedByColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }


        private void ResetForcedTarget()
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething();
            }
        }

        private bool CanToggleHoldFire
        {
            get
            {
                return base.Faction == Faction.OfPlayer || this.MannedByColonist;
            }
        }

        public override void Tick()
        {
            if (this.powerComp != null && !this.powerComp.PowerOn)
            {
                return;
            }
            if (this.mannableComp != null && !this.mannableComp.MannedNow)
            {
                return;
            }
            if (!this.CanSetForcedTarget && this.forcedTarget.IsValid)
            {
                this.ResetForcedTarget();
            }
            if (!this.CanToggleHoldFire)
            {
                this.holdFire = false;
            }
            this.GunCompEq.verbTracker.VerbsTick();
            if (this.stunner.Stunned)
            {
                return;
            }
            if (this.GunCompEq.PrimaryVerb.state == VerbState.Bursting)
            {
                return;
            }
            if (this.WarmingUp)
            {
                this.burstWarmupTicksLeft--;
                if (this.burstWarmupTicksLeft == 0)
                {
                    this.BeginBurst();
                }
            }
            else
            {
                if (this.burstCooldownTicksLeft > 0)
                {
                    this.burstCooldownTicksLeft--;
                }
                if (this.burstCooldownTicksLeft == 0)
                {
                    this.TryStartShootSomething();
                }
            }
            this.topXL.TurretTopTick();
        }

        public override void Draw()
        {
            this.topXL.DrawTurretXL(this.topSize);
            base.Comps_PostDraw();
        }
    }
}
