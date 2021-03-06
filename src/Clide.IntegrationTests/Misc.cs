﻿using System;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;
using Xunit.Abstractions;

namespace Clide
{
	public class Misc
	{
		ITestOutputHelper output;

		public Misc (ITestOutputHelper output)
		{
			this.output = output;
		}

		[VsixFact]
		public void when_reopening_solution_then_vssolution_is_same ()
		{
			var dte = GlobalServices.GetService<DTE>();
			var solutionEmpty = GlobalServices.GetService<SVsSolution, IVsSolution>();

			dte.Solution.Open (new FileInfo (Constants.SingleProjectSolution).FullName);

			var solution1 = GlobalServices.GetService<SVsSolution, IVsSolution>();

			dte.Solution.Close ();
			dte.Solution.Open (new FileInfo (Constants.BlankSolution).FullName);

			var solution2 = GlobalServices.GetService<SVsSolution, IVsSolution>();

			Assert.Same (solutionEmpty, solution1);
			Assert.Same (solution1, solution2);
			Assert.True (ComUtilities.IsSameComObject (solutionEmpty as IVsHierarchy, solution1 as IVsHierarchy));
			Assert.True (ComUtilities.IsSameComObject (solution1 as IVsHierarchy, solution2 as IVsHierarchy));
		}

		[VsixFact (RunOnUIThread = true)]
		public void when_reopenening_solution_then_hierarchy_item_is_same ()
		{
			var dte = GlobalServices.GetService<DTE>();
			var solutionEmpty = GlobalServices.GetService<SVsSolution, IVsSolution>();
			var manager = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<IVsHierarchyItemManager>();

			var solutionEmptyItem = manager.GetHierarchyItem (solutionEmpty as IVsHierarchy, (uint)VSConstants.VSITEMID.Root);
			Assert.NotNull (solutionEmptyItem);

			dte.Solution.Open (new FileInfo (Constants.SingleProjectSolution).FullName);

			var solution1 = GlobalServices.GetService<SVsSolution, IVsSolution>();
			var solution1Item = manager.GetHierarchyItem (solution1 as IVsHierarchy, (uint)VSConstants.VSITEMID.Root);

			dte.Solution.Close ();

			dte.Solution.Open (new FileInfo (Constants.BlankSolution).FullName);

			var solution2 = GlobalServices.GetService<SVsSolution, IVsSolution>();
			var solution2Item = manager.GetHierarchyItem (solution2 as IVsHierarchy, (uint)VSConstants.VSITEMID.Root);

			Assert.NotNull (solution1Item);
			Assert.NotNull (solutionEmptyItem);
			Assert.NotNull (solution2Item);

			Assert.Equal (solutionEmptyItem.HierarchyIdentity, solution1Item.HierarchyIdentity);
			Assert.Equal (solution1Item.HierarchyIdentity, solution2Item.HierarchyIdentity);
		}

		//[VsixFact (Skip = "Testing capabilities")]
		public void when_requesting_project_capabilities_then_succeeds ()
		{
			var dte = GlobalServices.GetService<DTE>();
			var sln = (Solution2)dte.Solution;

			var template = sln.GetProjectTemplate("Microsoft.CSharp.WPFApplication", "CSharp");
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory (tempDir);

			sln.AddFromTemplate (template, tempDir, "MyLibrary");

			var proj = dte.Solution.Projects.OfType<Project>().First();

			var vs = GlobalServices.GetService<SVsSolution, IVsSolution> ();
			IVsHierarchy vsproj;

			ErrorHandler.ThrowOnFailure (vs.GetProjectOfUniqueName (proj.UniqueName, out vsproj));

			object capabilities;

			ErrorHandler.ThrowOnFailure (vsproj.GetProperty ((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities, out capabilities));

			output.WriteLine (capabilities.ToString ());
		}
	}
}