﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Nereid
{
	namespace FinalFrontier
	{
		public abstract class Adapter
		{
			private bool isInstalled = false;

			public bool IsInstalled()
			{
				return isInstalled;
			}

			protected void SetInstalled(bool installed)
			{
				isInstalled = installed;
			}

			protected Type GetType(string name)
			{
				return AssemblyLoader.loadedAssemblies
					.SelectMany(x => x.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == name);
			}

			protected bool IsTypeLoaded(string name)
			{
				return GetType(name) != null;
			}

			public abstract void TryInstallPlugin();
		}

		public class FARAdapter : Adapter
		{
			private MethodInfo methodGetMachNumber;

			public override void TryInstallPlugin()
			{
				try
				{
					Type type = GetType("ferram4.FARAeroUtil");
					if (type != null)
					{
						methodGetMachNumber = type.GetMethod("GetMachNumber", BindingFlags.Public | BindingFlags.Static, null,
							new Type[] { typeof(CelestialBody), typeof(double), typeof(Vector3d) }, null);
						if (methodGetMachNumber != null)
						{
							SetInstalled(true);
						}
					}
				}
				catch (Exception e)
				{
					Log.Error("Failed to access F.A.R. plugin; exception: " + e.GetType() + " - " + e.Message);
					SetInstalled(false);
				}
			}

			public double GetMachNumber(CelestialBody body, double altitude, Vector3d velocity)
			{
				if (IsInstalled())
				{
					return (double)methodGetMachNumber.Invoke(null, new object[] { body, altitude, velocity });
				}
				return 0.0;
			}
		}
	}
}