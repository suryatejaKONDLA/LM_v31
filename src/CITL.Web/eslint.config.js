import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { defineConfig, globalIgnores } from "eslint/config";

export default defineConfig([
  globalIgnores([ "dist", "node_modules" ]),
  {
    files: [ "**/*.{ts,tsx}" ],
    extends: [
      js.configs.recommended,
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: "latest",
      globals: globals.browser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      // ── Allman brace style (matches C# formatting) ──
      "brace-style": [ "error", "allman", { allowSingleLine: false } ],

      // ── Strict TypeScript rules ──
      "@typescript-eslint/no-explicit-any": "error",
      "@typescript-eslint/no-deprecated": "error",
      "@typescript-eslint/no-unused-vars": [ "error", {
        argsIgnorePattern: "^_",
        varsIgnorePattern: "^_",
      } ],
      "@typescript-eslint/no-unnecessary-condition": "error",
      "@typescript-eslint/no-unnecessary-type-assertion": "error",
      "@typescript-eslint/prefer-nullish-coalescing": "error",
      "@typescript-eslint/prefer-optional-chain": "error",
      "@typescript-eslint/consistent-type-imports": [ "error", {
        prefer: "type-imports",
        fixStyle: "inline-type-imports",
      } ],
      "@typescript-eslint/consistent-type-definitions": [ "error", "interface" ],
      "@typescript-eslint/no-non-null-assertion": "error",
      "@typescript-eslint/no-floating-promises": "error",
      "@typescript-eslint/no-misused-promises": "error",
      "@typescript-eslint/restrict-template-expressions": "error",
      "@typescript-eslint/strict-boolean-expressions": "off",

      // ── Code quality ──
      "eqeqeq": [ "error", "always" ],
      "no-console": [ "warn", { allow: [ "warn", "error" ] } ],
      "no-debugger": "error",
      "no-alert": "error",
      "no-var": "error",
      "prefer-const": "error",
      "prefer-template": "error",
      "no-duplicate-imports": "error",
      "curly": [ "error", "all" ],
      "no-implicit-coercion": "error",

      // ── Formatting consistency ──
      "semi": [ "error", "always" ],
      "quotes": [ "error", "double", { avoidEscape: true } ],
      "comma-dangle": [ "error", "always-multiline" ],
      "arrow-parens": [ "error", "always" ],
      "object-curly-spacing": [ "error", "always" ],
      "array-bracket-spacing": [ "error", "always" ],
      "indent": [ "error", 4, { SwitchCase: 1 } ],
    },
  },
]);
