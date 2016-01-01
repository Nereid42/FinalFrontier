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

      public void AwardRibbonToKerbal(Ribbon ribbon, ProtoCrewMember kerbal)
      {

      }

      public void AwardRibbonToKerbals(Ribbon ribbon, ProtoCrewMember[] kerbals)
      {
         foreach(ProtoCrewMember kerbal in kerbals)
         {
            AwardRibbonToKerbal(ribbon, kerbal);
         }
      }


      public void AwardRibbonToKerbal(String code, ProtoCrewMember kerbal)
      {
         Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
         if(ribbon==null)
         {
            Log.Error("no ribbon for code '"+code+"' found!");
            return;
         }

         HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
         if (entry != null)
         {
            entry.Award(ribbon);
         }
      }
   }
}
