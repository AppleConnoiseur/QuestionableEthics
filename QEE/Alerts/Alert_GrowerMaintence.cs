using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    public class Alert_GrowerMaintenance : Alert_Critical
    {
        public Alert_GrowerMaintenance()
        {
            defaultLabel = "QE_AlertMaintenanceRequiredLabel".Translate();
            defaultExplanation = "QE_AlertMaintenanceRequiredExplanation".Translate();
        }

        public IEnumerable<Building> GrowersNeedingMaintenance()
        {
            return Find.CurrentMap.listerBuildings.allBuildingsColonist.Where(building => building is Building_GrowerBase grower && grower.status == CrafterStatus.Crafting && building is IMaintainableGrower maintainable && (maintainable.DoctorMaintenance < 0.1f || maintainable.ScientistMaintenance < 0.1f));
        }

        public override AlertReport GetReport()
        {
            IEnumerable<Building> growersNeedingMaintenance = GrowersNeedingMaintenance();
            if(growersNeedingMaintenance != null)
            {
                List<GlobalTargetInfo> culprits = new List<GlobalTargetInfo>();
                foreach(Building grower in growersNeedingMaintenance)
                {
                    culprits.Add(grower);
                    AlertReport report = new AlertReport();
                    report.culprits = culprits;
                    report.active = true;
                    return report;
                }
            }

            return false;
        }
    }
}
