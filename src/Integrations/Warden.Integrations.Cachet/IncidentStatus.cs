namespace Warden.Integrations.Cachet
{
    //Status of the incident
    public static class IncidentStatus
    {
        public static int Investigating => 1;
        public static int Identified => 2;
        public static int Watching => 3;
        public static int Fixed => 4;
    }
}