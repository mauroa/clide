﻿namespace Clide
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class Misc : VsHostedSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingHierarchyItem_ThenAlwaysGetsSameInstance()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");
            var projectId = new Guid("{883173CE-9FDE-4436-96F5-128EA8A471BE}");

            var solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            var solutionHashCode = solution.GetHashCode();
            
            IVsHierarchy hierarchy1;
            IVsHierarchy hierarchy2;

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy1));
            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.IsNotNull(hierarchy1);
            Assert.IsNotNull(hierarchy2);

            Assert.AreSame(hierarchy1, hierarchy2);

            var hashCodes = new List<int>(new[] { hierarchy1.GetHashCode(), hierarchy2.GetHashCode() });

            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            Assert.AreEqual(solutionHashCode, solution.GetHashCode());

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.AreNotSame(hierarchy1, hierarchy2);

            Assert.IsFalse(hashCodes.Contains(hierarchy2.GetHashCode()));
        }
    }
}