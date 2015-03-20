using RoytinBlog.Core;
using Nancy.TinyIoc;

namespace RoytinBlog.Features
{
    public class ViewProjectionFactory : IViewProjectionFactory
    {
        //hm?...
        private readonly TinyIoCContainer _container;

        public ViewProjectionFactory(TinyIoCContainer containtr)
        {
            _container = containtr;
        }

        public TOut Get<TIn, TOut>(TIn input)
        {
            var loadtr = _container.Resolve<IViewProjection<TIn, TOut>>();
            return loadtr.Project(input);
        }
    }
}