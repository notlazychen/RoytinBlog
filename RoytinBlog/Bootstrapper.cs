using iBoxDB.LocalServer;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using RoytinBlog.Core;
using RoytinBlog.Core.Cache;
using RoytinBlog.Core.Documents;
using RoytinBlog.Core.Extensions;
using RoytinBlog.Features;

namespace RoytinBlog
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
            StaticConfiguration.DisableErrorTraces = false;
            pipelines.OnError += ErrorHandler;
        }

        private Response ErrorHandler(NancyContext ctx, Exception ex)
        {
            if (ex is iBoxDB.E.DatabaseShutdownException)
            {
                return "DB can't connect.";
            }
            return null;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(typeof(ISpamShieldService), typeof(SpamShieldService));
            container.Register(typeof(ICache), typeof(RuntimeCache));

            RegisterIViewProjections(container);
            TagExtension.SetupViewProjectionFactory(container.Resolve<IViewProjectionFactory>());
            RegisterICommandInvoker(container);
            container.Register<DB.AutoBox>(this.Database);
            //container.Register(typeof(MongoDatabase), (cContainer, overloads) => Database);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("highlight", "/content/Highlight"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("css", "/content/css"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("js", "/content/js"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("images", "/content/img"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("fonts", "/content/fonts"));
        }

        public DB.AutoBox Database
        {
            get
            {
                var dbPath = Path.Combine(this.RootPathProvider.GetRootPath(), "App_Data", "ibox");
                var server = new DB(dbPath);
                var config = server.GetConfig();
                config.EnsureTable<Author>(DBTableNames.Authors, "Id");
                //config.EnsureIndex<Author>(DBTableNames.Authors, "Email");
                config.EnsureTable<BlogPost>(DBTableNames.BlogPosts, "Id");
                //config.EnsureIndex<BlogPost>(DBTableNames.BlogPosts, "TitleSlug", "Status", "PubDate", "DateUTC");
                config.EnsureTable<BlogComment>(DBTableNames.BlogComments, "Id");
                //config.EnsureIndex<BlogComment>(DBTableNames.BlogComments, "PostId");
                config.EnsureTable<SpamHash>(DBTableNames.SpamHashes, "Id");
                config.EnsureTable<Tag>(DBTableNames.Tags, "Slug");

                var db = server.Open();

                if (db.SelectCount("from " + DBTableNames.Authors) == 0)
                {
                    db.Insert(DBTableNames.Authors, new Author
                    {
                        Email = ConfigurationManager.AppSettings["AdminEmail"],
                        DisplayName = ConfigurationManager.AppSettings["AdminName"],
                        Roles = new[] { "admin" },
                        HashedPassword = Hasher.GetMd5Hash(ConfigurationManager.AppSettings["AdminPassword"])
                    });
                }

                return db;
            }
        }

        public static void RegisterICommandInvoker(TinyIoCContainer container)
        {
            var commandInvokerTypes = Assembly.GetAssembly(typeof(ICommandInvoker<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                              {
                                                  Type = t.AsType(),
                                                  Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                      i =>
                                                      i.IsGenericType() &&
                                                      i.GetGenericTypeDefinition() == typeof(ICommandInvoker<,>))
                                              })
                                              .Where(t => t.Interface != null)
                                              .ToArray();

            foreach (var commandInvokerType in commandInvokerTypes)
            {
                container.Register(commandInvokerType.Interface, commandInvokerType.Type);
            }

            container.Register(typeof(ICommandInvokerFactory), (cContainer, overloads) => new CommandInvokerFactory(cContainer));
        }

        public static void RegisterIViewProjections(TinyIoCContainer container)
        {
            var viewProjectionTypes = Assembly.GetAssembly(typeof(IViewProjection<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                                               {
                                                                   Type = t.AsType(),
                                                                   Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                                       i =>
                                                                       i.IsGenericType() &&
                                                                       i.GetGenericTypeDefinition() == typeof(IViewProjection<,>))
                                                               })
                                              .Where(t => t.Interface != null)
                                              .ToArray();

            foreach (var viewProjectionType in viewProjectionTypes)
            {
                container.Register(viewProjectionType.Interface, viewProjectionType.Type);
            }

            container.Register(typeof(IViewProjectionFactory), (cContainer, overloads) => new ViewProjectionFactory(cContainer));
        }
    }
}