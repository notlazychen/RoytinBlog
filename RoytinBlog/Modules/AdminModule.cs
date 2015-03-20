using RoytinBlog.Core;
using RoytinBlog.Core.Commands.Accounts;
using RoytinBlog.Core.ViewProjections.Admin;
using Nancy;
using Nancy.ModelBinding;

namespace RoytinBlog.Modules
{
    public class AdminModule : SecureModule
    {
        private readonly ICommandInvokerFactory _commandInvokerFactory;
        private readonly IRootPathProvider _rootPath;

        public AdminModule(ICommandInvokerFactory commandInvokerFactory, IViewProjectionFactory viewProjectionFactory, IRootPathProvider rootPath)
            : base(viewProjectionFactory)
        {
            _commandInvokerFactory = commandInvokerFactory;
            _rootPath = rootPath;

            Get["/admin"] = _ => Index();
            Get["/admin/change-password"] = _ => ChangePassword();
            Post["/admin/change-password"] = _ => ChangePassword(this.Bind<ChangePasswordCommand>());

            Get["/admin/change-profile"] = _ => ChangeProfile();
            Post["/admin/change-profile"] = _ => ChangeProfile(this.Bind<ChangeProfileCommand>());
        }

        private dynamic ChangePassword()
        {
            return View["ChangePassword"];
        }

        private dynamic ChangePassword(ChangePasswordCommand command)
        {
            var commandResult = _commandInvokerFactory.Handle<ChangePasswordCommand, CommandResult>(command);

            if (commandResult.Success)
            {
                AddMessage("密码已经被成功修改", "success");

                return View["ChangePassword"];
            }

            AddMessage("修改密码过程中发生问题", "warning");

            return View["ChangePassword"];
        }

        private dynamic ChangeProfile()
        {
            var authorModel = _viewProjectionFactory.Get<string, AuthorProfileViewModel>(CurrentUser.Id);
            return View["ChangeProfile",authorModel];
        }

        private dynamic ChangeProfile(ChangeProfileCommand command)
        {
            var commandResult = _commandInvokerFactory.Handle<ChangeProfileCommand, CommandResult>(command);

            if (commandResult.Success)
            {
                //AddMessage("用户信息已经被成功修改", "success");
                //return View["ChangeProfile"];
                //TODO:成功的消息无法传递给跳转页面，给当前页则会因为账号信息的前后不一致报错。
                return Response.AsRedirect("~/admin");
            }

            AddMessage("修改用户信息过程中发生问题", "warning");

            return View["ChangeProfile"];
        }

        private dynamic Index()
        {
            var stat = _viewProjectionFactory.Get<AllStatisticsBindingModel, AllStatisticsViewModel>(new AllStatisticsBindingModel { TagThreshold = 1 });

            return View["Index", stat];
        }
    }
}