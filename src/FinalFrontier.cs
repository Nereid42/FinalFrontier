using System;
using UnityEngine;
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

			public static readonly string ResourcePath = "Nereid/FinalFrontier/Resource/";

			public static readonly Configuration Config = new Configuration();

			public static readonly FARAdapter FarAdapter = new FARAdapter();

			private volatile IButton toolbarButton;
			private volatile HallOfFameBrowser browser;

			private volatile ApplicationLauncherButton stockToolbarButton;

			// Just to make sure that all pool instances exist
#if DEBUG
			private ActivityPool activities = ActivityPool.Instance();
			private RibbonPool ribbons = RibbonPool.Instance();
			private ActionPool actions = ActionPool.Instance();
#endif

			private volatile bool keyAltPressed = false;
			private volatile bool keyCtrlPressed = false;

			private volatile bool destroyed = false;


			public FinalFrontier()
			{
				Log.Info("New instance of Final Frontier");
			}

			public void Awake()
			{
				Log.Info("Awakening Final Frontier");
				Config.Load();
				Log.SetLevel(Config.GetLogLevel());
				Log.Info("Log level is " + Config.GetLogLevel());
				//
				// plugin adapters
				FarAdapter.TryInstallPlugin();
				//
				// log installed plugins
				Log.Info("FAR installed: " + FarAdapter.IsInstalled());

				DontDestroyOnLoad(this);
			}

			public void Start()
			{
				Log.Info("starting FinalFrontier");

				GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);

				CreateToolbarButton();
			}

			private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> e)
			{
				if (e.from != GameScenes.MAINMENU && e.to == GameScenes.MAINMENU)
				{
					Config.Save();
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
				if (Config.UseStockToolbar())
				{
					Log.Info("using stock toolbar button");
					if (ApplicationLauncher.Ready && stockToolbarButton == null)
					{
						Log.Info("creating stock toolbar button");
						stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
							OnAppLaunchToggleOn,
							OnAppLaunchToggleOff,
							DummyVoid,
							DummyVoid,
							DummyVoid,
							DummyVoid,
							ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER |
							ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB |
							ApplicationLauncher.AppScenes.TRACKSTATION,
							(Texture)GameDatabase.Instance.GetTexture(ResourcePath + "ToolbarIcon", false));
						if (stockToolbarButton == null) Log.Warning("no stock toolbar button registered");
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
				if (browser != null)
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

			private void DummyVoid()
			{
			}

			public void Update()
			{
				if (Input.GetKeyDown(KeyCode.LeftAlt)) keyAltPressed = true;
				if (Input.GetKeyUp(KeyCode.LeftAlt)) keyAltPressed = false;
				if (Input.GetKeyDown(KeyCode.LeftControl)) keyCtrlPressed = true;
				if (Input.GetKeyUp(KeyCode.LeftControl)) keyCtrlPressed = false;
				if (Config.IsHotkeyEnabled() && keyAltPressed && Input.GetKeyDown(KeyCode.F))
				{
					Log.Info("hotkey ALT-F detected");

					switch (HighLogic.LoadedScene)
					{
						case GameScenes.EDITOR:
						case GameScenes.FLIGHT:
						case GameScenes.SPACECENTER:
						case GameScenes.TRACKSTATION:
							if (!keyCtrlPressed)
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
				string iconOn = ResourcePath + "IconOn_24";
				string iconOff = ResourcePath + "IconOff_24";
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

					toolbarButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER,
						GameScenes.TRACKSTATION);
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
				//Config.Save(); 
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