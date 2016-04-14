namespace Warden.Web.Dto
{
    public class ExceptionDto
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTraceString { get; set; }
        public ExceptionDto InnerException { get; set; }
    }
}