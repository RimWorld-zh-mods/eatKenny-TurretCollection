// system namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;

//Rimwolrd namespaces
using RimWorld;
using RimWorld.Planet;
using RimWorld.SquadAI;
using Verse;
using Verse.Grammar;
using Verse.Sound;
using UnityEngine;


namespace BigGuns {
    public class Building_BigTurretGun : Building_TurretGun {

        protected new BigTurretTop top;

        private bool WarmingUp {
            get {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        public override void SpawnSetup() {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.mannableComp = base.GetComp<CompMannable>();
            this.gun = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
            this.GunCompEq.verbTracker.InitVerbs();
            for (int i = 0; i < this.GunCompEq.AllVerbs.Count; i++) {
                Verb verb = this.GunCompEq.AllVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = new Action(this.BurstComplete);
            }
            this.top = new BigTurretTop(this);
        }

        public override void Draw() {
            base.Draw();
            this.top.DrawTurret();
        }

        public override void Tick() {
            base.Tick();
            if (this.powerComp != null && !this.powerComp.PowerOn) {
                return;
            }
            if (this.mannableComp != null && !this.mannableComp.MannedNow) {
                return;
            }
            this.GunCompEq.verbTracker.VerbsTick();
            if (this.stunner.Stunned) {
                return;
            }
            if (this.GunCompEq.PrimaryVerb.state == VerbState.Bursting) {
                return;
            }
            if (this.WarmingUp) {
                this.burstWarmupTicksLeft--;
                if (this.burstWarmupTicksLeft == 0) {
                    this.BeginBurst();
                }
            }
            else {
                if (this.burstCooldownTicksLeft > 0) {
                    this.burstCooldownTicksLeft--;
                }
                if (this.burstCooldownTicksLeft == 0) {
                    this.TryStartShootSomething();
                }
            }
            this.top.TurretTopTick();
        }
    }
}
