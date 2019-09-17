using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Extreme.Statistics;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
using CorrelationCalculator;

namespace CorrelationCalculator.UnitTests
{
    [TestClass]
    public class CorrelationsTests
    { 
        [TestMethod]
        public void Calculate_RandomFile_ReturnsCorrectValues()
        {
            int n = 10000;
            IEnumerable<decimal> listA = CreateRandomList(n);
            IEnumerable<decimal> listB = CreateRandomList(n);

            double kendall = Stats.KendallTau(listA.ToList(),listB.ToList());
            double pearson = Correlation.Pearson(listA.Select(dec => (double)dec), listB.Select(dec => (double)dec));
            double spearman = Correlation.Spearman(listA.Select(dec => (double)dec), listB.Select(dec => (double)dec));

            
        }

        private IList<decimal> CreateRandomList(int n)
        {
            Random rand = new Random();
            IList<decimal> randomList = new List<decimal>();
            
            for(int i = 0; i < n; i++)
            {
                randomList.Add((decimal)(rand.NextDouble() * 100));
            }

            return randomList;
        }
    }
}
