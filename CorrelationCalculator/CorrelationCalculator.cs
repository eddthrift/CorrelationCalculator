using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationCalculator
{
    class CorrelationCalculator
    {
        static void Main(string[] args)
        {
            CalculateCorrelation();

            Console.WriteLine("\nPress any key to Exit.");
            Console.ReadKey();
        }

        public static void CalculateCorrelation()
        {
            IEnumerable<string> file = ReadFile();

            IList<DataColumn> dataColumns = new List<DataColumn>();
            try
            {
                dataColumns = ParseFileData(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                Console.WriteLine("Please press any key to quit program.");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            (int firstColumnIndex, int secondColumnIndex) = SelectDataColumns(dataColumns);

            Correlations.Calculate(dataColumns[firstColumnIndex], dataColumns[secondColumnIndex]);
        }

        #region Read File

        /// <summary>
        ///     Reads and returns the data from the user-inputted file path
        /// </summary>
        /// <returns> IEnumerable<string> of the file contents </returns>
        private static IEnumerable<string> ReadFile()
        {
            bool isValidPath = false;
            string filePath = "";
            IEnumerable<string> file = new List<string>();

            while (isValidPath == false)
            {
                // Allow user to input file path.
                Console.WriteLine("Please enter the file path of your data file. Hit ENTER to confirm.");
                filePath = Console.ReadLine();

                try
                {
                    // If file is valid, load it
                    file = ValidateAndLoadFilePath(filePath);
                    isValidPath = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return file;
        }

        /// <summary>
        ///     Validates the filepath and file, and loads the data line by line into an IEnumerable<string>
        /// </summary>
        /// <param name="filePath"> The path to the file to load </param>
        /// <returns> IEnumerable<string> of file contents, or throws appropriate exceptions. </returns>
        private static IEnumerable<string> ValidateAndLoadFilePath(string filePath)
        {
            IEnumerable<string> file = new List<string>();

            // If there is a file at the specified path, try to read it. 
            if (File.Exists(filePath))
            {
                try
                {
                    file = File.ReadAllLines(filePath);
                }
                // If this fails throw appropriate error message.
                catch(Exception ex)
                {
                    throw new Exception("File cannot be read. Please try another file: " + ex.Message);
                }

                // Validate file type
                if(Path.GetExtension(filePath) != ".csv")
                {
                    throw new Exception("Invalid file type. Please upload a .csv file.");
                }
            }
            // Otherwise throw appropriate error message.
            else
            {
                throw new Exception("There is no file at the specified path. Please try again.");
            }

            return file;
        }

        #endregion

        #region Parse File

        /// <summary>
        ///     Breaks the IEnumerable<string> down into individual DataColumns
        /// </summary>
        /// <param name="file"> The data that has been read line-by-line from the file </param>
        /// <returns> A list of the columns of data in the file. </returns>
        private static IList<DataColumn> ParseFileData(IEnumerable<string> file)
        {
            // Split first row in file into separate headers.
            IEnumerable<string> headers = file.ToList()[0].Split(',');

            // validate that the headers are non-numeric, error if they are invalid.
            if(ValidateHeaders(headers, out IList<DataColumn> dataColumns) == false)
            {
                throw new Exception("Header line contains numerical data. Please check file format and restart.");
            }
      
            // Parse data from rest of file (excludes header line)
            foreach(string dataRecord in file.Skip(1))
            {
                // Split each file line into individual data points
                IList<string> datumStrings = dataRecord.Split(',');

                // Should now have the same number of initialised DataColumns as individual data
                if(dataColumns.Count == datumStrings.Count)
                {
                    // If we do go through each value in line, validate that it is numeric and add to DataColumn.Data
                    for(int i = 0; i < dataColumns.Count; i++)
                    {
                        ValidateAndAddData(dataColumns[i], datumStrings[i]);
                    }
                }
                else
                {
                    throw new Exception("Parsed line does not contain the same number of data as columns in the header file. Please check file format.");
                }
            }

            return dataColumns;
        }

        /// <summary>
        ///     Validates the data is numeric and adds it to the specified DataColumn
        /// </summary>
        /// <param name="dataColumn"> The DataColumn to append the value to. </param>
        /// <param name="datumString"> The datum in string form to be parsed. </param>
        private static void ValidateAndAddData(DataColumn dataColumn, string datumString)
        {
            if(decimal.TryParse(datumString, out decimal datum))
            {
                dataColumn.Data.Add(datum);
            }
            else
            {
                throw new Exception("Cannot parse datum as decimal. Please check that all data are numeric.");
            }
        }

        /// <summary>
        ///     Validates the headers.
        /// </summary>
        /// <param name="headers"> An IEnumerable of the file headers. </param>
        /// <returns> A bool indicating if the headers are valid </returns>
        private static bool ValidateHeaders(IEnumerable<string> headers, out IList<DataColumn> dataColumns)
        {
            dataColumns = new List<DataColumn>();
            bool isValid = true;

            foreach(string header in headers)
            {
                if (double.TryParse(header, out double result))
                {
                    isValid = false;
                    break;
                }
                else
                {
                    dataColumns.Add(new DataColumn(header));
                }
            }

            return isValid;
        }

        #endregion

        #region Column Selection

        /// <summary>
        ///     Manages and validates dataColumn selection.
        /// </summary>
        /// <param name="dataColumns"> List of the datacolumns. </param>
        /// <returns> The indices of the selected columns. </returns>
        private static Tuple<int, int> SelectDataColumns(IList<DataColumn> dataColumns)
        {
            int firstColumnIndex;
            // Intialised as -1 so we can tell if this isn't a real selection (e.g. if file only contains 1 column)
            int secondColumnIndex = -1;

            Console.WriteLine("Please enter the corresponding number to select the first data column to calculate statistics:");
            ListDataFields(dataColumns);

            firstColumnIndex = ChooseDataColumnIndex(dataColumns);

            // Only ask for a 2nd column if the file contains more than 1 column.
            if (dataColumns.Count > 1)
            {
                Console.WriteLine("Please enter the corresponding number to select the second data column to calculate statistics:");
                ListDataFields(dataColumns);
                secondColumnIndex = ChooseDataColumnIndex(dataColumns);
            }

            // Ensure the selected columns aren't the same.
            while (firstColumnIndex == secondColumnIndex)
            {
                Console.WriteLine("Cannot select the same column twice. Please re-select the second column.");
                ListDataFields(dataColumns);
                secondColumnIndex = ChooseDataColumnIndex(dataColumns);
            }

            return new Tuple<int, int>(firstColumnIndex, secondColumnIndex);
        }

        /// <summary>
        ///     Lists the data columns for selection.
        /// </summary>
        /// <param name="dataColumns"> An IList of the datacolumns. </param>
        private static void ListDataFields(IList<DataColumn> dataColumns)
        {
            for(int i = 0; i < dataColumns.Count; i++)
            {
                DataColumn dataColumn = dataColumns[i];
                Console.WriteLine("\t" + (i + 1).ToString() + ")\t" + dataColumn.Header);
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Handles choosing the data columns.
        /// </summary>
        /// <param name="dataColumns"> An IList of the datacolumns. </param>
        /// <returns> Index of the chosen column. </returns>
        private static int ChooseDataColumnIndex(IList<DataColumn> dataColumns)
        {
            bool isValidInput = false;
            int inputInt = 0;

            while(isValidInput == false)
            {
                Console.WriteLine("Please enter the corresponding number and press ENTER to select a column.");
                string input = Console.ReadLine().ToString();
                
                // If input can be parsed as an int and is less or equal to the number of columns accept it 
                if(int.TryParse(input, out inputInt))
                {
                    if (inputInt <= dataColumns.Count)
                    {
                        isValidInput = true;
                    }
                }
            }

            int columnIndex = inputInt - 1;

            Console.WriteLine("Chosen field: " + dataColumns[columnIndex].Header + "\n");
            return columnIndex;
        }

        #endregion
    }

    /// <summary>
    ///     DataColumn class to hold List of data and a header describing the data.
    /// </summary>
    class DataColumn
    {
        public DataColumn(string header)
        {
            Header = header;
            Data = new List<decimal>();
        }
        public string Header { get; private set; }
        public IList<decimal> Data { get; set; }
    }

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
        public static void Calculate(DataColumn firstColumn, DataColumn secondColumn)
        {
            // Load data into class fields
            setX = firstColumn.Data;
            setY = secondColumn.Data;

            // Rank data and load into fields
            rankedSetX = RankDataSet(setX);
            rankedSetY = RankDataSet(setY);

            // Calculate and output the statistics
            CalculateCovarianceAndStDev();
            PearsonCoefficient();
            SpearmanCoefficient();
            KendallCoefficient();
            Console.WriteLine("Calculating statistics for " + firstColumn.Header + " and " + secondColumn.Header + "\n");
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
            for(int i = 0; i < rankedSetX.Count; i++)
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
            kendall = (c - d) / ((n)*(n-1) / 2);
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
