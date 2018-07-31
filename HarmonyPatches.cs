using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //Harmony
            HarmonyInstance harmony = HarmonyInstance.Create("chjees.QEthics");

            //Template
            /*{
                Type type = typeof(HarmonyPatches);
                MethodInfo originalMethod = type.GetMethod("MethodName");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Example_Example)));
                harmony.Patch(
                    originalMethod,
                    null,
                    null);
            }*/

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
