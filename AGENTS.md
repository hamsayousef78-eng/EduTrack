# AGENTS.md

Single ASP.NET Core MVC app on **.NET 10** (`EduTrack/EduTrack.csproj`), EF Core + SQL Server, ASP.NET Identity. Solution file is the newer `EduTrack.slnx`. There are **no tests and no CI** (`.github/workflows` is empty); verification is build + manual run.

## Commands

```powershell
dotnet build EduTrack.slnx
dotnet run --project EduTrack          # http://localhost:5250 (only the "http" profile exists)
dotnet watch --project EduTrack
```

EF Core:
- `dotnet-ef` is **not installed** in this environment: `dotnet tool install --global dotnet-ef` first.
- `dotnet ef migrations add <Name> --project EduTrack`
- `dotnet ef database update --project EduTrack`
- The app does **not** migrate or seed at startup.

## First run / data

- After `database update`, open `/Account/SetupSystem` once to create the `Admin`/`Instructor`/`Student` roles and the admin `hamsa@gmail.com` / `admin1234`. It silently ignores duplicate-create failures, so reruns are harmless.
- `appsettings.json` holds the **local (in-datacenter)** connection string used when the site runs on MonsterASP.NET; it resolves to a private IP (`10.x.x.x`) and is **unreachable from a dev machine** (SQL error 53). `appsettings.Development.json` holds the **public** host (`dbXXXX.public.databaseasp.net`) and is what `dotnet run` uses locally.
- Both DBs are the **same shared database**; migrations `20261118204021_start` and `20261123181614_editInstructor` are already applied. Treat it as shared: avoid destructive migrations or drop/recreate without confirmation.
- A fresh DB has no instructors or courses. The admin must create an instructor first, then a course, before student registration works (the Register form requires a `CourseId`).

## Deployment (MonsterASP.NET)

- Publish with Visual Studio **Web Deploy** (Control Panel → website → Deploy → enable WebDeploy → download `.publishSettings`). Keep `appsettings.json` on the local connection string; the host runs in `Production`.
- Run `/Account/SetupSystem` once after the first deploy (idempotent).
- Data Protection keys persist to `App_Data/DataProtection-Keys` (wired in `Program.cs`) so app-pool recycles don't sign everyone out. `App_Data/` is gitignored.
- `wwwroot/uploads/*` and `wwwroot/images/{courses,instructors}/*` are gitignored (only `.gitkeep` is tracked). Uploaded files live on the site's disk and are lost if a publish uses "Remove additional files at destination".
- Publish is framework-dependent `net10.0`; if the host lacks the .NET 10 runtime, publish self-contained or retarget.

## Implementation state (do not assume features work)

- `CourseController` implements `Index` (role-filtered), `Create`, `Edit`, and `Details`; `Views/Course/Edit.cshtml` is implemented.
- `AssignmentController` implements `Create` (GET/POST, Instructor/Admin, only for a course the instructor owns) and `Details` (Admin/owning instructor/enrolled student). `SubmissionController` implements `Submit` and `MySubmissions` (Student only) and `Grade` (Instructor/Admin, owning instructor only). `Views/Assignment` and `Views/Submission` exist; there are no `Index` views for these controllers.
- Links now use the singular controller names (`Course`, `Assignment`, `Submission`); keep that convention (`Home`, `Account`, `Course`, `Instructor`, `Student`, `Assignment`, `Submission`).
- `Views/Course/*`, `Views/Assignment/*`, and `Views/Submission/*` contain Arabic "student guide" comments describing the expected controller behavior; read the top comment before changing those views.
- Authorization: `InstructorController` and `StudentController` are `[Authorize(Roles = "Admin")]`; `Account` is public; `CourseController` is `[Authorize]` with Admin-only `Create`/`Edit`; `AssignmentController.Create` and `SubmissionController.Grade` are Instructor/Admin; `SubmissionController.Submit`/`MySubmissions` are Student-only.

## Conventions and traps

- UI is **Arabic RTL** (`<html lang="ar" dir="rtl">`, Bootstrap RTL CDN, Cairo font). Keep new user-facing text Arabic and preserve RTL markup.
- Domain/EF naming is lowercase and must be matched exactly: `_context.courses`, `_context.instructors`, `course.instructor`, `course.assignments`, `student.submissions`. A `DbSet<T> foos` produces table `foos`. `Student → Course` is explicitly `DeleteBehavior.NoAction`.
- Uploads are stored as **bare filenames** and the views add the folder prefix: course thumbnails in `wwwroot/images/courses`, assignment PDFs in `wwwroot/uploads/assignments`, and student solutions (`.pdf`/`.zip`) in `wwwroot/uploads/submissions`. `InstructorController.SaveImage` still returns `/images/instructors/{file}` while its views prefix `~/images/instructors/` (double path, broken image); fix that separately.
- `ViewModels/CourseFormViewModel.ThumbnailFile` is `IFormFile?` (not a string) so file uploads bind; `Id` and `ExistingThumbnailPath` support the Edit flow.
- `ViewModels/CreateInstructorViewModel .cs` has a space before `.cs` (class name is normal).
- Nullable is enabled and the build currently emits ~31 warnings (mostly CS8618), 0 errors. Treat the warnings as baseline; don't chase them unless asked.
- `Course.cs` / `Assignment.cs` / `Submission.cs` carry unused `Microsoft.Identity.Client.NativeInterop` usings — harmless noise, not a dependency to rely on.
