namespace Warden.Integrations.Cachet
{
    //Status of the component
    public static class ComponentStatus
    {
        public static int Operational => 1;
        public static int PerformanceIssues => 2;
        public static int PartialOutage => 3;
        public static int MajorOutage => 4;
    }
}