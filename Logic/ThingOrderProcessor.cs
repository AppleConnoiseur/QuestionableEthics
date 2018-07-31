using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace QEthics
{
    /// <summary>
    /// Helps process orders for Things in recipes.
    /// </summary>
    public class ThingOrderProcessor : IExposable
    {
        /// <summary>
        /// Things we desire that can be anything.
        /// </summary>
        public List<ThingOrderRequest> desiredIngredients = new List<ThingOrderRequest>();

        /// <summary>
        /// Contains the list of cached requests.
        /// </summary>
        private List<ThingOrderRequest> cachedRequests = new List<ThingOrderRequest>();

        /// <summary>
        /// The holder object we are observing orders for.
        /// </summary>
        public IThingHolder observedThingHolder;

        public ThingOwner ObservedThingOwner
        {
            get
            {
                return observedThingHolder.GetDirectlyHeldThings();
            }
        }

        public IEnumerable<ThingOrderRequest> PendingRequests
        {
            get
            {
                return cachedRequests.AsReadOnly();
            }
        }

        //Constructors.
        public ThingOrderProcessor()
        {
            this.observedThingHolder = null;
        }

        public ThingOrderProcessor(IThingHolder observedThingHolder)
        {
            this.observedThingHolder = observedThingHolder;
        }

        //Functions
        public void Cleanup()
        {
            desiredIngredients.RemoveAll(orderRequest => orderRequest == null || (orderRequest.HasThing && !orderRequest.thing.Spawned));
        }

        public void Reset()
        {
            cachedRequests.Clear();
            desiredIngredients.Clear();
        }

        /// <summary>
        /// Call this whenever contents change or upon loading the game.
        /// </summary>
        public void Notify_ContentsChanged()
        {
            Cleanup();

            //Recache our requests
            cachedRequests.Clear();

            IEnumerable<ThingOrderRequest> desiredRequests = GetDesiredRequests();
            if(desiredRequests != null)
            {
                cachedRequests.AddRange(desiredRequests);
            }
        }

        /// <summary>
        /// Returns a series of ThingOrderRequests.
        /// </summary>
        /// <returns>Thing order requests.</returns>
        public IEnumerable<ThingOrderRequest> GetDesiredRequests()
        {
            if (observedThingHolder != null)
            {
                //Desired other things.
                foreach (ThingOrderRequest desiredIngredient in desiredIngredients)
                {
                    int countDifference = (int)desiredIngredient.amount - desiredIngredient.thingFilter.TotalStackCountForFilterInContainer(ObservedThingOwner);
                    //Log.Message(desiredIngredient.Summary + "; countDifference: " + countDifference);
                    if (countDifference > 0)
                    {
                        yield return new ThingOrderRequest(desiredIngredient, countDifference);
                    }
                }
            }
        }

        //Inherited
        public void ExposeData()
        {
            Scribe_Collections.Look(ref desiredIngredients, "desiredThings", LookMode.Deep);
        }
    }
}
