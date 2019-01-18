using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using LiteDB;

namespace TestMetaServer
{
    public class Meta
    {
        [BsonId]
        public string MetaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public string PreviewPictureUrl { get; set; }
        public Dictionary<string, string> Misc { get; set; }
    }

    public class MetaModule : NancyModule
    {
        public MetaModule()
        {
            Get["/class-meta/{metaId}"] = parameters =>
            {
                string metaId = parameters.metaId;
                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("ClassMeta").FindById(metaId);
                    return this.Response.AsJson(meta);
                }
            };
            Get["/instance-meta/{metaId}"] = parameters =>
            {
                string metaId = parameters.metaId;
                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("InstanceMeta").FindById(metaId);
                    return this.Response.AsJson(meta);
                }
            };
        }
    }
}
