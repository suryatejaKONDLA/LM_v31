import { resolve } from "node:path";
import { readFileSync } from "node:fs";
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import { compression } from "vite-plugin-compression2";
import { visualizer } from "rollup-plugin-visualizer";

const packageJson = JSON.parse(readFileSync("./package.json", "utf-8")) as { version: string };

// https://vite.dev/config/
export default defineConfig(({ command, mode }) =>
{
    const env = loadEnv(mode, process.cwd(), "");

    return {
        // Multi-tenant base path — read from VITE_BASE_PATH env var.
        base: (() =>
        {
            const raw = env.VITE_BASE_PATH || "/";
            const trimmed = raw.trim().replace(/^\/+/, "").replace(/\/+$/, "");
            return trimmed.length > 0 ? `/${trimmed}/` : "/";
        })(),

        plugins: [
            react(),

            // Brotli + Gzip pre-compression for production
            compression({
                include: /\.(js|css|html|json|wasm)$/,
                threshold: 1024,
                algorithms: [ "brotliCompress", "gzip" ],
            }),

            // Bundle analyzer — only on build, opens report in browser
            command === "build" &&
            visualizer({
                open: true,
                gzipSize: true,
                brotliSize: true,
                filename: "dist/bundle-report.html",
            }),
        ].filter(Boolean),

        resolve: {
            alias: {
                "@": resolve(__dirname, "src"),
            },
        },

        server: {
            port: 5173,
            strictPort: true,
            proxy: {
                "/api": {
                    target: env.VITE_API_BASE_URL || "https://localhost:7001",
                    changeOrigin: true,
                    secure: false,
                },
            },
        },

        define: {
            __APP_VERSION__: JSON.stringify(packageJson.version),
        },

        // ─────────────────────────────────────────────────────────
        // Production Build — Vite 8 + Rolldown + Oxc
        // ─────────────────────────────────────────────────────────
        build: {
            target: "esnext",
            sourcemap: false,
            cssCodeSplit: true,
            modulePreload: { polyfill: true },
            minify: true,
            chunkSizeWarningLimit: 1700,

            rolldownOptions: {
                treeshake: true,
                output: {
                    minify: {
                        mangle: true,
                        compress: {
                            dropConsole: true,
                            dropDebugger: true,
                        },
                    },

                    /**
                     * OPTIMIZED CHUNKING STRATEGY
                     * ============================
                     * Groups vendor libraries by dependency relationships to:
                     * 1. Eliminate circular dependency warnings
                     * 2. Maximize browser cache efficiency
                     * 3. Keep independent libs in separate cacheable chunks
                     */
                    manualChunks(id: string)
                    {
                        if (!id.includes("node_modules"))
                        {
                            return;
                        }

                        const n = id.replace(/\\/g, "/");

                        // ═══════════════════════════════════════════
                        // 1. STANDALONE LIBRARIES (zero UI deps)
                        // ═══════════════════════════════════════════

                        if (n.includes("@microsoft/signalr"))
                        {
                            return "vendor-signalr";
                        }

                        if (n.includes("/axios/") || n.includes("/follow-redirects/"))
                        {
                            return "vendor-axios";
                        }

                        // ═══════════════════════════════════════════
                        // 2. ANT DESIGN — split into granular chunks
                        // ═══════════════════════════════════════════

                        // @ant-design/icons — lazy-loaded via iconRegistry.ts
                        if (n.includes("/@ant-design/icons"))
                        {
                            return "vendor-antd-icons";
                        }

                        if (n.includes("/@ant-design/cssinjs"))
                        {
                            return "vendor-antd-cssinjs";
                        }

                        // RC Components — by category
                        if (n.includes("/rc-table/") || n.includes("/@rc-component/table"))
                        {
                            return "vendor-rc-table";
                        }

                        if (n.includes("/rc-picker/") ||
                            n.includes("/rc-field-form/") ||
                            n.includes("/rc-select/") ||
                            n.includes("/rc-cascader/") ||
                            n.includes("/rc-tree-select/") ||
                            n.includes("/rc-input/") ||
                            n.includes("/rc-input-number/") ||
                            n.includes("/rc-checkbox/") ||
                            n.includes("/rc-radio/") ||
                            n.includes("/rc-switch/") ||
                            n.includes("/rc-rate/") ||
                            n.includes("/rc-slider/") ||
                            n.includes("/rc-upload/") ||
                            n.includes("/rc-mentions/"))
                        {
                            return "vendor-rc-form";
                        }

                        if (n.includes("/rc-menu/") ||
                            n.includes("/rc-dropdown/") ||
                            n.includes("/rc-tabs/") ||
                            n.includes("/rc-collapse/") ||
                            n.includes("/rc-tree/") ||
                            n.includes("/rc-pagination/") ||
                            n.includes("/rc-steps/") ||
                            n.includes("/rc-segmented/"))
                        {
                            return "vendor-rc-nav";
                        }

                        if (n.includes("/rc-dialog/") ||
                            n.includes("/rc-drawer/") ||
                            n.includes("/rc-tooltip/") ||
                            n.includes("/rc-notification/") ||
                            n.includes("/rc-image/") ||
                            n.includes("/rc-motion/") ||
                            n.includes("/rc-trigger/") ||
                            n.includes("/rc-overflow/") ||
                            n.includes("/rc-resize-observer/") ||
                            n.includes("/rc-virtual-list/"))
                        {
                            return "vendor-rc-overlay";
                        }

                        if (n.includes("/@rc-component/") || n.includes("/rc-"))
                        {
                            return "vendor-rc-misc";
                        }

                        // Emotion (CSS-in-JS engine)
                        if (n.includes("/@emotion/") || n.includes("/stylis/"))
                        {
                            return "vendor-emotion";
                        }

                        // Antd by component category
                        if (n.includes("/antd/es/table") ||
                            n.includes("/antd/es/list") ||
                            n.includes("/antd/es/descriptions") ||
                            n.includes("/antd/es/tree") ||
                            n.includes("/antd/es/transfer"))
                        {
                            return "vendor-antd-data";
                        }

                        if (n.includes("/antd/es/form") ||
                            n.includes("/antd/es/input") ||
                            n.includes("/antd/es/select") ||
                            n.includes("/antd/es/checkbox") ||
                            n.includes("/antd/es/radio") ||
                            n.includes("/antd/es/switch") ||
                            n.includes("/antd/es/slider") ||
                            n.includes("/antd/es/rate") ||
                            n.includes("/antd/es/upload") ||
                            n.includes("/antd/es/date-picker") ||
                            n.includes("/antd/es/time-picker") ||
                            n.includes("/antd/es/cascader") ||
                            n.includes("/antd/es/auto-complete") ||
                            n.includes("/antd/es/mentions") ||
                            n.includes("/antd/es/color-picker"))
                        {
                            return "vendor-antd-form";
                        }

                        if (n.includes("/antd/es/modal") ||
                            n.includes("/antd/es/drawer") ||
                            n.includes("/antd/es/popover") ||
                            n.includes("/antd/es/popconfirm") ||
                            n.includes("/antd/es/tooltip") ||
                            n.includes("/antd/es/message") ||
                            n.includes("/antd/es/notification") ||
                            n.includes("/antd/es/alert") ||
                            n.includes("/antd/es/progress") ||
                            n.includes("/antd/es/result") ||
                            n.includes("/antd/es/spin") ||
                            n.includes("/antd/es/skeleton"))
                        {
                            return "vendor-antd-feedback";
                        }

                        if (n.includes("/antd/es/menu") ||
                            n.includes("/antd/es/dropdown") ||
                            n.includes("/antd/es/tabs") ||
                            n.includes("/antd/es/breadcrumb") ||
                            n.includes("/antd/es/pagination") ||
                            n.includes("/antd/es/steps") ||
                            n.includes("/antd/es/anchor"))
                        {
                            return "vendor-antd-nav";
                        }

                        if (n.includes("/antd/es/config-provider") ||
                            n.includes("/antd/es/theme") ||
                            n.includes("/antd/es/locale"))
                        {
                            return "vendor-antd-config";
                        }

                        if (n.includes("/antd/es/button") ||
                            n.includes("/antd/es/space") ||
                            n.includes("/antd/es/grid") ||
                            n.includes("/antd/es/row") ||
                            n.includes("/antd/es/col") ||
                            n.includes("/antd/es/divider") ||
                            n.includes("/antd/es/layout") ||
                            n.includes("/antd/es/typography") ||
                            n.includes("/antd/es/flex"))
                        {
                            return "vendor-antd-layout";
                        }

                        if (n.includes("/antd/es/card") ||
                            n.includes("/antd/es/badge") ||
                            n.includes("/antd/es/tag") ||
                            n.includes("/antd/es/avatar") ||
                            n.includes("/antd/es/image") ||
                            n.includes("/antd/es/empty") ||
                            n.includes("/antd/es/statistic") ||
                            n.includes("/antd/es/timeline") ||
                            n.includes("/antd/es/calendar") ||
                            n.includes("/antd/es/carousel") ||
                            n.includes("/antd/es/qrcode") ||
                            n.includes("/antd/es/segmented") ||
                            n.includes("/antd/es/tour") ||
                            n.includes("/antd/es/watermark"))
                        {
                            return "vendor-antd-display";
                        }

                        if (n.includes("/antd/") || n.includes("/@ant-design/"))
                        {
                            return "vendor-antd-misc";
                        }

                        // Shared UI utilities
                        if (n.includes("/@babel/runtime/") ||
                            n.includes("/dayjs/") ||
                            n.includes("/clsx/") ||
                            n.includes("/scroll-into-view-if-needed/") ||
                            n.includes("/compute-scroll-into-view/") ||
                            n.includes("/throttle-debounce/") ||
                            n.includes("/hoist-non-react-statics/") ||
                            n.includes("/tslib/"))
                        {
                            return "vendor-ui-utils";
                        }

                        // ═══════════════════════════════════════════
                        // 3. REACT UTILITIES
                        // ═══════════════════════════════════════════

                        if (n.includes("/react-router") || n.includes("/cookie/"))
                        {
                            return "vendor-react-router";
                        }

                        if (n.includes("/react-hook-form/"))
                        {
                            return "vendor-react-hook-form";
                        }

                        if (n.includes("/zustand/"))
                        {
                            return "vendor-zustand";
                        }

                        // ═══════════════════════════════════════════
                        // 4. EVERYTHING ELSE → Rolldown decides
                        // ═══════════════════════════════════════════
                    },

                    entryFileNames: "assets/js/[name]-[hash].js",
                    chunkFileNames: "assets/js/[name]-[hash].js",
                    assetFileNames: "assets/[ext]/[name]-[hash].[ext]",
                },
            },
        },
    };
});
