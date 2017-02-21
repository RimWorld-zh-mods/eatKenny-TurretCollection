using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace TurretCollection
{
    /// <summary>
    /// Common laser type projectile class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Projectile_ElectricArc : Projectile
    {
        private Map map;

        // Variables.
        public int tickCounter = 0;
        public Thing hitThing = null;

        // Miscellaneous.
        public const float stunChance = 0.3f;
        
        // Draw variables.
        public Material preFiringTexture;
        public Material postFiringTexture;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        public float drawingIntensity = 1f;
        public Material drawingTexture = null;

        // Custom XML variables.
            // Miscellaneous.
        public bool isWarmupProjectile = false;
        public string warmupProjectileDefName = null;
        public int preFiringDuration = 0;
        public int postFiringDuration = 3;
            // Draw.
        public float preFiringInitialIntensity = 0f;
        public float preFiringFinalIntensity = 1f;
        public float postFiringInitialIntensity = 1f;
        public float postFiringFinalIntensity = 0f;
        public Material warmupTexture = null;
            // Sound.
        public SoundDef warmupSound = null;

        /// <summary>
        /// Get parameters from XML.
        /// </summary>
        /// <summary>
        /// Save/load data from a savegame file (apparently not used for projectile for now).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref tickCounter, "tickCounter", 0);
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.map = map;
            MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), this.map, 1.5f);
        }

        /// <summary>
        /// Main projectile sequence.
        /// </summary>
        public override void Tick()
        {
            // Directly call the Projectile base Tick function (we want to completely override the Projectile Tick() function).
            //((ThingWithComponents)this).Tick(); // Does not work...
            
            if (tickCounter == 0)
            {
                PerformPreFiringTreatment();
            }

            // Pre firing.
            if (tickCounter < preFiringDuration)
            {
                GetPreFiringDrawingParameters();
            }
            // Firing.
            else if (tickCounter == this.preFiringDuration)
            {
                Fire();
                GetPostFiringDrawingParameters();
            }
            // Post firing.
            else
            {
                GetPostFiringDrawingParameters();
            }

            if (tickCounter == (this.preFiringDuration + this.postFiringDuration))
            {
                this.Destroy(DestroyMode.Vanish);
            }
            if (this.launcher is Pawn)
            {
                Pawn launcherPawn = this.launcher as Pawn;
                if (((launcherPawn.stances.curStance is Stance_Warmup) == false)
                    && ((launcherPawn.stances.curStance is Stance_Cooldown) == false))
                {
                    this.Destroy(DestroyMode.Vanish);
                }
            }

            tickCounter++;
        }
        
        /// <summary>
        /// Performs prefiring treatment: data initalization.
        /// </summary>
        public virtual void PerformPreFiringTreatment()
        {
            drawingTexture = this.def.DrawMatSingle;
            DetermineImpactExactPosition();
            Vector3 cannonMouthOffset = ((this.destination - this.origin).normalized * 0.9f);
            drawingScale = new Vector3(1f, 1f, (this.destination - this.origin).magnitude - cannonMouthOffset.magnitude);
            drawingPosition = this.origin + (cannonMouthOffset / 2) + ((this.destination - this.origin) / 2) + Vector3.up * this.def.Altitude;
            drawingMatrix.SetTRS(drawingPosition, this.ExactRotation, drawingScale);
        }
        
        /// <summary>
        /// Gets the prefiring drawing parameters.
        /// </summary>
        public virtual void GetPreFiringDrawingParameters()
        {
            if (preFiringDuration != 0)
            {
                drawingIntensity = preFiringInitialIntensity + (preFiringFinalIntensity - preFiringInitialIntensity) * (float)tickCounter / (float)preFiringDuration;
            }
        }

        /// <summary>
        /// Gets the postfiring drawing parameters.
        /// </summary>
        public virtual void GetPostFiringDrawingParameters()
        {
            if (postFiringDuration != 0)
            {
                drawingIntensity = postFiringInitialIntensity + (postFiringFinalIntensity - postFiringInitialIntensity) * (((float)tickCounter - (float)preFiringDuration) / (float)postFiringDuration);
            }
        }

        /// <summary>
        /// Manages the projectile damage application.
        /// </summary>
        public virtual void Fire()
        {
            ApplyDamage(this.hitThing);
        }

        /// <summary>
        /// Checks for colateral targets (cover, neutral animal, pawn) along the trajectory.
        /// </summary>
        protected void DetermineImpactExactPosition()
        {
            // We split the trajectory into small segments of approximatively 1 cell size.
            Vector3 trajectory = (this.destination - this.origin);
            int numberOfSegments = (int)trajectory.magnitude;
            Vector3 trajectorySegment = (trajectory / trajectory.magnitude);

            Vector3 temporaryDestination = this.origin; // Last valid tested position in case of an out of boundaries shot.
            Vector3 exactTestedPosition = this.origin;
            IntVec3 testedPosition = exactTestedPosition.ToIntVec3();

            for (int segmentIndex = 1; segmentIndex <= numberOfSegments; segmentIndex++)
            {
                exactTestedPosition += trajectorySegment;
                testedPosition = exactTestedPosition.ToIntVec3();

                if (!exactTestedPosition.InBounds(this.map))
                {
                    this.destination = temporaryDestination;
                    break;
                }

                if (!this.def.projectile.flyOverhead && this.FreeIntercept && segmentIndex >= 5)
                {
                    foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(testedPosition))
                    {
                        // Check impact on a wall.
                        if (current.def.Fillage == FillCategory.Full)
                        {
                            this.destination = testedPosition.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                            this.hitThing = current;
                            break;
                        }

                        // Temporarily removed as there are too much missed shots.
                        /*// Check impact on a cover building.
                        if (Rand.Value < current.def.fillPercent)
                        {
                            this.destination = testedPosition.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                            hitThing = current;
                            break;
                        }*/

                        // Check impact on a pawn.
                        if (current.def.category == ThingCategory.Pawn)
                        {
                            Pawn pawn = (Pawn)current;
                            float chanceToHitCollateralTarget;
                            if (pawn.Downed)
                            {
                                chanceToHitCollateralTarget = 0.05f;
                            }
                            else
                            {
                                float targetDistance = (exactTestedPosition - this.origin).magnitude;
                                if (targetDistance < 5f)
                                {
                                    chanceToHitCollateralTarget = 0f;
                                }
                                else
                                {
                                    if (targetDistance < 10f)
                                    {
                                        chanceToHitCollateralTarget = 0.4f;
                                    }
                                    else
                                    {
                                        chanceToHitCollateralTarget = 0.8f;
                                    }
                                }
                            }
                            if (Rand.Value < chanceToHitCollateralTarget)
                            {
                                Pawn pawn2 = Find.VisibleMap.thingGrid.ThingAt(testedPosition, ThingCategory.Pawn) as Pawn;
                                if (pawn2 != null)
                                {
                                    this.destination = testedPosition.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                                    this.hitThing = (Thing)pawn2;
                                    break;
                                }
                            }
                        }
                    }
                }

                temporaryDestination = exactTestedPosition;
            }
        }

        /// <summary>
        /// Applies damage on a collateral pawn or an object.
        /// </summary>
        protected void ApplyDamage(Thing hitThing)
        {
            if (hitThing != null)
            {
                // Impact collateral target.
                this.Impact(hitThing);
            }
            else
            {
                this.ImpactSomething();
            }
        }

        /// <summary>
        /// Computes what should be impacted in the DestinationCell.
        /// </summary>
        protected void ImpactSomething()
        {
            // Impact the initial targeted pawn.
            if (this.assignedTarget != null)
            {
                Pawn pawn = this.assignedTarget as Pawn;
                if (pawn != null && pawn.Downed && (this.origin - this.destination).magnitude > 5f && Rand.Value < 0.2f)
                {
                    this.Impact(null);
                    return;
                }
                this.Impact(this.assignedTarget);
                return;
            }
            else
            {
                // Impact a pawn in the destination cell if present.
                Thing thing = Find.VisibleMap.thingGrid.ThingAt(this.DestinationCell, ThingCategory.Pawn);
                if (thing != null)
                {
                    this.Impact(thing);
                    return;
                }
                // Impact any cover object.
                foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(this.DestinationCell))
                {
                    if (current.def.fillPercent > 0f || current.def.passability != Traversability.Standable)
                    {
                        this.Impact(current);
                        return;
                    }
                }
                this.Impact(null);
                return;
            }
        }

        /// <summary>
        /// Impacts a pawn/object or the ground.
        /// </summary>
        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                int damageAmountBase = this.def.projectile.damageAmountBase;
                //BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef);
                hitThing.TakeDamage(dinfo);
                hitThing.def.soundImpactDefault.PlayOneShot(new TargetInfo(this.DestinationCell, this.map, false));
                MoteMaker.ThrowLightningGlow(hitThing.Position.ToVector3Shifted(), this.map, 1.5f);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Downed && Rand.Value < Projectile_ElectricArc.stunChance)
                {
                    hitThing.TakeDamage(new DamageInfo(DamageDefOf.Stun, 10, -1, this.launcher, null, null));
                }
            }
            else
            {
                MoteMaker.ThrowMicroSparks(this.destination, this.map);
            }
        }
        
        /// <summary>
        /// Draws the laser ray.
        /// </summary>
        public override void Draw()
        {
            this.Comps_PostDraw();
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, drawingMatrix, FadedMaterialPool.FadedVersionOf(drawingTexture, drawingIntensity), 0);
        }
    }
}
