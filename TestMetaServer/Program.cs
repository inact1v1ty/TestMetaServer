using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Hosting;

namespace TestMetaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var db = new LiteDatabase(@"Meta.db"))
            {
                var c = db.GetCollection<KeyValue>("Settings");
                if(c.FindById("pictureUrlBase") == null) {
                    c.Upsert("pictureUrlBase",
                    new KeyValue(){
                        Key = "pictureUrlBase",
                        Value = "localhost:5000/"
                    });
                }
                if(c.FindById("password") == null) {
                    c.Upsert("password",
                    new KeyValue(){
                        Key = "password",
                        Value = UserMapper.DefaultPassword
                    });
                }
            }

            CreateFolders();
            using (var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build())
            {
                host.Run();
            }
        }

        static void CreateFolders(){
            Console.WriteLine(Directory.GetCurrentDirectory());
            if (!Directory.Exists("images"))
            {
                Directory.CreateDirectory("images");
            }
            if (!Directory.Exists(Path.Combine("images", "class")))
            {
                Directory.CreateDirectory(Path.Combine("images", "class"));
            }
            if (!Directory.Exists(Path.Combine("images", "instance")))
            {
                Directory.CreateDirectory(Path.Combine("images", "instance"));
            }
            if (!Directory.Exists(Path.Combine("images", "class", "preview")))
            {
                Directory.CreateDirectory(Path.Combine("images", "class", "preview"));
            }
            if (!Directory.Exists(Path.Combine("images", "instance", "preview")))
            {
                Directory.CreateDirectory(Path.Combine("images", "instance", "preview"));
            }
        }
    }
}
