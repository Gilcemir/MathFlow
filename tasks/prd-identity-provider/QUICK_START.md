# Identity Provider - Quick Start Guide

## Overview
This guide helps you quickly set up and test the Identity Provider implementation (Tasks 5.0-8.0).

**⚠️ IMPORTANT**: Email confirmation is now REQUIRED for user registration. Users must confirm their email before they can sign in. See `EMAIL_CONFIRMATION_UPDATE.md` for implementation details.

---

## Prerequisites

- ✅ PostgreSQL running (via Docker or local)
- ✅ .NET 9.0 SDK installed
- ✅ Tasks 1.0-4.0 completed (database and migrations)

---

## Step 1: Configure Master Admin

Create or update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mathflow_db;Username=mathflow;Password=mathflow_password_change_me"
  },
  "Identity": {
    "MasterAdmin": {
      "Email": "admin@mathflow.local",
      "Password": "Dev@1234"
    }
  },
  "Authentication": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

**Note**: Password must have 8+ characters, 1 uppercase, 1 special character.

---

## Step 2: Apply Migrations

```bash
cd src/MathFlow
dotnet ef database update
```

**Expected output**: Migration applied successfully.

---

## Step 3: Build and Run

```bash
dotnet build
dotnet run
```

**Look for these log messages**:
- "Roles seeded successfully"
- "Master admin user created: admin@mathflow.local"

---

## Step 4: Verify Database

### Check Roles (should be 4)
```bash
docker exec -it mathflow-postgres psql -U mathflow -d mathflow_db \
  -c "SELECT \"Id\", \"Name\" FROM \"AspNetRoles\";"
```

**Expected output**:
```
                  Id                  |    Name     
--------------------------------------+-------------
 <guid>                               | masterAdmin
 <guid>                               | admin
 <guid>                               | premium
 <guid>                               | normal
```

### Check Master Admin
```bash
docker exec -it mathflow-postgres psql -U mathflow -d mathflow_db \
  -c "SELECT \"UserName\", \"Email\", \"TwoFactorEnabled\" FROM \"AspNetUsers\";"
```

**Expected output**:
```
       UserName        |         Email          | TwoFactorEnabled 
-----------------------+------------------------+------------------
 admin@mathflow.local  | admin@mathflow.local   | f
```

### Check Role Assignment
```bash
docker exec -it mathflow-postgres psql -U mathflow -d mathflow_db \
  -c "SELECT u.\"Email\", r.\"Name\" FROM \"AspNetUsers\" u 
      JOIN \"AspNetUserRoles\" ur ON u.\"Id\" = ur.\"UserId\"
      JOIN \"AspNetRoles\" r ON ur.\"RoleId\" = r.\"Id\";"
```

**Expected output**:
```
         Email          |    Name     
------------------------+-------------
 admin@mathflow.local   | masterAdmin
```

---

## Step 5: Test Authorization Policies

### Create a Test Page

Create `Pages/Test/AdminOnly.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Infrastructure.IdentityServer.Configuration;

namespace MathFlow.Pages.Test;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminOnlyModel : PageModel
{
    public void OnGet()
    {
    }
}
```

Create `Pages/Test/AdminOnly.cshtml`:

```html
@page
@model MathFlow.Pages.Test.AdminOnlyModel

<h1>Admin Only Page</h1>
<p>If you can see this, authorization is working!</p>
```

### Test Access
1. Navigate to `https://localhost:5124/Test/AdminOnly`
2. Should redirect to `/Account/Login` (not implemented yet)
3. This confirms authorization middleware is working

---

## Troubleshooting

### Issue: "Master admin credentials not configured"
**Solution**: Add Identity section to `appsettings.Development.json`

### Issue: "Failed to create master admin: Passwords must have..."
**Solution**: Use password with 8+ chars, 1 uppercase, 1 special character  
**Example**: `Dev@1234`, `Admin!2024`, `Test#Pass1`

### Issue: "Unable to resolve service for type 'ApplicationDbContext'"
**Solution**: Ensure you are running the application, not just building it

### Issue: Roles not appearing in database
**Solution**: 
1. Check seeder logs for errors
2. Verify connection string is correct
3. Ensure migrations were applied

---

## What's Available Now

### Services Registered
- ✅ `ApplicationDbContext` - Database access
- ✅ `UserManager<ApplicationUser>` - User management
- ✅ `RoleManager<IdentityRole>` - Role management
- ✅ `SignInManager<ApplicationUser>` - Sign-in operations

### Authorization Policies
- ✅ `AuthorizationPolicies.MasterAdminOnly`
- ✅ `AuthorizationPolicies.AdminOnly`
- ✅ `AuthorizationPolicies.PremiumAccess`
- ✅ `AuthorizationPolicies.AuthenticatedUser`

### Database Data
- ✅ 4 roles: masterAdmin, admin, premium, normal
- ✅ 1 master admin user (no 2FA required)

---

## Using Authorization Policies

### In Razor Pages
```csharp
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersModel : PageModel { }
```

### In Controllers
```csharp
[Authorize(Policy = AuthorizationPolicies.PremiumAccess)]
public class PremiumController : Controller { }
```

### In Code
```csharp
public class MyService
{
    private readonly IAuthorizationService _authService;
    
    public async Task<bool> CanAccessPremium(ClaimsPrincipal user)
    {
        var result = await _authService.AuthorizeAsync(
            user, 
            AuthorizationPolicies.PremiumAccess);
        return result.Succeeded;
    }
}
```

---

## Next Steps

### Immediate
1. ✅ Verify all steps above work
2. ✅ Check logs for any warnings
3. ✅ Confirm database seeding

### Development
1. Implement UserService (Task 9.0)
2. Implement RoleService (Task 10.0)
3. Create Login/Register pages (Task 12.0)

---

## Quick Reference

### Configuration Files
- `appsettings.json` - Production config template
- `appsettings.Development.json` - Local development config
- `Program.cs` - Application startup

### Implementation Files
- `IdentityConfiguration.cs` - Identity services setup
- `AuthorizationPolicies.cs` - Policy definitions
- `IdentitySeeder.cs` - Data seeding logic

### Documentation
- `LANE2_COMPLETION_SUMMARY.md` - Complete overview
- `EXECUTION_SUMMARY.md` - Implementation details
- Individual task completion summaries (5.0-8.0)

---

## Support

### Documentation
- Review task completion summaries for detailed information
- Check `LANE2_COMPLETION_SUMMARY.md` for architecture decisions
- See `EXECUTION_SUMMARY.md` for validation steps

### Common Commands
```bash
# Build
dotnet build

# Run
dotnet run

# Apply migrations
dotnet ef database update

# Create new migration
dotnet ef migrations add <MigrationName>

# Check database
docker exec -it mathflow-postgres psql -U mathflow -d mathflow_db
```

---

## Success Checklist

- [ ] Application builds without errors
- [ ] Application runs without errors
- [ ] Seeder logs show success messages
- [ ] 4 roles exist in database
- [ ] Master admin user exists in database
- [ ] Master admin has masterAdmin role
- [ ] Master admin has TwoFactorEnabled = false
- [ ] Authorization policies are accessible in code

**If all items checked**: ✅ **Setup Complete!**

---

## What to Do If Something Fails

1. **Check logs** - Look for error messages in console
2. **Verify configuration** - Ensure appsettings.Development.json is correct
3. **Check database** - Ensure PostgreSQL is running and accessible
4. **Review migrations** - Ensure all migrations are applied
5. **Consult documentation** - Check completion summaries for troubleshooting

---

**Ready to proceed with Lane 3 (Serviços de Aplicação)!**
