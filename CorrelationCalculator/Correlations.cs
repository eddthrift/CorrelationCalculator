using System;
using System.Collections.Generic;
using System.Linq;

namespace CorrelationCalculator
{
    /// <summary>
    ///     Class to hold the correlation calculations.
    /// </summary>
    static class Correlations
    {
        public static decimal pearson;
        public static decimal spearman;
        public static decimal kendall;

        private static decimal covariance;
        private static decimal xBar;
        private static decimal yBar;
        private static decimal stDevX;
        private static decimal stDevY;

        private static decimal rankCovariance;
        private static decimal xRankBar;
        private static decimal yRankBar;
        private static decimal stDevXRank;
        private static decimal stDevYRank;

        private static IList<decimal> setX;
        private static IList<decimal> setY;

        private static IList<RankedDatum> rankedSetX;
        private static IList<RankedDatum> rankedSetY;

        /// <summary>
        ///     Calculates and writes the results of the correlation calculations.
        /// </summary>
        /// <param name="firstColumn"> The first column of data. </param>
        /// <param name="secondColumn"> The second column of data. </param>
        public static void Calculate(string firstHeader, IList<decimal> firstColumn, string secondHeader, IList<decimal> secondColumn)
        {
            // Load data into class fields
            setX = firstColumn;
            setY = secondColumn;

            // Rank data and load into fields
            rankedSetX = RankDataSet(setX);
            rankedSetY = RankDataSet(setY);

            // Calculate and output the statistics
            CalculateCovarianceAndStDev();
            PearsonCoefficient();
            SpearmanCoefficient();
            KendallCoefficient();

            Console.WriteLine("Calculating statistics for " + firstHeader + " and " + secondHeader + "\n");
            Console.WriteLine("The Pearson Coefficient is: " + pearson.ToString());
            Console.WriteLine("The Spearman Coefficient is: " + spearman.ToString());
            Console.WriteLine("The Kendall Coefficient is: " + kendall.ToString() + "\n");
        }

        /// <summary>
        ///     Ranks the data sets and returns a list of the data and its corresponding rank.
        /// </summary>
        /// <param name="dataSet"> the data set to rank </param>
        /// <returns> IList<RankedDatum> of the variables with corresponding ranks </RankedDatum> </returns>
        private static IList<RankedDatum> RankDataSet(IList<decimal> dataSet)
        {
            IList<RankedDatum> rankedSet = new List<RankedDatum>();
            List<decimal> orderedSet = dataSet.OrderBy(value => value).ToList();

            // Rank each datum in dataSet
            foreach (decimal datum in dataSet)
            {
                // Find the index (+1) of the matching value to find rank
                decimal rank = orderedSet.FindIndex(value => value == datum) + 1;
                rankedSet.Add(new RankedDatum(datum, rank));
            }

            // Need to account for tied values in the data, set tied values to the mean of their sequential ranks
            for (int i = 1; i <= rankedSet.Count; i++)
            {
                int count = rankedSet.Where(datum => datum.Rank == i).Count();
                decimal mean;

                // Can calculate the mean, but summing the sequential ranks and dividing by count. However in logic above all ranks are given as the lowest number.
                // Sum of sequential ranks (e.g. 5+6+7) can be found by summing the previously assigned ranks (5+5+5) and adding the triangular number of order (count - 1).
                if (count > 1)
                {
                    int triangularOrder = count - 1;
                    decimal triangularNumber = (triangularOrder * (triangularOrder + 1)) / 2;

                    // To calculate the mean we divide the sum by the number of tied data
                    mean = ((count * i) + triangularNumber) / count;

                    foreach (RankedDatum rankedDatum in rankedSet.Where(datum => datum.Rank == i))
                    {
                        rankedDatum.Rank = mean;
                    }
                }
            }

            return rankedSet;
        }

        /// <summary>
        ///     Calculates the values used in correlation calculations.
        /// </summary>
        private static void CalculateCovarianceAndStDev()
        {
            decimal stDevXSquared = 0;
            decimal stDevYSquared = 0;
            decimal newCovariance = 0;

            // Calculate the mean of each dataset
            xBar = setX.Average();
            yBar = setY.Average();

            for (int i = 0; i < setX.Count; i++)
            {
                // Calculate (xi - xbar) for all data in setX and setY
                decimal varX = setX[i] - xBar;
                decimal varY = setY[i] - yBar;

                // Calculate contribution to covariance and stDev for each data point
                newCovariance += varX * varY;
                stDevXSquared += varX * varX;
                stDevYSquared += varY * varY;
            }

            covariance = newCovariance;
            stDevX = Sqrt(stDevXSquared);
            stDevY = Sqrt(stDevYSquared);
        }

        /// <summary>
        ///     Calculates the Pearson coefficient of the given dataset. 
        ///     Pearson coefficient is given by the covariance divided by the multiple of the standard deviations of each data set
        /// </summary>
        private static void PearsonCoefficient()
        {
            pearson = covariance / (stDevX * stDevY);
        }

        /// <summary>
        ///     Calculates the Spearman coefficient of the given dataset.
        /// </summary>
        private static void SpearmanCoefficient()
        {
            // Spearman correlation coefficient is the Pearson coefficient of the ranked variables.
            CalculateRankCovarianceAndStDev(rankedSetX.Select(datum => datum.Rank).ToList(), rankedSetY.Select(datum => datum.Rank).ToList());

            // Spearman coefficient is the Pearson coefficient of the data ranks
            spearman = rankCovariance / (stDevXRank * stDevYRank);
        }

        /// <summary>
        ///     Calculates the values used in spearman calculations.
        /// </summary>
        /// <param name="setX"> The ranks of the first data set </param>
        /// <param name="setY"> The ranks of the second data set </param>
        private static void CalculateRankCovarianceAndStDev(IList<decimal> setXRank, IList<decimal> setYRank)
        {
            decimal stDevXRankSquared = 0;
            decimal stDevYRankSquared = 0;
            decimal newRankCovariance = 0;

            // Calculate the mean of each dataset
            xRankBar = setXRank.Average();
            yRankBar = setYRank.Average();

            for (int i = 0; i < setX.Count; i++)
            {
                // Calculate (xi - xbar) for all data in setX and setY
                decimal varXRank = setXRank[i] - xRankBar;
                decimal varYRank = setYRank[i] - yRankBar;

                // Calculate contribution to covariance and stDev for each data point
                newRankCovariance += varXRank * varYRank;
                stDevXRankSquared += varXRank * varXRank;
                stDevYRankSquared += varYRank * varYRank;
            }

            rankCovariance = newRankCovariance;
            stDevXRank = Sqrt(stDevXRankSquared);
            stDevYRank = Sqrt(stDevYRankSquared);
        }

        /// <summary>
        ///     Calculates the Tau - a Kendall coefficient of the given dataset.
        /// </summary>
        private static void KendallCoefficient()
        {
            IList<Tuple<RankedDatum, RankedDatum>> pairedData = new List<Tuple<RankedDatum, RankedDatum>>();


            // Create paired data list and order by set X
            for (int i = 0; i < rankedSetX.Count; i++)
            {
                pairedData.Add(new Tuple<RankedDatum, RankedDatum>(rankedSetX[i], rankedSetY[i]));
            }

            pairedData = pairedData.OrderBy(dataPair => dataPair.Item1.Rank).ToList();

            // Initialise variables to count the number of concordant and discordant pairs
            decimal c = 0;
            decimal d = 0;
            int n = pairedData.Count();

            // Ties are neither concordant or discordant
            for (int i = 0; i < pairedData.Count; i++)
            {
                c += pairedData.Skip(i + 1).Where(pair => pair.Item2.Rank > pairedData[i].Item2.Rank).Count();
                d += pairedData.Skip(i + 1).Where(pair => pair.Item2.Rank < pairedData[i].Item2.Rank).Count();
            }

            // Kendalls tau a = (C – D) / (n(n-1)/2)  
            // C = sum of the concordant pairs, D = sum of discordant, n = number of measurements
            kendall = (c - d) / ((n) * (n - 1) / 2);
        }

        /// <summary>
        ///     Finds the decimal square root of a number with more accuracy that converting to double and using Math.Sqrt().
        ///     The result of the calculations will differ from an actual value of the root on less than epslion.
        ///     Credit:
        ///     SLenik - https://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
        /// </summary>
        /// <param name="x"> The number to find the sqrt of </param>
        /// <param name="epsilon"> An accuracy of calculation of the root from our number. </param>
        /// <returns></returns>
        private static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
        {
            if (x < 0) throw new OverflowException("Cannot calculate square root from a negative number");

            decimal current = (decimal)Math.Sqrt((double)x), previous;
            do
            {
                previous = current;
                if (previous == 0.0M) return 0;
                current = (previous + x / previous) / 2;
            }
            while (Math.Abs(previous - current) > epsilon);
            return current;
        }

        /// <summary>
        ///     Class to store a datum and its rank concurrently.
        /// </summary>
        private class RankedDatum
        {
            public RankedDatum(decimal datum, decimal rank)
            {
                Datum = datum;
                Rank = rank;
            }

            public decimal Datum { get; set; }
            public decimal Rank { get; set; }
        }
    }
}
