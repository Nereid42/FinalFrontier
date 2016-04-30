﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace Nereid
{
	namespace FinalFrontier
	{
		class CodeBrowser : AbstractWindow
		{
			private Vector2 scrollPosition = Vector2.zero;

			public static int WIDTH = 350;
			public static int HEIGHT = 500;

			private static readonly GUIStyle STYLE_CODE = new GUIStyle(HighLogic.Skin.label);
			private static readonly GUIStyle STYLE_NAME = new GUIStyle(HighLogic.Skin.label);

			private List<Pair<string, string>> codes = new List<Pair<string, string>>();


			public CodeBrowser()
				: base(Constants.WINDOW_ID_CODEBROWSER, "Ribbon Codes")
			{
				STYLE_CODE.stretchWidth = false;
				STYLE_CODE.fixedWidth = 100;
				STYLE_CODE.alignment = TextAnchor.MiddleLeft;
				STYLE_NAME.stretchWidth = true;
				STYLE_NAME.alignment = TextAnchor.MiddleLeft;
				//
				// copy activitiesfor sorting into code list
				Log.Detail("adding action codes to code browser " + ActionPool.Instance());
				foreach (Activity activity in ActivityPool.Instance())
				{
					Pair<string, string> code = new Pair<string, string>(activity.GetCode(), activity.GetName());
					codes.Add(code);
				}

				// sort by code
				Log.Detail("sorting codes in code browser");
				codes.Sort(
					delegate (Pair<string, string> left, Pair<string, string> right) { return left.first.CompareTo(right.first); });
			}

			protected override void OnWindow(int id)
			{
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Close", FFStyles.STYLE_BUTTON)) SetVisible(false);
				GUILayout.EndHorizontal();
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, FFStyles.STYLE_SCROLLVIEW, GUILayout.Height(HEIGHT));
				GUILayout.BeginVertical();
				foreach (Pair<string, string> entry in codes)
				{
					string code = entry.first;
					string name = entry.second;
					GUILayout.BeginHorizontal();
					GUILayout.Label(code, STYLE_CODE);
					GUILayout.Label(name, STYLE_NAME);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndVertical();

				DragWindow();
			}

			public override int GetInitialWidth()
			{
				return WIDTH;
			}
		}
	}
}