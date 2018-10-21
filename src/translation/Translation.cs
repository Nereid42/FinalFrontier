using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      // just a wrapper class currently 
      public class Translation
      {
         public readonly Translator translator;
         public readonly String TEXT_RIBBON; 

         public Translation(String language)
         {
            this.translator = Translator.CreateTranslator(language);
            this.TEXT_RIBBON = translator.Get("TEXT_RIBBON");
         }

         public String Get(String id)
         {
            return translator.Get(id);
         }
      }
   }
}
