namespace Sentry
{
    /// <summary>
    /// Common interface with a single property determing the success of an operation.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Flag that holds the result of the operation.
        /// </summary>
        bool IsValid { get; }
    }
}