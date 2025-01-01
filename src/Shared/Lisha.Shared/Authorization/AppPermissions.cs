using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Shared.Authorization
{
    public static class AppPermissions
    {
        private static readonly AppPermission[] AllPermissions =
        [
            ////tenants
            //new("View Tenants", AppActions.View, AppResources.Tenants, IsRoot: true),
            // new("Create Tenants", AppActions.Create, AppResources.Tenants, IsRoot: true),
            // new("Update Tenants", AppActions.Update, AppResources.Tenants, IsRoot: true),
            // new("Upgrade Tenant Subscription", AppActions.UpgradeSubscription, AppResources.Tenants, IsRoot: true),

            //identity
            new("View Users", AppActions.View, AppResources.Users),
            new("Search Users", AppActions.Search, AppResources.Users),
            new("Create Users", AppActions.Create, AppResources.Users),
            new("Update Users", AppActions.Update, AppResources.Users),
            new("Delete Users", AppActions.Delete, AppResources.Users),
            new("Export Users", AppActions.Export, AppResources.Users),
            new("View UserRoles", AppActions.View, AppResources.UserRoles),
            new("Update UserRoles", AppActions.Update, AppResources.UserRoles),
            new("View Roles", AppActions.View, AppResources.Roles),
            new("Create Roles", AppActions.Create, AppResources.Roles),
            new("Update Roles", AppActions.Update, AppResources.Roles),
            new("Delete Roles", AppActions.Delete, AppResources.Roles),
            new("View RoleClaims", AppActions.View, AppResources.RoleClaims),
            new("Update RoleClaims", AppActions.Update, AppResources.RoleClaims),
        
            ////products
            //new("View Products", AppActions.View, AppResources.Products, IsBasic: true),
            //new("Search Products", AppActions.Search, AppResources.Products, IsBasic: true),
            //new("Create Products", AppActions.Create, AppResources.Products),
            //new("Update Products", AppActions.Update, AppResources.Products),
            //new("Delete Products", AppActions.Delete, AppResources.Products),
            //new("Export Products", AppActions.Export, AppResources.Products),

            ////brands
            //new("View Brands", AppActions.View, AppResources.Brands, IsBasic: true),
            //new("Search Brands", AppActions.Search, AppResources.Brands, IsBasic: true),
            //new("Create Brands", AppActions.Create, AppResources.Brands),
            //new("Update Brands", AppActions.Update, AppResources.Brands),
            //new("Delete Brands", AppActions.Delete, AppResources.Brands),
            //new("Export Brands", AppActions.Export, AppResources.Brands),

            ////todos
            //new("View Todos", AppActions.View, AppResources.Todos, IsBasic: true),
            //new("Search Todos", AppActions.Search, AppResources.Todos, IsBasic: true),
            //new("Create Todos", AppActions.Create, AppResources.Todos),
            //new("Update Todos", AppActions.Update, AppResources.Todos),
            //new("Delete Todos", AppActions.Delete, AppResources.Todos),
            //new("Export Todos", AppActions.Export, AppResources.Todos),

            new("View Hangfire", AppActions.View, AppResources.Hangfire),
            new("View Dashboard", AppActions.View, AppResources.Dashboard),

            //audit
            new("View Audit Trails", AppActions.View, AppResources.AuditTrails),
        ];

        public static IReadOnlyList<AppPermission> All { get; } = new ReadOnlyCollection<AppPermission>(AllPermissions);
        public static IReadOnlyList<AppPermission> Root { get; } = new ReadOnlyCollection<AppPermission>(AllPermissions.Where(p => p.IsRoot).ToArray());
        public static IReadOnlyList<AppPermission> Admin { get; } = new ReadOnlyCollection<AppPermission>(AllPermissions.Where(p => !p.IsRoot).ToArray());
        public static IReadOnlyList<AppPermission> Basic { get; } = new ReadOnlyCollection<AppPermission>(AllPermissions.Where(p => p.IsBasic).ToArray());
    }

    public record AppPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
    {
        public string Name => NameFor(Action, Resource);
        public static string NameFor(string action, string resource)
        {
            return $"Permissions.{resource}.{action}";
        }
    }
}
