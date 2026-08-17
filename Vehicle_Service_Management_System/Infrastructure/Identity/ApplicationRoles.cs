namespace Vehicle_Service_Management_System.Infrastructure.Identity
{
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Mechanic = "Mechanic";

        public static readonly string[] All = { Admin, Manager, Mechanic };
    }
}
