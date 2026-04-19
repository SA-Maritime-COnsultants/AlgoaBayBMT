# Copilot Instructions

## Project Guidelines
- For this Blazor app, prefer an enterprise maritime dashboard layout: shell belongs in `MainLayout`, with a fixed/collapsible left sidebar, operational topbar, serious compliance-first styling, and a clearly visible logo around 100px+ high.
- Use Syncfusion controls on all forms and lists, keep existing routes/authorization/business logic, and apply changes consistently across all pages with a Portflow-inspired navy/teal dashboard style.
- Implement compact one-row action areas on data grids, utilizing colored icon-only buttons with tooltip text for a neater UI.
- Ensure that auto-generated passwords comply with the application's configured Identity password requirements.
- Use `IDbContextFactory<ApplicationDbContext>` in services instead of injecting a shared `DbContext` directly to ensure safe concurrent component loading in Blazor SSR.
- For AlgoaBayBMT training feature changes, preserve existing workflows and layout, make only the requested UI/reporting updates, keep Blazor pages compile-friendly, and continue using `IDbContextFactory<ApplicationDbContext>` patterns already used in services.

## Training Component Formatting
- When converting training content for AlgoaBayBMT, expect the user to provide plain text plus a selected content type (Slide, Flashcard, Knowledge Check MCQ/True-False, Scenario, Lesson Narrative, Course Overview, Competency Task, or Question Bank Item).
- Return the corresponding training component format while cleaning and structuring the text automatically.

## Video/Player Behavior
- When fixing the training video/player behavior in this repo, preserve the current UI and avoid visual changes unless explicitly requested.