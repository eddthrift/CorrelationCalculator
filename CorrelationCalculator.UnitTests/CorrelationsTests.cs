using Extreme.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CorrelationCalculator.UnitTests
{
    [TestClass]
    public class CorrelationsTests
    {
        // Number of items in the random list
        private int numberOfDataPoints = 10;
        // Number of decimal places that constitutes acceptable accuracy
        private int numberOfDpAccuracy = 8;

        [TestMethod]
        public void Calculate_RandomFile_ReturnsCorrectPearson()
        {
            // Create random lists to feed into test
            IEnumerable<decimal> listA = CreateRandomList(numberOfDataPoints);
            IEnumerable<decimal> listB = CreateRandomList(numberOfDataPoints);
            double[] arrayA = listA.Select(dec => (double)dec).ToArray();
            double[] arrayB = listB.Select(dec => (double)dec).ToArray();

            // Use Extreme.Statistics Correlation method to test result against
            double pearsonActual = Stats.Correlation(arrayA, arrayB);
            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(Math.Round(pearsonActual, numberOfDpAccuracy), Math.Round((double)Correlations.pearson, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_PerfectlyCorrelatedFile_ReturnsCorrectPearson()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(1, Math.Round((double)Correlations.pearson, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_AntiCorrelatedFile_ReturnsCorrectPearson()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(11 - i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(-1, Math.Round((double)Correlations.pearson, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_RandomFile_ReturnsCorrectSpearman()
        {
            // Create random lists to feed into test
            IEnumerable<decimal> listA = CreateRandomList(numberOfDataPoints);
            IEnumerable<decimal> listB = CreateRandomList(numberOfDataPoints);
            double[] arrayA = listA.Select(dec => (double)dec).ToArray();
            double[] arrayB = listB.Select(dec => (double)dec).ToArray();
            
            // Use Extreme.Statistics RankCorrelation method to test result against
            double spearmanActual = Stats.RankCorrelation(arrayA, arrayB);
            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());
           
            // Check answer
            Assert.AreEqual(Math.Round(spearmanActual, numberOfDpAccuracy), Math.Round((double)Correlations.spearman, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_PerfectlyCorrelatedFile_ReturnsCorrectSpearman()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(1, Math.Round((double)Correlations.spearman, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_AntiCorrelatedFile_ReturnsCorrectSpearman()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(11 - i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(-1, Math.Round((double)Correlations.spearman, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_RandomFile_ReturnsCorrectKendall()
        {
            // Create random lists to feed into test
            IEnumerable<decimal> listA = CreateRandomList(numberOfDataPoints);
            IEnumerable<decimal> listB = CreateRandomList(numberOfDataPoints);

            // Use Extreme.Statistics Kendall method to test result against
            double kendallActual = Stats.KendallTau(listA.ToList(), listB.ToList());

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(Math.Round(kendallActual, numberOfDpAccuracy), Math.Round((double)Correlations.kendall, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_PerfectlyCorrelatedFile_ReturnsCorrectKendall()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for(int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(1, Math.Round((double)Correlations.kendall, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_AntiCorrelatedFile_ReturnsCorrectKendall()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(11-i);
            }

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Check answer
            Assert.AreEqual(-1, Math.Round((double)Correlations.kendall, numberOfDpAccuracy));
        }

        [TestMethod]
        public void Calculate_FileWithDuplicates_ReturnsCorrectKendall()
        {
            // Create lists to feed into test
            IEnumerable<decimal> listA = new List<decimal>();
            IEnumerable<decimal> listB = new List<decimal>();

            for (int i = 1; i <= 10; i++)
            {
                listA = listA.Append(i);
                listB = listB.Append(11 - i);
            }

            // Add some duplicated numbers to the list
            listA = listA.Append(2);
            listB = listB.Append(5);
 
            listA = listA.Append(7);
            listB = listB.Append(3);

            listA = listA.Append(1);
            listB = listB.Append(11);

            listA = listA.Append(11);
            listB = listB.Append(6);

            // Calculate using CorrelationCalculator
            Correlations.Calculate("A", listA.ToList(), "B", listB.ToList());

            // Use Extreme.Statistics Kendall method to test result against
            double kendallActual = Stats.KendallTau(listA.ToList(), listB.ToList());

            // Check answer
            Assert.AreEqual(Math.Round(kendallActual, numberOfDpAccuracy), Math.Round((double)Correlations.kendall, numberOfDpAccuracy));
        }

        /// <summary>
        ///     Creates a list of random decimals
        /// </summary>
        /// <param name="n"> The length of the list </param>
        /// <returns> IList<decimal> of random numbers </returns>
        private IList<decimal> CreateRandomList(int n)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            IList<decimal> randomList = new List<decimal>();
            
            for(int i = 0; i < n; i++)
            {
                randomList.Add((decimal)(rand.NextDouble() * 100));
            }

            return randomList;
        }
    }
}
