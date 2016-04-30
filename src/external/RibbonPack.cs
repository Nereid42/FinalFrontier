using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Nereid
{
	namespace FinalFrontier
	{
		class RibbonPack : IEnumerable<Ribbon>
		{
			public string name { get; private set; }
			private int baseId = 0;
			private string baseFolder;
			private string ribbonFolder;

			private List<Ribbon> ribbons = new List<Ribbon>();

			public RibbonPack(string config)
			{
				name = "unnamed";
				var directoryName = Path.GetDirectoryName(config);
				if (directoryName != null)
					baseFolder = directoryName.Substring(Constants.GAMEDATA_PATH.Length + 1).Replace("\\", "/");
				ribbonFolder = baseFolder;
				Load(config);
			}

			private void ReadLine(string line)
			{
				// ignore comments
				if (!line.StartsWith("#") && line.Length > 0)
				{
					string[] fields = line.Split(':');
					if (fields.Length > 0)
					{
						string what = fields[0];
						if (what.Equals("NAME") && fields.Length == 2)
						{
							this.name = fields[1];
						}
						else if (what.Equals("FOLDER") && fields.Length == 2)
						{
							this.ribbonFolder = baseFolder + "/" + fields[1];
							Log.Detail("changing ribbon folder of ribbon pack '" + name + "' to '" + this.ribbonFolder + "'");
						}
						else if (what.Equals("BASE") && fields.Length == 2)
						{
							try
							{
								this.baseId = int.Parse(fields[1]);
								Log.Detail("changing base id of ribbon pack '" + name + "' to " + this.baseId);
							}
							catch
							{
								Log.Error("failed to parse custom ribbon base id");
							}
						}
						else if (fields.Length == 4 || fields.Length == 5)
						{
							int id;
							try
							{
								id = baseId + int.Parse(fields[0]);
							}
							catch
							{
								Log.Error("failed to parse custom ribbon id");
								return;
							}
							string fileOfRibbon = ribbonFolder + "/" + fields[1];
							string nameOfRibbon = fields[2];
							string descOfRibbon = fields[3];
							int prestigeOfRibbon = id;
							if (fields.Length == 5)
							{
								try
								{
									prestigeOfRibbon = int.Parse(fields[4]);
								}
								catch
								{
									Log.Error("failed to parse custom ribbon id");
								}
							}
							try
							{
								Log.Detail("adding custom ribbon '" + nameOfRibbon + "' (id " + id + ") to ribbon pack '" + name + "'");
								Log.Trace("path of custom ribbon file is '" + fileOfRibbon + "'");
								CustomAchievement achievement = new CustomAchievement(id, prestigeOfRibbon);
								achievement.SetName(nameOfRibbon);
								achievement.SetDescription(descOfRibbon);
								Ribbon ribbon = new Ribbon(fileOfRibbon, achievement);
								ribbons.Add(ribbon);
							}
							catch
							{
								Log.Warning("failed to add custom ribbon '" + nameOfRibbon + "' to ribbon pack '" + name + "'");
							}
						}
						else
						{
							Log.Warning("invalid ribbon pack file for " + name + " custom ribbon pack. failed to parse line '" + line + "'");
						}
					}
				}
			}

			private void Load(string config)
			{
				using (TextReader reader = File.OpenText(config))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						line = line.Trim();
						ReadLine(line);
					}
				}
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return ribbons.GetEnumerator();
			}

			IEnumerator<Ribbon> IEnumerable<Ribbon>.GetEnumerator()
			{
				return ribbons.GetEnumerator();
			}
		}
	}
}