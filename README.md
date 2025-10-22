St10442488       ///word document includes links,screenshots and references.
Adrian Chetty
Prog Part 2

GitHub link: https://github.com/Addec1/ProgPoeS2.git 
YouTube Video Link for MVC: https://youtu.be/4qPfymI7tOw 
YouTube Video for Unit Testing: https://youtu.be/QfS45TFo_YU  


Part 1 feedback and how I implemented the fixes: 
 
“Uploading the document choice should be given to the lecturer when they create a claim, not on a separate view.”

What was wrong in P1:
The document upload feature lived on a standalone Documents/Upload page, disconnected from the claim workflow.

What I changed in P2:
Integrated upload into the Create Claim page so the lecturer can attach supporting files at the same time they add daily entries.
Client-side validation limits type (PDF/DOCX/XLSX) and size (≤10 MB) in the create form.
Added help text explaining that files are encrypted at rest (Part 2 requirement).

Files touched:
Views/Claims/Create.cshtml – added <input type="file" multiple> + validation + helper text.
Controllers/ClaimsController.cs – extended Create (POST) to accept IFormFileCollection and pass to IFileStore.
Services/EncryptedFileStore.cs – saves with AES encryption for P2.

Why this fixes it:
Upload is now part of the claim creation journey, which aligns with the lecturer’s instruction and rubric’s “user-friendliness” expectations.



2) “My Claims should only show the lecturer’s own claims.”
What was wrong in P1:
The Claims/Index view listed demo data for multiple lecturers.

What I changed in P2:
Introduced a current-user filter (simulated for the prototype) so the list binds only to the signed-in lecturer’s claims.
In the prototype, I parameterize with the lecturer’s name (e.g., Adrian Chetty) and filter server-side before binding.

Files touched:
Controllers/ClaimsController.cs – filters the in-memory list by LecturerName (placeholder for auth).
Views/Claims/Index.cshtml – unchanged structurally; now receives a filtered model.



Why this fixes it:
The lecturer can’t see other lecturers’ claims, matching the least-privilege expectations, and the feedback.

3) “Approvals will be made by the managers, not lecturers.”
What was wrong in P1:
UI actions suggested submission/approval in the same area, and roles were not clearly separated.

What I changed in P2:
Split the workflow by role-specific inboxes:
Coordinator can Verify or Reject submitted claims.
Manager can Approve or Reject verified claims.
Lecturer UI no longer shows any approval buttons—only Create, View, and Submit (disabled/placeholder as required by P1 scope).

Files touched:
Controllers/CoordinatorController.cs – list submitted claims; Verify(id)/Reject(id) actions.
Controllers/ManagerController.cs – list verified claims; Approve(id)/Reject(id) actions.
Views/Coordinator/Index.cshtml, Views/Manager/Index.cshtml – role-appropriate action buttons; document links; status badges.

Why this fixes it:
•	Approval actions are not visible in lecturer screens and exist only in Coordinator/Manager views, following the stated governance.




4) “Managers and coordinators should be able to see all the claims.”

What was wrong in P1:
No distinct inboxes; visibility implied lecturer-centric views.

What I changed in P2:
Built two administrative inboxes that each query all claims relevant to their stage:
Coordinator: shows all submitted claims.
Manager: shows all verified claims.
Both inboxes show lecturer name, month/year, totals, status, and document links (download via /documents/download/{id}).

Files touched:
CoordinatorController.cs, ManagerController.cs
Views/Coordinator/Index.cshtml, Views/Manager/Index.cshtml
DocumentsController.cs – download endpoint
Services/IFileStore.cs, EncryptedFileStore.cs – to supply document streams.

Why this fixes it:
Admin users can see every claim at their stage, not just their own, which is exactly what the feedback requested.








5) “Why is there a separate view to upload documents?”

What was wrong in P1:
A dedicated Documents/Upload page created an extra step and confused the flow.

What I changed in P2:
Removed the Upload button from nav and deprecated the standalone upload page.
Placed upload inline on the create form (see item #1).

Files touched:
Views/Shared/_Layout.cshtml – removed “Upload Document” nav entry; kept clean nav: Home, My Claims, Coordinator, Manager, Privacy.
Home/Index.cshtml – adjusted quick actions; tip explains “uploading happens inside the Create page.”

Why this fixes it:
Eliminates redundant navigation and aligns the UX with the real task sequence (create → attach → submit).

6) General GUI upgrades (QOL Improvements)
Status legend + consistent badges across pages (Draft/Submitted/Verified/Approved/Rejected).
Status filters on “My Claims” (client-side) to quickly view by state.
Quick preview modal on Approvals list for a rapid glance at lecturer, period, status (actions still disabled in prototype unless in the right role).
Cleaner layout: subdued card headers, spacing, and compact action bars using Bootstrap 5 utilities.
Privacy page updated** to describe data handling and prototype disclaimer (no real data; encryption planned/partially implemented).

Files: Claims/Index.cshtml, Approvals/Index.cshtml (or split Coordinator/Manager), Home/Index.cshtml, Home/Privacy.cshtml, _Layout.cshtml.








Unit Testing Screenshot proof:
 

















Bibliography     (Referenced in code):
Microsoft. (2024). ASP.NET Core MVC controllers and routing documentation.
Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
(Accessed: 22 October 2025)

Microsoft. (2024). Upload files in ASP.NET Core.
Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
(Accessed: 22 October 2025)

Microsoft. (2023). C# Properties and expression-bodied members.
Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
(Accessed: 22 October 2025)

Microsoft. (2024). Aes Class – Symmetric Encryption Example.
Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes
(Accessed: 22 October 2025)

Bootstrap. (2024). Bootstrap 5 Documentation – Navbar, Table, and Form Layout Components.
Available at: https://getbootstrap.com/docs/5.3/components/navbar/
(Accessed: 22 October 2025)



Microsoft. (2024). Dependency Injection in ASP.NET Core.
Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
(Accessed: 22 October 2025)

Microsoft. (2024). MSTest Framework – Writing and running tests in Visual Studio.
Available at: https://learn.microsoft.com/en-us/visualstudio/test/unit-test-basics
Accessed: 22 October 2025)

W3Schools. (2024). Responsive Web Design and Meta Viewport.
Available at: https://www.w3schools.com/css/css_rwd_intro.asp
Accessed: 22 October 2025)

Information Regulator of South Africa. (2023). Protection of Personal Information Act (POPIA).
Available at: https://inforegulator.org.za/legislation
Accessed: 22 October 2025)

Stack Overflow. (2024). View ‘Upload’ not found in ASP.NET Core.
Available at: https://stackoverflow.com/questions/49170068/view-not-found-in-asp-net-core 
(Accessed: 22 October 2025)



