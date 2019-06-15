using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace QEthics
{
    /// <summary>
    /// Utility functions for dealing with ingredients.
    /// </summary>
    public static class IngredientUtility
    {
        public static Thing FindClosestRequestForThingOrderProcessor(ThingOrderProcessor orderProcessor, Pawn finder)
        {
            Thing result = GenClosest.ClosestThingReachable(finder.Position, finder.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(finder),
                    validator: 
                    delegate(Thing thing)
                    {
                        if(thing.IsForbidden(finder))
                        {
                            return false;
                        }

                        foreach (ThingOrderRequest request in orderProcessor.PendingRequests)
                        {
                            if(request.ThingMatches(thing))
                            {
                                return true;
                            }
                        }

                        return false;
                    });

            return result;
        }

        public static string FormatIngredientsInThingOrderProcessor(this ThingOrderProcessor orderProcessor, string format = "{0} x {1}", char delimiter = ',')
        {
            StringBuilder builder = new StringBuilder();

            //Ingredients Wanted
            foreach (ThingOrderRequest ingredient in orderProcessor.desiredIngredients)
            {
                builder.Append(string.Format(format, ingredient.LabelCap, ingredient.amount));
                if (orderProcessor.desiredIngredients.Count > 1 && ingredient != orderProcessor.desiredIngredients.Last())
                {
                    builder.Append(delimiter);
                    builder.Append(' ');
                }
            }

            return builder.ToString().TrimEndNewlines();
        }

        public static string FormatCachedIngredientsInThingOrderProcessor(this ThingOrderProcessor orderProcessor, string format = "{0} x {1}", char delimiter = ',')
        {
            StringBuilder builder = new StringBuilder();

            //Ingredients Requested
            foreach (ThingOrderRequest request in orderProcessor.PendingRequests)
            {
                builder.Append(string.Format(format, request.LabelCap, request.amount));
                if (orderProcessor.PendingRequests.Count() > 1 && request != orderProcessor.PendingRequests.Last())
                {
                    builder.Append(delimiter);
                    builder.Append(' ');
                }
            }

            return builder.ToString().TrimEndNewlines();
        }

        public static string FormatIngredientsInThingOwner(this ThingOwner thingOwner, string format = "{0} x {1}", char delimiter = ',')
        {
            StringBuilder builder = new StringBuilder();

            //Ingredients Wanted
            foreach (Thing ingredient in thingOwner)
            {
                builder.Append(string.Format(format, ingredient.LabelCapNoCount, ingredient.stackCount));
                if (thingOwner.Count > 1 && ingredient != thingOwner.Last())
                {
                    builder.Append(delimiter);
                    builder.Append(' ');
                }
            }

            return builder.ToString().TrimEndNewlines();
        }

        public static void FillOrderProcessorFromVatGrowerRecipe(ThingOrderProcessor orderProcessor, GrowerRecipeDef recipeDef)
        {
            foreach (IngredientCount ingredientCount in recipeDef.ingredients)
            {
                ThingFilter filterCopy = new ThingFilter();
                filterCopy.CopyAllowancesFrom(ingredientCount.filter);

                ThingOrderRequest copy = new ThingOrderRequest(filterCopy);
                copy.amount = (int)(ingredientCount.GetBaseCount() * QEESettings.instance.organTotalResourcesFloat);

                orderProcessor.desiredIngredients.Add(copy);
            }
        }

        public static void FillOrderProcessorFromPawnKindDef(ThingOrderProcessor orderProcessor, PawnKindDef pawnKind)
        {
            ThingDef pawnThingDef = pawnKind.race;
            RaceProperties raceProps = pawnThingDef.race;

            //Assemble all required materials for a fully grown adult.
            ThingDef meatDef = raceProps.meatDef;
            float meatBaseNutrition = meatDef.ingestible.CachedNutrition;
            float meatAmount = pawnThingDef.GetStatValueAbstract(StatDefOf.MeatAmount);
            int nutritionAmount, proteinAmount;

            //protein cost = Meat * base Meat nutrition (vanilla is .05) * magic multiplier of 27
            //Minimum protein cost is 25, max is 750. Multiply by mod setting for clone ingredients
            //finally, round up to the nearest number divisible by 5
            proteinAmount = (int)(Math.Ceiling((meatAmount * meatBaseNutrition * 27f).Clamp(25, 750)
                * QEESettings.instance.cloneTotalResourcesFloat / 5) * 5);

            nutritionAmount = 4 * proteinAmount;

            {
                ThingOrderRequest orderRequest = new ThingOrderRequest(QEThingDefOf.QE_ProteinMash, proteinAmount);
                orderProcessor.desiredIngredients.Add(orderRequest);
            }

            {
                ThingOrderRequest orderRequest = new ThingOrderRequest(QEThingDefOf.QE_NutrientSolution, nutritionAmount);
                orderProcessor.desiredIngredients.Add(orderRequest);
            }
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}
