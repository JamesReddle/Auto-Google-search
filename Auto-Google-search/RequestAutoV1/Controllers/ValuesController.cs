using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using RequestAutoV1.Data;
using RequestAutoV1.Models;

namespace RequestAutoV1.Controllers
{
    public class TestController : ApiController
    {
        public List<string> GetResut ()
        {
            List<string> URLs = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                var results = db.Database.SqlQuery<Result>("SELECT * FROM results");
                foreach (var result in results)
                    URLs.Add(HttpUtility.UrlDecode(result.URL));
            }
        
            return URLs;
        }
    }
}
