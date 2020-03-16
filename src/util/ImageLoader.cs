using System;
using UnityEngine;
using KSP.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      static class ImageLoader
      {

         public static Texture2D GetTexture(String pathInGameData)
         {
            Log.Detail("get texture " + pathInGameData + "(texture quality is "+ GameSettings.TEXTURE_QUALITY + ")");
            // load texture directly if texture settings are to low
            if (FinalFrontier.configuration.alwaysUseDirectTextureLoad || GameSettings.TEXTURE_QUALITY != 0)
            {
               Log.Detail("loading texture directly from " + pathInGameData);
               return LoadImageFromFile(pathInGameData);
            }
            // texture is at fullres and we want to reuse the ingame textures (slightly lower quality)
            Texture2D texture = GameDatabase.Instance.GetTexture(pathInGameData, false);
            if (texture != null)
            {
               return texture;
            }
            else
            {
               Log.Error("texture '" + pathInGameData + "' not found");
               return null;
            }
         }


         public static Texture2D LoadImageFromFile(String pathInGameData)
         {
            String fullPath = Constants.GAMEDATA_PATH + System.IO.Path.DirectorySeparatorChar + pathInGameData +".png";

            Log.Detail("loading texture from file " + fullPath);

            Texture2D tex = new Texture2D(1, 1);

            try
            {
               if (System.IO.File.Exists(fullPath))
               {
                  try
                  {
                     tex.LoadImage(System.IO.File.ReadAllBytes(fullPath));
                  }
                  catch (Exception ex)
                  {
                     Log.Error(ex.Message);
                  }
               }
               else
               {
                  Log.Error("cannot find image file '"+ fullPath+"'");
               }
            }
            catch (Exception ex)
            {
               Log.Error(ex.Message);
            }
            Log.Trace("size of texture is " + tex.width + "x" + tex.height);
            return tex;
         }
      }
   }
}