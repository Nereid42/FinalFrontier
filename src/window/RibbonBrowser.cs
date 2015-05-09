using System;
using UnityEngine;
using System.Collections.Generic;

namespace Nereid
{
   namespace FinalFrontier
   {
      class RibbonBrowser : AbstractWindow
      {

         private Vector2 scrollPosition = Vector2.zero;

         public static int WIDTH = 480;
         public static int HEIGHT = 600;


         public RibbonBrowser()
            : base(Constants.WINDOW_ID_RIBBONBROWSER, "Ribbons")
         {


         }


         protected override void OnWindow(int id)
         {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Button("Ribbons:", GUIStyles.STYLE_LABEL);
            if (GUILayout.Button("Close", FFStyles.STYLE_BUTTON)) SetVisible(false);
            GUILayout.EndHorizontal();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, FFStyles.STYLE_SCROLLVIEW, GUILayout.Height(HEIGHT));
            GUILayout.BeginVertical();
            if (RibbonPool.Instance() != null)
            {
               foreach (Ribbon ribbon in RibbonPool.Instance())
               {
                  GUILayout.BeginHorizontal(FFStyles.STYLE_RIBBON_AREA);
                  GUILayout.Label(ribbon.GetTexture(), FFStyles.STYLE_SINGLE_RIBBON);
                  GUILayout.Label(ribbon.GetName() + ": " + ribbon.GetText(), FFStyles.STYLE_RIBBON_DESCRIPTION);
                  GUILayout.EndHorizontal();
               }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.Label(RibbonPool.Instance().Count() + " ribbons in total (" + RibbonPool.Instance().GetCustomRibbons().Count + " custom ribbons)", FFStyles.STYLE_LABEL);
           
            GUILayout.EndVertical();

            DragWindow();
         }

         public override int GetInitialWidth()
         {
            return WIDTH;
         }
      }
   }
}