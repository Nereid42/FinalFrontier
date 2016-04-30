using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nereid
{
	namespace FinalFrontier
	{
		// this class is not used yet
		public class RibbonSupersedeChain
		{
			private static readonly string ROOT_PATH = Utils.GetRootPath();
			private static readonly string PATH = ROOT_PATH + "/GameData/Nereid/FinalFrontier";
			private static readonly string FILE_NAME = "supersede.cfg";

			Dictionary<Ribbon, Ribbon> chain = new Dictionary<Ribbon, Ribbon>();

			public void Load()
			{
				Load(PATH + "/" + FILE_NAME);
			}

			public void Load(string filename)
			{
				StreamReader file = null;
				try
				{
					file = File.OpenText(filename);
					string line;
					while ((line = file.ReadLine()) != null)
					{
						string[] tokens = line.Split(' ');
						if (tokens.Count() > 1)
						{
							string code = tokens[0];
							string supersede = tokens[1];
							Ribbon ribbon = RibbonPool.Instance().GetRibbonForCode(code);
							if (ribbon == null) continue;
							Ribbon super = RibbonPool.Instance().GetRibbonForCode(supersede);
							if (super == null) continue;
							chain.Add(ribbon, super);
						}
					}
				}
				finally
				{
					if (file != null) file.Close();
				}
			}
		}
	}
}