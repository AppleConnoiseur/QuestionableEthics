using Verse;
using RimWorld;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("KongMD.QEE");
            //HarmonyInstance.DEBUG = true;

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(MedicalRecipesUtility))]
        [HarmonyPatch(nameof(MedicalRecipesUtility.SpawnNaturalPartIfClean))]
        static class SpawnNaturalPartIfClean_Patch
        {
            [HarmonyPostfix]
            static void SpawnNaturalPartIfCleanPostfix(ref Thing __result, Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
            {
                bool isOrganTransplant = false;
                bool shouldDropTransplantOrgan = false;
                int badHediffCount = 0;
                foreach (Hediff currHediff in pawn.health.hediffSet.hediffs)
                {
                    if (currHediff.Part == part)
                    {
                        QEEMod.TryLog("Hediff on this body part: " + currHediff.def.defName + " isBad: " + currHediff.def.isBad);
                        if (currHediff.def == QEHediffDefOf.QE_OrganRejection)
                        {
                            isOrganTransplant = true;
                        }
                        else if (currHediff.def.isBad)
                        {
                            badHediffCount++;
                        }
                    }
                }

                //if the only hediff on bodypart is organ rejection, that organ should spawn
                //vanilla game would not spawn it, because part hediffs > 0
                if (isOrganTransplant && badHediffCount == 0 && part.def.spawnThingOnRemoved != null)
                {
                    shouldDropTransplantOrgan = true;
                }

                QEEMod.TryLog("shouldDropTransplantOrgan: " + shouldDropTransplantOrgan + " [isOrganTransplant: " + 
                    isOrganTransplant + " " + part.LabelShort + " bad hediffs: " + badHediffCount + "]");

                //spawn a biological arm when a shoulder is removed with a healthy arm attached
                if (part.LabelShort == "shoulder")
                {
                    foreach (BodyPartRecord childPart in part.parts)
                    {
                        bool isHealthy = MedicalRecipesUtility.IsClean(pawn, childPart);
                        QEEMod.TryLog("body part: " + childPart.LabelShort + " defName: " + childPart.def.defName +
                            " healthy: " + isHealthy);

                        if (childPart.def == BodyPartDefOf.Arm && isHealthy && (shouldDropTransplantOrgan = true || 
                            isOrganTransplant == false))
                        {
                            QEEMod.TryLog("Spawn natural arm from shoulder replacement");
                            __result = GenSpawn.Spawn(QEThingDefOf.QE_Organ_Arm, pos, map);
                        }
                    }
                }
                else if (shouldDropTransplantOrgan)
                {
                    __result = GenSpawn.Spawn(part.def.spawnThingOnRemoved, pos, map);
                }

            }
        }

        //this is a lightweight patch that will simply add any hediffs in the <addsHediff> element of the recipe
        //for surgery involving natural body parts
        [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart))]
        [HarmonyPatch(nameof(Recipe_InstallNaturalBodyPart.ApplyOnPawn))]
        class ApplyOnPawn_Patch
        {
            [HarmonyPostfix]
            static void ApplyOnPawnPostfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
            {
                if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part) && bill.recipe.addsHediff != null)
                {
                    pawn.health.AddHediff(bill.recipe.addsHediff, part);
                }
            }
        }
    }
}
