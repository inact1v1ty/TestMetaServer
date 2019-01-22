using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Nancy.Owin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Owin;
using LiteDB;

namespace TestMetaServer
{
    public class KeyValue
    {
        [BsonId]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
