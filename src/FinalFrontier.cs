﻿using System;
using UnityEngine;
using KSP.IO;
using FinalFrontierAdapter;
using KSP.UI.Screens;

namespace Nereid
{
   namespace FinalFrontier
   {

      [KSPAddon(KSPAddon.Startup.Instantly, true)]
      public class FinalFrontier : MonoBehaviour
      {
         private static readonly EventObserver observer = new EventObserver();

         public static readonly String RESOURCE_PATH =  "Nereid/FinalFrontier/Resource/";

         // Must initialize in Awake()
         private static Configuration _configuration = null;
         public static Configuration configuration { get { return _configuration; } }

         // Must initialize in Awake()
         private static FARAdapter _farAdapter = null;
         public static FARAdapter farAdapter {  get {  return _farAdapter; } }

         private volatile IButton toolbarButton;
         private volatile HallOfFameBrowser browser;

         private volatile ApplicationLauncherButton stockToolbarButton = null;

         private ActivityPool activities = null;
         private RibbonPool ribbons = null;
         private ActionPool actions = null;

         private volatile bool destroyed = false;


         public FinalFrontier()
         {
            Log.Info("new instance of Final Frontier");
         }

         public void Awake()
         {
            Log.Info("awakening Final Frontier");
            _configuration = new Configuration();
            _configuration.Load();
            Log.SetLevel(_configuration.GetLogLevel());
            Log.Info("log level is " + _configuration.GetLogLevel());

            //
            // plugin adapters
            _farAdapter = new FARAdapter();
            _farAdapter.Plugin();
            //
            // log installed plugins
            Log.Info("FAR installed: " + _farAdapter.IsInstalled());
            //
            // masterTextureLimit should not be 1
            if (QualitySettings.masterTextureLimit!=0)
            {
               Log.Warning("changing masterTextureLimit to 0");
               QualitySettings.masterTextureLimit = 0;
            }

            // make sure that all pool instances exists
            activities = ActivityPool.Instance();
            ribbons = RibbonPool.Instance();
            actions = ActionPool.Instance();

            DontDestroyOnLoad(this);
         }

         public void Start()
         {
            Log.Info("starting FinalFrontier");

            GameEvents.onGameSceneSwitchRequested.Add(this.OnGameSceneSwitchRequested);

            CreateToolbarButton();
         }

         private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes,GameScenes> e)
         {
            if(e.from!=GameScenes.MAINMENU && e.to==GameScenes.MAINMENU)
            {
               configuration.Save();
               WindowManager.instance.CloseAll();
            }
         }

         private void CreateToolbarButton()
         {
            if (ApplicationLauncher.Instance != null && ApplicationLauncher.Ready)
            {
               Log.Detail("ApplicationLauncher is ready");
               OnGUIAppLauncherReady();
            }
            else
            {
               Log.Detail("ApplicationLauncher is not ready");
               GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
         }


         private void OnGUIAppLauncherReady()
         {
            if (destroyed) return;
             if(configuration.UseStockToolbar())
             {
                Log.Info("using stock toolbar button");
                if (ApplicationLauncher.Ready && stockToolbarButton==null)
                {
                   Log.Info("creating stock toolbar button");
                   stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                   OnAppLaunchToggleOn,
                   OnAppLaunchToggleOff,
                   DummyVoid,
                   DummyVoid,
                   DummyVoid,
                   DummyVoid,
                   ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.TRACKSTATION,
                   (Texture)GameDatabase.Instance.GetTexture(RESOURCE_PATH + "ToolbarIcon", false));
                   if(stockToolbarButton==null) Log.Warning("no stock toolbar button registered");
                }
             }
             else
             {
                AddBlizzysToolbarButtons();
             }
         }

         void OnAppLaunchToggleOn()
         {
            createBrowserOnce();
            if(browser!=null)
            {
               browser.SetVisible(true);
            }
         }

         void OnAppLaunchToggleOff()
         {
            if (browser != null)
            {
               browser.SetVisible(false);
            }
         }

         private void DummyVoid() { }


         public void Update()
         {

            bool keyAlt = Input.GetKey(KeyCode.LeftAlt);
            bool keyCtrl = Input.GetKey(KeyCode.LeftControl);

            bool hotkey = Input.GetKeyDown(configuration.hotkey);


            if (configuration.IsHotkeyEnabled() && keyAlt && hotkey)
            {
               Log.Info("hotkey chord detected");
               
               switch (HighLogic.LoadedScene)
               {
                  case GameScenes.EDITOR:
                  case GameScenes.FLIGHT:
                  case GameScenes.SPACECENTER:
                  case GameScenes.TRACKSTATION:
                     if (!keyCtrl)
                     {
                        Log.Info("hotkey hall of fame browser");
                        createBrowserOnce();
                        browser.SetVisible(!browser.IsVisible());
                     }
                     else
                     {
                        Log.Info("hotkey reset window positions");
                        PositionableWindow.ResetAllWindowPositions();
                     }
                     break;
                  default:
                     Log.Info("cant open/close hall of fame in game scene " + HighLogic.LoadedScene);
                     break;
               }
            }

            if (observer != null)
            {
               observer.Update();
            }
         }

         private void AddBlizzysToolbarButtons()
         {
            Log.Detail("adding toolbar buttons");
            String iconOn = RESOURCE_PATH + "IconOn_24";
            String iconOff = RESOURCE_PATH + "IconOff_24";
            toolbarButton = ToolbarManager.Instance.add("FinalFrontier", "button");
            if (toolbarButton != null)
            {
               toolbarButton.TexturePath = iconOff;
               toolbarButton.ToolTip = "Open Final Frontier";
               toolbarButton.OnClick += (e) =>
                   {
                      createBrowserOnce();
                      if (browser != null) browser.registerToolbarButton(toolbarButton, iconOn, iconOff);
                      toggleBrowserVisibility();
                   };

               toolbarButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION);
            }
            else
            {
               Log.Error("toolbar button was null");
            }
            Log.Detail("toolbar buttons added");
         }

         private void toggleBrowserVisibility()
         {
            browser.SetVisible(!browser.IsVisible());
         }

         private void createBrowserOnce()
         {
            if (browser == null)
            {
               Log.Info("creating new hall of fame browser");
               browser = new HallOfFameBrowser();
               browser.CallOnWindowClose(OnBrowserClose);
            }
         }

         public void OnBrowserClose()
         {
            if (stockToolbarButton != null)
            {
               stockToolbarButton.toggleButton.Value = false;
            }            
         }

         private void OnGUI()
         {
            WindowManager.instance.OnGUI();
         }

         internal void OnDestroy()
         {
            /*Log.Info("destroying Final Frontier");
            if (stockToolbarButton != null)
            {
               Log.Detail("removing stock toolbar button");
               ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
            }
            //configuration.Save(); 
            if(toolbarButton!=null)
            {
               Log.Detail("removing toolbar button");
               toolbarButton.Destroy();
            }
            stockToolbarButton = null;*/
         }

      }
   }
}
