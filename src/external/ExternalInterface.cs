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
         if(ribbon==null)
         {
            Log.Warning("can't award null ribbon");
            return;
         }
         //
         Ribbon asRibbon = ribbon as Ribbon;
         //
         if(asRibbon==null)
         {
            Log.Error("type mismatch for ribbon: "+ribbon.GetType());
            return;
         }
         // record ribbon
         HallOfFame.Instance().Record(kerbal, asRibbon);
      }

      public void AwardRibbonToKerbals(Ribbon ribbon, ProtoCrewMember[] kerbals)
      {
         HallOfFame halloffame = HallOfFame.Instance();
         halloffame.BeginArwardOfRibbons();
         foreach(ProtoCrewMember kerbal in kerbals)
         {
            halloffame.Record(kerbal, ribbon);
         }
         halloffame.EndArwardOfRibbons();
      }


      public void AwardRibbonToKerbal(String code, ProtoCrewMember kerbal)
      {
         Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
         if(ribbon==null)
         {
            Log.Error("no ribbon for code '"+code+"' found!");
            return;
         }
         AwardRibbonToKerbal(ribbon,kerbal);

      }
   }
}
