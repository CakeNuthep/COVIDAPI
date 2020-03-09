using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace COVIDApi.Controllers
{
    public class ValuesController : ApiController
    {
        [Route("covid19")]
        public IHttpActionResult Get()
        {
            string url = ConfigurationManager.AppSettings["URL_WEB"];
            CovidWebScraper covid = new CovidWebScraper(url);
            DataTable dt = covid.ScrapeWebsite();
            return Ok(dt);
        }

    }
}
