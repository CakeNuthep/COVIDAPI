using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace COVIDApi.Controllers
{
    public class CovidWebScraper
    {
        private string siteUrl;
        public CovidWebScraper(string urlWeb)
        {
            this.siteUrl = urlWeb;
        }
        public DataTable ScrapeWebsite()
        {
            DataTable dt = null;
            // Create a request for the URL.  
            WebRequest request = WebRequest.Create(siteUrl);
            // If required by the server, set the credentials.  
            request.Credentials = CredentialCache.DefaultCredentials;

            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server. 
            // The using block ensures the stream is automatically closed. 
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = parser.ParseDocument(responseFromServer);
                dt = GetScrapeResults(document);
            }

            // Close the response.  
            response.Close();
            return dt;
        }

        private DataTable GetScrapeResults(IHtmlDocument document)
        {
            IEnumerable<IElement> tableRawData = null;
            IEnumerable<IElement> headTableRawData = null;
            IEnumerable<IElement> bodyTableRawData = null;

            tableRawData = document.All.Where(x => x.Id == "main_table_countries_today");
            foreach (Element element in tableRawData)
            {
                headTableRawData = element.GetElementsByTagName("thead");
                if (headTableRawData != null && headTableRawData.Any())
                {
                    break;
                }

            }

            foreach (Element element in tableRawData)
            {
                bodyTableRawData = element.GetElementsByTagName("tbody");
                if (bodyTableRawData != null && bodyTableRawData.Any())
                {
                    break;
                }

            }

            if (headTableRawData != null && headTableRawData.Any())
            {
                DataTable dt = prepareHeadData(headTableRawData);
                if (bodyTableRawData != null && bodyTableRawData.Any())
                {
                    prepareBodyData(bodyTableRawData, ref dt);
                    return dt;
                }
            }
            return null;
        }

        public DataTable prepareHeadData(IEnumerable<IElement> headRawData)
        {
            DataTable dt = new DataTable();
            foreach (IElement element in headRawData)
            {
                IEnumerable<IElement> listElement = null;
                listElement = element.GetElementsByTagName("th");
                if (listElement != null && listElement.Any())
                {
                    foreach (IElement column in listElement)
                    {
                        dt.Columns.Add(ClearMessage(RemoveTag(column.InnerHtml)));
                    }
                }
            }

            return dt;
        }

        public void prepareBodyData(IEnumerable<IElement> bodyRawData, ref DataTable dt)
        {
            foreach (IElement element in bodyRawData)
            {
                IEnumerable<IElement> listRow = null;
                listRow = element.GetElementsByTagName("tr");
                if (listRow != null && listRow.Any())
                {
                    foreach (IElement row in listRow)
                    {
                        IEnumerable<IElement> listColumnData = null;
                        listColumnData = row.GetElementsByTagName("td");
                        if (listColumnData != null && listColumnData.Any())
                        {
                            DataRow dataRow = dt.NewRow();
                            int indexColumn = 0;
                            foreach (IElement columData in listColumnData)
                            {
                                if (indexColumn < dt.Columns.Count)
                                {
                                    dataRow[indexColumn] = ClearMessage(RemoveTag(columData.InnerHtml));
                                    indexColumn++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            dt.Rows.Add(dataRow);
                        }
                    }
                }
            }
        }

        private string RemoveTag(string str)
        {
            string cleanStr;
            cleanStr = Regex.Replace(str, "<.*?>", String.Empty);
            return cleanStr;
        }

        private string ClearMessage(string str)
        {
            string cleanStr;
            cleanStr = Regex.Replace(str, ",", String.Empty);
            cleanStr = Regex.Replace(cleanStr, "&.*;", "String.Empty");
            cleanStr = Regex.Replace(cleanStr, "\\W", String.Empty);

            return cleanStr;
        }
    }
}