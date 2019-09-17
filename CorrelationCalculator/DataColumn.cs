using System.Collections.Generic;

namespace CorrelationCalculator
{
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
}
