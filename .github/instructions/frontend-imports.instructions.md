# Frontend Import Rules

> Applies to all `**/*.ts` and `**/*.tsx` files in `src/CITL.Web/src/`.

---

## Layer Barrel Rule — ALWAYS use layer-level barrel imports

Every layer exposes a single `Index.ts` barrel. **Never import from a sub-path of another layer.**

| Layer | Barrel to import from |
|---|---|
| Domain entities / types | `@/Domain/Index` |
| Application stores / use-cases | `@/Application/Index` |
| Infrastructure services / storage / HTTP | `@/Infrastructure/Index` |
| Shared types / constants / helpers | `@/Shared/Index` |
| Presentation controls / layouts / pages | `@/Presentation/Controls/Index`, etc. |

### ✅ Correct

```ts
import { type LoginRequest, type CaptchaResponse } from "@/Domain/Index";
import { useAuthStore }                             from "@/Application/Index";
import { AuthService, AuthStorage }                 from "@/Infrastructure/Index";
import { ApiResponseCode }                          from "@/Shared/Index";
import { TextBox, PasswordBox }                     from "@/Presentation/Controls/Index";
```

### ❌ Wrong — deep paths that bypass barrels

```ts
import { AuthService }     from "@/Infrastructure/Services/AuthService";   // ❌
import { AuthStorage }     from "@/Infrastructure/Authentication/AuthStorage"; // ❌
import { type User }       from "@/Domain/Entities/User";                  // ❌
import { ApiResponseCode } from "@/Shared/Types/ApiResponseCode";          // ❌
import { TextBox }         from "@/Presentation/Controls/TextBox/TextBox"; // ❌
```

---

## Within-Layer Import Rule

When a file imports from **another folder inside the same layer** (e.g., `Infrastructure/Services` importing from `Infrastructure/Persistence`), use the **sub-folder barrel**, never the layer root (to avoid circular dependencies).

```ts
// ✅ Within Infrastructure layer — use sub-folder barrel
import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { AuthStorage }          from "@/Infrastructure/Authentication/Index";

// ❌ Would cause circular dependency via Infrastructure/Index
import { apiClient } from "@/Infrastructure/Index";
```

For same-folder imports, use a **relative path**:

```ts
import { ApiRoutes } from "./ApiRoutes"; // ✅ same folder — relative is correct
```

---

## Lazy Route Imports — exception to the barrel rule

`React.lazy()` in `AppRoutes.tsx` **must** point to a single file — it cannot go through a barrel. This is the only valid exception.

```ts
// ✅ Lazy import — must be a direct file path (Vite code-splitting)
const Login = lazy(() => import("@/Presentation/Pages/Authentication/Login"));

// ❌ Cannot lazy-import from a barrel — bundles everything into one chunk
const Login = lazy(() => import("@/Presentation/Pages/Index"));
```

---

## CSS Module Import Rule

CSS modules must be imported as a **default import** directly from the `.module.css` file (no barrel):

```ts
import cssModule from "./Login.module.css"; // ✅ always direct — no barrel for CSS
```

Use the `c()` helper pattern to safely access class names (avoids `noUncheckedIndexedAccess` errors):

```ts
const c = (...names: string[]): string =>
    names.map((n) => cssModule[n] ?? "").filter(Boolean).join(" ");
```

---

## Barrel Maintenance Rule

When you add a new exported symbol to any file, **update its layer's `Index.ts` barrel** immediately:

- New entity in `Domain/Entities/Foo.ts` → add to `Domain/Entities/Index.ts`
- New service in `Infrastructure/Services/FooService.ts` → add to `Infrastructure/Services/Index.ts`
- New store in `Application/Stores/FooStore.ts` → add to `Application/Stores/Index.ts`
- New control in `Presentation/Controls/Foo/Foo.tsx` → add to `Presentation/Controls/Index.ts`

The chain bubbles up automatically: sub-barrel → layer barrel → (consumed by other layers).
