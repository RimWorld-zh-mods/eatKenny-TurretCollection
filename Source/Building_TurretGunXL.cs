using RimWorld;
using System;
using Verse;

namespace TurretCollection
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunXL : Building_TurretGun
    {
        protected TurretTopXL topXL;

        private bool holdFire;

        public Building_TurretGunXL()
        {
            this.topXL = new TurretTopXL(this);
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
            this.forcedTarget = TargetInfo.Invalid;
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
            this.topXL.DrawTurretXL();
            base.Comps_PostDraw();
        }
    }
}
