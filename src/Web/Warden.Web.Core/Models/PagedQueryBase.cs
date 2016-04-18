namespace Warden.Web.Core.Models
{
    public abstract class PagedQueryBase
    {
        public int Page { get; set; }
        public int Results { get; set; }
    }
}