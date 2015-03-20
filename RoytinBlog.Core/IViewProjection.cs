namespace RoytinBlog.Core
{
    public interface IViewProjection<tIn, tOut>
    {
        tOut Project(tIn input);
    }
}