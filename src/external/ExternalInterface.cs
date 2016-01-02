using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid.FinalFrontier
{
   public class ExternalInterface
   {
      public ExternalInterface()
      {
         // no member, no init
      }

      public int GetMissionCountForKerbal(ProtoCrewMember kerbal)
      {
         HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
         if (entry != null)
         {
            return entry.MissionsFlown;
         }
         return 0;
      }

      public Ribbon RegisterRibbon(String code, String pathToRibbonTexture, String name, String description, bool first, int prestige)
      {
         return RibbonPool.Instance().RegisterExternalRibbon(code, pathToRibbonTexture, name, description, first, prestige);
      }

      public void AwardRibbonToKerbal(object ribbon, ProtoCrewMember kerbal)
      {
         HallOfFame.Instance().Record(kerbal, (Ribbon)ribbon);
      }

      public void AwardRibbonToKerbal(String code, ProtoCrewMember kerbal)
      {
         Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
         if(ribbon==null)
         {
            Log.Error("no ribbon for code '"+code+"' found!");
            return;
         }
         AwardRibbonToKerbal((object)ribbon, kerbal);
      }

      public void AwardRibbonToKerbals(object ribbon, ProtoCrewMember[] kerbals)
      {
         HallOfFame halloffame = HallOfFame.Instance();
         halloffame.BeginArwardOfRibbons();
         foreach (ProtoCrewMember kerbal in kerbals)
         {
            halloffame.Record(kerbal, (Ribbon)ribbon);
         }
         halloffame.EndArwardOfRibbons();
      }

      public void AwardRibbonToKerbals(String code, ProtoCrewMember[] kerbals)
      {
         Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
         if (ribbon == null)
         {
            Log.Error("no ribbon for code '" + code + "' found!");
            return;
         }
         AwardRibbonToKerbals(ribbon, kerbals);
      }

      public bool IsRibbonAwardedToKerbal(object ribbon, ProtoCrewMember kerbal)
      {
         HallOfFame halloffame = HallOfFame.Instance();
         HallOfFameEntry entry = halloffame.GetEntry(kerbal);
         if (entry == null) return false;
         return entry.HasRibbon((Ribbon)ribbon);
      }

      public bool IsRibbonAwardedToKerbal(String code, ProtoCrewMember kerbal)
      {
         Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
         if (ribbon == null) return false;
         return IsRibbonAwardedToKerbal((object)ribbon, kerbal);
      }
   }
}
