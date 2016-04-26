namespace Warden.Web.Core.Domain
{
    public interface IValidatable
    {
        bool IsValid { get; }
    }
}