using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TurretCollection
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunCustomTop : Building_TurretGun
    {
        private const int TryStartShootSomethingIntervalTicks = 10;

        //protected int burstCooldownTicksLeft;

        //protected int burstWarmupTicksLeft;

        //protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        //public bool loaded;

        private bool holdFire;

        private Thing gunInt;

        protected new TurretTopCustomSize top;

        //protected CompPowerTrader powerComp;

        //protected CompMannable mannableComp;

        //public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

        public Vector3 topSize;

        //public CompEquippable GunCompEq
        //{
        //    get
        //    {
        //        return this.Gun.TryGetComp<CompEquippable>();
        //    }
        //}

        //public override LocalTargetInfo CurrentTarget
        //{
        //    get
        //    {
        //        return this.currentTargetInt;
        //    }
        //}

        private bool WarmingUp
        {
            get
            {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        //public override Verb AttackVerb
        //{
        //    get
        //    {
        //        return this.GunCompEq.verbTracker.PrimaryVerb;
        //    }
        //}

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

        //public Thing Gun
        //{
        //    get
        //    {
        //        if (this.gunInt == null)
        //        {
        //            this.gunInt = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
        //            List<Verb> allVerbs = this.gunInt.TryGetComp<CompEquippable>().AllVerbs;
        //            for (int i = 0; i < allVerbs.Count; i++)
        //            {
        //                Verb verb = allVerbs[i];
        //                verb.caster = this;
        //                verb.castCompleteCallback = new Action(this.BurstComplete);
        //            }
        //        }
        //        return this.gunInt;
        //    }
        //}

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

        public Building_TurretGunCustomTop()
        {
            this.top = new TurretTopCustomSize(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            //this.powerComp = base.GetComp<CompPowerTrader>();
            //this.mannableComp = base.GetComp<CompMannable>();
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

        //public override void DeSpawn()
        //{
        //    base.DeSpawn();
        //    this.ResetCurrentTarget();
        //}

        //public override void ExposeData()
        //{
        //    base.ExposeData();
        //    Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
        //    Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
        //    Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
        //    Scribe_Values.Look<bool>(ref this.loaded, "loaded", false, false);
        //    Scribe_Values.Look<bool>(ref this.holdFire, "holdFire", false, false);
        //}

        //public override void OrderAttack(LocalTargetInfo targ)
        //{
        //    if (!targ.IsValid)
        //    {
        //        if (this.forcedTarget.IsValid)
        //        {
        //            this.ResetForcedTarget();
        //        }
        //        return;
        //    }
        //    if ((targ.Cell - base.Position).LengthHorizontal < this.GunCompEq.PrimaryVerb.verbProps.minRange)
        //    {
        //        Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageSound.RejectInput);
        //        return;
        //    }
        //    if ((targ.Cell - base.Position).LengthHorizontal > this.GunCompEq.PrimaryVerb.verbProps.range)
        //    {
        //        Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageSound.RejectInput);
        //        return;
        //    }
        //    if (this.forcedTarget != targ)
        //    {
        //        this.forcedTarget = targ;
        //        if (this.burstCooldownTicksLeft <= 0)
        //        {
        //            this.TryStartShootSomething(false);
        //        }
        //    }
        //}

        public override void Tick()
        {
            base.Tick();
            if (this.forcedTarget.IsValid && !this.CanSetForcedTarget)
            {
                this.ResetForcedTarget();
            }
            if (!this.CanToggleHoldFire)
            {
                this.holdFire = false;
            }
            if (this.forcedTarget.ThingDestroyed)
            {
                this.ResetForcedTarget();
            }
            bool flag = (this.powerComp == null || this.powerComp.PowerOn) && (this.mannableComp == null || this.mannableComp.MannedNow);
            if (flag && base.Spawned)
            {
                this.GunCompEq.verbTracker.VerbsTick();
                if (!this.stunner.Stunned && this.GunCompEq.PrimaryVerb.state != VerbState.Bursting)
                {
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
                        if (this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                        {
                            this.TryStartShootSomething(true);
                        }
                    }
                    this.top.TurretTopTick();
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
        }

        //protected void TryStartShootSomething(bool canBeginBurstImmediately)
        //{
        //    if (!base.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.GunCompEq.PrimaryVerb.verbProps.projectileDef.projectile.flyOverhead && base.Map.roofGrid.Roofed(base.Position)))
        //    {
        //        this.ResetCurrentTarget();
        //        return;
        //    }
        //    bool isValid = this.currentTargetInt.IsValid;
        //    if (this.forcedTarget.IsValid)
        //    {
        //        this.currentTargetInt = this.forcedTarget;
        //    }
        //    else
        //    {
        //        this.currentTargetInt = this.TryFindNewTarget();
        //    }
        //    if (!isValid && this.currentTargetInt.IsValid)
        //    {
        //        SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        //    }
        //    if (this.currentTargetInt.IsValid)
        //    {
        //        if (this.def.building.turretBurstWarmupTime > 0f)
        //        {
        //            this.burstWarmupTicksLeft = this.def.building.turretBurstWarmupTime.SecondsToTicks();
        //        }
        //        else if (canBeginBurstImmediately)
        //        {
        //            this.BeginBurst();
        //        }
        //        else
        //        {
        //            this.burstWarmupTicksLeft = 1;
        //        }
        //    }
        //    else
        //    {
        //        this.ResetCurrentTarget();
        //    }
        //}

        //protected LocalTargetInfo TryFindNewTarget()
        //{
        //    IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
        //    Faction faction = attackTargetSearcher.Thing.Faction;
        //    float range = this.GunCompEq.PrimaryVerb.verbProps.range;
        //    float minRange = this.GunCompEq.PrimaryVerb.verbProps.minRange;
        //    Building t;
        //    if (Rand.Value < 0.5f && this.GunCompEq.PrimaryVerb.verbProps.projectileDef.projectile.flyOverhead && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
        //    {
        //        float num = (float)x.Position.DistanceToSquared(this.Position);
        //        return num > minRange * minRange && num < range * range;
        //    }).TryRandomElement(out t))
        //    {
        //        return t;
        //    }
        //    TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
        //    if (!this.GunCompEq.PrimaryVerb.verbProps.projectileDef.projectile.flyOverhead)
        //    {
        //        targetScanFlags |= TargetScanFlags.NeedLOSToAll;
        //        targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
        //    }
        //    if (this.GunCompEq.PrimaryVerb.verbProps.ai_IsIncendiary)
        //    {
        //        targetScanFlags |= TargetScanFlags.NeedNonBurning;
        //    }
        //    return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, new Predicate<Thing>(this.IsValidTarget), range, minRange, targetScanFlags);
        //}

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

        //protected void BeginBurst()
        //{
        //    this.GunCompEq.PrimaryVerb.TryStartCastOn(this.CurrentTarget, false, true);
        //    base.OnAttackedTarget(this.CurrentTarget);
        //}

        //protected void BurstComplete()
        //{
        //    if (this.def.building.turretBurstCooldownTime >= 0f)
        //    {
        //        this.burstCooldownTicksLeft = this.def.building.turretBurstCooldownTime.SecondsToTicks();
        //    }
        //    else
        //    {
        //        this.burstCooldownTicksLeft = this.GunCompEq.PrimaryVerb.verbProps.defaultCooldownTime.SecondsToTicks();
        //    }
        //    this.loaded = false;
        //}

        //public override string GetInspectString()
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    string inspectString = base.GetInspectString();
        //    if (!inspectString.NullOrEmpty())
        //    {
        //        stringBuilder.AppendLine(inspectString);
        //    }
        //    stringBuilder.AppendLine("GunInstalled".Translate() + ": " + this.Gun.Label);
        //    if (this.GunCompEq.PrimaryVerb.verbProps.minRange > 0f)
        //    {
        //        stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.GunCompEq.PrimaryVerb.verbProps.minRange.ToString("F0"));
        //    }
        //    if (base.Spawned && this.burstCooldownTicksLeft > 0)
        //    {
        //        stringBuilder.AppendLine("CanFireIn".Translate() + ": " + this.burstCooldownTicksLeft.TicksToSecondsString());
        //    }
        //    if (this.def.building.turretShellDef != null)
        //    {
        //        if (this.loaded)
        //        {
        //            stringBuilder.AppendLine("ShellLoaded".Translate());
        //        }
        //        else
        //        {
        //            stringBuilder.AppendLine("ShellNotLoaded".Translate());
        //        }
        //    }
        //    return stringBuilder.ToString().TrimEndNewlines();
        //}

        public override void Draw()
        {
            this.top.DrawTurret();
            //base.Draw();
            base.Comps_PostDraw();
        }

        //public override void DrawExtraSelectionOverlays()
        //{
        //    float range = this.GunCompEq.PrimaryVerb.verbProps.range;
        //    if (range < 90f)
        //    {
        //        GenDraw.DrawRadiusRing(base.Position, range);
        //    }
        //    float minRange = this.GunCompEq.PrimaryVerb.verbProps.minRange;
        //    if (minRange < 90f && minRange > 0.1f)
        //    {
        //        GenDraw.DrawRadiusRing(base.Position, minRange);
        //    }
        //    if (this.WarmingUp)
        //    {
        //        int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
        //        GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, (float)this.def.size.x * 0.5f);
        //    }
        //    if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
        //    {
        //        Vector3 b;
        //        if (this.forcedTarget.HasThing)
        //        {
        //            b = this.forcedTarget.Thing.TrueCenter();
        //        }
        //        else
        //        {
        //            b = this.forcedTarget.Cell.ToVector3Shifted();
        //        }
        //        Vector3 a = this.TrueCenter();
        //        b.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
        //        a.y = b.y;
        //        GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
        //    }
        //}

        //[DebuggerHidden]
        //public override IEnumerable<Gizmo> GetGizmos()
        //{
        //    Building_TurretGun.< GetGizmos > c__Iterator148 < GetGizmos > c__Iterator = new Building_TurretGun.< GetGizmos > c__Iterator148();

        //    < GetGizmos > c__Iterator.<> f__this = this;
        //    Building_TurretGun.< GetGizmos > c__Iterator148 expr_0E = < GetGizmos > c__Iterator;
        //    expr_0E.$PC = -2;
        //    return expr_0E;
        //}

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
    }
}
