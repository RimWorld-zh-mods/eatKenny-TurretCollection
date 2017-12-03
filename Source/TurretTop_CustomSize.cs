using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace TurretCollection {
    public class TurretTop_CustomSize {

        private Building_TurretGunCustom parentTurret;

        private float curRotationInt;

        private int ticksUntilIdleTurn;

        private int idleTurnTicksLeft;

        private bool idleTurnClockwise;

        private const float IdleTurnDegreesPerTick = 0.26f;

        private const int IdleTurnDuration = 140;

        private const int IdleTurnIntervalMin = 150;

        private const int IdleTurnIntervalMax = 350;

        private float CurRotation {
            get {
                return this.curRotationInt;
            }
            set {
                this.curRotationInt = value;
                if (this.curRotationInt > 360.0) {
                    this.curRotationInt -= 360f;
                }
                if (this.curRotationInt < 0.0) {
                    this.curRotationInt += 360f;
                }
            }
        }

        public TurretTop_CustomSize(Building_TurretGunCustom ParentTurret) {
            this.parentTurret = ParentTurret;
        }

        public void TurretTopTick() {
            LocalTargetInfo currentTarget = this.parentTurret.CurrentTarget;
            if (currentTarget.IsValid) {
                IntVec3 cell = currentTarget.Cell;
                float num2 = this.CurRotation = (cell.ToVector3Shifted() - this.parentTurret.DrawPos).AngleFlat();
                this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
            } else if (this.ticksUntilIdleTurn > 0) {
                this.ticksUntilIdleTurn--;
                if (this.ticksUntilIdleTurn == 0) {
                    if (Rand.Value < 0.5) {
                        this.idleTurnClockwise = true;
                    } else {
                        this.idleTurnClockwise = false;
                    }
                    this.idleTurnTicksLeft = 140;
                }
            } else {
                if (this.idleTurnClockwise) {
                    this.CurRotation += 0.26f;
                } else {
                    this.CurRotation -= 0.26f;
                }
                this.idleTurnTicksLeft--;
                if (this.idleTurnTicksLeft <= 0) {
                    this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
                }
            }
        }

        public void DrawTurret() {
            Matrix4x4 matrix4x = default(Matrix4x4);
            matrix4x.SetTRS(this.parentTurret.DrawPos + Altitudes.AltIncVect, this.CurRotation.ToQuat(), this.parentTurret.TopSizeComp == null ? Vector3.one : this.parentTurret.TopSizeComp.Props.topSize);
            Graphics.DrawMesh(MeshPool.plane20, matrix4x, this.parentTurret.def.building.turretTopMat, 0);
        }
    }
}
