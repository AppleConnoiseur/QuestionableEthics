using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace QEthics
{
    public class QEESettings : ModSettings
    {
        public float maintRateFloat = 1.0f;
        public float organGrowthRateFloat = 1.0f;
        public float cloneGrowthRateFloat = 1.0f;
        public float organTotalResourcesFloat = 1.0f;
        public float cloneTotalResourcesFloat = 1.0f;
        public List<Pawn> exampleListOfPawns = new List<Pawn>();
        public static bool debugLogging = false;
        

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref maintRateFloat, "maintRateFloat", 1.0f);
            Scribe_Values.Look(ref organGrowthRateFloat, "organGrowthRateFloat", 1.0f);
            Scribe_Values.Look(ref cloneGrowthRateFloat, "cloneGrowthRateFloat", 1.0f);
            Scribe_Values.Look(ref organTotalResourcesFloat, "organTotalResourcesFloat", 1.0f);
            Scribe_Values.Look(ref cloneTotalResourcesFloat, "cloneTotalResourcesFloat", 1.0f);
            Scribe_Values.Look(ref debugLogging, "debugLogging", false);
            base.ExposeData();
        }
    }

    public class QEEMod : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        QEESettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public QEEMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<QEESettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.SliderLabeled("QE_OrganGrowthDuration".Translate(), ref settings.organGrowthRateFloat, settings.organGrowthRateFloat.ToString("0.00"), 0.00f, 4.0f, "QE_OrganGrowthDurationTooltip".Translate());
            listingStandard.SliderLabeled("QE_OrganIngredientMult".Translate(), ref settings.organTotalResourcesFloat, settings.organTotalResourcesFloat.ToString("0.00"), 0.00f, 4.0f, "QE_OrganIngredientMultTooltip".Translate());
            listingStandard.SliderLabeled("QE_CloneGrowthDuration".Translate(), ref settings.cloneGrowthRateFloat, settings.cloneGrowthRateFloat.ToString("0.00"), 0.00f, 4.0f, "QE_CloneGrowthDurationTooltip".Translate());
            listingStandard.SliderLabeled("QE_CloneIngredientMult".Translate(), ref settings.cloneTotalResourcesFloat, settings.cloneTotalResourcesFloat.ToString("0.00"), 0.00f, 4.0f, "QE_CloneIngredientMultTooltip".Translate());
            listingStandard.SliderLabeled("QE_VatMaintTime".Translate(), ref settings.maintRateFloat, settings.maintRateFloat.ToString("0.00"), 0.01f, 4.0f, "QE_VatMaintTimeTooltip".Translate());
            listingStandard.CheckboxLabeled("QE_DebugLogging".Translate(), ref QEESettings.debugLogging, "QE_DebugLoggingTooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public static void TryLog(string message)
        {
            if (QEESettings.debugLogging)
            {
                Log.Message("QEE: " + message);
            }
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory() => "Questionable Ethics Enhanced";
    }


}

