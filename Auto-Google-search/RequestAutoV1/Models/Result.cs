using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RequestAutoV1.Models
{
    public class Result
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public int StatusCode { get; set; }
        public int SuccessProbability { get; set; }
        public ICollection<Search> searches { get; set; }

        public Result()
        {
            Name = string.Empty;
            URL = string.Empty;
            StatusCode = 0;
            SuccessProbability = 0;
            searches = new List<Search>();
        }
    }
}