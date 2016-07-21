namespace Warden.Integrations.Cachet
{
    public static class Status
    {
        public static int Operational => 1;
        public static int PerformanceIssues => 2;
        public static int PartialOutage => 3;
        public static int MajorOutage => 4;
    }
}