using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        public Dictionary<string, List<string>> Tags { get; set; }
    }

    public class MetaModule : NancyModule
    {
        public MetaModule()
        {
            Get("/class-meta/{metaId}", parameters =>
            {
                string metaId = parameters.metaId;
                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("ClassMeta").FindById(metaId);
                    if (meta == null)
                        return this.Response.AsJson(new { });
                    var pictureUrlBase = db.GetCollection<KeyValue>("Settings")
                        .FindById("pictureUrlBase").Value;
                    var metaForJson = new
                    {
                        Name = meta.Name,
                        Description = meta.Description ?? "",
                        PictureUrl = pictureUrlBase + meta.PictureUrl,
                        PreviewPictureUrl = pictureUrlBase + meta.PreviewPictureUrl,
                        Tags = meta.Tags
                    };
                    return this.Response.AsText(
                        JsonConvert.SerializeObject(metaForJson,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                        "application/json");
                }
            });
            Get("/instance-meta/{metaId}", parameters =>
            {
                string metaId = parameters.metaId;
                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("InstanceMeta").FindById(metaId);
                    if (meta == null)
                        return this.Response.AsJson(new { });
                    var pictureUrlBase = db.GetCollection<KeyValue>("Settings")
                        .FindById("pictureUrlBase").Value;
                    var metaForJson = new
                    {
                        Name = meta.Name,
                        Description = meta.Description,
                        PictureUrl = meta.PictureUrl == null ? null : pictureUrlBase + meta.PictureUrl,
                        PreviewPictureUrl = meta.PreviewPictureUrl == null ? null : pictureUrlBase + meta.PreviewPictureUrl,
                        Tags = meta.Tags
                    };
                    return this.Response.AsText(
                        JsonConvert.SerializeObject(metaForJson,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                        "application/json");
                }
            });
        }
    }
}
