using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Pesky
{
    public class PsychicRitualToil_SummonThing : PsychicRitualToil
    {
        private PsychicRitualRoleDef roleDef;
        private PsychicRitualDef_SummonThing parentDef;

        public PsychicRitualToil_SummonThing(PsychicRitualRoleDef roleDef, PsychicRitualDef_SummonThing parentDef)
        {
            this.roleDef = roleDef;
            this.parentDef = parentDef;
        }

        public override void Start(PsychicRitual ritual, PsychicRitualGraph graph)
        {
            base.Start(ritual, graph);

            Pawn invoker = ritual.assignments.FirstAssignedPawn(roleDef);
            if (invoker == null || parentDef == null || parentDef.thingToSummon == null || parentDef.countFromQualityCurve == null || !ritual.assignments.Target.IsValid)
            {
                Log.Error($"[SummonThingToil] Toil started with missing data or invalid target. Ritual: {ritual.def?.LabelCap ?? "Unknown"}");
                return;
            }

            Map map = invoker.Map;
            if (map == null)
            {
                Log.Error($"[SummonThingToil] Invoker {invoker.LabelShort} has no map. Ritual: {ritual.def?.LabelCap ?? "Unknown"}");
                return;
            }

            List<QualityFactor> qualityFactorsRuntime = new List<QualityFactor>();
            parentDef.CalculateMaxPower(ritual.assignments, qualityFactorsRuntime, out float calculatedQuality);

            int count = GenMath.RoundRandom(parentDef.countFromQualityCurve.Evaluate(calculatedQuality));

            if (count > 0)
            {
                IntVec3 targetCell = ritual.assignments.Target.Cell;
                IntVec3 finalSpawnCell = IntVec3.Invalid;

                Thing item = ThingMaker.MakeThing(parentDef.thingToSummon);
                item.stackCount = count;

                if (targetCell.Standable(map) && map.reachability.CanReachColony(targetCell))
                {
                    if (GenPlace.TryPlaceThing(item, targetCell, map, ThingPlaceMode.Direct))
                    {
                        finalSpawnCell = targetCell;
                        Messages.Message("PGG_ThingSummoned".Translate(item.LabelCapNoCount, count), item, MessageTypeDefOf.PositiveEvent, historical: false);
                    }
                }

                if (!finalSpawnCell.IsValid)
                {
                    Log.Message($"[SummonThingToil] Could not place directly on target cell {targetCell}. Trying near...");
                    if (DropCellFinder.TryFindDropSpotNear(targetCell, map, out IntVec3 nearCell, false, false, true))
                    {
                        if (GenPlace.TryPlaceThing(item, nearCell, map, ThingPlaceMode.Near))
                        {
                            finalSpawnCell = nearCell;
                            Messages.Message("PGG_ThingSummoned".Translate(item.LabelCapNoCount, count), item, MessageTypeDefOf.PositiveEvent, historical: false);
                        }
                    }
                }

                if (!finalSpawnCell.IsValid)
                {
                    Log.Warning($"[SummonThingToil] Could not find valid drop spot near ritual target {targetCell}. Using random map spot as fallback. Ritual: {ritual.def?.LabelCap ?? "Unknown"}");
                    IntVec3 randomCell = DropCellFinder.RandomDropSpot(map);
                    if (randomCell.IsValid)
                    {
                        if (GenPlace.TryPlaceThing(item, randomCell, map, ThingPlaceMode.Near))
                        {
                            finalSpawnCell = randomCell;
                            Messages.Message("PGG_ThingSummoned".Translate(item.LabelCapNoCount, count), item, MessageTypeDefOf.PositiveEvent, historical: false);
                        }
                    }
                }

                if (!finalSpawnCell.IsValid)
                {
                    Log.Error($"[SummonThingToil] Failed to place {count}x{parentDef.thingToSummon.defName} after all attempts for ritual {ritual.def?.LabelCap ?? "Unknown"}");
                    Messages.Message("PGG_ThingSummoningFailed_Placement".Translate(), invoker, MessageTypeDefOf.NegativeEvent, historical: false);
                }
            }
            else
            {
                 Messages.Message("PGG_ThingSummoningFailed".Translate(parentDef.thingToSummon.label), invoker, MessageTypeDefOf.NegativeEvent, historical: false);
            }
        }

        public PsychicRitualToil_SummonThing() { }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref roleDef, "roleDef");
            Scribe_Defs.Look(ref parentDef, "parentDef");
        }
    }
} 