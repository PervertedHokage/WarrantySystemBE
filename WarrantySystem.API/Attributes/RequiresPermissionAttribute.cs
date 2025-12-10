namespace WarrantySystem.API.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequiresPermissionAttribute : Attribute
{
    public string Permission;

    public RequiresPermissionAttribute(string permission)
    {
        this.Permission = permission;
    }
}