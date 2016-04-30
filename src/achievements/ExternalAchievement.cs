using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid.FinalFrontier
{
	class ExternalAchievement : Achievement
	{
		readonly string description;

		public ExternalAchievement(string code, string name, int prestige, bool first, string description)
			: base(code, name, prestige, first)
		{
			this.description = description;
		}

		public override string GetDescription()
		{
			return description;
		}
	}
}