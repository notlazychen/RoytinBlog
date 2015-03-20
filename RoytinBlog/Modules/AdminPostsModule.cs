using RoytinBlog.Core;
using RoytinBlog.Core.Commands.Posts;
using RoytinBlog.Core.Extensions;
using RoytinBlog.Core.ViewProjections.Admin;
using RoytinBlog.Core.ViewProjections.Home;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace RoytinBlog.Modules
{
    public class AdminPostsModule : SecureModule
    {
        private readonly ICommandInvokerFactory _commandInvokerFactory;

        public AdminPostsModule(IViewProjectionFactory factory, ICommandInvokerFactory commandInvokerFactory)
            : base(factory)
        {
            _commandInvokerFactory = commandInvokerFactory;
            Get["/admin/posts/{page?1}"] = _ => ShowPosts(_.page);
            Get["/admin/posts/new"] = _ => ShowNewPost();
            Post["/admin/posts/new"] = _ =>
                                           {
                                               var command = this.Bind<NewPostCommand>();
                                               command.Author = CurrentUser;
                                               return CreateNewPost(command);
                                           };
            Get["/admin/posts/edit/{postId}"] = _ => ShowPostEdit(_.postId);
            Post["/admin/posts/edit/{postid}"] = _ => EditPost(this.Bind<EditPostCommand>());
            Get["/admin/posts/delete/{postid}"] = _ => DeletePost(this.Bind<DeletePostCommand>());

            Get["/admin/comments/{page?1}"] = _ => ShowComments(_.page);
            Get["/admin/comments/delete/{commentid}"] = _ => DeleteComment(this.Bind<DeleteCommentCommand>());
            Get["/admin/tags"] = _ => ShowTags();
            Post["/admin/slug"] = _ => GetSlug();
        }

        private string GetSlug()
        {
            string title = Request.Form["title"];
            return title.ToSlug();
        }

        private dynamic ShowTags()
        {
            var tags = _viewProjectionFactory.Get<TagCloudBindingModel, TagCloudViewModel>(new TagCloudBindingModel() { Threshold = 1 });
            return View["Tags", tags];
        }

        private dynamic DeletePost(DeletePostCommand deletePostCommand)
        {
            _commandInvokerFactory.Handle<DeletePostCommand, CommandResult>(deletePostCommand);
            string returnURL = Request.Headers.Referrer;
            return Response.AsRedirect(returnURL);
        }

        private dynamic ShowNewPost()
        {
            return View["New", new NewPostCommand()];
        }

        private dynamic CreateNewPost(NewPostCommand command)
        {
            var commandResult = _commandInvokerFactory.Handle<NewPostCommand, CommandResult>(command);

            if (commandResult.Success)
            {
                return this.Context.GetRedirect("/admin/posts");
            }

            AddMessage("保存文章时发生错误", "warning");

            return View["New", command];
        }

        private Negotiator EditPost(EditPostCommand command)
        {
            var commandResult = _commandInvokerFactory.Handle<EditPostCommand, CommandResult>(command);

            if (commandResult.Success)
            {
                AddMessage("文章更新成功", "success");

                return ShowPostEdit(command.PostId);
            }

            return View["Edit", commandResult.GetErrors()];
        }

        private Negotiator ShowPosts(int page)
        {
            var model =
                _viewProjectionFactory.Get<AllBlogPostsBindingModel, AllBlogPostsViewModel>(new AllBlogPostsBindingModel()
                {
                    Page = page,
                    Take = 30
                });
            return View["Posts", model];
        }

        private Negotiator ShowPostEdit(string blogPostId)
        {
            var model =
                _viewProjectionFactory.Get<BlogPostEditBindingModel, BlogPostEditViewModel>(
                    new BlogPostEditBindingModel
                    {
                        PostId = blogPostId
                    }
                );

            return View["Edit", model];
        }

        private Negotiator ShowComments(int page)
        {
            var model =
                    _viewProjectionFactory.Get<AllBlogCommentsBindingModel, AllBlogCommentsViewModel>(new AllBlogCommentsBindingModel()
                    {
                        Page = page
                    });
            return View["Comments", model];
        }

        private dynamic DeleteComment(DeleteCommentCommand deletePostCommand)
        {
            _commandInvokerFactory.Handle<DeleteCommentCommand, CommandResult>(deletePostCommand);
            string returnURL = Request.Headers.Referrer;
            return Response.AsRedirect(returnURL);
        }
    }
}