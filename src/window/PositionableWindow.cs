using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
	namespace FinalFrontier
	{
		public abstract class PositionableWindow : AbstractWindow
		{
			private static readonly LinkedList<PositionableWindow> INSTANCES = new LinkedList<PositionableWindow>();

			// flag if this window was opened before; the initial postion will only be set on the first opening
			private bool opened = false;

			protected PositionableWindow(int id, string title, int initialX = 0, int initialY = 0) : base(id, title)
			{
				INSTANCES.AddLast(this);
			}

			public static void ResetAllWindowPositions()
			{
				if (FinalFrontier.Config != null)
				{
					FinalFrontier.Config.ResetWindowPositions();
				}
				foreach (PositionableWindow window in INSTANCES)
				{
					window.Reset();
				}
			}

			protected void Reset()
			{
				opened = IsVisible();
				if (FinalFrontier.Config != null)
				{
					SetPosition(FinalFrontier.Config.GetWindowPosition(this));
				}
			}

			protected override void OnDrawFinished(int id)
			{
				if (FinalFrontier.Config != null)
				{
					FinalFrontier.Config.SetWindowPosition(this);
				}
				else
				{
					Log.Warning("PositionableWindow:: no Config created, cant store window position");
				}
			}

			public void SetPosition(Pair<int, int> position)
			{
				SetPosition(position.first, position.second);
			}

			protected override void OnOpen()
			{
				Log.Trace("positionable window " + GetWindowId() + " opened");
				if (!opened)
				{
					if (FinalFrontier.Config != null)
					{
						Log.Detail("first opening of window " + GetWindowId());
						Pair<int, int> position = FinalFrontier.Config.GetWindowPosition(GetWindowId());
						if (position != null)
						{
							SetPosition(position.first, position.second);
						}
						else
						{
							Log.Warning("no initial position found for window " + GetWindowId() + " while opening");
						}
						opened = true;
					}
					else
					{
						Log.Warning("PositionableWindow:: no Config created");
					}
				}
			}
		}
	}
}