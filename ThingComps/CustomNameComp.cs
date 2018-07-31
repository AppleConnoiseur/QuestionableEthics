using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace QEthics
{
    public class CustomNameComp : ThingComp
    {
        public string customName = null;

        public override string TransformLabel(string label)
        {
            if(customName != null)
            {
                return "\"" + customName + "\" " + label;
            }

            return base.TransformLabel(label);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "QE_CustomNameCompGizmoLabel".Translate(),
                defaultDesc =  "QE_CustomNameCompGizmoDescription".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true),
                order = 100,
                action = delegate()
                {
                    Dialog_RenameCustomNameComp dialog = new Dialog_RenameCustomNameComp();
                    dialog.optionalTitle = "QE_CustomNameCompDialogTitle".Translate(parent.LabelCapNoCount);
                    dialog.nameComp = this;
                    Find.WindowStack.Add(dialog);
                }
            };
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref customName, "customName");
        }

        public override bool AllowStackWith(Thing other)
        {
            if(other.TryGetComp<CustomNameComp>() is CustomNameComp otherComp && otherComp.customName == customName)
            {
                return true;
            }

            return false;
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CustomNameComp>().customName = customName;
        }
    }
}
