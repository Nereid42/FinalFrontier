using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class MissionSummary : IEnumerable<MissionSummary.Summary>
      {
         private readonly List<Summary> summaries = new List<Summary>();

         public class Summary
         {
            public readonly ProtoCrewMember kerbal;
            public readonly List<Ribbon> newRibbons = new List<Ribbon>();

            public Summary(ProtoCrewMember kerbal)
            {
               this.kerbal = kerbal;
            }
         }


         public System.Collections.IEnumerator GetEnumerator()
         {
            return summaries.GetEnumerator();
         }

         IEnumerator<MissionSummary.Summary> IEnumerable<MissionSummary.Summary>.GetEnumerator()
         {
            return summaries.GetEnumerator();
         }

         public void Clear()
         {
            summaries.Clear();
            Log.Info("mission summary cleared");
         }


         public void AddVessel(ProtoVessel vessel, double missionEndTime = 0)
         {
            if (vessel == null) return;
            Log.Info("adding mission summary for vessel " + vessel);
            foreach (ProtoCrewMember kerbal in vessel.GetVesselCrew())
            {
               Log.Info("adding mission summary for kerbal " + kerbal.name + ", crew=" + kerbal.IsCrew());
               if (kerbal.IsCrew())
               {
                  Summary summary = new Summary(kerbal);
                  summaries.Add(summary);
                  // only real missions will count
                  //if (vessel.missionTime > 0)
                  {
                     foreach (Ribbon ribbon in HallOfFame.Instance().GetRibbonsOfLatestMission(kerbal, missionEndTime))
                     {
                        summary.newRibbons.Add(ribbon);
                     }
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
