using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class Translator
      {
         private static readonly String ROOT_PATH = Utils.GetRootPath();
         private static readonly String BASE_FILENAME = ROOT_PATH + "/GameData/Nereid/FinalFrontier/Translations/language.";

         private readonly String language;

         Dictionary<String, String> data = new Dictionary<String, String>();

         public Translator(String language)
         {
            this.language = language;
         }

         public String Get(String id)
         {
            try
            {
               return data[id];
            } catch (KeyNotFoundException)
            {
               Log.Warning("no translation found for key '"+id+"'");
               return "[" + id + "]";
            }
         }

         public void Load()
         {
            Log.Info("loading translator for language '" + language + "'");

            if (Load(language)) return;
            if (Load(language.Substring(0,2))) return;
            Log.Warning("no translation file found; using default transaltion");
            Load("en-us");
         }

         private bool Load(String suffix)
         {

            String filename = BASE_FILENAME + suffix;

            if (File.Exists(filename))
            {
               using (StreamReader reader = new StreamReader(File.OpenRead(filename)))
               {
                  Log.Detail("loading translation from " + filename);
                  String line;
                  int linenr = 0;
                  char[] DELIMITER = { '>' };
                  while ((line = reader.ReadLine()) != null)
                  {
                     linenr++;
                     if(Log.IsLogable(Log.LEVEL.DETAIL))
                     {
                        Log.Detail("translation, line " + linenr + ": '" + line + "'");
                     }
                     String[] fields = line.Split(DELIMITER, 2);
                     if (fields.Length==2)
                     {
                        String key = fields[0].Trim();
                        String value = fields[1].Trim();
                        data[key] = value;
                     }
                     else
                     {
                        Log.Warning("invalid translation entry; line " + linenr+ ": '"+line+"'");
                     }

                  }
               }
               return true;
            }
            else
            {
               Log.Error("translation file '" + filename + "' not found'");
               return false;
            }
         }

         public static Translator CreateTranslator(string language)
         {
            Translator translator = new Translator(language);
            translator.Load();
            return translator;
         }

         public override String ToString()
         {
            StringBuilder sb = new StringBuilder();

            foreach(String key in data.Keys)
            {
               String value = data[key];
               sb.AppendLine(key+" = "+value);
            }

            return sb.ToString();
         }
      }
   }
}
