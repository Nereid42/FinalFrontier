﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nereid.FinalFrontier
{
	public class ExternalInterface
	{
		private const string VERSION_INFIX = "\"VERSION\":";
		private const string MAJOR_INFIX = "\"MAJOR\":";
		private const string MINOR_INFIX = "\"MINOR\":";
		private const string PATCH_INFIX = "\"PATCH\":";
		private const string BUILD_INFIX = "\"BUILD\":";

		public ExternalInterface()
		{
			// no member, no init
		}

		public string GetVersionNumber(string s, int index)
		{
			int e1 = s.IndexOf(",", index, StringComparison.Ordinal);
			int e2 = s.IndexOf("}", index, StringComparison.Ordinal);
			if (e1 < e2)
			{
				return s.Substring(index, e1 - index);
			}
			return s.Substring(index, e2 - index);
		}

		public string GetVersion()
		{
			string filename = Utils.GetRootPath() + "/GameData/Nereid/FinalFrontier/FinalFrontier.version";
			if (File.Exists(filename))
			{
				try
				{
					string text = File.ReadAllText(filename);
					int pVersion = text.IndexOf(VERSION_INFIX);
					if (pVersion < 0) return "invalid file structure";
					int pMajor = text.IndexOf(MAJOR_INFIX, pVersion);
					if (pMajor < 0) return "invalid file structure";
					int pMinor = text.IndexOf(MINOR_INFIX, pVersion);
					if (pMinor < 0) return "invalid file structure";
					int pPatch = text.IndexOf(PATCH_INFIX, pVersion);
					if (pPatch < 0) return "invalid file structure";
					int pBuild = text.IndexOf(BUILD_INFIX, pVersion);
					if (pBuild < 0) return "invalid file structure";
					string major = GetVersionNumber(text, pMajor + MAJOR_INFIX.Length);
					string minor = GetVersionNumber(text, pMinor + MINOR_INFIX.Length);
					string patch = GetVersionNumber(text, pPatch + PATCH_INFIX.Length);
					string build = GetVersionNumber(text, pBuild + BUILD_INFIX.Length);
					return major + "." + minor + "." + patch + "-" + build;
				}
				catch (Exception e)
				{
					Log.Warning("failed to read version file: " + e.GetType() + " (" + e.Message + ")");
					return "failed to read version file";
				}
			}
			return "file not found";
		}

		public Ribbon RegisterRibbon(string code, string pathToRibbonTexture, string name, string description, bool first,
			int prestige)
		{
			return RibbonPool.Instance().RegisterExternalRibbon(code, pathToRibbonTexture, name, description, first, prestige);
		}

		public Ribbon RegisterCustomRibbon(int id, string pathToRibbonTexture, string name, string description, int prestige)
		{
			return RibbonPool.Instance().RegisterCustomRibbon(id, pathToRibbonTexture, name, description, prestige);
		}

		public void AwardRibbonToKerbal(object ribbon, ProtoCrewMember kerbal)
		{
			HallOfFame.Instance().Record(kerbal, (Ribbon)ribbon);
		}

		public void AwardRibbonToKerbal(string code, ProtoCrewMember kerbal)
		{
			Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
			if (ribbon == null)
			{
				Log.Error("no ribbon for code '" + code + "' found!");
				return;
			}
			AwardRibbonToKerbal((object)ribbon, kerbal);
		}

		public void AwardRibbonToKerbals(object ribbon, ProtoCrewMember[] kerbals)
		{
			HallOfFame halloffame = HallOfFame.Instance();
			halloffame.BeginArwardOfRibbons();
			foreach (ProtoCrewMember kerbal in kerbals)
			{
				halloffame.Record(kerbal, (Ribbon)ribbon);
			}
			halloffame.EndArwardOfRibbons();
		}

		public void AwardRibbonToKerbals(string code, ProtoCrewMember[] kerbals)
		{
			Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
			if (ribbon == null)
			{
				Log.Error("no ribbon for code '" + code + "' found!");
				return;
			}
			AwardRibbonToKerbals(ribbon, kerbals);
		}

		public bool IsRibbonAwardedToKerbal(object ribbon, ProtoCrewMember kerbal)
		{
			HallOfFame halloffame = HallOfFame.Instance();
			HallOfFameEntry entry = halloffame.GetEntry(kerbal);
			if (entry == null) return false;
			return entry.HasRibbon((Ribbon)ribbon);
		}

		public bool IsRibbonAwardedToKerbal(string code, ProtoCrewMember kerbal)
		{
			Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
			if (ribbon == null) return false;
			return IsRibbonAwardedToKerbal((object)ribbon, kerbal);
		}


		public int GetMissionsFlownForKerbal(ProtoCrewMember kerbal)
		{
			HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
			if (entry != null)
			{
				return entry.MissionsFlown;
			}
			return 0;
		}

		public int GetDockingsForKerbal(ProtoCrewMember kerbal)
		{
			HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
			if (entry != null)
			{
				return entry.Dockings;
			}
			return 0;
		}

		public int GetContractsCompletedForKerbal(ProtoCrewMember kerbal)
		{
			HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
			if (entry != null)
			{
				return entry.ContractsCompleted;
			}
			return 0;
		}

		public double GetResearchForKerbal(ProtoCrewMember kerbal)
		{
			HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
			if (entry != null)
			{
				return entry.Research;
			}
			return 0;
		}

		public double GetTotalMissionTimeForKerbal(ProtoCrewMember kerbal)
		{
			HallOfFameEntry entry = HallOfFame.Instance().GetEntry(kerbal);
			if (entry != null)
			{
				return entry.TotalMissionTime;
			}
			return 0;
		}
	}
}