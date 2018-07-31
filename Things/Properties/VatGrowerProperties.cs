using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace QEthics
{
    /// <summary>
    /// Properties specific to the VatGrower.
    /// </summary>
    public class VatGrowerProperties : GrowerProperties
    {
        /// <summary>
        /// Graphic for the "lid".
        /// </summary>
        public GraphicData topGraphic;

        /// <summary>
        /// Graphic for the base.
        /// </summary>
        public GraphicData bottomGraphic;

        /// <summary>
        /// Graphic for the glow.
        /// </summary>
        public GraphicData glowGraphic;

        /// <summary>
        /// Graphic for the top detail.
        /// </summary>
        public GraphicData topDetailGraphic;

        /// <summary>
        /// Offset for where the product is rendered.
        /// </summary>
        public Vector3 productOffset = new Vector3();

        /// <summary>
        /// Scale for the product.
        /// </summary>
        public float productScaleModifier = 1f;

        public override IEnumerable<string> ConfigErrors()
        {
            ResolveAll();
            return base.ConfigErrors();
        }

        public void ResolveAll()
        {
            if(topGraphic != null)
            {
                topGraphic.ResolveReferencesSpecial();
            }

            if (bottomGraphic != null)
            {
                bottomGraphic.ResolveReferencesSpecial();
            }

            if (glowGraphic != null)
            {
                glowGraphic.ResolveReferencesSpecial();
            }
        }
    }
}
