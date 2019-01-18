using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.Json;
using Nancy.Security;

namespace TestMetaServer
{
    public class AdminModule : NancyModule
    {
        public AdminModule() : base("admin")
        {
            Get["/login"] = _ =>
            {
                return View["admin/login"];
            };

            Post["/login"] = _ => {
                var userGuid = UserMapper.ValidateUser((string)this.Request.Form.login, (string)this.Request.Form.password);

                if (userGuid == null)
                {
                    return this.Context.GetRedirect("~/admin/login?error=true&username=" + (string)this.Request.Form.Username);
                }

                return this.LoginAndRedirect(userGuid.Value);
            };

            Get["/logout"] = _ => {
                return this.LogoutAndRedirect("~/admin");
            };

            Get["/"] = _ =>
            {
                return View["admin/index"];
            };

            Get["/settings"] = _ =>
            {
                this.RequiresAuthentication();

                return View["admin/settings", Settings.Default.pictureUrlBase];
            };

            Post["/settings"] = _ =>
            {
                this.RequiresAuthentication();

                var newHostUrl = (string)this.Request.Form.hostUrl;

                if (!string.IsNullOrEmpty(newHostUrl))
                {
                    Settings.Default.pictureUrlBase = newHostUrl;
                }

                return this.Context.GetRedirect("~/admin");
            };

            Get["/view-class-meta"] = _ =>
            {
                this.RequiresAuthentication();

                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("ClassMeta").FindAll().ToList();
                    var viewMeta = meta.Select(metai =>
                        new
                        {
                            MetaId = metai.MetaId,
                            Name = metai.Name,
                            Description = metai.Description,
                            PictureUrl = metai.PictureUrl,
                            PreviewPictureUrl = metai.PreviewPictureUrl,
                            Misc = new JavaScriptSerializer().Serialize(metai.Misc)
                        }
                    );
                    return View["admin/view-class-meta", viewMeta];
                }
            };

            Get["/add-class-meta"] = _ =>
            {
                this.RequiresAuthentication();

                return View["admin/add-class-meta"];
            };

            Post["/add-class-meta", runAsync:true] = async (ctx, ct) =>
            {
                this.RequiresAuthentication();

                var metaId = (string)this.Request.Form.metaId;
                var name = (string)this.Request.Form.name;
                var desc = (string)this.Request.Form.description;
                var misc = (string)this.Request.Form.misc;

                if (string.IsNullOrEmpty(desc))
                    desc = "";
                if (string.IsNullOrEmpty(misc))
                    misc = "{}";

                var miscDecoded = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(misc);

                var imageUrl = this.Request.Url.BasePath +
                    await HandleUpload(
                        metaId,
                        this.Request.Files.First(file => file.Key == "image").Name.Substring(
                            this.Request.Files.First(file => file.Key == "image").Name.LastIndexOf('.')
                            ),
                        this.Request.Files.First(file => file.Key == "image").Value,
                        false,
                        false
                        );
                var previewImageUrl = this.Request.Url.BasePath +
                    await HandleUpload(
                        metaId,
                        this.Request.Files.First(file => file.Key == "previewImage").Name.Substring(
                            this.Request.Files.First(file => file.Key == "previewImage").Name.LastIndexOf('.')
                            ),
                        this.Request.Files.First(file => file.Key == "previewImage").Value,
                        false,
                        true
                        );
                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    db.GetCollection<Meta>("ClassMeta").Insert(
                            new Meta()
                            {
                                MetaId = metaId,
                                Name = name,
                                Description = desc,
                                PictureUrl = imageUrl,
                                PreviewPictureUrl = previewImageUrl,
                                Misc = miscDecoded
                            }
                            );
                }
                return this.Context.GetRedirect("~/admin/view-class-meta");
            };

            Post["/delete-class-meta"] = _ =>
            {
                this.RequiresAuthentication();

                var metaId = (string)this.Request.Form.metaId;

                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("ClassMeta").FindById(metaId);

                    if (!string.IsNullOrEmpty(meta.PictureUrl) && File.Exists(meta.PictureUrl))
                        File.Delete(meta.PictureUrl);
                    if (!string.IsNullOrEmpty(meta.PreviewPictureUrl) && File.Exists(meta.PreviewPictureUrl))
                        File.Delete(meta.PreviewPictureUrl);
                    db.GetCollection<Meta>("ClassMeta").Delete(metaId);
                }
                return this.Context.GetRedirect("~/admin/view-class-meta");
            };

            Get["/view-instance-meta"] = _ =>
            {
                this.RequiresAuthentication();

                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("InstanceMeta").FindAll().ToList();
                    var viewMeta = meta.Select(metai =>
                        new
                        {
                            MetaId = metai.MetaId,
                            Name = metai.Name,
                            Description = metai.Description,
                            PictureUrl = metai.PictureUrl,
                            PreviewPictureUrl = metai.PreviewPictureUrl,
                            Misc = new JavaScriptSerializer().Serialize(metai.Misc)
                        }
                    );
                    return View["admin/view-instance-meta", viewMeta];
                }
            };

            Get["/add-instance-meta"] = _ =>
            {
                this.RequiresAuthentication();

                return View["admin/add-instance-meta"];
            };

            Post["/add-instance-meta", runAsync: true] = async (ctx, ct) =>
            {
                this.RequiresAuthentication();

                var metaId = (string)this.Request.Form.metaId;
                var name = (string)this.Request.Form.name;
                var desc = (string)this.Request.Form.description;
                var misc = (string)this.Request.Form.misc;

                if (string.IsNullOrEmpty(misc))
                    misc = "{}";

                var miscDecoded = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(misc);

                bool hasImage = this.Request.Files.Any(file => file.Key == "image");

                string imageUrl = "";

                if (hasImage)
                {
                    imageUrl = this.Request.Url.BasePath +
                        await HandleUpload(
                            metaId,
                            this.Request.Files.First(file => file.Key == "image").Name.Substring(
                                this.Request.Files.First(file => file.Key == "image").Name.LastIndexOf('.')
                                ),
                            this.Request.Files.First(file => file.Key == "image").Value,
                            true,
                            false
                            );
                }

                bool hasPreviewImage = this.Request.Files.Any(file => file.Key == "previewImage");

                string previewImageUrl = "";

                if (hasPreviewImage)
                {
                    previewImageUrl = this.Request.Url.BasePath +
                        await HandleUpload(
                            metaId,
                            this.Request.Files.First(file => file.Key == "previewImage").Name.Substring(
                                this.Request.Files.First(file => file.Key == "previewImage").Name.LastIndexOf('.')
                                ),
                            this.Request.Files.First(file => file.Key == "previewImage").Value,
                            true,
                            true
                            );
                }

                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    db.GetCollection<Meta>("InstanceMeta").Insert(
                            new Meta()
                            {
                                MetaId = metaId,
                                Name = string.IsNullOrEmpty(name) ? null : name,
                                Description = string.IsNullOrEmpty(desc) ? null : desc,
                                PictureUrl = string.IsNullOrEmpty(imageUrl) ? null : imageUrl,
                                PreviewPictureUrl = string.IsNullOrEmpty(previewImageUrl) ? null : previewImageUrl,
                                Misc = miscDecoded
                            }
                            );
                }
                return this.Context.GetRedirect("~/admin/view-instance-meta");
            };

            Post["/delete-instance-meta"] = _ =>
            {
                this.RequiresAuthentication();

                var metaId = (string)this.Request.Form.metaId;

                using (var db = new LiteDatabase(@"Meta.db"))
                {
                    var meta = db.GetCollection<Meta>("InstanceMeta").FindById(metaId);
                    
                    if(!string.IsNullOrEmpty(meta.PictureUrl) && File.Exists(meta.PictureUrl))
                        File.Delete(meta.PictureUrl);
                    if (!string.IsNullOrEmpty(meta.PreviewPictureUrl) && File.Exists(meta.PreviewPictureUrl))
                        File.Delete(meta.PreviewPictureUrl);
                    db.GetCollection<Meta>("InstanceMeta").Delete(metaId);
                }
                return this.Context.GetRedirect("~/admin/view-instance-meta");
            };
        }

        public async Task<string> HandleUpload(string fileName, string ext, Stream stream, bool instance, bool preview)
        {
            var targetFile = "images/" + (instance ? "instance" : "class");
            if (preview)
                targetFile = targetFile + "/preview";
            targetFile = targetFile + "/" + fileName + ext;
            using (FileStream destinationStream = File.Create(targetFile))
            {
                await stream.CopyToAsync(destinationStream);
            }

            return targetFile;
        }
    }
}
