# Identity Provider - Product Requirements Document

## Overview

Identity Provider introduces centralized authentication and authorization for MathFlow using ASP.NET Core Identity. The feature enables secure user onboarding, mandatory two-factor verification, and static role-based access so that the platform can differentiate privileges between master administrators, administrators, premium users, normal users, and unauthenticated visitors. It also aligns the infrastructure `IdentityUser` with the future domain `User` aggregate to support upcoming wallet and usage tracking capabilities.

## Goals

- Establish a production-ready authentication stack with ASP.NET Core Identity and Google external login.
- Enforce two-factor authentication for every authenticated role except `masterAdmin`.
- Deliver static role assignments (`masterAdmin`, `admin`, `premium`, `normal`) with durable storage and audit-friendly logging.
- Provide self-service registration with strong password policies and automatic role defaults.
- Prepare the infrastructure for linking authenticated users to domain entities such as wallets and usage logs.

## User Stories

- As a `masterAdmin`, I want to access all management features without two-factor prompts so that I can maintain the system during incidents.
- As an `admin`, I want to manage content and monitor premium usage, provided I complete two-factor verification, so that premium users receive consistent service.
- As a `premium` user, I want to log in with email or Google, complete two-factor verification, and access premium-only features so that my subscription benefits are protected.
- As a `normal` user, I want to self-register, verify my account, and access public exercises and track personal progress while being prompted to upgrade for premium capabilities.
- As a `visitant`, I want to browse public content and reach registration and login flows without friction so that I can evaluate MathFlow before committing.

## Core Features

- **Authentication Foundation**
  - ASP.NET Core Identity integration with password hashing, lockout policies, and recovery flows.
  - Google external login support with secure callback handling.
  - Mandatory two-factor authentication for all roles except `masterAdmin`.
  - **Functional Requirements**
    1. System shall persist users using ASP.NET Core Identity stores within `Infrastructure/IdentityServer`.
    2. System shall support registration through email/password and Google OAuth 2.0.
    3. System shall require two-factor authentication completion before issuing an authenticated session for any role other than `masterAdmin`.
    4. System shall enforce password rules: minimum eight characters, at least one uppercase letter, and at least one special character.

- **Role and Permission Model**
  - Static role definitions stored in Identity role tables.
  - Startup seeding for `masterAdmin`, `admin`, `premium`, and `normal` roles.
  - Authorization policies mapping controller endpoints and actions to appropriate roles.
  - **Functional Requirements**
    5. System shall assign the `normal` role by default to self-registered users after successful two-factor verification.
    6. System shall allow administrators to promote users to other static roles through secured administrative workflows.
    7. System shall gate administrative dashboards to `admin` and `masterAdmin` roles.
    8. System shall allow `masterAdmin` to bypass two-factor enforcement while remaining audit logged.

- **Domain Alignment**
  - Infrastructure `ApplicationUser : IdentityUser` exposed to the domain layer for simplicity.
  - Domain services able to reference identity user identifiers to link wallets, usage analytics, and logs.
  - Event hooks or transaction scripts to ensure domain invariants (for example, default wallet creation) can subscribe to identity lifecycle events in future iterations.
  - **Functional Requirements**
    9. Infrastructure project shall define `Infrastructure/IdentityServer` folder containing Identity configuration, context, and migrations.
    10. Domain layer shall accept identity user identifiers when creating aggregates that depend on authenticated users.
    11. System shall expose extension points (domain services or application services) for future wallet linkage triggered post-registration.

## User Experience

- **Personas**
  - `masterAdmin`: internal owner responsible for operations.
  - `admin`: operational staff managing content and premium workflows.
  - `premium`: paying users with elevated feature access.
  - `normal`: registered users without premium benefits.
  - `visitant`: anonymous user exploring public resources.
- **Flows**
  - Registration: visitant chooses email/password or Google, completes password strength validation, receives two-factor enrollment prompt, completes second factor, receives `normal` role.
  - Authentication: existing users submit credentials, complete two-factor (if required), and are redirected according to role.
  - Role upgrade: admin promotes normal user to premium via administrative panel (static set of roles).
- **UI/UX Considerations**
  - Provide accessible forms with clear error messaging and input labels.
  - Two-factor prompts should support common authenticators (for example, authenticator apps via time-based one-time passwords) and display recovery options.
  - Login and registration pages should remain consistent with global C# Razor conventions already in MathFlow.
- **Accessibility**
  - Meet WCAG 2.1 Level AA standards for all authentication flows.
  - Ensure all interactive elements are keyboard navigable and screen-reader friendly.
  - Maintain sufficient contrast and provide descriptive feedback for validation errors.

## High-Level Technical Constraints

- Integration limited to ASP.NET Core Identity; no additional third-party identity providers beyond Google for this release.
- Infrastructure follows layered architecture: `Domain`, `Application/Service`, `Infrastructure`, with Identity assets located under `Infrastructure/IdentityServer`.
- Two-factor authentication must support authenticator apps; SMS and email codes are out of scope.
- Logging must capture authentication events for traceability while respecting privacy. Logs should avoid storing sensitive credential data.
- Password storage relies on ASP.NET Core Identity password hashing (PBKDF2) by default.

## Non-Goals (Out of Scope)

- Dynamic role or permission creation by end users.
- Wallet management and credit enforcement (future dependency only).
- Multi-tenant identity structures or external directory federation.
- Advanced security features such as conditional access, risk-based authentication, or hardware key support.
- SMS or email-based two-factor authentication methods.

## Phased Rollout Plan

- **MVP**
  - Deploy ASP.NET Core Identity with email/password registration, Google login, static roles, and two-factor enforcement.
  - Seed `masterAdmin` account with bypass capability and enforce password policy.
  - Provide administrative interface for assigning static roles.
  - Success Indicators: users can register and authenticate, administrators can assign roles, two-factor enforcement blocks access until completion.
- **Phase 2**
  - Integrate audit logging dashboards for authentication events and role changes.
  - Introduce hooks for wallet provisioning and usage tracking upon successful registration.
  - Success Indicators: authenticated events visible to admins, domain services receive identity linkage triggers.
- **Phase 3**
  - Explore premium feature gating based on credits or subscriptions once wallet model is defined.
  - Extend two-factor options and introduce recovery codes management.
  - Success Indicators: premium gating controlled by credits/subscriptions, recovery flows reduce support overhead.

## Success Metrics

- Registration success rate ≥ 95% for completed two-factor flows.
- Authentication success rate ≥ 99% without lockout for valid credentials.
- Zero critical security incidents related to identity in the first release cycle.
- Administrative role changes logged with 100% coverage.
- Two-factor completion rate ≥ 90% among eligible roles (all non-master accounts).

## Risks and Mitigations

- **Two-Factor Friction**: Users may abandon onboarding when prompted for two-factor authentication.
  - Mitigation: Provide clear guidance and recovery codes to simplify setup.
- **Static Role Drift**: Hard-coded roles may complicate future expansion.
  - Mitigation: Document role responsibilities and prepare migration scripts for future configurable roles.
- **Domain Coupling**: Exposing infrastructure `IdentityUser` directly could increase coupling.
  - Mitigation: Encapsulate identity interactions in application services, enabling future refactor to dedicated domain aggregate if needed.
- **Operational Overload**: Single master administrator may become bottleneck for role changes.
  - Mitigation: Provide secure admin panel workflows and audit logs to distribute responsibility to admin role.
- **External Provider Dependency**: Google OAuth service outages could block external login flows.
  - Mitigation: Implement monitoring for OAuth endpoint availability, provide clear error messaging directing users to email/password fallback, and maintain service status communication.

## Open Questions

- None at this time.

## Appendix

- Reference: ASP.NET Core Identity documentation (`/dotnet/aspnetcore`, Identity security guidance).
- Planned directory structure example:
  - `Infrastructure/IdentityServer/` (context, stores, seeding, configuration)
  - `Infrastructure/Migrations/` (database migrations for identity schema)
- Future domain linkage sketch: wallet and usage services subscribe to registration completion events.

## Planning Artifacts (Post-Approval)

- Docs Plan: `tasks/prd-identity-provider/_docs.md` (template: `tasks/docs/_docs-plan-template.md`)
- Examples Plan: `tasks/prd-identity-provider/_examples.md` (template: `tasks/docs/_examples-plan-template.md`)
- Tests Plan: `tasks/prd-identity-provider/_tests.md` (template: `tasks/docs/_tests-plan-template.md`)
