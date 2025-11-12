# Product Requirements Document: URL Structure Refactoring

## Overview

The current MathFlow application uses implementation-centric URL structure that exposes internal architecture details (e.g., `/Identity/Manage/ChangePassword`) and uses CamelCase in URLs, which is not aligned with modern web conventions. This refactoring aims to transform all application URLs to be user-centric, semantic, and follow kebab-case convention, improving user experience, SEO, and overall application professionalism.

**Problem:** URLs currently reveal implementation details (ASP.NET Identity structure) and use non-standard casing, making them less intuitive for users and harder to remember.

**Solution:** Restructure all URLs to be user-focused, semantic, and follow web best practices (kebab-case), while maintaining file organization in PascalCase for C# conventions.

**Target Users:** End users navigating the application, developers maintaining the codebase, and search engines indexing the site.

## Goals

- **Primary Goal:** Transform all application URLs to be user-centric and follow kebab-case convention
- **User Experience:** Make URLs intuitive, memorable, and aligned with user mental models
- **SEO Improvement:** Implement clean, semantic URLs that are search-engine friendly
- **Code Quality:** Maintain clean separation between URL structure (user-facing) and file organization (developer-facing)
- **Consistency:** Establish a clear URL convention that can be followed for all future features

**Success Metrics:**
- 100% of application URLs follow kebab-case convention
- 0 URLs expose implementation details (Identity, Manage, Account as technical terms)
- All internal links updated to use new URL structure
- Zero broken links after refactoring

## User Stories

### Primary User Stories

**As a user**, I want to access my profile settings through intuitive URLs like `/profile` and `/settings/password` so that I can easily remember and bookmark important pages.

**As a user**, I want to see clean, professional URLs in my browser's address bar so that I feel confident in the application's quality and professionalism.

**As a developer**, I want URL structure to be independent of internal code organization so that I can refactor backend code without breaking user-facing URLs.

**As a new user**, I want authentication URLs like `/account/login` and `/account/register` to be self-explanatory so that I can navigate the application without confusion.

### Secondary User Stories

**As a search engine**, I want semantic, keyword-rich URLs so that I can properly index and rank the application's pages.

**As a user sharing links**, I want clean URLs that I'm not embarrassed to share with colleagues or friends.

**As a mobile user**, I want shorter, cleaner URLs that are easier to read on small screens.

## Core Features

### F1: Authentication URL Refactoring

**What:** Transform authentication-related URLs to be user-centric and kebab-case.

**Why:** Authentication is the entry point for users; URLs must be immediately recognizable and professional.

**Functional Requirements:**

**R1.1:** Login page must be accessible at `/account/login` (currently `/Account/Login`)

**R1.2:** Registration page must be accessible at `/account/register` (currently `/Account/Register`)

**R1.3:** Logout endpoint must be accessible at `/account/logout` (currently `/Account/Logout`)

**R1.4:** Two-factor authentication page must be accessible at `/account/two-factor` (currently `/Account/TwoFactor`)

**R1.5:** Two-factor setup page must be accessible at `/account/two-factor-setup` (currently `/Account/TwoFactorSetup`)

**R1.6:** External login callback must be accessible at `/account/external-login` (currently `/Account/ExternalLogin`)

**R1.7:** Access denied page must be accessible at `/account/access-denied` (currently `/Account/AccessDenied`)

**R1.8:** Lockout page must be accessible at `/account/lockout` (currently `/Account/Lockout`)

### F2: Profile and Settings URL Refactoring

**What:** Restructure user profile and settings URLs to separate concerns clearly.

**Why:** Users need clear distinction between viewing profile information and modifying settings.

**Functional Requirements:**

**R2.1:** User profile view must be accessible at `/profile` (currently `/Identity/Manage`)

**R2.2:** Password change page must be accessible at `/settings/password` (currently `/Identity/Manage/ChangePassword`)

**R2.3:** Security settings (2FA management) must be accessible at `/settings/security` (currently `/Identity/Manage/TwoFactorAuthentication`)

### F3: Administrative URL Refactoring

**What:** Ensure administrative URLs follow the same kebab-case convention.

**Why:** Consistency across all application areas improves maintainability and user experience.

**Functional Requirements:**

**R3.1:** User management page must be accessible at `/admin/users` (currently `/Admin/Users`)

**R3.2:** User edit page must be accessible at `/admin/users/edit` with id parameter (currently `/Admin/Users/Edit`)

### F4: Public Pages URL Refactoring

**What:** Ensure public-facing pages follow kebab-case convention.

**Why:** Public pages are often the first impression; they must follow best practices.

**Functional Requirements:**

**R4.1:** Home page remains at `/` (currently `/`)

**R4.2:** Privacy policy page must be accessible at `/privacy` (currently `/Public/Privacy`)

**R4.3:** Error page must be accessible at `/error` (currently `/Error`)

### F5: File Organization Preservation

**What:** Maintain PascalCase file and class naming while using kebab-case URLs.

**Why:** C# conventions require PascalCase for classes; separation of concerns between URLs and code organization.

**Functional Requirements:**

**R5.1:** All `.cshtml` and `.cshtml.cs` files must maintain PascalCase naming (e.g., `ChangePassword.cshtml`)

**R5.2:** All C# classes and namespaces must maintain PascalCase naming

**R5.3:** URL routing must be configured using `@page` directive with kebab-case paths

**R5.4:** Folder structure can remain organized by technical concerns (Account, Identity, Admin) as this is internal organization

### F6: Link Updates

**What:** Update all internal navigation links to use new URL structure.

**Why:** Broken links create poor user experience and indicate incomplete refactoring.

**Functional Requirements:**

**R6.1:** All `asp-page` tag helper references must be updated to point to correct pages

**R6.2:** All `RedirectToPage` calls in C# code must use correct page paths

**R6.3:** All hardcoded URL strings must be updated to new structure

**R6.4:** Navigation menu links must reflect new URL structure

## User Experience

### User Personas

**Primary Persona - End User (Student/Teacher):**
- Needs: Clear, intuitive navigation
- Pain Point: Confused by technical URLs like `/Identity/Manage`
- Benefit: Can easily remember and bookmark important pages

**Secondary Persona - Developer:**
- Needs: Clean code organization, maintainable structure
- Pain Point: URL structure tightly coupled to code organization
- Benefit: Can refactor code without breaking URLs

### Key User Flows

**Flow 1: User Authentication**
1. User navigates to `/account/login`
2. After login, redirected to `/profile` or intended destination
3. Can access `/settings/password` or `/settings/security` from profile

**Flow 2: Profile Management**
1. User clicks "Profile" in navigation → `/profile`
2. User clicks "Change Password" → `/settings/password`
3. User clicks "Security Settings" → `/settings/security`

**Flow 3: Admin User Management**
1. Admin navigates to `/admin/users`
2. Admin clicks "Edit" on a user → `/admin/users/edit?id={userId}`
3. Admin returns to user list → `/admin/users`

### UI/UX Considerations

- URLs must be visible and readable in browser address bar
- Breadcrumb navigation should reflect URL structure
- Navigation menu labels should align with URL segments (e.g., "Profile" → `/profile`)
- URLs should be shareable and bookmarkable

### Accessibility Requirements

- URLs must be screen-reader friendly (kebab-case is more readable than CamelCase)
- Navigation structure must be logical and hierarchical
- Keyboard navigation must work with new URL structure

## High-Level Technical Constraints

### Required Integrations
- ASP.NET Core Razor Pages routing system
- Existing authentication and authorization middleware
- Tag helpers (`asp-page`, `asp-route-*`)

### Performance Targets
- URL resolution must not add measurable latency (< 1ms overhead)
- Page load times must remain unchanged

### Technology Requirements
- Must use `@page` directive for custom routing
- Must maintain compatibility with ASP.NET Core 9.0
- Must work with existing Identity system

### Data Sensitivity
- No sensitive data in URLs (already compliant)
- Session/authentication tokens must remain in cookies, not URLs

## Non-Goals (Out of Scope)

### Explicitly Excluded

**NG1:** Creating new pages (e.g., `/settings/account` for profile editing) - this is future work

**NG2:** Implementing URL redirects from old to new URLs - application is not in production, no users to migrate

**NG3:** Changing internal code organization or folder structure beyond what's necessary for routing

**NG4:** Modifying database schema or data models

**NG5:** Implementing URL versioning (e.g., `/v1/profile`)

**NG6:** Internationalization of URLs (e.g., `/pt/perfil`)

### Future Considerations

- Advanced profile editing capabilities at `/settings/account`
- API endpoint URL structure (if REST API is added)
- URL shortening or aliasing features

### Boundaries and Limitations

- This refactoring focuses solely on user-facing URLs
- Internal routing logic and middleware remain unchanged
- No changes to authentication/authorization logic

## Phased Rollout Plan

### MVP (Phase 1) - Core URL Refactoring

**Scope:** Refactor all existing URLs to new structure

**Deliverables:**
- All authentication URLs converted to kebab-case under `/account/*`
- Profile and settings URLs restructured to `/profile` and `/settings/*`
- Admin URLs converted to kebab-case under `/admin/*`
- All internal links updated

**Success Criteria:**
- Application runs without errors
- All pages accessible via new URLs
- No broken internal links
- Manual testing confirms all flows work

**Estimated Effort:** 1-2 days

### Phase 2 (Future) - Enhanced Profile Features

**Scope:** Add comprehensive profile editing capabilities

**Deliverables:**
- New `/settings/account` page for editing personal information
- Enhanced profile view with more user data
- Profile picture upload functionality

**Success Criteria:**
- Users can edit all profile information
- Changes persist correctly
- Validation works as expected

### Phase 3 (Future) - Advanced Features

**Scope:** Additional user-facing features

**Deliverables:**
- User activity history at `/profile/activity`
- Notification preferences at `/settings/notifications`
- Connected accounts management at `/settings/connections`

**Success Criteria:**
- All new features follow established URL conventions
- User engagement with new features meets targets

## Success Metrics

### User Engagement Metrics
- **Bounce rate on authentication pages:** Should remain stable or improve (< 5% change)
- **Time to complete authentication:** Should remain unchanged
- **Profile page visits:** Track baseline for future comparison

### Technical Metrics
- **URL resolution time:** < 1ms overhead from routing changes
- **Page load time:** No regression (< 50ms difference)
- **Link coverage:** 100% of internal links updated and functional

### Quality Metrics
- **Broken links:** 0 broken internal links after refactoring
- **URL convention compliance:** 100% of URLs follow kebab-case
- **Code quality:** No increase in cyclomatic complexity

### Business Impact
- **User satisfaction:** No negative feedback about navigation
- **Developer velocity:** Maintain or improve development speed
- **SEO readiness:** All URLs follow best practices for future SEO efforts

## Risks and Mitigations

### R1: Breaking Changes During Refactoring

**Risk:** Incomplete refactoring could leave some links broken

**Likelihood:** Medium

**Impact:** High (broken user experience)

**Mitigation:**
- Create comprehensive checklist of all pages and links
- Use IDE search functionality to find all `asp-page` references
- Implement systematic testing of all navigation paths
- Test in development environment before any deployment

### R2: Developer Confusion

**Risk:** Developers might be confused by separation of file names (PascalCase) and URLs (kebab-case)

**Likelihood:** Medium

**Impact:** Low (slows development temporarily)

**Mitigation:**
- Document URL conventions clearly in README
- Provide examples of `@page` directive usage
- Create developer guide for adding new pages
- Establish code review checklist for URL compliance

### R3: Future Maintenance Burden

**Risk:** Maintaining custom `@page` directives for every file adds overhead

**Likelihood:** Low

**Impact:** Low (minor inconvenience)

**Mitigation:**
- Establish clear conventions that are easy to follow
- Create templates for new pages with correct `@page` directive
- Document the pattern in developer guidelines

### R4: Testing Coverage Gaps

**Risk:** Missing some edge cases or less-frequently-used pages

**Likelihood:** Medium

**Impact:** Medium (some broken links in production)

**Mitigation:**
- Create comprehensive test plan covering all pages
- Manual testing of all user flows
- Automated link checking (if tooling available)
- Staged rollout with testing at each stage

## Open Questions

### Q1: Converter Pages
**Question:** How should converter-related pages be structured? Currently under `/Converters/*`

**Options:**
- Keep as `/converters/*` (kebab-case)
- Move to `/tools/*` (more user-friendly)
- Create separate section like `/convert/*`

**Decision Needed By:** Before implementation starts

### Q2: Error Handling Pages
**Question:** Should error pages follow a specific pattern?

**Current:** `/Error`

**Options:**
- Keep as `/error`
- Use `/errors/{code}` for specific error codes
- Create `/error/not-found`, `/error/server-error`, etc.

**Decision Needed By:** Before implementation starts

### Q3: Future API Endpoints
**Question:** If REST API is added, should it follow similar conventions?

**Consideration:** API URLs typically use different conventions (e.g., `/api/v1/users`)

**Decision Needed By:** When API development begins (not blocking current work)

## Appendix

### Current URL Inventory

#### Authentication Pages (Account/)
- `/Account/Login` → `/account/login`
- `/Account/Register` → `/account/register`
- `/Account/Logout` → `/account/logout`
- `/Account/TwoFactor` → `/account/two-factor`
- `/Account/TwoFactorSetup` → `/account/two-factor-setup`
- `/Account/ExternalLogin` → `/account/external-login`
- `/Account/AccessDenied` → `/account/access-denied`
- `/Account/Lockout` → `/account/lockout`

#### Profile & Settings (Identity/Manage/)
- `/Identity/Manage` → `/profile`
- `/Identity/Manage/ChangePassword` → `/settings/password`
- `/Identity/Manage/TwoFactorAuthentication` → `/settings/security`

#### Admin Pages (Admin/)
- `/Admin/Users` → `/admin/users`
- `/Admin/Users/Edit` → `/admin/users/edit`

#### Public Pages
- `/` → `/` (unchanged)
- `/Public/Privacy` → `/privacy`
- `/Error` → `/error`

### Reference Materials

**Web URL Best Practices:**
- Use lowercase letters
- Use hyphens (kebab-case) for word separation
- Keep URLs short and descriptive
- Avoid exposing technical implementation details
- Make URLs human-readable and memorable

**ASP.NET Core Razor Pages Routing:**
- `@page` directive defines custom route
- `@page "/custom-route"` overrides default convention-based routing
- Route parameters: `@page "{id}"`
- Optional parameters: `@page "{id?}"`

### Implementation Notes

**File Organization Strategy:**
- Keep existing folder structure (Account/, Identity/Manage/, Admin/)
- Add `@page` directive to each `.cshtml` file with custom route
- Update all `asp-page` references to use new paths
- Update all `RedirectToPage()` calls in code-behind

**Testing Strategy:**
- Manual testing of all navigation paths
- Verify all forms submit correctly
- Test authentication flows end-to-end
- Verify admin functions work correctly
- Check all external login flows

## Planning Artifacts (Post-Approval)

[Created by the Tech Spec workflow and maintained alongside this PRD.]

- Docs Plan: `tasks/prd-url-refactoring/_docs.md`
- Examples Plan: `tasks/prd-url-refactoring/_examples.md`
- Tests Plan: `tasks/prd-url-refactoring/_tests.md`
