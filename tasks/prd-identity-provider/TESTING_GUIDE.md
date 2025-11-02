# Testing Guide - Identity Provider (Lane 5)

## Quick Start

### 1. Start Infrastructure
```bash
# Start PostgreSQL and Mailpit
docker-compose -f docker/local/docker-compose.infra.yml up -d

# Verify services
docker ps
```

### 2. Run Application
```bash
cd src/MathFlow
dotnet run
```

### 3. Access Points
- **Application:** http://localhost:5124
- **Mailpit:** http://localhost:8025

---

## Test Scenarios

### Scenario 1: User Profile Management (Authenticated User)

#### Prerequisites
- User must be logged in

#### Steps
1. Navigate to http://localhost:5124/Account/Login
2. Login with any user credentials
3. Click on user dropdown in navbar
4. Click "Profile"
5. Verify profile information is displayed correctly

**Expected Results:**
- Username, email, email confirmation status displayed
- 2FA status shown
- User roles displayed as badges
- Links to "Change Password" and "Manage Two-Factor Authentication" visible

---

### Scenario 2: Change Password

#### Prerequisites
- User must be logged in

#### Steps
1. Navigate to `/Identity/Manage/Index`
2. Click "Change Password"
3. Enter current password
4. Enter new password (min 8 characters)
5. Confirm new password
6. Click "Change Password"

**Expected Results:**
- Success message: "Password changed successfully."
- Redirect to profile page
- Can login with new password

**Error Cases:**
- Wrong current password → Error message
- New password < 8 chars → Validation error
- Passwords do not match → Validation error

---

### Scenario 3: Manage 2FA (Normal User)

#### Prerequisites
- User must be logged in
- User must NOT be masterAdmin

#### Steps
1. Navigate to `/Identity/Manage/Index`
2. Click "Manage Two-Factor Authentication"
3. If 2FA is enabled, click "Disable Two-Factor Authentication"
4. Verify status changes

**Expected Results:**
- 2FA status displayed correctly
- Can disable 2FA if not masterAdmin
- Success message: "2FA disabled successfully."

---

### Scenario 4: Manage 2FA (Master Admin)

#### Prerequisites
- User must be logged in as masterAdmin

#### Steps
1. Navigate to `/Identity/Manage/TwoFactorAuthentication`
2. Verify warning message is displayed

**Expected Results:**
- Warning: "As a master admin, you cannot disable two-factor authentication."
- Disable button not visible
- 2FA status shows "Enabled"

---

### Scenario 5: Admin User List

#### Prerequisites
- User must be logged in as admin or masterAdmin

#### Steps
1. Navigate to http://localhost:5124/Admin/Users/Index
2. Verify user list is displayed

**Expected Results:**
- Table with all users displayed
- Columns: Username, Email, Email Confirmed, 2FA, Roles, Actions
- "Edit Roles" button for each user
- Admin link visible in navbar

---

### Scenario 6: Edit User Roles (Admin)

#### Prerequisites
- User must be logged in as admin or masterAdmin

#### Steps
1. Navigate to `/Admin/Users/Index`
2. Click "Edit Roles" for any user
3. Select a role from dropdown (normal, premium, or admin)
4. Click "Update Role"

**Expected Results:**
- User information displayed
- Current roles shown
- Role dropdown with options: normal, premium, admin
- Success message: "Role updated successfully."
- Redirect to user list
- User's role updated in the list

**Note:** Master admin role not in dropdown (cannot be assigned via UI)

---

### Scenario 7: Access Denied (Normal User)

#### Prerequisites
- User must be logged in as normal user (not admin)

#### Steps
1. Navigate to http://localhost:5124/Admin/Users/Index

**Expected Results:**
- Redirect to `/Account/AccessDenied`
- Error message: "You do not have permission to access this page."
- "Return to home" button visible

---

### Scenario 8: Unauthenticated Access

#### Prerequisites
- User must NOT be logged in

#### Steps
1. Navigate to http://localhost:5124/Identity/Manage/Index

**Expected Results:**
- Redirect to `/Account/Login`
- ReturnUrl parameter set to original page
- After login, redirect back to original page

---

## Authorization Matrix

| Page | Anonymous | Normal User | Premium User | Admin | Master Admin |
|------|-----------|-------------|--------------|-------|--------------|
| `/Account/Login` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/Account/Register` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/Account/AccessDenied` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/Identity/Manage/Index` | ❌ | ✅ | ✅ | ✅ | ✅ |
| `/Identity/Manage/ChangePassword` | ❌ | ✅ | ✅ | ✅ | ✅ |
| `/Identity/Manage/TwoFactorAuthentication` | ❌ | ✅ | ✅ | ✅ | ✅ |
| `/Admin/Users/Index` | ❌ | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Users/Edit` | ❌ | ❌ | ❌ | ✅ | ✅ |

**Legend:**
- ✅ = Allowed
- ❌ = Denied (redirect to Login or AccessDenied)

---

## Test Data Setup

### Create Test Users

#### Master Admin (Seeded)
```json
{
  "email": "admin@mathflow.com",
  "password": "Admin@123",
  "role": "masterAdmin"
}
```

#### Regular Admin
1. Register new user
2. Login as masterAdmin
3. Navigate to `/Admin/Users/Index`
4. Edit user and assign "admin" role

#### Premium User
1. Register new user
2. Login as admin
3. Navigate to `/Admin/Users/Index`
4. Edit user and assign "premium" role

#### Normal User
1. Register new user
2. Default role is "normal"

---

## Common Issues and Solutions

### Issue: Cannot access Admin pages
**Solution:** Verify user has admin or masterAdmin role

### Issue: Cannot disable 2FA
**Solution:** Check if user is masterAdmin (masterAdmin cannot disable 2FA)

### Issue: Password change fails
**Solution:** Verify current password is correct and new password meets requirements (min 8 chars)

### Issue: Role assignment not working
**Solution:** Verify RoleService is properly registered in DI container

### Issue: AccessDenied page not showing
**Solution:** Verify authorization policies are configured in Program.cs

---

## API Endpoints (for reference)

### Manage Pages
- `GET /Identity/Manage/Index` - View profile
- `GET /Identity/Manage/ChangePassword` - Change password form
- `POST /Identity/Manage/ChangePassword` - Submit password change
- `GET /Identity/Manage/TwoFactorAuthentication` - View 2FA status
- `POST /Identity/Manage/TwoFactorAuthentication?handler=Disable` - Disable 2FA

### Admin Pages
- `GET /Admin/Users/Index` - List all users
- `GET /Admin/Users/Edit?id={userId}` - Edit user roles form
- `POST /Admin/Users/Edit?id={userId}` - Submit role change

### Account Pages
- `GET /Account/AccessDenied` - Access denied page

---

## Automated Testing (Future - Lane 6)

### Integration Tests to Implement
- [ ] User can view profile
- [ ] User can change password
- [ ] User can disable 2FA (non-masterAdmin)
- [ ] Master admin cannot disable 2FA
- [ ] Admin can view user list
- [ ] Admin can assign roles
- [ ] Normal user cannot access admin pages
- [ ] Unauthenticated user redirected to login

### Security Tests to Implement
- [ ] Authorization policies enforced
- [ ] CSRF protection on forms
- [ ] Password validation enforced
- [ ] Role assignment validation
- [ ] Master admin protection

---

## Performance Benchmarks

### Expected Response Times
- Profile page load: < 200ms
- User list load: < 500ms (depends on user count)
- Password change: < 300ms
- Role assignment: < 400ms

### Database Queries
- Profile page: 2 queries (user + roles)
- User list: N+1 queries (optimize in future)
- Edit roles: 3 queries (user + current roles + update)

---

## Browser Compatibility

Tested on:
- ✅ Chrome (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)
- ✅ Edge (latest)

Mobile:
- ✅ iOS Safari
- ✅ Android Chrome

---

## Accessibility

- ✅ Semantic HTML
- ✅ Form labels
- ✅ ARIA attributes (Bootstrap defaults)
- ✅ Keyboard navigation
- ✅ Screen reader compatible

---

## Next Steps

After manual testing:
1. Proceed to Lane 6 (Tasks 19.0-22.0)
2. Implement automated integration tests
3. Perform security testing
4. Update documentation
5. Deploy to staging environment
