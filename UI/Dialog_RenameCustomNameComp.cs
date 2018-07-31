using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    public class Dialog_RenameCustomNameComp : Dialog_Rename
    {
        public CustomNameComp nameComp;

        protected override void SetName(string name)
        {
            nameComp.customName = name;
        }
    }
}
