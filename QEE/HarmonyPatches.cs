using Verse;
using RimWorld;
using Harmony;
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
            static void Postfix(ref Thing __result, Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
            {
                //this is a hack to spawn a biological arm when a shoulder is removed with a healthy arm attached
                if (part.LabelShort == "shoulder")
                {
                    foreach (BodyPartRecord childPart in part.parts)
                    {
                        bool healthy = MedicalRecipesUtility.IsClean(pawn, childPart);
                        QEEMod.TryLog("body part: " + childPart.LabelShort + " defName: " + childPart.def.defName + 
                            " clean: " + healthy);

                        if (childPart.def.defName == "Arm" && healthy)
                        {
                            QEEMod.TryLog("Spawn natural arm from shoulder replacement");
                            __result = GenSpawn.Spawn(QEThingDefOf.QE_Organ_Arm, pos, map);
                        }
                    }
                }
            }
        }
    }
}
