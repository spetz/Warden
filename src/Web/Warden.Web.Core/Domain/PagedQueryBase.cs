namespace Warden.Web.Core.Domain
{
    public abstract class PagedQueryBase
    {
        public int Page { get; set; }
        public int Results { get; set; }
    }
}