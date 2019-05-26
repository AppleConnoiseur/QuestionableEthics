using System.Linq;
using Verse;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class CompatibilityTracker
    {
        private static bool alienRacesActiveInt = false;

        public static bool AlienRacesActive
        {
            get
            {
                return alienRacesActiveInt;
            }
        }

        static CompatibilityTracker()
        {
            //Check for Alien Races Compatiblity.
            Log.Message("Questionable Ethics checking for compatibility...");
            Log.Message("Checking for compatibility for: Alien Race Framework...");
            if(GenTypes.AllTypes.Any(type => type.FullName == "AlienRace.ThingDef_AlienRace"))
            {
                alienRacesActiveInt = true;
                Log.Message("Is OK.");
            }
            else
            {
                Log.Message("No compatibility.");
            }
        }
    }
}
