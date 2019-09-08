using System.Linq;
using Verse;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class CompatibilityTracker
    {
        private static bool alienRacesActiveInt = false;
        private static string[] incompatibleModArr = { "Questionable Ethics", "Multiplayer" };

        public static bool AlienRacesActive
        {
            get
            {
                return alienRacesActiveInt;
            }
        }

        public static string[] IncompatibleMods
        {
            get
            {
                return incompatibleModArr;
            }
            set
            {
                incompatibleModArr = value;
            }
        }

        static CompatibilityTracker()
        {
            foreach (string s in incompatibleModArr)
            {
                if (ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == s))
                {
                    Log.Error("Questionable Ethics Enhanced is incompatible with " + s);
                }
            }

            //Check for Alien Races Compatiblity
            if (GenTypes.AllTypes.Any(type => type.FullName == "AlienRace.ThingDef_AlienRace"))
            {
                alienRacesActiveInt = true;
                QEEMod.TryLog("Humanoid Alien Races detected");
            }
        }
    }
}
