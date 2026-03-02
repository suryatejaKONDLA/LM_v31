# CITL — Component Catalog (from Old Project)

> Complete inventory of every component, page, hook, service, store, and utility from the old `CITLREACT_v31` project.
> Use this as the build checklist for the new project.

---

## Summary

| Category | Count |
|----------|-------|
| Pages | ~60 |
| Layout Components | 8 |
| UI Controls / Widgets | 29 |
| Hooks | 16 |
| API Services | ~50 |
| Zustand Stores | 7 |
| React Contexts → Zustand | 2 |
| App Services | 1 |
| Utilities | 3 |
| Helpers | 6 |
| Constants | 4 |
| Models / Types | ~130+ |

### Module Breakdown

| Module | Description |
|--------|-------------|
| **Core** | Auth, admin masters, finance, grid views, error pages — shared across all tenants |
| **Dt7** | POS — restaurant ordering, billing, receipts, table/floor management |
| **Ogo** | Education — students, programs, registration, documents |

---

## 1. PAGES

### Entry Points

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 1 | Main | `Main.tsx` | ✅ Done |
| 2 | Application (Router) | `Application.tsx` | ✅ Done (AppRoutes) |
| 3 | Home | `Components/Pages/Home.tsx` | ✅ Done |

### Core > Authentication

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 4 | Login | `Core/Authentication/Login/` | ✅ Done |
| 5 | ChangePassword | `Core/Authentication/ChangePassword/` | ❌ |
| 6 | Profile | `Core/Authentication/Profile/` | ❌ |

### Core > Admin

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 7 | AppMaster | `Core/Admin/AppMaster/` | ❌ |
| 8 | BranchMaster | `Core/Admin/BranchMaster/` | ❌ |
| 9 | FinYearMaster | `Core/Admin/FinYearMaster/` | ❌ |
| 10 | LoginMaster | `Core/Admin/LoginMaster/` | ❌ |
| 11 | Mappings | `Core/Admin/Mappings/` | ❌ |
| 12 | RoleMenuMapping | `Core/Admin/Mappings/RoleMenuMapping` | ❌ |
| 13 | Reports | `Core/Admin/Reports/` | ❌ |
| 14 | RoleMaster | `Core/Admin/RoleMaster/` | ❌ |
| 15 | ThemeEditor | `Core/Admin/ThemeEditor/` | ❌ |
| 16 | UomMaster | `Core/Admin/UomMasters/` | ❌ |

### Core > GridViews

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 17 | GridView3 | `Core/GridViews/GridView3` | ❌ |

### Core > SubLedgerMasters

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 18 | SupplierMaster | `Core/SubLedgerMasters/SupplierMaster` | ❌ |

### Core > Error Pages

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 19 | Forbidden (403) | `Core/ErrorPages/Forbidden` | ✅ Done |
| 20 | NetworkError | `Core/ErrorPages/NetworkError` | ✅ Done |
| 21 | NotFound (404) | `Core/ErrorPages/NotFound` | ✅ Done |

### Dt7 > POS Masters

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 22 | AreaMaster | `Dt7/Pos/Masters/AreaMaster` | ❌ |
| 23 | CategoryMaster | `Dt7/Pos/Masters/CategoryMaster` | ❌ |
| 24 | FloorPlan | `Dt7/Pos/Masters/FloorPlan` | ❌ |
| 25 | GroupMaster | `Dt7/Pos/Masters/GroupMaster` | ❌ |
| 26 | ModifierMaster | `Dt7/Pos/Masters/ModifierMaster` | ❌ |
| 27 | PrintMaster | `Dt7/Pos/Masters/PrintMaster` | ❌ |
| 28 | ProductMaster | `Dt7/Pos/Masters/ProductMaster` | ❌ |
| 29 | RateMaster | `Dt7/Pos/Masters/RateMaster` | ❌ |
| 30 | TableMaster | `Dt7/Pos/Masters/TableMaster` | ❌ |
| 31 | TypeMaster | `Dt7/Pos/Masters/TypeMaster` | ❌ |
| 32 | VegFlagMaster | `Dt7/Pos/Masters/VegFlagMaster` | ❌ |

### Dt7 > POS Transactions

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 33 | Order (list) | `Dt7/Pos/Transactions/Order/` | ❌ |
| 34 | OrderEntry (POS screen) | `Dt7/Pos/Transactions/Order/OrderEntry/` | ❌ |
| 35 | BillCreation | `Dt7/Pos/Transactions/Bill/` | ❌ |
| 36 | SplitBillDialog | `Dt7/Pos/Transactions/Bill/SplitBillDialog` | ❌ |
| 37 | KotUpdation | `Dt7/Pos/Transactions/KotUpdation/` | ❌ |
| 38 | Receipts | `Dt7/Pos/Transactions/Receipt/` | ❌ |
| 39 | StaffAttendance | `Dt7/Pos/Transactions/StaffAttendance/` | ❌ |

### Dt7 > POS Sub-Components

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 40 | BillDetailsDialog | `Dt7/Pos/Transactions/Order/BillDetailsDialog` | ❌ |
| 41 | OrderDetailsDialog | `Dt7/Pos/Transactions/Order/OrderDetailsDialog` | ❌ |
| 42 | CartSidebar | `Dt7/Pos/Transactions/Order/OrderEntry/CartSidebar` | ❌ |
| 43 | CartItemsDialog | `Dt7/Pos/Transactions/Order/OrderEntry/CartItemsDialog` | ❌ |
| 44 | CustomerDetailsDialog | `Dt7/Pos/Transactions/Order/OrderEntry/CustomerDetailsDialog` | ❌ |
| 45 | MenuFilters | `Dt7/Pos/Transactions/Order/OrderEntry/MenuFilters` | ❌ |
| 46 | ModifierDialog | `Dt7/Pos/Transactions/Order/OrderEntry/ModifierDialog` | ❌ |
| 47 | ProductCard | `Dt7/Pos/Transactions/Order/OrderEntry/ProductCard` | ❌ |
| 48 | VirtualizedProductList | `Dt7/Pos/Transactions/Order/OrderEntry/VirtualizedProductList` | ❌ |
| 49 | SplitBillDraggableItem | `Dt7/Pos/Transactions/Bill/SplitBillDraggableItem` | ❌ |

### Ogo > Masters

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 50 | AcademicStream | `Ogo/Masters/AcademicStream` | ❌ |
| 51 | CityMaster | `Ogo/Masters/CityMaster` | ❌ |
| 52 | CountryMaster | `Ogo/Masters/CountryMaster` | ❌ |
| 53 | ExamMaster | `Ogo/Masters/ExamMaster` | ❌ |
| 54 | IntakeMaster | `Ogo/Masters/IntakeMaster` | ❌ |
| 55 | ProgramMaster | `Ogo/Masters/ProgramMaster` | ❌ |
| 56 | SectorMaster | `Ogo/Masters/SectorMaster` | ❌ |
| 57 | StaffMaster | `Ogo/Masters/StaffMaster` | ❌ |
| 58 | StateMaster | `Ogo/Masters/StateMaster` | ❌ |
| 59 | UniversityMaster | `Ogo/Masters/UniversityMaster` | ❌ |
| 60 | VendorMaster | `Ogo/Masters/VendorMaster` | ❌ |
| 61 | StudentComposedMaster | `Ogo/Masters/StudentMaster/StudentComposedMaster` | ❌ |
| 62 | StudentMaster (list) | `Ogo/Masters/StudentMaster/StudentMaster` | ❌ |
| 63 | StudentMaster1–9 (tabs) | `Ogo/Masters/StudentMaster/StudentMaster[1-9]` | ❌ |
| 64 | StudentMasterCheckup | `Ogo/Masters/StudentMaster/StudentMasterCheckup` | ❌ |
| 65 | StudentMasterReview | `Ogo/Masters/StudentMaster/StudentMasterReview` | ❌ |
| 66 | TestRecordModal | `Ogo/Masters/StudentMaster/TestRecordModal` | ❌ |
| 67 | WorkExperienceModal | `Ogo/Masters/StudentMaster/WorkExperienceModal` | ❌ |

### Ogo > Transactions

| # | Name | Old Path | Status |
|---|------|----------|--------|
| 68 | Registration | `Ogo/Transactions/Registration` | ❌ |
| 69 | DocumentUpdate | `Ogo/Transactions/DocumentUpdate` | ❌ |

---

## 2. LAYOUT COMPONENTS

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | MainLayout | `Layouts/MainLayout` | App shell — sidebar + header + footer + content | ✅ Done |
| 2 | LoginLayout | `Layouts/LoginLayout` | Minimal layout for login page | ✅ Done |
| 3 | ProtectedRoute | `Layouts/ProtectedRoute` | Auth guard — redirect to login if unauthenticated | ✅ Done |
| 4 | NetworkErrorGuard | `Layouts/NetworkErrorGuard` | Monitor API health, redirect after 5s offline | ❌ |
| 5 | Header | `Layouts/Header/Header` | Top nav — branch selector, user menu, theme toggle, health, search | ✅ Done |
| 6 | Sidebar | `Layouts/SideBar/Sidebar` | Collapsible sidebar — menu tree, favorites, responsive drawer | ✅ Done |
| 7 | Footer | `Layouts/Footer/Footer` | Company name, contact info, copyright | ✅ Done |
| 8 | PageTransition | `Layouts/PageTransition/PageTransition` | CSS-animated page transitions (GPU-accelerated) | ❌ |

---

## 3. UI CONTROLS / WIDGETS

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | AutoRefreshControl | `Controls/AutoRefreshControl/` | Countdown badge with auto-refresh trigger | ❌ |
| 2 | CheckBox | `Controls/CheckBox/` | Checkbox + react-hook-form | ✅ Done |
| 3 | DataGridPro | `Controls/DataGrid/` | Full data grid — sort, filter, paginate, group, virtualize, export | ❌ |
| 4 | DatePickerBox | `Controls/DatePicker/` | Date picker + react-hook-form | ✅ Done |
| 5 | DraggableItem | `Controls/DraggableItem/` | Generic drag-and-drop item (HTML5 Drag API) | ❌ |
| 6 | DropDown | `Controls/DropDown/` | Dropdown select + react-hook-form + API data loading | ✅ Done |
| 7 | DynamicIcon | `Controls/DynamicIcon/` | Render Ant Design icon by name string (memo'd) | ✅ Done |
| 8 | FileUpload | `Controls/FileUpload/` | File upload with drag-and-drop, validation | ✅ Done |
| 9 | FormActionButtons | `Controls/FormActionButtons/` | Save / Reset / Cancel button group (memo'd) | ✅ Done |
| 10 | ImageUpload | `Controls/ImageUpload/` | Image upload with preview, base64 (objectURL lifecycle) | ✅ Done |
| 11 | InputBox | `Controls/InputBox/` | Standalone text input (memo'd) | ✅ Done |
| 12 | GlobalShortcutsHelp | `Controls/KeyboardShortcutsHelp/` | Global keyboard shortcuts help panel | ✅ Done |
| 13 | KeyboardShortcutsHelp | `Controls/KeyboardShortcutsHelp/` | Keyboard shortcuts dialog | ✅ Done |
| 14 | Message | `Controls/Message/` | Imperative toast API (zero-render) | ✅ Done |
| 15 | ModalDialog | `Controls/ModalDialog/` | Imperative modal API (zero-render) | ✅ Done |
| 16 | Notification | `Controls/Notification/` | Imperative notification API (zero-render) | ✅ Done |
| 17 | NumberBox | `Controls/NumberBox/` | Number input + react-hook-form (memo'd) | ✅ Done |
| 18 | NumberInputBox | `Controls/NumberInputBox/` | Standalone number input (memo'd) | ✅ Done |
| 19 | PasswordBox | `Controls/PasswordBox/` | Password input with visibility toggle | ✅ Done |
| 20 | PDFViewer | `Controls/PDFViewer/` | PDF viewer (URL or base64) | ❌ |
| 21 | PDFViewerDialog | `Controls/PDFViewer/` | PDF viewer in modal | ❌ |
| 22 | RadioGroupBox | `Controls/RadioGroupBox/` | Radio group (memo'd) | ✅ Done |
| 23 | RecordDetailsBanner | `Controls/RecordDetailsBanner/` | Banner showing record metadata (memo'd) | ✅ Done |
| 24 | SearchBox | `Controls/SearchBox/` | Search input with clear (memo'd) | ✅ Done |
| 25 | SearchDialog | `Controls/SearchDialog/` | Command-palette search (memo'd, useDeferredValue) | ✅ Done |
| 26 | SelectBox | `Controls/SelectBox/` | Standalone select (memo'd, memoized showSearch) | ✅ Done |
| 27 | Spinner | `Controls/Spinner/` | Full-page loading spinner + GlobalSpinner API | ✅ Done |
| 28 | SystemHealthBadge | `Controls/SystemHealthBadge/` | API/network health badge (green/yellow/red) | ❌ |
| 29 | TextAreaBox | `Controls/TextAreaBox/` | Standalone textarea (memo'd) | ✅ Done |
| 30 | TextBox | `Controls/TextBox/` | Text input + react-hook-form | ✅ Done |
| 31 | ThemeToggleButton | `Controls/ThemeToggleButton/` | Light/dark mode toggle with ripple animation | ✅ Done |

---

## 4. HOOKS

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | useApiHealth | `Hooks/useApiHealth` | SignalR + ping health monitoring | ❌ |
| 2 | useAutoFitColumns | `Hooks/useAutoFitColumns` | Canvas-based column width for data grids | ❌ |
| 3 | useAutoRefreshCountdown | `Hooks/useAutoRefreshCountdown` | Countdown timer with callback | ❌ |
| 4 | useCascadingDropdowns | `Hooks/useCascadingDropdowns` | Dependent/cascading dropdown chains | ❌ |
| 5 | useColumnAutoFit | `Hooks/useColumnAutoFit` | Column auto-fit for virtualized grids | ❌ |
| 6 | useEncryptedQueryParam | `Hooks/useEncryptedQueryParam` | Decrypt URL query params | ❌ |
| 7 | useIsMobile | `Hooks/useIsMobile` | Mobile viewport detection (<768px) | ❌ |
| 8 | useKeyboardShortcuts | `Hooks/useKeyboardShortcuts` | Register keyboard shortcut handlers | ❌ |
| 9 | useMasterForm | `Hooks/useMasterForm` | Generic CRUD form hook for master pages | ❌ |
| 10 | usePageTitle | `Hooks/usePageTitle` | Set document.title | ❌ |
| 11 | useProfileForm | `Hooks/useProfileForm` | Profile-style form hook (single record) | ❌ |
| 12 | useReportPrint | `Hooks/useReportPrint` | Report download + PDF viewer dialog | ❌ |
| 13 | useBranchInitializer | `Services/App/BranchService/` | Branch state consumer | ❌ |
| 14 | useUserProfileInitializer | `Services/App/UserProfileService/` | Profile state consumer | ❌ |
| 15 | useGuardedNavigate | `Services/App/NavigationGuardService/` | Navigate with unsaved changes check | ❌ |
| 16 | useBillData | `Dt7/Pos/Transactions/Bill/` | Bill-specific data loading | ❌ |

---

## 5. API SERVICES

### Base

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | BaseApiService | `Services/Api/BaseApiService` | Axios base — tenant header, JWT, token refresh | ✅ Done |
| 2 | ApiRoutes | `Services/Api/ApiRoutes` | All API route constants | ✅ Done |

### Authentication

| # | Name | Description | Status |
|---|------|-------------|--------|
| 3 | AuthenticationApiService | Login, logout, refresh, profile, menus, branches, theme | ✅ Done |

### Common

| # | Name | Description | Status |
|---|------|-------------|--------|
| 4 | FileStorageApiService | File download, upload, info, existence | ❌ |
| 5 | ReportsApiService | Report tree, export, download URL | ❌ |

### Core > Admin (17 services)

| # | Name | Status |
|---|------|--------|
| 6 | AppMasterApiService | ✅ Done |
| 7 | BranchMasterApiService | ❌ |
| 8 | CompanyMasterApiService | ✅ Done |
| 9 | CurrencyMasterApiService | ❌ |
| 10 | DiscountMasterApiService | ❌ |
| 11 | FinYearApiService | ❌ |
| 12 | GenderMasterApiService | ❌ |
| 13 | GstMasterApiService | ❌ |
| 14 | LoginMasterApiService | ❌ |
| 15 | MailMasterApiService | ❌ |
| 16 | MappingApiService | ❌ |
| 17 | RoleMenuMappingApiService | ❌ |
| 18 | MaritalStatusMasterApiService | ❌ |
| 19 | RoleMasterApiService | ❌ |
| 20 | StateMasterApiService | ❌ |
| 21 | TimeZoneMasterApiService | ❌ |
| 22 | UomMasterApiService | ❌ |

### Core > Finance (2 services)

| # | Name | Status |
|---|------|--------|
| 23 | BankMasterApiService | ❌ |
| 24 | InstrumentMasterApiService | ❌ |

### Core > GridView

| # | Name | Status |
|---|------|--------|
| 25 | GridViewApiService | ❌ |

### Ogo > Masters (15 services)

| # | Name | Status |
|---|------|--------|
| 26 | AcademicStreamMasterApiService | ❌ |
| 27 | CityMasterApiService | ❌ |
| 28 | CountryMasterApiService | ❌ |
| 29 | DegreeLevelMasterApiService | ❌ |
| 30 | EducationMasterApiService | ❌ |
| 31 | ExamMasterApiService | ❌ |
| 32 | IntakeMasterApiService | ❌ |
| 33 | InterDiplomaMasterApiService | ❌ |
| 34 | ProgramMasterApiService | ❌ |
| 35 | SectorMasterApiService | ❌ |
| 36 | StaffMasterApiService | ❌ |
| 37 | StateMasterApiService (Ogo) | ❌ |
| 38 | StudentMasterApiService | ❌ |
| 39 | TestMasterApiService | ❌ |
| 40 | VendorMasterApiService | ❌ |

### Ogo > Transactions (2 services)

| # | Name | Status |
|---|------|--------|
| 41 | RegistrationApiService | ❌ |
| 42 | DocumentUpdateApiService | ❌ |

### Dt7 > POS Masters (11 services)

| # | Name | Status |
|---|------|--------|
| 43 | AreaMasterApiService | ❌ |
| 44 | CategoryMasterApiService | ❌ |
| 45 | GroupMasterApiService | ❌ |
| 46 | ModifierMasterApiService | ❌ |
| 47 | ModifierRateMasterApiService | ❌ |
| 48 | PrintMasterApiService | ❌ |
| 49 | ProductMasterApiService | ❌ |
| 50 | RateMasterApiService | ❌ |
| 51 | TableMasterApiService | ❌ |
| 52 | TypeMasterApiService | ❌ |
| 53 | VegFlagMasterApiService | ❌ |

### Dt7 > POS Transactions (4 services)

| # | Name | Status |
|---|------|--------|
| 54 | OrderApiService | ❌ |
| 55 | BillApiService | ❌ |
| 56 | ReceiptApiService | ❌ |
| 57 | StaffAttendanceApiService | ❌ |

---

## 6. STORES

### Zustand Stores

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | AuthStore | N/A (new) | JWT tokens, user info | ✅ Done |
| 2 | BranchStore | `BranchService/BranchStore` | Selected branch, branch list | ❌ |
| 3 | CompanyStore | `CompanyService/CompanyStore` | Company info, logos, contact | ✅ Done |
| 4 | DraftOrderStore | `DraftOrderService/DraftOrderStore` | Unsaved POS orders per table | ❌ |
| 5 | MenuStore | `MenuService/MenuStore` | Menu tree + favorites | ✅ Done |
| 6 | NavigationGuardStore | `NavigationGuardService/` | Block navigation with unsaved changes | ❌ |
| 7 | GlobalSpinner | `SpinnerService/GlobalSpinner` | Imperative full-page spinner (module-scoped API) | ✅ Done |
| 8 | ThemeStore | N/A (new) | Light/dark mode, custom tokens | ✅ Done |
| 9 | UserProfileStore | `UserProfileService/` | Current user profile | ❌ |

### Old React Contexts → Replaced

| # | Name | Old Pattern | New Pattern | Status |
|---|------|-------------|-------------|--------|
| 1 | ThemeContext | React Context + Provider | Zustand ThemeStore | ✅ Replaced |
| 2 | ShortcutsContext | React Context + Provider | TBD (Zustand or keep Context) | ❌ |

---

## 7. APP SERVICES

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | AutoLogoutService | `Services/App/AutoLogoutService` | Inactivity detection — warn at 18m, logout at 20m | ❌ |

---

## 8. UTILITIES

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | passwordUtils | `Utils/passwordUtils` | Password generation + strength calculation | ❌ |
| 2 | QueryStringHelper | `Utils/QueryStringHelper` | XOR-based query param encryption/decryption | ❌ |
| 3 | SystemHealthUtils | `Utils/SystemHealthUtils` | Health status label/color/icon mapping | ❌ |

---

## 9. HELPERS

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | AuthStorageHelpers | `Helpers/AuthStorageHelpers` | JWT storage in sessionStorage | ✅ Done |
| 2 | ImageHelpers | `Helpers/ImageHelpers` | Image validation, resize, compression | ❌ |
| 3 | PdfHelpers | `Helpers/PdfHelpers` | PDF source validation | ❌ |
| 4 | SanitizeHtmlHelpers | `Helpers/SanitizeHtmlHelpers` | DOMPurify HTML sanitizer | ❌ |
| 5 | SearchEngineHelper | `Helpers/SearchEngineHelper` | In-memory search engine (AND/OR) | ✅ Done |
| 6 | StringHelpers | `Helpers/StringHelpers` | Empty strings → null for DB | ✅ Done |

---

## 10. CONSTANTS

| # | Name | Old Path | Description | Status |
|---|------|----------|-------------|--------|
| 1 | Constants | `Constants/Constants` | Default values, thresholds | ❌ |
| 2 | EnvironmentConstants | `Constants/EnvironmentConstants` | Vite env variable keys | ✅ Done (AppConfig) |
| 3 | MediaTypeNameConstants | `Constants/MediaTypeNameConstants` | MIME type constants | ✅ Done |
| 4 | ThemeConstants | `Constants/ThemeConstants` | Fixed theme tokens, localStorage keys | ✅ Done |

---

## Build Priority (Suggested Order)

### Phase 1 — Core Controls (foundation for all pages) ✅ COMPLETE
1. ~~Message (toast notifications)~~ ✅
2. ~~Notification (corner notifications)~~ ✅
3. ~~ModalDialog (confirm/warning/info/error)~~ ✅
4. ~~Spinner (full-page loader)~~ ✅
5. ~~InputBox (editable text input)~~ ✅
6. ~~NumberBox / NumberInputBox~~ ✅
7. ~~DatePicker~~ ✅
8. ~~DropDown~~ ✅
9. ~~SelectBox~~ ✅
10. ~~TextAreaBox~~ ✅
11. ~~RadioGroupBox~~ ✅
12. ~~FormActionButtons~~ ✅
13. ~~SearchBox~~ ✅
14. ~~RecordDetailsBanner~~ ✅

### Phase 2 — Core Infrastructure
15. DataGridPro (used by every master page)
16. ~~DynamicIcon~~ ✅
17. ~~FileUpload / ImageUpload~~ ✅
18. PDFViewer / PDFViewerDialog
19. useMasterForm hook
20. useEncryptedQueryParam hook
21. usePageTitle hook
22. useIsMobile hook
23. passwordUtils
24. QueryStringHelper
25. ~~StringHelpers~~ ✅
26. ImageHelpers

### Phase 3 — Layout & Navigation
27. NetworkErrorGuard
28. PageTransition
29. ~~SearchDialog~~ ✅
30. ~~GlobalShortcutsHelp / KeyboardShortcutsHelp~~ ✅
31. useKeyboardShortcuts hook
32. ShortcutsContext → Zustand
33. BranchStore
34. UserProfileStore
35. NavigationGuardStore
36. ~~GlobalSpinner~~ ✅
37. AutoLogoutService
38. useApiHealth hook
39. SystemHealthBadge

### Phase 4 — Core Admin Pages
40. Profile + ChangePassword
41. AppMaster
42. BranchMaster
43. RoleMaster
44. LoginMaster
45. Mappings / RoleMenuMapping
46. FinYearMaster
47. UomMaster
48. ThemeEditor
49. Reports
50. GridView3
51. SupplierMaster

### Phase 5 — Dt7 POS Module
52–62. POS Masters (Area, Category, Group, Modifier, Print, Product, Rate, Table, Type, VegFlag, FloorPlan)
63–69. POS Transactions (Order, OrderEntry, Bill, SplitBill, KOT, Receipts, StaffAttendance)

### Phase 6 — Ogo Education Module
70–80. Ogo Masters (Country, State, City, Sector, University, Program, Intake, Exam, Staff, Vendor, AcademicStream)
81–89. StudentMaster (Composed + 9 tabs + Checkup + Review + Modals)
90–91. Ogo Transactions (Registration, DocumentUpdate)

---

**Created**: March 2, 2026
**Source**: `old-project/` (CITLREACT_v31)
