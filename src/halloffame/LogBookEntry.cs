using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
	namespace FinalFrontier
	{
		public class LogbookEntry
		{
			// serializion
			public static readonly char TEXT_DELIM = '~';
			private static readonly char[] TEXT_SEPARATORS = new char[] { TEXT_DELIM };
			private static readonly char[] FIELD_SEPARATORS = new char[] { ' ' };

			public double UniversalTime { get; set; }
			public string Code { get; set; }
			public string Name { get; set; }
			public string Data { get; set; }

			public LogbookEntry(double time, string code, string name, string text = "")
			{
				this.UniversalTime = time;
				this.Code = code;
				this.Name = name ?? "";
				this.Data = text;
				//
				if (Name.Contains(TEXT_DELIM))
				{
					Log.Error("name field contains invalid character '" + TEXT_DELIM + "': " + Name);
					Name = Name.Replace(TEXT_DELIM, '_');
				}
			}

			public override string ToString()
			{
				string timestamp = Utils.ConvertToEarthTime(UniversalTime) + ": ";
				Action action = ActionPool.Instance().GetActionForCode(Code);
				if (action != null)
				{
					return timestamp + action.CreateLogBookEntry(this);
				}

				Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(Code);
				if (ribbon != null)
				{
					Achievement achievement = ribbon.GetAchievement();
					return timestamp + achievement.CreateLogBookEntry(this);
				}

				return "unknown logbook entry (code " + Code + ")";
			}

			public string Asstring()
			{
				return Utils.ConvertToKerbinTime(UniversalTime) + ": " + Name + " " + Code;
			}

			public string Serialize()
			{
				string line = UniversalTime.ToString() + " " + Code + " " + Name;
				if (Data != null && Data.Length > 0) line = line + TEXT_DELIM + Data;
				return line;
			}

			public static LogbookEntry Deserialize(string line)
			{
				string[] field = line.Split(FIELD_SEPARATORS, 3);
				if (field.Length == 3)
				{
					double time = double.Parse(field[0]);
					string code = field[1];
					string name = field[2];
					string text = "";
					if (name.Contains(TEXT_DELIM))
					{
						string[] subfields = field[2].Split(TEXT_SEPARATORS, 2);
						name = subfields[0];
						text = (subfields.Length == 2) ? subfields[1] : "";
					}
					return new LogbookEntry(time, code, name, text);
				}
				else
				{
					Log.Warning("invalid logbook entry: " + line);
				}
				return null;
			}
		}
	}
}