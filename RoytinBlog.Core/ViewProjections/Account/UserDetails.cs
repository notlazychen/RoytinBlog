using iBoxDB.LocalServer;
using RoytinBlog.Core.Documents;

namespace RoytinBlog.Core.ViewProjections.Account
{
    public class GetUserDetails : IViewProjection<string, Author>
    {
        private readonly DB.AutoBox _db;

        public GetUserDetails(DB.AutoBox db)
        {
            _db = db;
        }

        public Author Project(string input)
        {
            return _db.SelectKey<Author>(DBTableNames.Authors, input);
        }
    }
}