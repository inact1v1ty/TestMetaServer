using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace TestMetaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("images"))
            {
                Directory.CreateDirectory("images");
            }
            if (!Directory.Exists("images/class"))
            {
                Directory.CreateDirectory("images/class");
            }
            if (!Directory.Exists("images/instance"))
            {
                Directory.CreateDirectory("images/instance");
            }
            if (!Directory.Exists("images/class/preview"))
            {
                Directory.CreateDirectory("images/class/preview");
            }
            if (!Directory.Exists("images/instance/preview"))
            {
                Directory.CreateDirectory("images/instance/preview");
            }


            var hostConfigs = new HostConfiguration
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            using (var host = new NancyHost(hostConfigs, new Uri("http://localhost:9696")))
            {
                host.Start();
                Console.WriteLine("Running on localhost:9696");
                Console.ReadLine();
            }
        }
    }
}
