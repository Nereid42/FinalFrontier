using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Activity 
      {
         private static ActivityPool ACTIVITY_POOL = ActivityPool.Instance();

         private readonly String code;
         protected readonly String translationId;
         private String name;

         public Activity(String code, String translationId)
         {
            this.code = code;
            this.translationId = translationId;
            this.name = FinalFrontier.translator.Get("RN_"+ translationId);
            ACTIVITY_POOL.RegisterActivity(this);
         }

         // name of the activity
         public virtual String GetName() 
         {
            return name;
         }

         // unique code of the activity
         public String GetCode()
         {
            return code;
         }

         public override bool Equals(System.Object right)
         {
            if (right == null) return false;
            Activity cmp = right as Activity;
            if (cmp == null) return false;
            return GetCode().Equals(cmp.code);
         }

         public override int GetHashCode()
         {
            return code.GetHashCode();
         }

         public abstract String CreateLogBookEntry(LogbookEntry entry);

         public override string ToString()
         {
            return code;
         }

         protected void Rename(String name)
         {
            this.name = name;
         }
      }
   }
}
