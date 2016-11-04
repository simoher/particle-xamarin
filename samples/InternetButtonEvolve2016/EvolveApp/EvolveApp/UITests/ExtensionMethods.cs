﻿using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Linq.Expressions;

using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

using NUnit.Framework;

namespace UITests
{
	public static class Extensions
	{
		public static void ScrollDownAndTap(this AndroidApp app, Func<AppQuery, AppQuery> lambda = null, string screenshot = null)
		{
			app.ScrollDownEnough(lambda);
			app.Tap(lambda);

			if (screenshot != null)
				app.Screenshot(screenshot);
		}

		public static void ScrollDownAndTap(this AndroidApp app, string screenshot, Func<AppQuery, AppQuery> lambda = null)
		{
			app.ScrollDownEnough(lambda);

			if (screenshot != null)
				app.Screenshot(screenshot);

			app.Tap(lambda);
		}

		public static bool GetHeightWidth(this IApp app, Func<AppQuery, AppQuery> query, out int outX, out int outY)//, int scrollNumberLimit, AppResult result)
		{
			AppResult[] queryResult = app.Query(query);

			outX = 0;
			outY = 0;

			if (queryResult != null)
			{
				AppResult result = queryResult[0];
				var viewBounds = result.Rect;
				outX = Convert.ToInt32(viewBounds.Width);
				outY = Convert.ToInt32(viewBounds.Height);
				//outX = Convert.ToInt32( ((int)viewBounds.CenterX / (float)(.857)));
				//outY = Convert.ToInt32(((int)viewBounds.CenterY / (float).845));
			}

			return true;
		}

		/// <summary>
		/// Incrementally scrolls down until the desired elements are found
		/// </summary>
		public static AppResult[] ScrollDownEnough(this AndroidApp app, Func<AppQuery, AppQuery> lambda, string screenshot = null)
		{
			AppResult rootView = null;
			int count = 0;
			int maxTries = 20;

			AppResult[] lastTry;
			while (count < maxTries)
			{
				lastTry = app.Query(lambda);

				if (lastTry.Any())
				{
					if (screenshot != null)
						app.Screenshot(screenshot);

					return lastTry;
				}

				if (rootView == null)
				{
					rootView = app.Query(e => e.All()).FirstOrDefault();

					if (rootView == null)
						throw new Exception("Unable to get root view");
				}

				//Will try to scroll +/-100 from the vertical center point
				float gap = 100;

				//Take into account where the screen is not large and the gap would be too big
				if (rootView.Rect.Height < gap * 2)
					gap = rootView.Rect.Height / 4;

				app.DragCoordinates(rootView.Rect.CenterX, rootView.Rect.CenterY + gap, rootView.Rect.CenterX, rootView.Rect.CenterY - gap);
				count++;
			}

			if (count == maxTries)
			{
				throw new Exception("Unable to scroll down to find element");
			}

			return new AppResult[0];
		}

		public static void WaitThenEnterText(this IApp app, Func<AppQuery, AppQuery> lambda, string text, string screenshot = null)
		{
			app.WaitForElement(lambda);
			app.EnterText(lambda, text);

			if (screenshot != null)
				app.Screenshot(screenshot);
		}

		public static void EnterText(this IApp app, Func<AppQuery, AppQuery> lambda, string text, string screenshot)
		{
			app.EnterText(lambda, text);
			app.Screenshot(screenshot);
		}


		public static void Tap(this IApp app, string screenshot, Func<AppQuery, AppQuery> lambda)
		{
			app.Screenshot(screenshot);
			app.Tap(lambda);
		}

		public static void Tap(this IApp app, Func<AppQuery, AppQuery> lambda, string screenshot)
		{
			app.Tap(lambda);
			app.Screenshot(screenshot);
		}

		public static void WaitThenTapIfExists(this IApp app, Func<AppQuery, AppQuery> lambda, int timeout = 5, string screenshot = null)
		{
			int count = 0;

			while (count < timeout && app.Query(lambda).Length == 0)
			{
				Thread.Sleep(1000);
				count++;
			}

			if (app.Query(lambda).Length > 0)
			{
				if (screenshot != null)
					app.Screenshot(screenshot);

				app.Tap(lambda);
			}
		}

		public static void WaitThenTap(this IApp app, string screenshot, Func<AppQuery, AppQuery> lambda, int seconds = 20)
		{
			app.WaitForElement(lambda, "Timed out waiting for element", TimeSpan.FromSeconds(seconds));

			if (screenshot != null)
				app.Screenshot(screenshot);

			app.Tap(lambda);
		}

		public static void WaitThenTap(this IApp app, Func<AppQuery, AppQuery> lambda, string screenshot = null, int seconds = 20)
		{
			app.WaitForElement(lambda);
			app.Tap(lambda);

			if (screenshot != null)
				app.Screenshot(screenshot);
		}

		public static string ToString(this AppResult[] result, bool repl)
		{
			var sb = new StringBuilder();
			var index = 0;

			foreach (var res in result)
			{
				var innerSb = new StringBuilder();
				innerSb.AppendLine("{");
				innerSb.AppendLine(string.Format("    Index         - {0}", index));
				innerSb.AppendLine(string.Format("    Class         - {0}", res.Class));
				innerSb.AppendLine(string.Format("    Description   - {0}", res.Description));

				if (res.Text != null)
					innerSb.AppendLine(string.Format("    Text           - {0}", res.Text));

				innerSb.AppendLine(string.Format("    ID            - {0}", res.Id));
				innerSb.AppendLine(string.Format("    Rect          - {0} x {1}, {2} x {3}", res.Rect.X, res.Rect.Y, res.Rect.Width, res.Rect.Height));
				innerSb.AppendLine("}");
				innerSb.AppendLine("");

				sb.Append(innerSb.ToString());
				index++;
			}

			return sb.ToString();
		}

		//--------------------------------------------------------------

		public static bool IsTextDifferent(string oldText, string newText)
		{
			bool bReturn = false;

			if (string.IsNullOrEmpty(oldText) == false && string.IsNullOrEmpty(newText) == false)
			{
				if (string.Compare(oldText, newText) != 0)
				{
					bReturn = true;
				}

			}

			return bReturn;
		}

		public static void WaitForThenGetTextOfIndexAndCompare(this IApp app, Func<AppQuery, AppQuery> lambda, int index, string sExpected, string screenShotText)
		{
			string sActual = string.Empty;
			WaitFor(app, lambda);
			if (app.Query(lambda).Length >= index)
			{
				sActual = app.Query(lambda)[index].Text;
				CompareText(sActual, sExpected);
				Screenshot(app, screenShotText);
			}
		}

		private static void CompareText(string sExpected, string sActual)
		{
			if (string.IsNullOrEmpty(sActual) == false && string.IsNullOrEmpty(sExpected) == false)
			{
				if (String.Compare(sExpected, sActual) != 0)
				{ //not equal
					Assert.Fail(string.Format("TEST FAILURE. Expecting: {0} , Actual: {1}", sExpected, sActual));
				}

			}
		}

		public static void WaitForThenTap(this IApp app, Func<AppQuery, AppQuery> lambda, string screenShotText = null)
		{
			WaitFor(app, lambda);
			app.Tap(lambda);
			if (!String.IsNullOrEmpty(screenShotText))
				Screenshot(app, screenShotText);
		}

		public static void WaitFor(this IApp app, Func<AppQuery, AppQuery> lambda)//, string screenShotText)
		{
			app.WaitForElement(lambda, "Timed out after 30 seconds", new TimeSpan(0, 0, 0, 30, 0), null, null);
			//Screenshot(app, screenShotText);
		}

		public static void WaitForThenEnterText(this IApp app, Func<AppQuery, AppQuery> lambda, string text, string screenShotText)
		{
			WaitFor(app, lambda);
			app.EnterText(lambda, text);
			Screenshot(app, screenShotText);
		}

		public static void WaitForThenClearText(this IApp app, Func<AppQuery, AppQuery> lambda)
		{
			WaitFor(app, lambda);
			app.DoubleTap(lambda);
			app.ClearText();
		}

		public static void Screenshot(this IApp app, string screenShotText)
		{
			if (screenShotText.Length > 0)
			{
				app.Screenshot(screenShotText);
			}
		}

		public static bool IsItThere(this IApp app, Func<AppQuery, AppQuery> lambda)
		{
			bool bIsThere = false;

			WaitFor(app, lambda);

			if (app.Query(lambda).Length > 0)
			{
				bIsThere = true;
			}

			return bIsThere;
		}

		public static void IsItThere(this IApp app, Func<AppQuery, AppQuery> lambda, string screenShotText)
		{
			bool bIsThere = false;

			WaitFor(app, lambda);
			Screenshot(app, screenShotText);

			if (app.Query(lambda).Length <= 0)
			{
				Assert.Fail(string.Format("FAILURE: {0}"), screenShotText);
			}

		}


	}
}