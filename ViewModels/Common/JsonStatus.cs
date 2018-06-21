using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Common
{
    public class JsonStatus
    {

        public JsonStatus()
	{
	    Error = false;
	    Message = "";
	}

        public JsonStatus(bool error, string message)
	{
	     Error = error;
	     Message = message;
	}

        public bool Error {get;set;}

        public string Message {get;set;}
    }
}
