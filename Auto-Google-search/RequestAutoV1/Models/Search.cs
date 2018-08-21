using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RequestAutoV1.Models
{
    public class Search
    {
        public int id { get; set; }
        public string URL { get; set; }
        public int StatusCode { get; set; }
        public string CorrectionWord { get; set; }
        public ICollection<Result> results { get; set; }

        public Search ()
        {
            StatusCode = 0;
            CorrectionWord = string.Empty;
            URL = string.Empty;
            results = new List<Result>();
        }
    }
}