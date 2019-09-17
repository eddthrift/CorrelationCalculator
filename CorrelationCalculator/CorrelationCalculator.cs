using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            Correlations.Calculate(dataColumns[firstColumnIndex].Header, dataColumns[firstColumnIndex].Data, dataColumns[secondColumnIndex].Header, dataColumns[secondColumnIndex].Data);
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
                filePath = UserInput.InputString();

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
}
