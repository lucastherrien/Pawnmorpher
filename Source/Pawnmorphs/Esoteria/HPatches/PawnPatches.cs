﻿// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

#pragma warning disable 01591
#if true
namespace Pawnmorph.HPatches
{
    public static class PawnPatches
    {
        [HarmonyPatch(typeof(Pawn)),HarmonyPatch(nameof(Pawn.IsColonistPlayerControlled), MethodType.Getter)]
        internal static class IsColonistPlayerControlledPatch
        {
            [HarmonyPostfix]
            static void MakeSapientAnimalsColonists(Pawn __instance, ref bool __result)
            {
                if (__instance.Faction?.IsPlayer != true) return;
                if (!__instance.RaceProps.Animal) return;
                if (__instance.MentalStateDef != null) return;
                if (!__instance.Spawned) return;
                if (__instance.HostFaction != null) return; 
                
                __result = __instance.IsSapientOrFeralFormerHuman(); 
            }
        }
#if true
        [HarmonyPatch(typeof(Pawn)), HarmonyPatch(nameof(Pawn.IsColonist), MethodType.Getter)]
        internal static class MakeFormerHumansColonists
        {
            [HarmonyPostfix]
            static void MakeSapientAnimalsColonists([NotNull] Pawn __instance, ref bool __result)
            {
                if (!__result && __instance.Faction?.IsPlayer == true)
                {
                    __result = __instance.IsColonistAnimal();
                }
            }
        }

#endif 

        [HarmonyPatch(typeof(Pawn_NeedsTracker)), HarmonyPatch("ShouldHaveNeed")]
        internal static class NeedsTracker_ShouldHaveNeedPatch
        {
            [HarmonyPostfix]
            static void GiveSapientAnimalsNeeds(Pawn_NeedsTracker __instance, Pawn ___pawn, NeedDef nd, ref bool __result)
            {
                if (__result)
                {
                    __result = nd.IsValidFor(___pawn);
                    return;
                }
                if (___pawn?.IsSapientOrFeralFormerHuman() != true) return;
                if (nd?.defName == "SapientAnimalControl")
                {
                    __result = true;
                    return; 
                }
                
                var isColonist = ___pawn.Faction?.IsPlayer == true;
                if (nd.defName == "Mood")
                {
                    __result = true; 
                }else if (nd.defName == "Joy" && isColonist)
                    __result = true; 

            }
        }

        [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
        static class PawnRenderAnglePatch
        {
            static bool Prefix(ref float __result, [NotNull] Pawn ___pawn)
            {
                if (___pawn.IsSapientFormerHuman() && ___pawn.GetPosture() == PawnPosture.LayingInBed)
                {
                    Building_Bed buildingBed = ___pawn.CurrentBed();
                    Rot4 rotation = buildingBed.Rotation;
                    rotation.AsInt += Rand.ValueSeeded(___pawn.thingIDNumber) > 0.5 ?  1 : 3;
                    __result = rotation.AsAngle;
                    return false; 
                }

                return true; 
            }
        }
    }
}
#endif