using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BLink.Models;
using System.Web.Mvc;

namespace BLink.Controllers
{
    public class PreviewController : ApiController
    {
        private ILinkParseable _parser;        

        public PreviewController(ILinkParseable parser)
        {
            _parser = parser;            
        }
        
        public HttpResponseMessage Post([FromBody]string value)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Url is empty");

                var response = Request.CreateResponse<IPreviewable>(_parser.Parse(value));
                response.Headers.Add("Acess-Control-Allow-Origin", "*");

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse<string>(HttpStatusCode.BadRequest, ex.Message);
            }
        }    
    }
}
