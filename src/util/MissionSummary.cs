using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class MissionSummary : IEnumerable<MissionSummary.Event>
      {
         private readonly List<Event> summaries = new List<Event>();

         public class Event
         {
            public readonly ProtoCrewMember kerbal;
            public readonly List<Ribbon> newRibbons = new List<Ribbon>();

            public Event(ProtoCrewMember kerbal)
            {
               this.kerbal = kerbal;
            }
         }


         public System.Collections.IEnumerator GetEnumerator()
         {
            return summaries.GetEnumerator();
         }

         IEnumerator<MissionSummary.Event> IEnumerable<MissionSummary.Event>.GetEnumerator()
         {
            return summaries.GetEnumerator();
         }

         public void Clear()
         {
            summaries.Clear();
            Log.Info("mission summary cleared");
         }


         public void AddSummaryForCrewOfVessel(ProtoVessel vessel)
         {
            if (vessel == null) return;
            Log.Info("adding mission summary for vessel " + vessel);
            foreach (ProtoCrewMember kerbal in vessel.GetVesselCrew())
            {
               // DEBUG
               Log.Test("ADD MISSION SUMMRAY ");
               HallOfFameEntry hoe = HallOfFame.Instance().GetEntry(kerbal);
               Log.Test(hoe.ToString());


               Log.Info("adding mission summary for kerbal " + kerbal.name + ", crew=" + kerbal.IsCrew());
               if (kerbal.IsCrew())
               {
                  Event summary = new Event(kerbal);
                  summaries.Add(summary);
                  // only real missions will count
                  foreach (Ribbon ribbon in HallOfFame.Instance().GetRibbonsOfLatestOngoingMission(kerbal))
                  {
                     summary.newRibbons.Add(ribbon);
                  }
               }
            }
         }

         public int Count()
         {
            return summaries.Count();
         }
      }
   }
}
