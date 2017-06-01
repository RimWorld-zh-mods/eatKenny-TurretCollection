using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace TurretCollection
{
    public class TurretTopCustomSize : TurretTop
    {
        private Building_Turret parentTurret;

        private float curRotationInt;

        private float CurRotation
        {
            get
            {
                return this.curRotationInt;
            }
            set
            {
                this.curRotationInt = value;
                if (this.curRotationInt > 360f)
                {
                    this.curRotationInt -= 360f;
                }
                if (this.curRotationInt < 0f)
                {
                    this.curRotationInt += 360f;
                }
            }
        }

        public TurretTopCustomSize(Building_Turret ParentTurret):base(ParentTurret)
        {
            this.parentTurret = ParentTurret;
        }

        public void DrawTurret()
        {
            //Log.Message("TurretCollection.TurretTopCustomSize.DrawTurret(), topSize: " + ((Building_TurretGunCustomTop)this.parentTurret).topSize.ToString());
            Matrix4x4 matrix4x = default(Matrix4x4);
            matrix4x.SetTRS(this.parentTurret.DrawPos + Altitudes.AltIncVect, this.CurRotation.ToQuat(), ((Building_TurretGunCustomTop)this.parentTurret).topSize);
            Graphics.DrawMesh(MeshPool.plane20, matrix4x, this.parentTurret.def.building.turretTopMat, 0);
        }
    }
}
