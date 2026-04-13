# Copilot Instructions

## Project Guidelines
- For this Blazor app, prefer an enterprise maritime dashboard layout: shell belongs in `MainLayout`, with a fixed/collapsible left sidebar, operational topbar, serious compliance-first styling, and a clearly visible logo around 100px+ high.
- Use Syncfusion controls on all forms and lists, keep existing routes/authorization/business logic, and apply changes consistently across all pages with a Portflow-inspired navy/teal dashboard style.
- Implement compact one-row action areas on data grids, utilizing colored icon-only buttons with tooltip text for a neater UI.
- Ensure that auto-generated passwords comply with the application's configured Identity password requirements.
- Use `IDbContextFactory<ApplicationDbContext>` in services instead of injecting a shared `DbContext` directly to ensure safe concurrent component loading in Blazor SSR.