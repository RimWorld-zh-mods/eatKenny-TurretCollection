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

namespace TurretCollection {
    [StaticConstructorOnStartup]
    public class Building_TurretGunCustom : Building_TurretGun {

        // For custom top gun size
        // hide base field 'top'
        protected new TurretTop_CustomSize top;
        protected CompTurretTopSize topSizeComp;
        public CompTurretTopSize TopSizeComp => this.topSizeComp;

        // For smart mine
        protected CompTurretDestroySelfAfterFire destroySelfComp;

        // For force target
        protected CompTurretRemoteControl remoteControlComp;
        
        private bool holdFire;

        private const int TryStartShootSomethingIntervalTicks = 10;

        private bool WarmingUp {
            get {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        // Modified due to remoteControlComp
        public bool CanSetForcedTarget {
            get {
                if (this.mannableComp == null) {
                    return this.remoteControlComp != null;
                } else {
                    return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
                }
            }
        }

        private bool CanToggleHoldFire {
            get {
                return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        private bool MannedByColonist {
            get {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }

        private bool MannedByNonColonist {
            get {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }

        public Building_TurretGunCustom() {
            this.top = new TurretTop_CustomSize(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.topSizeComp = base.GetComp<CompTurretTopSize>();
            this.destroySelfComp = base.GetComp<CompTurretDestroySelfAfterFire>();
            this.remoteControlComp = base.GetComp<CompTurretRemoteControl>();
        }

        public override void Tick() {
            base.Tick();
            if (base.forcedTarget.IsValid && !this.CanSetForcedTarget) {
                this.ResetForcedTarget();
            }
            if (!this.CanToggleHoldFire) {
                this.holdFire = false;
            }
            if (base.forcedTarget.ThingDestroyed) {
                this.ResetForcedTarget();
            }
            if ((this.powerComp == null || this.powerComp.PowerOn) && (this.mannableComp == null || this.mannableComp.MannedNow) && base.Spawned) {
                this.GunCompEq.verbTracker.VerbsTick();
                if (!base.stunner.Stunned && this.GunCompEq.PrimaryVerb.state != VerbState.Bursting) {
                    if (this.WarmingUp) {
                        this.burstWarmupTicksLeft--;
                        if (this.burstWarmupTicksLeft == 0) {
                            this.BeginBurst();
                        }
                    } else {
                        if (this.burstCooldownTicksLeft > 0) {
                            if (this.destroySelfComp != null) {
                                this.Destroy(DestroyMode.Vanish);
                                return;
                            }
                            this.burstCooldownTicksLeft--;
                        }
                        if (this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10)) {
                            this.TryStartShootSomething(true);
                        }
                    }
                    this.top.TurretTopTick();
                }
            } else {
                this.ResetCurrentTarget();
            }
        }

        private IAttackTargetSearcher TargSearcher() {
            if (this.mannableComp != null && this.mannableComp.MannedNow) {
                return this.mannableComp.ManningPawn;
            }
            return this;
        }

        private bool IsValidTarget(Thing t) {
            Pawn pawn = t as Pawn;
            if (pawn != null) {
                if (this.GunCompEq.PrimaryVerb.ProjectileFliesOverhead()) {
                    RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof) {
                        return false;
                    }
                }
                if (this.mannableComp == null) {
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer) {
                    return false;
                }
            }
            return true;
        }

        private void ResetForcedTarget() {
            base.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0) {
                this.TryStartShootSomething(false);
            }
        }

        private void UpdateGunVerbs() {
            List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++) {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = this.BurstComplete;
            }
        }

        private void ResetCurrentTarget() {
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }


        public override void Draw() {
            this.top.DrawTurret();
            base.Comps_PostDraw();
        }
    }
}
