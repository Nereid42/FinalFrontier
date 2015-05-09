using System;
using UnityEngine;

namespace Nereid
{
   namespace FinalFrontier
   {
      public static class Log
      {
         public enum LEVEL { OFF=0, ERROR=1, WARNING=2, INFO=3, DETAIL=4, TRACE=5 };

         public  static LEVEL level = LEVEL.INFO ;

         private static readonly String PREFIX = "FF: ";

         public static LEVEL GetLevel()
         {
            return level;
         }

         public static void SetLevel(LEVEL level)
         {
            Debug.Log("log level "+level);
            Log.level = level;
         }

         public static LEVEL GetLogLevel()
         {
            return level;
         }

         private static bool IsLevel(LEVEL level)
         {
            return level == Log.level;
         }

         public static bool IsLogable(LEVEL level)
         {
            return level <= Log.level;
         }

         public static void Trace(String msg)
         {
            if (IsLogable(LEVEL.TRACE))
            {
               Debug.Log(PREFIX + msg);
            }
         }

         public static void Detail(String msg)
         {
            if (IsLogable(LEVEL.DETAIL))
            {
               Debug.Log(PREFIX + msg);
            }
         }


         public static void Info(String msg)
         {
            if (IsLogable(LEVEL.INFO))
            {
               Debug.Log(PREFIX + msg);
            }
         }

         // for Debbung only; calls should be removed for release
         public static void Test(String msg)
         {
            //if (IsLogable(LEVEL.INFO))
            {
               Debug.LogWarning(PREFIX+"TEST:" + msg);
            }
         }

         public static void Warning(String msg)
         {
            if (IsLogable(LEVEL.WARNING))
            {
               Debug.LogWarning(PREFIX + msg);
            }
         }

         public static void Error(String msg)
         {
            if (IsLogable(LEVEL.ERROR))
            {
               Debug.LogError(PREFIX + msg);
            }
         }

      }
   }
}
