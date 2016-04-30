using System;
using UnityEngine;
using FinalFrontierAdapter;
using System.Collections.Generic;


namespace Nereid
{
	namespace FinalFrontier
	{
		public class HallOfFameBrowser : PositionableWindow
		{
			private static readonly int kButtonWidth = 140;
			private static readonly int kAreaWidth = 450;
			private static readonly int kAreaHeight = 65;
			private readonly Texture2D textureAvailable;
			private readonly Texture2D textureAssigned;
			private readonly Texture2D textureKilled;
			private GUIStyle styleKerbalButton;
			private GUIStyle styleKerbalStatus;
			private GUIStyle styleKerbalArea;
			private GUIStyle styleKerbalInfo;
			private GUIStyle styleKerbalAreaExpanded;
			private GUIStyle styleRibbonArea;
			private static readonly int styleScrollviewHeight = 450;

			private RibbonBrowser ribbonBrowser;
			private DisplayWindow display;
			private AboutWindow about;
			private ConfigWindow configWindow;

			private Vector2 scrollPosition = Vector2.zero;

			private IButton toolbarButton;
			private string toolbarButtonTextureOn;
			private string toolbarButtonTextureOff;

			// Filter
			HallOfFameFilter filter;
			// Sorter
			HallOfFameSorter sorter;

			public class GameSceneBase
			{
				private readonly GameScenes scene;

				protected GameSceneBase(GameScenes scene)
				{
					this.scene = scene;
				}

				public GameScenes GetScene()
				{
					return scene;
				}

				public override bool Equals(object test)
				{
					GameSceneBase cmp = test as GameSceneBase;
					return cmp != null && scene.Equals(cmp.scene);
				}

				public override int GetHashCode()
				{
					return scene.GetHashCode();
				}
			}

			public class HallOfFameFilter : GameSceneBase, Filter<HallOfFameEntry>
			{
				public bool showDead { get; set; }
				public bool showAssigned { get; set; }
				public bool showAvailable { get; set; }
				public bool showUndecorated { get; set; }
				public bool showFlightOnly { get; set; }

				public HallOfFameFilter(GameScenes scene, bool showDead = true, bool showAssigned = true, bool showAvailable = true,
					bool showUndecorated = true, bool showFlightOnly = true)
					: base(scene)
				{
					this.showDead = showDead;
					this.showAssigned = showAssigned;
					this.showAvailable = showAvailable;
					this.showUndecorated = showUndecorated;
					this.showFlightOnly = showFlightOnly;
				}

				public bool Accept(HallOfFameEntry x)
				{
					ProtoCrewMember kerbal = x.GetKerbal();
					if (kerbal == null) return false;
					if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && !showDead) return false;
					if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing && !showDead) return false;
					if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && !showAssigned) return false;
					if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && !showAvailable) return false;
					//
					if (showFlightOnly && FlightGlobals.ActiveVessel != null && !kerbal.InCrewOfActiveFlight()) return false;
					//
					if (x.GetRibbons().Count == 0 && !showUndecorated) return false;
					return true;
				}

				public override string ToString()
				{
					return GetScene() + ": dead=" + showDead + ", assigned=" + showAssigned + ", available=" + showAvailable +
						   ", undecorated=" + showUndecorated + ", flight only=" + showFlightOnly;
				}
			}

			public class HallOfFameSorter : GameSceneBase, Sorter<HallOfFameEntry>
			{
				public enum SortDirection
				{
					Ascending = 1,
					Descending = 2
				};

				public enum SortableStats
				{
					Name = 1,
					Missions = 2,
					Missiontime = 3,
					Ribbons = 4,
					Dockings = 5,
					Contracts = 6,
					Science = 7,
					EVA = 8,
					State = 9
				}

				public enum SortableSkills
				{
					Specialist = 1,
					Stupidity = 2,
					Courage = 3,
					Experience = 4
				}

				//
				private SortDirection sortingDirection;
				private SortableStats sortingStat;
				private SortableSkills sortingSkill;
				//
				// kerbal display sortMode: statistics / skills
				public enum SortMode
				{
					Stat = 1,
					Skill = 2
				};

				private SortMode sortMode;

				public HallOfFameSorter(GameScenes scene, SortDirection sortingDirection = SortDirection.Ascending,
					SortMode mode = SortMode.Stat,
					SortableStats sortByStat = SortableStats.Name, SortableSkills sortBySkill = SortableSkills.Specialist)
					: base(scene)
				{
					this.sortingDirection = sortingDirection;
					this.sortMode = mode;
					this.sortingStat = sortByStat;
					this.sortingSkill = sortBySkill;
				}

				public string PredicateAsstring()
				{
					switch (sortMode)
					{
						case SortMode.Stat:
							switch (sortingStat)
							{
								case SortableStats.Name:
									return "Name";
								case SortableStats.Missions:
									return "Missions";
								case SortableStats.Missiontime:
									return "Mission Time";
								case SortableStats.Ribbons:
									return "Ribbons";
								case SortableStats.Dockings:
									return "Dockings";
								case SortableStats.Contracts:
									return "Contracts";
								case SortableStats.Science:
									return "Science";
								case SortableStats.EVA:
									return "Eva";
								case SortableStats.State:
									return "State";
								default:
									Log.Warning("Invalid statistic sort predicate: " + sortingStat + " (in " + HighLogic.LoadedScene + ")");
									break;
							}
							break;
						case SortMode.Skill:
							switch (sortingSkill)
							{
								case SortableSkills.Specialist:
									return "Specialist";
								case SortableSkills.Stupidity:
									return "Stupidy";
								case SortableSkills.Courage:
									return "Courage";
								case SortableSkills.Experience:
									return "Experience";
								default:
									Log.Warning("Invalid skill sort predicate: " + sortingSkill + " (in " + HighLogic.LoadedScene + ")");
									break;
							}
							break;
					}
					return "Unknown";
				}

				public string ModeAsstring()
				{
					switch (sortMode)
					{
						case SortMode.Skill:
							return "Skills";
						case SortMode.Stat:
							return "Statistics";
					}
					return "Unknown";
				}

				public string DirectionAsstring()
				{
					switch (sortingDirection)
					{
						case SortDirection.Ascending:
							return "Ascending";
						case SortDirection.Descending:
							return "Descending";
					}
					return "Unknown";
				}


				public void NextStatistic()
				{
					sortingStat++;
					if ((int)sortingStat > 9) sortingStat = SortableStats.Name;
					HallOfFame.Instance().Sort();
				}

				public void NextSkill()
				{
					sortingSkill++;
					if ((int)sortingSkill > 4) sortingSkill = SortableSkills.Specialist;
					HallOfFame.Instance().Sort();
				}

				public void NextMode()
				{
					sortMode++;
					if ((int)sortMode > 2) sortMode = SortMode.Stat;
					HallOfFame.Instance().Sort();
				}

				public void NextDirection()
				{
					sortingDirection++;
					if ((int)sortingDirection > 2) sortingDirection = SortDirection.Ascending;
					HallOfFame.Instance().Sort();
				}

				private void SortBySkill(List<HallOfFameEntry> list)
				{
					list.Sort(delegate (HallOfFameEntry left, HallOfFameEntry right)
					{
						int sign = sortingDirection == SortDirection.Ascending ? 1 : -1;
						ProtoCrewMember leftkerbal = left.GetKerbal();
						ProtoCrewMember rightkerbal = right.GetKerbal();
						if (leftkerbal == null) return -1;
						if (rightkerbal == null) return 1;
						int cmp;
						switch (sortingSkill)
						{
							case SortableSkills.Specialist:
								cmp = sign *
									  string.Compare(leftkerbal.experienceTrait.TypeName, rightkerbal.experienceTrait.TypeName,
										  StringComparison.Ordinal);
								if (cmp != 0) return cmp;
								cmp = sign * (leftkerbal.experienceLevel - rightkerbal.experienceLevel);
								if (cmp != 0) return cmp;
								return sign * string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableSkills.Courage:
								cmp = sign * Math.Sign(leftkerbal.courage - rightkerbal.courage);
								if (cmp != 0) return cmp;
								return sign * string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableSkills.Stupidity:
								cmp = sign * Math.Sign(leftkerbal.stupidity - rightkerbal.stupidity);
								if (cmp != 0) return cmp;
								return sign * string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableSkills.Experience:
								cmp = sign * Math.Sign(leftkerbal.experience - rightkerbal.experience);
								if (cmp != 0) return cmp;
								return sign * string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							default:
								Log.Error("Unknown sorting method");
								return 0;
						}
					});
				}

				private void SortByStatistic(List<HallOfFameEntry> list)
				{
					list.Sort(delegate (HallOfFameEntry left, HallOfFameEntry right)
					{
						int sign = sortingDirection == SortDirection.Ascending ? 1 : -1;
						int cmp;
						switch (sortingStat)
						{
							case SortableStats.Name:
								return sign * string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Missions:
								cmp = sign * (left.MissionsFlown - right.MissionsFlown);
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Missiontime:
								cmp = (int)(sign * (left.TotalMissionTime - right.TotalMissionTime));
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Ribbons:
								cmp = sign * (left.GetRibbons().Count - right.GetRibbons().Count);
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.State:
								cmp = sign * (left.GetKerbal().rosterStatus.CompareTo(right.GetKerbal().rosterStatus));
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Dockings:
								cmp = sign * (left.Dockings - right.Dockings);
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Contracts:
								cmp = sign * (left.ContractsCompleted - right.ContractsCompleted);
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.Science:
								cmp = sign * (int)(100.0 * (left.Research - right.Research));
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							case SortableStats.EVA:
								cmp = sign * (int)(left.TotalEvaTime - right.TotalEvaTime);
								if (cmp != 0) return cmp;
								return string.Compare(left.GetName(), right.GetName(), StringComparison.Ordinal);
							default:
								Log.Error("Unknown sorting method");
								return 0;
						}
					});
				}

				public void Sort(List<HallOfFameEntry> list)
				{
					if (list == null) return;
					switch (sortMode)
					{
						case SortMode.Stat:
							SortByStatistic(list);
							break;
						case SortMode.Skill:
							SortBySkill(list);
							break;
					}
				}

				public void SetDirection(SortDirection sortDirection)
				{
					sortingDirection = sortDirection;
					HallOfFame.Instance().Sort();
				}

				public void SetStatsPredicate(SortableStats predicate)
				{
					sortingStat = predicate;
					HallOfFame.Instance().Sort();
				}

				public void SetSkillPredicate(SortableSkills predicate)
				{
					sortingSkill = predicate;
					HallOfFame.Instance().Sort();
				}

				public void SetMode(SortMode mode)
				{
					this.sortMode = mode;
					HallOfFame.Instance().Sort();
				}

				public SortMode GetMode()
				{
					return sortMode;
				}

				public SortDirection GetDirection()
				{
					return sortingDirection;
				}

				public SortableStats GetStatsPredicate()
				{
					return sortingStat;
				}

				public SortableSkills GetSkillPredicate()
				{
					return sortingSkill;
				}

				public override string ToString()
				{
					switch (sortMode)
					{
						case SortMode.Stat:
							return GetScene() + ": sort by " + sortingStat + " " + sortingDirection;
						case SortMode.Skill:
							return GetScene() + ": sort by " + sortingSkill + " " + sortingDirection;
					}
					return "Unknown sort mode";
				}
			}

			// expanded ribbon area (-1: none)
			private int expandedRibbonAreaIndex = -1;


			public HallOfFameBrowser()
				: base(Constants.WINDOW_ID_HALLOFFAMEBROWSER, FinalFrontier.Config.GetHallOfFameWindowTitle())
			{
				styleKerbalButton = new GUIStyle(HighLogic.Skin.button);
				styleKerbalButton.fixedWidth = kButtonWidth;
				styleKerbalButton.clipping = TextClipping.Clip;
				styleKerbalStatus = new GUIStyle(HighLogic.Skin.button);
				styleKerbalStatus.fixedWidth = 20;
				styleKerbalArea = new GUIStyle(HighLogic.Skin.box);
				styleKerbalArea.fixedWidth = kAreaWidth;
				styleKerbalArea.fixedHeight = kAreaHeight;
				styleKerbalArea.clipping = TextClipping.Clip;
				styleKerbalAreaExpanded = new GUIStyle(HighLogic.Skin.box);
				styleKerbalAreaExpanded.fixedWidth = kAreaWidth;
				styleKerbalAreaExpanded.stretchHeight = true;
				styleKerbalAreaExpanded.clipping = TextClipping.Clip;
				styleKerbalInfo = new GUIStyle(HighLogic.Skin.label);
				styleRibbonArea = new GUIStyle(HighLogic.Skin.label);
				styleRibbonArea.stretchHeight = true;
				styleRibbonArea.stretchWidth = true;
				styleRibbonArea.padding = new RectOffset(10, 10, 2, 2);

				textureAvailable = ImageLoader.GetTexture(FinalFrontier.ResourcePath + "active");
				textureAssigned = ImageLoader.GetTexture(FinalFrontier.ResourcePath + "assigned");
				textureKilled = ImageLoader.GetTexture(FinalFrontier.ResourcePath + "killed");

				ribbonBrowser = new RibbonBrowser();
				display = new DisplayWindow();
				about = new AboutWindow();
				configWindow = new ConfigWindow();
			}


			protected override void OnWindow(int id)
			{
				// persistent filter for displaying kerbals
				if (filter == null || filter.GetScene() != HighLogic.LoadedScene)
					filter = FinalFrontier.Config.GetDisplayFilterForSzene(HighLogic.LoadedScene);
				//
				// persistent sorter for displaying kerbals
				if (sorter == null || sorter.GetScene() != HighLogic.LoadedScene)
				{
					// sorter has changed
					sorter = FinalFrontier.Config.GetHallOfFameSorterForScene(HighLogic.LoadedScene);
					HallOfFame.Instance().SetSorter(sorter);
				}
				// color of info
				switch (sorter.GetMode())
				{
					case HallOfFameSorter.SortMode.Stat:
						styleKerbalInfo.normal.textColor = HighLogic.Skin.label.normal.textColor;
						break;
					case HallOfFameSorter.SortMode.Skill:
						styleKerbalInfo.normal.textColor = Color.cyan;
						break;
				}
				//
				GUILayout.BeginHorizontal();
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(490),
					GUILayout.Height(styleScrollviewHeight));
				GUILayout.BeginVertical();
				int index = 0;
				bool expandDetected = false;
				bool autoexpandEnabled = FinalFrontier.Config.IsAutoExpandEnabled();
				lock (HallOfFame.Instance())
				{
					foreach (HallOfFameEntry entry in HallOfFame.Instance())
					{
						// autoexpand this entry on mouse hover?
						bool expandedEntry = autoexpandEnabled && (expandedRibbonAreaIndex == index) &&
											 (entry.GetRibbons().Count > Constants.MAX_RIBBONS_PER_AREA);
						//
						ProtoCrewMember kerbal = entry.GetKerbal();
						string info = GetInfo(entry);
						string missionTimeInDays = Utils.GameTimeInDaysAsstring(entry.TotalMissionTime) +
												   (GameUtils.IsKerbinTimeEnabled() ? " kerbin" : "");
						if (kerbal != null && filter.Accept(entry) && kerbal.IsCrew())
						{
							string buttonTooltip = kerbal.name + ": " + entry.MissionsFlown + " missions, " + missionTimeInDays +
												   " days mission time";
							GUILayout.BeginHorizontal(styleKerbalAreaExpanded);
							//expandedEntry ? styleKerbalAreaExpanded : styleKerbalArea);          
							GUILayout.BeginVertical();
							GUILayout.BeginHorizontal();
							// butto to open decoration board
							if (GUILayout.Button(new GUIContent(entry.GetName(), buttonTooltip), styleKerbalButton))
							{
								Log.Detail("opening decoration board for kerbal " + entry.GetName());
								display.SetEntry(entry);
								display.SetVisible(true);
							}
							DrawStatus(kerbal);
							GUILayout.EndHorizontal();
							GUILayout.Label(info, styleKerbalInfo);
							GUILayout.EndVertical();
							DrawRibbons(entry, expandedEntry ? Constants.MAX_RIBBONS_PER_EXPANDED_AREA : Constants.MAX_RIBBONS_PER_AREA);
							GUILayout.EndHorizontal();
							//
							if (Event.current.type == EventType.Repaint)
							{
								if (MouseOver(0.0f, scrollPosition.y))
								{
									expandedRibbonAreaIndex = index;
									expandDetected = true;
								}
							}
						}
						index++;
					} // foreach entry
				} // lock (HAllOfFame)
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.BeginVertical();
				if (GUILayout.Button("Close", FFStyles.STYLE_BUTTON))
				{
					SetVisible(false);
					ribbonBrowser.SetVisible(false);
					display.SetVisible(false);
				}
				GUILayout.Label("", FFStyles.STYLE_STRETCHEDLABEL);
				if (GUILayout.Button("Ribbons", FFStyles.STYLE_BUTTON))
				{
					if (!ribbonBrowser.IsVisible())
					{
						OpenRibbonBrowser();
					}
					else
					{
						ribbonBrowser.SetVisible(false);
					}
				}
				GUILayout.Label("", FFStyles.STYLE_STRETCHEDLABEL);
				GUILayout.Label("Filter:", FFStyles.STYLE_STRETCHEDLABEL);
				if (GUILayout.Toggle(filter.showDead, "dead", FFStyles.STYLE_TOGGLE)) filter.showDead = true;
				else filter.showDead = false;
				if (GUILayout.Toggle(filter.showAssigned, "active", FFStyles.STYLE_TOGGLE)) filter.showAssigned = true;
				else filter.showAssigned = false;
				if (GUILayout.Toggle(filter.showAvailable, "available", FFStyles.STYLE_TOGGLE)) filter.showAvailable = true;
				else filter.showAvailable = false;
				if (GUILayout.Toggle(filter.showUndecorated, "undecorated", FFStyles.STYLE_TOGGLE)) filter.showUndecorated = true;
				else filter.showUndecorated = false;
				if (HighLogic.LoadedScene == GameScenes.FLIGHT)
				{
					if (GUILayout.Toggle(filter.showFlightOnly, "flight only", FFStyles.STYLE_TOGGLE)) filter.showFlightOnly = true;
					else filter.showFlightOnly = false;
				}
				GUILayout.Label("", FFStyles.STYLE_STRETCHEDLABEL); // fixed space

				// sorter
				GUILayout.FlexibleSpace();
				GUILayout.Label("Sort by:", FFStyles.STYLE_STRETCHEDLABEL);
				DrawSorterButtons();
				GUILayout.Label("", FFStyles.STYLE_STRETCHEDLABEL); // fixed space

				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Config", FFStyles.STYLE_BUTTON))
				{
					if (!configWindow.IsVisible()) MoveWindowAside(configWindow);
					configWindow.SetVisible(!configWindow.IsVisible());
				}
				if (GUILayout.Button("About", FFStyles.STYLE_BUTTON)) about.SetVisible(!about.IsVisible());
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();

				if (Event.current.type == EventType.Repaint && !expandDetected)
				{
					expandedRibbonAreaIndex = -1;
				}

				DragWindow();
			}

			private string GetInfo(HallOfFameEntry entry)
			{
				switch (sorter.GetMode())
				{
					default:
					case HallOfFameSorter.SortMode.Stat:
						switch (sorter.GetStatsPredicate())
						{
							default:
								return entry.MissionsFlown + " missions";
							case HallOfFameSorter.SortableStats.EVA:
								return Utils.GameTimeAsstring(entry.TotalEvaTime) + " in EVA";
							case HallOfFameSorter.SortableStats.Dockings:
								return entry.Dockings + " docking operations";
							case HallOfFameSorter.SortableStats.Missiontime:
								return Utils.GameTimeInDaysAsstring(entry.TotalMissionTime) + " days in missions";
							case HallOfFameSorter.SortableStats.Contracts:
								return entry.ContractsCompleted + " contracts completed";
							case HallOfFameSorter.SortableStats.Science:
								return entry.Research.ToString("0.0") + " science points";
						}
					case HallOfFameSorter.SortMode.Skill:
						ProtoCrewMember kerbal = entry.GetKerbal();
						if (kerbal == null) return "no kerbal";
						string specialist = kerbal.experienceTrait.TypeName;
						switch (sorter.GetSkillPredicate())
						{
							case HallOfFameSorter.SortableSkills.Stupidity:
								return specialist + " (" + kerbal.stupidity.ToString("0.00") + " stupidity)";
							case HallOfFameSorter.SortableSkills.Courage:
								return specialist + " (" + kerbal.courage.ToString("0.00") + " courage)";
							case HallOfFameSorter.SortableSkills.Experience:
								return specialist + " (" + kerbal.experience + " xp)";
							default:
								return specialist + " (level " + kerbal.experienceLevel + ")";
						}
				}
			}

			private void DrawSorterButtons()
			{
				if (GUILayout.Button(sorter.ModeAsstring(), FFStyles.STYLE_NARROW_BUTTON))
				{
					sorter.NextMode();
				}
				if (GUILayout.Button(sorter.PredicateAsstring(), FFStyles.STYLE_BUTTON))
				{
					switch (sorter.GetMode())
					{
						case HallOfFameSorter.SortMode.Stat:
							sorter.NextStatistic();
							break;
						case HallOfFameSorter.SortMode.Skill:
							sorter.NextSkill();
							break;
					}
				}
				if (GUILayout.Button(sorter.DirectionAsstring(), FFStyles.STYLE_NARROW_BUTTON))
				{
					sorter.NextDirection();
				}
			}


			private void DrawStatus(ProtoCrewMember kerbal)
			{
				ProtoCrewMember.RosterStatus status = kerbal.rosterStatus;
				string tooltip;
				switch (status)
				{
					case ProtoCrewMember.RosterStatus.Dead:
					case ProtoCrewMember.RosterStatus.Missing:
						tooltip = kerbal.name + " is dead";
						GUILayout.Label(new GUIContent(textureKilled, tooltip), styleKerbalStatus);
						break;
					case ProtoCrewMember.RosterStatus.Assigned:
						tooltip = kerbal.name + " is currently on a mission";
						GUILayout.Label(new GUIContent(textureAssigned, tooltip), styleKerbalStatus);
						break;
					default:
						tooltip = kerbal.name + " is available for next mission";
						GUILayout.Label(new GUIContent(textureAvailable, tooltip), styleKerbalStatus);
						break;
				}
			}

			private void OpenRibbonBrowser()
			{
				float x = bounds.x + bounds.width + 8;
				float y = bounds.y;
				if (x + RibbonBrowser.WIDTH > Screen.width)
				{
					x = Screen.width - RibbonBrowser.WIDTH;
				}
				ribbonBrowser.SetVisible(true, x, y);
			}


			private void DrawRibbons(HallOfFameEntry entry, int max)
			{
				ProtoCrewMember kerbal = entry.GetKerbal();
				List<Ribbon> ribbons = entry.GetRibbons();

				GUILayout.BeginVertical(styleRibbonArea);
				int n = 0;
				int RIBBONS_PER_LINE = 4;
				foreach (Ribbon ribbon in ribbons)
				{
					if (n % RIBBONS_PER_LINE == 0) GUILayout.BeginHorizontal();
					string tooltip = ribbon.GetName() + "\n" + ribbon.GetDescription();
					GUILayout.Button(new GUIContent(ribbon.GetTexture(), tooltip), FFStyles.STYLE_RIBBON);
					n++;
					if (n % RIBBONS_PER_LINE == 0) GUILayout.EndHorizontal();
					if (n >= max) break;
				}
				if (n % RIBBONS_PER_LINE != 0) GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			}

			protected void DrawRibbon(int x, int y, Ribbon ribbon, int scale = 1)
			{
				Rect rect = new Rect(x, y, ribbon.GetWidth() / scale, ribbon.GetHeight() / scale);
				GUI.DrawTexture(rect, ribbon.GetTexture());
			}

			protected override void OnOpen()
			{
				Log.Info("hall of fame browser opened");
				base.OnOpen();
				HallOfFame.Instance().Refresh();
				if (toolbarButton != null)
				{
					toolbarButton.TexturePath = toolbarButtonTextureOn;
				}
			}

			protected override void OnClose()
			{
				base.OnClose();
				if (toolbarButton != null)
				{
					toolbarButton.TexturePath = toolbarButtonTextureOff;
				}
			}

			public override int GetInitialWidth()
			{
				return 650;
			}

			public void registerToolbarButton(IButton button, string textureOn, string textureOff)
			{
				this.toolbarButton = button;
				this.toolbarButtonTextureOn = textureOn;
				this.toolbarButtonTextureOff = textureOff;
			}
		}
	}
}