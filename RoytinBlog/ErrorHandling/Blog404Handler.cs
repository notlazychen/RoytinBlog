using Nancy;
using Nancy.ErrorHandling;
using Nancy.IO;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace RoytinBlog.ErrorHandling
{
    public class Blog404Handler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return (int)statusCode == 404 || (int)statusCode == 422;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Contents = s =>
                {
                    using (var writer = new StreamWriter(new UnclosableStreamWrapper(s), Encoding.UTF8))
                    {
                        switch (statusCode)
                        {
                            case HttpStatusCode.NotFound:
                                writer.Write(LoadResource("404.html"));
                                break;
                            case HttpStatusCode.UnprocessableEntity:
                                writer.Write(LoadResource("422.html"));
                                break;
                        }
                    }
                };
        }

        private static string LoadResource(string filename)
        {
            var resourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(String.Format("RoytinBlog.ErrorHandling.Resources.{0}", filename));

            if (resourceStream == null)
            {
                return string.Empty;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}