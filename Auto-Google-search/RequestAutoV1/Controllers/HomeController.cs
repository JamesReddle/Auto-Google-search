using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using RequestAutoV1.Models;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading;
using System.IO;
using OfficeOpenXml;
using RequestAutoV1.Data;


namespace RequestAutoV1.Controllers
{
    public class HomeController : Controller
    {
        //private string connection = "Database=searchresult;Data Source=localhost;User Id=root;Password=36987254;charset=utf8";
        private string commandText;

        private void SearchRecording (Search addsearch) //новое
        {
            //Запись запроса в бд
            DataBaseContext db = new DataBaseContext();
            commandText = @"use searchresult;
            Insert searches (URL, StatusCode, CorrectionWord)
            Values ('" + addsearch.URL + "', " + addsearch.StatusCode + ", '" + addsearch.CorrectionWord + "');";
            db.Database.ExecuteSqlCommand(commandText);
        }

        private void ResultRecording (Result addresult, int search_id) //новое
        {
            //Запсись в БД
            DataBaseContext db = new DataBaseContext();
            commandText = @"use searchresult;

            Insert results (Name, URL, StatusCode, SuccessProbability)
            Values ('" + addresult.Name + "', '" + addresult.URL + "', " + addresult.StatusCode + ", " + addresult.SuccessProbability + ");" + @"
            insert searchresults (Search_id, Result_id)
            select searches.id, results.id
            from results
            join searches on searches.id = " + search_id + @"
            where results.URL = '" + addresult.URL + "';";
            db.Database.ExecuteSqlCommand(commandText);
        }

        private int htmlParcer (string query)
        {
            Regex tmpRegex;
            MatchCollection tmpCollection;
            DataBaseContext db = new DataBaseContext();
            Search addsearch = new Search();
            string htmlCode = string.Empty;
            string SearchURL;
            SearchURL = "https://www.google.ru/search?q=" + query;
            using (WebClient client = new WebClient())
            {
                int i = 0;
                bool exit = false;
                while (i < 10 && !exit)
                {
                    try //новое
                    {
                        htmlCode = client.DownloadString(SearchURL);
                        exit = true;
                    }
                    catch (WebException e)
                    {
                        addsearch.StatusCode = (int)(((HttpWebResponse)e.Response).StatusCode);
                        SearchRecording(addsearch);
                        Thread.Sleep(30000);
                    }
                    i++;
                }
                if (exit == false)
                    return 1;
            }

            Regex wholeRegex = new Regex(@"Нажмите <a href=(.*)здесь</a>");
            MatchCollection whole = wholeRegex.Matches(htmlCode);
            htmlCode = Regex.Replace(whole[0].Value, @"</a>", string.Empty);
            Regex quoteResultRegex = new Regex(@"""(.*)""");
            MatchCollection quoteResult = quoteResultRegex.Matches(htmlCode);
            string searchEnd = quoteResult[0].Value;
            searchEnd = searchEnd.Remove(0, 1);
            searchEnd = searchEnd.Remove(searchEnd.Length - 1, 1);
            addsearch.URL = "https://www.google.ru" + searchEnd; // URL на сайт без js

            using (WebClient client = new WebClient())
            {
                int i = 0;
                bool exit = false;
                while (i < 10 && !exit)
                {
                    try //новое
                    {
                        htmlCode = client.DownloadString(addsearch.URL);
                        htmlCode = Regex.Replace(htmlCode, "\n", string.Empty);
                        htmlCode = Regex.Replace(htmlCode, @"\|", string.Empty);
                        exit = true;
                    }
                    catch (WebException e)
                    {
                        addsearch.StatusCode = (int)(((HttpWebResponse)e.Response).StatusCode);
                        SearchRecording(addsearch);
                        Thread.Sleep(30000);
                    }
                    i++;
                }
                if (exit == false)
                    return 1;
            }
            
            //ViewBag.HTML = htmlCode; //***************

            wholeRegex = new Regex(@"Показаны результаты по запросу.*?<b><i>.*?Искать вместо этого"); //Код 0 - автоисправление Google
            whole = wholeRegex.Matches(htmlCode);
            if(whole.Count > 0)
            {
                addsearch.CorrectionWord = whole[0].Value;
                addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @"Показаны результаты по запросу.*?<b><i>", string.Empty);
                addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @"</i></b>.*?Искать вместо этого", string.Empty);
                addsearch.CorrectionWord = addsearch.CorrectionWord + " {0}";
            }

            wholeRegex = new Regex(@"Возможно, вы имели в виду.*?<b><i>.+?</i></b>.*?</a></div></div>"); //Код 1 - предложение исправить
            whole = wholeRegex.Matches(htmlCode);
            if (whole.Count > 0)
            {
                addsearch.CorrectionWord = whole[0].Value;
                tmpRegex = new Regex(@""">.*?</a></div></div>");
                tmpCollection = tmpRegex.Matches(addsearch.CorrectionWord);
                if (tmpCollection.Count > 0)
                {
                    addsearch.CorrectionWord = tmpCollection[0].Value;
                    addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @"</i></b>", string.Empty);
                    addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @"<b><i>", string.Empty);
                    addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @"</a></div></div>", string.Empty);
                    addsearch.CorrectionWord = Regex.Replace(addsearch.CorrectionWord, @""">", string.Empty);
                }
                addsearch.CorrectionWord = addsearch.CorrectionWord + " {1}";
            }

            SearchRecording(addsearch); //новое

            //Получение id данного запроса
            commandText = "Select id from searchresult.searches where URL = '" + addsearch.URL + "';";
            var tmp = db.Database.SqlQuery<int>(commandText).ToList();
            int search_id = tmp.First();

            wholeRegex = new Regex(@"target=""_blank"">.+?</a></h3><div class=""s"">.*?<cite.*?</cite>.*?target=""_blank"">Сохраненная");
            whole = wholeRegex.Matches(htmlCode);
            for (int i = 0; i < whole.Count; i++)
            {
                try
                {
                    Result addresult = new Result();
                    string resultcode = string.Empty;

                    //Поиск названия данного результата
                    string Name = whole[i].Value;
                    tmpRegex = new Regex(@"target=""_blank"">.*</a></h3><div class=""s"">");
                    tmpCollection = tmpRegex.Matches(Name);
                    Name = tmpCollection[0].Value;
                    Name = Regex.Replace(Name, @"target=""_blank""><img.*target=""_blank"">", string.Empty);
                    Name = Regex.Replace(Name, @"target=""_blank"">", string.Empty);
                    Name = Regex.Replace(Name, @"</a></h3><div class=""s"">", string.Empty);
                    addresult.Name = Name;

                    //Поиск URL данного результата
                    string URL = whole[i].Value;
                    tmpRegex = new Regex(@"target=""_blank"">" + Name + @"</a></h3><div class=""s"">.*target=""_blank"">");
                    tmpCollection = tmpRegex.Matches(URL);
                    if (tmpCollection.Count == 0)
                        continue;
                    URL = tmpCollection[0].Value;
                    URL = Regex.Replace(URL, @"target=""_blank"">.*</cite>", string.Empty);
                    //Новая версия
                    tmpRegex = new Regex(@"href=""/url.*target=""_blank""");
                    tmpCollection = tmpRegex.Matches(URL);
                    if (tmpCollection.Count == 0)
                        continue;
                    URL = tmpCollection[0].Value;
                    URL = Regex.Replace(URL, @"href=""", string.Empty);
                    URL = Regex.Replace(URL, @""" target=""_blank""", string.Empty);
                    URL = "https://www.google.ru" + URL;

                    using (WebClient client = new WebClient())
                    {
                        int j = 0;
                        bool exit = false;
                        while (j < 10 && !exit)
                        {
                            try //новое
                            {
                                resultcode = client.DownloadString(URL);
                                exit = true;
                            }
                            catch (WebException e)
                            {
                                addresult.StatusCode = (int)(((HttpWebResponse)e.Response).StatusCode);
                                ResultRecording(addresult, search_id);
                                Thread.Sleep(30000);
                            }
                            j++;
                        }
                        if (exit == false)
                            continue;
                    }

                    //Декодирование ссылок
                    tmpRegex = new Regex(@"<a href=""http://webcache.*ct=clnk"">");
                    tmpCollection = tmpRegex.Matches(resultcode);
                    URL = tmpCollection[0].Value;
                    URL = Regex.Replace(URL, @"<a href=""", string.Empty);
                    URL = Regex.Replace(URL, @""">", string.Empty);
                    URL = HttpUtility.UrlDecode(URL);
                    tmpRegex = new Regex(@":http\w?://.+\+");
                    tmpCollection = tmpRegex.Matches(URL);
                    URL = tmpCollection[0].Value;
                    URL = URL.Remove(URL.Length - 1);
                    addresult.URL = URL.Substring(1);

                    //Проверка отношения найденного резульата к объекту запроса
                    string relevancyString = htmlCode;
                    tmpRegex = new Regex(@"target=""_blank"">" + Name + @".+?Сохраненная копия.+?</span><br></div></div>");
                    tmpCollection = tmpRegex.Matches(relevancyString);
                    if (tmpCollection.Count == 0)
                        continue;
                    int n = query.Count(c => c == ' ') + 1;
                    int success = 0;
                    string[] words = query.Split(' ');
                    for (int k = 0; k < words.Length; k++)
                        if (tmpCollection[0].Value.Contains(words[k]))
                            success++;
                    float probability = ((float)success / n) * 100f;
                    addresult.SuccessProbability = (int) probability;

                    ResultRecording(addresult, search_id); 
                }
                catch(Exception ex)
                {
                    continue;
                }
            }
            return 0;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            //******************Словарь русского языка***************************************************************************
            /*string dictionary = System.IO.File.ReadAllText(@"E:\All_kind_of_programmes\HTTPS_request_without_Js\Auto-Google-search\Auto-Google-search\RequestAutoV1\App_Data\dictionary.txt");
            Regex FindRecodrs = new Regex(@"(\d+)\s[а-яА-я]+");
            MatchCollection Records = FindRecodrs.Matches(dictionary);
            for(int i = 0; i<1000; i++)
            {
                Random rnd = new Random();
                int value = rnd.Next(0, Records.Count - 1);
                string query = Records[value].Value;
                query = Regex.Replace(query, @"\d", string.Empty);
                query = Regex.Replace(query, @"\s", string.Empty);
                htmlParcer(query);
                Thread.Sleep(1000);
            }*/
            //htmlParcer("стол");

            //*********************Таблица Excel*************************************************************************
            //string FilePath = @"E:\All_kind_of_programmes\HTTPS_request_without_Js\Auto-Google-search\Auto-Google-search\RequestAutoV1\App_Data\decomposition_08.07.2018_16.54.xlsx";
            string FilePath = ControllerContext.HttpContext.Server.MapPath("~/App_Data/decomposition_08.07.2018_16.54.xlsx");
            FileInfo existingFile = new FileInfo(FilePath);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[2];
                for(int row = 2; row < 179; row++)
                {
                    string klass = worksheet.GetValue<string>(row, 1);
                    klass = Regex.Replace(klass, @" \|", string.Empty);
                    string code = worksheet.GetValue<string>(row, 2);
                    code = Regex.Replace(code, @" \|", string.Empty);
                    code = Regex.Replace(code, @",", " ");
                    string query = klass + " " + code;
                    htmlParcer(query);
                    Thread.Sleep(1000);
                }
            }
            //htmlParcer("ти точна не тот");

            //*********************Для загрузки страницы в целях анализа содеражния (ТЕСТИРОВАНИЕ)************************
            /*using (WebClient client = new WebClient())
            {
                ViewBag.HTML = client.DownloadString("https://www.google.ru/search?q=%D0%9F%D1%80%D0%BE%D0%BA%D0%BB%D0%B0%D0%B4%D0%BA%D0%B0+740-1318218-10&amp;newwindow=1&amp;ie=UTF-8&amp;gbv=1&amp;sei=_451W-jyNazkkgWk2IWYBw");
            }*/

            return View();
        }
    }
}