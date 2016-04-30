using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nereid
{
	namespace FinalFrontier
	{
		static class Utils
		{
			private static readonly char[] SINGLE_SPACE_ARRAY = new char[] { ' ' };

			private static void MoveFile(string from, string to)
			{
				try
				{
					File.Move(from, to);
				}
				catch
				{
					Log.Warning("failed to move  file '" + from + "' to '" + to + "'");
				}
			}

			public static string ToString<T>(List<T> list)
			{
				string result = "";
				foreach (T x in list)
				{
					if (result.Length > 0) result = result + ",";
					result = result + x.ToString();
				}
				return result + " (" + list.Count + " entries)";
			}

			public static string Roman(int value)
			{
				switch (value)
				{
					case 0:
						return "";
					case 1:
						return "I";
					case 2:
						return "II";
					case 3:
						return "III";
					case 4:
						return "IV";
					case 5:
						return "V";
					case 6:
						return "VI";
					case 7:
						return "VII";
					case 8:
						return "VIII";
					case 9:
						return "IX";
					case 10:
						return "X";
					case 11:
						return "XI";
					case 12:
						return "XII";
					case 13:
						return "XIII";
					case 14:
						return "XIV";
					case 15:
						return "XV";
					case 16:
						return "XVI";
					case 17:
						return "XVII";
					case 18:
						return "XVIII";
					case 19:
						return "XIX";
					case 20:
						return "XX";
				}
				return "?";
			}

			public static CelestialBody GetCelestialBody(string name)
			{
				foreach (CelestialBody body in PSystemManager.Instance.localBodies)
				{
					if (body.GetName().Equals(name)) return body;
				}
				return null;
			}

			public static void FileRotate(string filename, int maxNr)
			{
				//
				Log.Info("rotating file '" + filename + "' (" + maxNr + " versions)");
				string obsolete = filename + "." + maxNr;
				if (File.Exists(obsolete))
				{
					try
					{
						File.Delete(obsolete);
					}
					catch
					{
						Log.Warning("failed to delete obsolete file '" + obsolete + "'");
					}
				}

				for (int n = maxNr - 1; n > 0; n--)
				{
					string oldFilename = filename + "." + n;
					string newFilename = filename + "." + (n + 1);
					if (File.Exists(oldFilename))
					{
						MoveFile(oldFilename, newFilename);
					}
				}
				//
				MoveFile(filename, filename + ".1");
			}


			public static int ConvertDaysToSeconds(int days)
			{
				return days * 60 * 60 * 24;
			}

			public static int ConvertHoursToSeconds(int hours)
			{
				return hours * 60 * 60;
			}

			public static string GetRootPath()
			{
				string path = KSPUtil.ApplicationRootPath;
				path = path.Replace("\\", "/");
				if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
				//
				return path;
			}

			public static string ConvertToKerbinDuration(double ut)
			{
				double hours = ut / 60.0 / 60.0;
				double kHours = Math.Floor(hours % 24.0);
				double kMinutes = Math.Floor((ut / 60.0) % 60.0);
				double kSeconds = Math.Floor(ut % 60.0);


				double kYears = Math.Floor(hours / 2556.5402); // Kerbin year is 2556.5402 hours
				double kDays = Math.Floor(hours % 2556.5402 / 24.0);
				return ((kYears > 0) ? (kYears.ToString() + " Years ") : "")
					   + ((kDays > 0) ? (kDays.ToString() + " Days ") : "")
					   + ((kHours > 0) ? (kHours.ToString() + " Hours ") : "")
					   + ((kMinutes > 0) ? (kMinutes.ToString() + " Minutes ") : "")
					   + ((kSeconds > 0) ? (kSeconds.ToString() + " Seconds ") : "");
			}


			public static string ConvertToKerbinTime(double ut)
			{
				double hours = ut / 60.0 / 60.0;
				double kHours = Math.Floor(hours % 6.0);
				double kMinutes = Math.Floor((ut / 60.0) % 60.0);
				double kSeconds = Math.Floor(ut % 60.0);


				double kYears = Math.Floor(hours / 2556.5402) + 1; // Kerbin year is 2556.5402 hours
				double kDays = Math.Floor(hours % 2556.5402 / 6.0) + 1;

				return "Year " + kYears.ToString() + ", Day " + kDays.ToString() + " " + " " + kHours.ToString("00") + ":" +
					   kMinutes.ToString("00") + ":" + kSeconds.ToString("00");
			}


			public static string ConvertToEarthTime(double ut)
			{
				double hours = ut / 60.0 / 60.0;
				double eHours = Math.Floor(hours % 24.0);
				double eMinutes = Math.Floor((ut / 60.0) % 60.0);
				double eSeconds = Math.Floor(ut % 60.0);


				double eYears = Math.Floor(hours / (365 * 24)) + 1;
				double eDays = Math.Floor(hours % (365 * 24) / 24.0) + 1;

				return "Year " + eYears.ToString() + ", Day " + eDays.ToString() + " " + " " + eHours.ToString("00") + ":" +
					   eMinutes.ToString("00") + ":" + eSeconds.ToString("00");
			}

			public static double GameTimeInDays(double time)
			{
				if (GameUtils.IsKerbinTimeEnabled())
				{
					return time / 6 / 60 / 60;
				}
				else
				{
					return time / 24 / 60 / 60;
				}
			}


			/**
          * Remove multiple spaces
          */

			public static string Compress(this string s)
			{
				if (s == null) return "";
				return string.Join(" ", s.Trim().Split(SINGLE_SPACE_ARRAY, StringSplitOptions.RemoveEmptyEntries));
			}

			public static string GameTimeInDaysAsstring(double time)
			{
				double inDays = GameTimeInDays(time);
				if (inDays >= 1000)
				{
					return (inDays / 1000).ToString("0") + "k";
				}
				else if (inDays >= 1000000)
				{
					return (inDays / 1000000).ToString("0") + "m";
				}
				return inDays.ToString("0.00");
			}

			public static string GameTimeAsstring(double time)
			{
				long seconds = (long)time;
				long days = (long)GameTimeInDays(time);
				long hours_per_day = GameUtils.IsKerbinTimeEnabled()
					? Constants.HOURS_PER_KERBIN_DAY
					: Constants.HOURS_PER_EARTH_DAY;
				long seconds_per_day = Constants.SECONDS_PER_HOUR * hours_per_day;
				long hours = (seconds % seconds_per_day) / Constants.SECONDS_PER_HOUR;
				long minutes = (seconds % Constants.SECONDS_PER_HOUR) / Constants.SECONDS_PER_MINUTE;

				string hhmm = hours.ToString("00") + ":" + minutes.ToString("00");

				if (days == 0)
				{
					return hhmm;
				}
				else if (days < 1000)
				{
					return days.ToString("0") + "d " + hhmm;
				}
				else if (days >= 1000)
				{
					return (days / 1000).ToString("0") + "kd " + hhmm;
				}
				else
				{
					return (days / 1000000).ToString("0") + "md " + hhmm;
				}
			}
		}
	}
}