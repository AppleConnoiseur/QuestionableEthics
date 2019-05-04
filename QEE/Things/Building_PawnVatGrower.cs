﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace QEthics
{
    public class Building_PawnVatGrower : Building_GrowerBase, IMaintainableGrower
    {
        public PawnKindDef pawnKindToGrow;

        public Pawn pawnBeingGrown;

        static Building_PawnVatGrower()
        {
            cleanlinessCurve.Add(-5.0f, 5.00f);
            cleanlinessCurve.Add(-2.0f, 1.75f);
            cleanlinessCurve.Add(0.0f, 1.0f);
            cleanlinessCurve.Add(0.4f, 0.35f);
            cleanlinessCurve.Add(2.0f, 0.1f);
        }

        public Building_PawnVatGrower() : base()
        {
            
        }

        public static SimpleCurve cleanlinessCurve = new SimpleCurve();

        /// <summary>
        /// From 0.0 to 1.0. If the maintence is below 50% there is a chance for failure.
        /// </summary>
        public float scientistMaintence;

        /// <summary>
        /// From 0.0 to 1.0. If the maintence is below 50% there is a chance for failure.
        /// </summary>
        public float doctorMaintence;

        public override int TicksNeededToCraft => (int)(pawnKindToGrow.RaceProps.lifeStageAges.Last().minAge * pawnKindToGrow.RaceProps.baseBodySize * (float)GenDate.TicksPerDay);

        public float ScientistMaintence { get => scientistMaintence; set => scientistMaintence = value; }

        public float DoctorMaintence { get => doctorMaintence; set => doctorMaintence = value; }

        private RenderTexture renderTexture;
        private Material renderMaterial;
        private LifeStageAge lastLifestageAge;

        private VatGrowerProperties vatGrowerPropsInt;

        public VatGrowerProperties VatGrowerProps
        {
            get
            {
                if (vatGrowerPropsInt == null)
                {
                    vatGrowerPropsInt = def.GetModExtension<VatGrowerProperties>();

                    //Fallback; Is defaults.
                    if (vatGrowerPropsInt == null)
                    {
                        vatGrowerPropsInt = new VatGrowerProperties();
                    }
                }

                return vatGrowerPropsInt;
            }
        }

        public float RoomCleanliness
        {
            get
            {
                Room room = this.GetRoom(RegionType.Set_Passable);
                if (room != null)
                {
                    return room.GetStat(RoomStatDefOf.Cleanliness);
                }

                return 0f;
            }
        }

        public override Thing ExtractProduct(Pawn pawn)
        {
            if (status == CrafterStatus.Finished)
            {
                CraftingFinished();
                if (innerContainer.Count > 0)
                {
                    innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
                }
            }

            renderTexture = null;
            renderMaterial = null;
            lastLifestageAge = null;

            Pawn tempPawn = pawnBeingGrown;
            pawnBeingGrown = null;

            if(tempPawn.RaceProps.Humanlike)
            {
                Find.LetterStack.ReceiveLetter("QE_LetterHumanlikeGrownLabel".Translate(), "QE_LetterHumanlikeGrownDescription".Translate(tempPawn.Name.ToStringShort), LetterDefOf.PositiveEvent, new LookTargets(tempPawn));
                TaleRecorder.RecordTale(QETaleDefOf.QE_Vatgrown, tempPawn);
                tempPawn.needs.mood.thoughts.memories.TryGainMemory(QEThoughtDefOf.QE_VatGrownCloneConfusion);
            }
            return tempPawn;
        }

        public override void Notify_CraftingStarted()
        {
            //Remove all except for the GenomeSequence.
            orderProcessor.Reset();

            innerContainer.RemoveAll(thing => !(thing is GenomeSequence));

            //Create and setup pawn.
            GenomeSequence genome = innerContainer.First(thing => thing is GenomeSequence) as GenomeSequence;

            Pawn pawn = GenomeUtility.MakePawnFromGenomeSequence(genome, this);

            //Finalized.
            pawnBeingGrown = pawn;
        }

        public override void Notify_CraftingFinished()
        {
            //Give hair.
            if (pawnBeingGrown != null && pawnBeingGrown.RaceProps.Humanlike && pawnBeingGrown.story != null)
            {
                HairDef hairDef = PawnHairChooser.RandomHairDefFor(pawnBeingGrown, Faction.def);
                pawnBeingGrown.story.hairDef = hairDef;
                pawnBeingGrown.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawnBeingGrown);
                PortraitsCache.PortraitsCacheUpdate();
                Messages.Message("QE_MessageGrowingDone".Translate(pawnBeingGrown.LabelCap), new LookTargets(this), MessageTypeDefOf.PositiveEvent);
            }
        }

        public override void Tick_Crafting()
        {
            base.Tick_Crafting();

            //Increment Pawn age
            if(pawnBeingGrown != null)
            {
                if (pawnBeingGrown.Rotation != Rotation.Opposite)
                {
                    pawnBeingGrown.Rotation = Rotation.Opposite;
                }

                float minAge = pawnKindToGrow.RaceProps.lifeStageAges.Last().minAge;
                long calculatedAgeTicks = (long)(minAge * CraftingProgressPercent * (float)GenDate.TicksPerYear);
                pawnBeingGrown.ageTracker.AgeBiologicalTicks = calculatedAgeTicks;
                pawnBeingGrown.ageTracker.AgeChronologicalTicks = calculatedAgeTicks;
                if(lastLifestageAge != pawnBeingGrown.ageTracker.CurLifeStageRace)
                {
                    PortraitsCache.SetDirty(pawnBeingGrown);
                    PortraitsCache.PortraitsCacheUpdate();
                    lastLifestageAge = pawnBeingGrown.ageTracker.CurLifeStageRace;
                    renderTexture = null;

                    //Log.Message("Tick_Crafting: New lifestage texture.");
                }
            }

            //Deduct maintence, fail if any of them go below 0%.
            float powerModifier = 1f;
            if (PowerTrader != null && !PowerTrader.PowerOn)
            {
                powerModifier = 10f;
            }
            float cleanlinessModifer = cleanlinessCurve.Evaluate(RoomCleanliness);
            float decayRate = 0.00003f * cleanlinessModifer * powerModifier;

            scientistMaintence -= decayRate;
            doctorMaintence -= decayRate;

            if (scientistMaintence < 0f || doctorMaintence < 0f)
            {
                //Fail the craft and spawn a abomination.
                Reset();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref pawnKindToGrow, "pawnKindToGrow");
            Scribe_Values.Look(ref scientistMaintence, "scientistMaintence");
            Scribe_Values.Look(ref doctorMaintence, "doctorMaintence");
            Scribe_Deep.Look(ref pawnBeingGrown, "pawnBeingGrown");
        }

        public override string TransformStatusLabel(string label)
        {
            string pawnLabel = pawnKindToGrow?.race.LabelCap ?? "no living being";

            if (status == CrafterStatus.Filling || status == CrafterStatus.Finished)
            {
                return label + " " + pawnLabel.CapitalizeFirst();
            }
            if (status == CrafterStatus.Crafting)
            {
                return label + " " + pawnLabel.CapitalizeFirst() + " (" + CraftingProgressPercent.ToStringPercent() + ")";
            }

            return base.TransformStatusLabel(label);
        }

        public override string GetInspectString()
        {
            if (!(ParentHolder is Map))
            {
                return null;
            }

            StringBuilder builder = new StringBuilder(base.GetInspectString());

            if (status == CrafterStatus.Crafting)
            {
                builder.AppendLine();
                builder.AppendLine("QE_VatGrowerScientistMaintence".Translate() + ": " + scientistMaintence.ToStringPercent());
                builder.AppendLine("QE_VatGrowerDoctorMaintence".Translate() + ": " + doctorMaintence.ToStringPercent());
            }

            return builder.ToString().TrimEndNewlines();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            //Draw bottom graphic
            Vector3 drawAltitude = drawLoc;
            if (VatGrowerProps.bottomGraphic != null)
            {
                VatGrowerProps.bottomGraphic.Graphic.Draw(drawAltitude, Rotation, this);
            }

            //Draw product
            drawAltitude += new Vector3(0f, 0.005f, 0f);
            if ((status == CrafterStatus.Crafting || status == CrafterStatus.Finished) && pawnBeingGrown != null)
            {
                if(pawnBeingGrown.RaceProps.Humanlike)
                {
                    if (renderTexture == null)
                    {
                        int size = 256;
                        RenderTexture renderTextureTemp = new RenderTexture(size, size, 24);
                        Find.PortraitRenderer.RenderPortrait(pawnBeingGrown, renderTextureTemp, default(Vector3), 1f);

                        renderTexture = renderTextureTemp;
                        Texture2D tempTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                        RenderTexture.active = renderTexture;
                        tempTexture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                        tempTexture.Apply();
                        RenderTexture.active = null;

                        MaterialRequest req2 = new MaterialRequest(tempTexture);
                        req2.shader = ShaderDatabase.Mote;
                        renderMaterial = MaterialPool.MatFrom(req2);

                        //Log.Message("DrawAt: New render texture");
                    }

                    float scale = (0.2f + (CraftingProgressPercent * 0.8f)) * 1.75f;

                    Vector3 scaleVector = new Vector3(scale, 1f, scale);
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(drawAltitude + VatGrowerProps.productOffset, Quaternion.AngleAxis(0f, Vector3.up), scaleVector);
                    
                    Graphics.DrawMesh(MeshPool.plane10, matrix, renderMaterial, 0);
                    //pawnBeingGrown.DrawAt(drawAltitude);
                }
                else
                {
                    pawnBeingGrown.DrawAt(drawAltitude);
                }
            }

            //Draw top graphic
            if (VatGrowerProps.topGraphic != null)
            {
                drawAltitude += new Vector3(0f, 0.020f, 0f);
                VatGrowerProps.topGraphic.Graphic.Draw(drawAltitude, Rotation, this);
            }

            //Draw top detail graphic
            if (VatGrowerProps.topDetailGraphic != null && (PowerTrader?.PowerOn ?? false))
            {
                drawAltitude += new Vector3(0f, 0.005f, 0f);
                VatGrowerProps.topDetailGraphic.Graphic.Draw(drawAltitude, Rotation, this);
            }

            //Draw glow graphic
            if ((status == CrafterStatus.Crafting || status == CrafterStatus.Finished) && VatGrowerProps.glowGraphic != null && (PowerTrader?.PowerOn ?? false))
            {
                drawAltitude += new Vector3(0f, 0.005f, 0f);
                VatGrowerProps.glowGraphic.Graphic.Draw(drawAltitude, Rotation, this);
            }
        }

        public void StopCrafting()
        {
            craftingProgress = 0;
            orderProcessor.Reset();
            renderTexture = null;
            renderMaterial = null;
            lastLifestageAge = null;

            pawnKindToGrow = null;
            if(pawnBeingGrown != null && !pawnBeingGrown.Destroyed && status == CrafterStatus.Crafting)
            {
                pawnBeingGrown.Destroy();
                pawnBeingGrown = null;
            }

            status = CrafterStatus.Idle;
            if (innerContainer.Count > 0)
            {
                innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            }
        }

        public void StartCrafting(GenomeSequence genome)
        {
            //Setup recipe order
            orderProcessor.Reset();
            orderProcessor.desiredIngredients.Add(new ThingOrderRequest(genome));
            IngredientUtility.FillOrderProcessorFromPawnKindDef(orderProcessor, genome.pawnKindDef);
            orderProcessor.Notify_ContentsChanged();
            craftingProgress = 0;

            //Reset stuff
            pawnBeingGrown = null;

            //Initialize maintence
            scientistMaintence = 0.25f;
            doctorMaintence = 0.25f;

            pawnKindToGrow = genome.pawnKindDef;
            status = CrafterStatus.Filling;
        }

        public override void Notify_StartedCarryThing(Pawn pawn)
        {
            if(pawn.carryTracker.CarriedThing is GenomeSequence genome)
            {
                ThingOrderRequest genomeRequest = orderProcessor.desiredIngredients.FirstOrDefault(req => req.HasThing && req.thing is GenomeSequence);
                if(genomeRequest != null)
                {
                    //Update Thing we got a request for.
                    genomeRequest.thing = genome;
                    //orderProcessor.Notify_ContentsChanged();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (status == CrafterStatus.Idle)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "QE_VatGrowerStartCraftingGizmoLabel".Translate(),
                    defaultDesc = "QE_VatGrowerStartCraftingGizmoDescription".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Item/Health/HealthItemNatural", true),
                    order = -100,
                    action = delegate ()
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();

                        List<Building_PawnVatGrower> otherVats = new List<Building_PawnVatGrower>();
                        IEnumerable<Building_PawnVatGrower> otherVatGrowers = Map.listerBuildings.allBuildingsColonist.Where(building => building is Building_PawnVatGrower)?.Select(bldng => bldng as Building_PawnVatGrower);
                        if(otherVatGrowers != null)
                        {
                            otherVats.AddRange(otherVatGrowers);
                        }

                        IEnumerable<Thing> validGenomes = Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)?.
                        Where(thing => !thing.Position.Fogged(Map) && thing is GenomeSequence && !thing.IsForbidden(Faction.OfPlayer) &&
                        (thing.stackCount - otherVats.Count(vat => vat.status == CrafterStatus.Filling && vat.orderProcessor.desiredIngredients.FirstOrDefault(req => req.HasThing && req.thing == thing) != null) > 0));
                        if(validGenomes != null)
                        {
                            foreach (Thing genomeThing in validGenomes)
                            {
                                GenomeSequence genome = genomeThing as GenomeSequence;
                                string label = genome.pawnKindDef.race.LabelCap + " <- " + genome.sourceName;

                                FloatMenuOption option = new FloatMenuOption(label, delegate ()
                                {
                                    StartCrafting(genome);
                                });

                                options.Add(option);
                            }
                        }

                        if (options.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                        else
                        {
                            //Give a hint.
                            FloatMenuOption option = new FloatMenuOption("QE_VatGrowerGenomesHint".Translate(), null);
                            option.Disabled = true;
                            options.Add(option);

                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    }
                };
            }
            else
            {
                if (status != CrafterStatus.Finished)
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "QE_VatGrowerStopCraftingGizmoLabel".Translate(),
                        defaultDesc = "QE_VatGrowerStopCraftingGizmoDescription".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                        order = -100,
                        action = delegate ()
                        {
                            StopCrafting();
                        }
                    };

                    if(Prefs.DevMode)
                    {
                        yield return new Command_Action()
                        {
                            defaultLabel = "DEBUG: Finish growing",
                            defaultDesc = "Instantly finishes the growing of the pawn.",
                            //icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                            action = delegate ()
                            {
                                craftingProgress = TicksNeededToCraft;
                            }
                        };
                    }
                }
            }
        }
    }
}
