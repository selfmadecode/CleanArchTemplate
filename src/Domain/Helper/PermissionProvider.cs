using System.ComponentModel;

namespace Domain.Helper;

public static class PermissionProvider
{
    public const string SUPERADMIN = nameof(SUPERADMIN);
    public const string ADMIN = nameof(ADMIN);
    public const string USER = nameof(USER);

    public enum Permission
    {
        APPROVAL = 1,
        LISTING,
        [Description("USER MANAGEMENT")]
        USER_MANAGEMENT,
        SETTINGS,
        DASHBOARD,
        USER,
        [Description("AUDIT TRAIL")]
        AUDIT_TRAIL,
        CONFIGURATION,
        INQUIRY_MESSAGE
    }

    public enum Role
    {
        ADMIN,
        SUPERADMIN,
        USER
    }

    public static List<string> GetAllRoles()
    {
        return [SUPERADMIN, ADMIN, USER];
    }

    public static List<Permission> GetPermissionsForRole(string role)
    {
        return role switch
        {
            USER => new List<Permission>
            {
                Permission.USER,
                Permission.INQUIRY_MESSAGE
            },
            ADMIN => new List<Permission>
            {
                Permission.USER,
                Permission.LISTING,
                Permission.DASHBOARD,
                Permission.APPROVAL
            },
            SUPERADMIN => Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList(),
            _ => new List<Permission>()
        };
    }
}
