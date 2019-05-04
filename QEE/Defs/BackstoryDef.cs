using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace QEthics
{
    /// <summary>
    /// Specially tailored BackgroundDef for the mod.
    /// </summary>
    public class BackstoryDef : Def
    {
        public string title = "";
        public string titleShort = "";
        public string titleFemale = "";
        public string titleShortFemale = "";
        public BackstorySlot slot = BackstorySlot.Childhood;

        public string bodyTypeFemale = null; //Female
        public string bodyTypeMale = null; //Male
        public string bodyTypeGlobal = null;

        public string baseDesc = null;
        public List<string> spawnCategories = new List<string>();

        [Unsaved]
        public string identifier = null;

        public Backstory GetFromDatabase()
        {
            Backstory bs = null;
            BackstoryDatabase.TryGetWithIdentifier(identifier, out bs, false);

            return bs;
        }
    }
}
