import { useState, useEffect, useRef, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useForm, type SubmitHandler } from "react-hook-form";
import { Form, Button, Typography, Space, Avatar, Divider, Grid, theme } from "antd";
import { LoadingOutlined, ReloadOutlined, SafetyCertificateFilled, CustomerServiceOutlined, WhatsAppOutlined } from "@ant-design/icons";
import { TextBox, PasswordBox, ThemeToggleButton, Message } from "@/Presentation/Controls/Index";
import { GlobalSpinner } from "@/Shared/UI/Index";
import { AuthService, AppMasterService, CompanyMasterService, MenuService, ThemeService, AuthStorage, TokenRefreshManager } from "@/Infrastructure/Index";
import { useAuthStore, useCompanyStore, useMenuStore, useThemeStore, useLocationStore } from "@/Application/Index";
import { type LoginRequest, type CaptchaResponse } from "@/Domain/Index";
import { ApiResponseCode } from "@/Shared/Index";
import cssModule from "./Login.module.css";

const loginBgImage = `${import.meta.env.BASE_URL}images/login-bg.jpg`;

/** Safe CSS-module class accessor — returns empty string if class not found. */
const c = (...names: string[]): string =>
    names.map((n) => cssModule[n] ?? "").filter(Boolean).join(" ");

/** Form-level subset — only fields the user types. */
interface LoginFormValues
{
    Login_User: string;
    Login_Password: string;
    Captcha_Value: string;
}

/**
 * Login page — authenticates user and stores JWT tokens.
 *
 * Performance:
 *  - CSS Modules (zero runtime — static .css)
 *  - react-hook-form (minimal re-renders — no controlled inputs)
 *  - Zustand stores (no Context cascade re-renders)
 *  - Parallel API fetches (AppMaster + CompanyMaster)
 *  - Pre-cached geolocation + IP
 *  - requestAnimationFrame deferred fetch (loader paints first)
 */
export default function Login(): React.JSX.Element
{
    // ── State ─────────────────────────────────────────────────
    const [ loading, setLoading ] = useState(false);
    const [ appHeader1, setAppHeader1 ] = useState("Loading...");
    const [ appHeader2, setAppHeader2 ] = useState("");
    const [ appLogo, setAppLogo ] = useState<string | null>(null);
    const [ logoBgColor, setLogoBgColor ] = useState<string | null>(null);
    const [ captchaData, setCaptchaData ] = useState<CaptchaResponse | null>(null);
    const [ captchaLoading, setCaptchaLoading ] = useState(false);
    const [ bgImageReady, setBgImageReady ] = useState(false);

    // Global location state — modal is managed by LocationBlockedModal at the root
    const { position: cachedPosition, isDenied: locationDenied } = useLocationStore();

    // Pre-cached IP for faster login submission
    const cachedIpRef = useRef("X.X.X.X");

    const navigate = useNavigate();
    const setUser = useAuthStore((s) => s.setUser);
    const setMenus = useMenuStore((s) => s.setMenus);
    const { fullName: companyFullName, mobile1: contactMobile1, mobile2: contactMobile2, tagline, logo1: companyLogo, initialized: companyLoaded, setCompany } = useCompanyStore();
    const isDarkMode = useThemeStore((s) => s.isDarkMode);
    const setCustomTokens = useThemeStore((s) => s.setCustomTokens);
    const setCompact = useThemeStore((s) => s.setCompact);
    const setHappyWork = useThemeStore((s) => s.setHappyWork);
    const screens = Grid.useBreakpoint();
    const { token } = theme.useToken();

    const themeClass = isDarkMode ? "dark" : "light";

    // ── Form ──────────────────────────────────────────────────
    const { control, handleSubmit, watch, setValue } = useForm<LoginFormValues>({
        defaultValues: {
            Login_User: "",
            Login_Password: "",
            Captcha_Value: "",
        },
    });

    const loginUser = watch("Login_User");

    // ── Computed ──────────────────────────────────────────────
    const bgImage = companyLoaded
        ? (companyLogo ? `data:image/png;base64,${companyLogo}` : loginBgImage)
        : undefined;

    // ── Preload background image — prevent black flash ───────
    useEffect(() =>
    {
        if (!bgImage)
        {
            return;
        }

        setBgImageReady(false);
        const img = new Image();
        img.onload = () =>
        {
            setBgImageReady(true);
        };
        img.onerror = () =>
        {
            setBgImageReady(true);
        };
        img.src = bgImage;
    }, [ bgImage ]);

    // ── Extract dominant colour from company logo ────────────
    useEffect(() =>
    {
        if (!companyLogo)
        {
            return;
        }

        const img = new Image();
        img.crossOrigin = "anonymous";
        img.onload = () =>
        {
            const canvas = document.createElement("canvas");
            const size = 64;
            canvas.width = size;
            canvas.height = size;
            const ctx = canvas.getContext("2d");
            if (!ctx)
            {
                return;
            }
            ctx.drawImage(img, 0, 0, size, size);
            const data = ctx.getImageData(0, 0, size, size).data;
            let r = 0, g = 0, b = 0, count = 0;

            // Sample edge pixels for a reliable average
            for (let x = 0; x < size; x++)
            {
                for (const y of [ 0, 1, size - 2, size - 1 ])
                {
                    const i = (y * size + x) * 4;
                    if ((data[i + 3] ?? 0) > 50)
                    {
                        r += data[i] ?? 0;
                        g += data[i + 1] ?? 0;
                        b += data[i + 2] ?? 0;
                        count++;
                    }
                }
            }
            for (let y = 0; y < size; y++)
            {
                for (const x of [ 0, 1, size - 2, size - 1 ])
                {
                    const i = (y * size + x) * 4;
                    if ((data[i + 3] ?? 0) > 50)
                    {
                        r += data[i] ?? 0;
                        g += data[i + 1] ?? 0;
                        b += data[i + 2] ?? 0;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                setLogoBgColor(
                    `rgb(${String(Math.round(r / count))}, ${String(Math.round(g / count))}, ${String(Math.round(b / count))})`,
                );
            }
        };
        img.src = `data:image/png;base64,${companyLogo}`;
    }, [ companyLogo ]);

    // ── Fetch app + company master on mount ──────────────────
    useEffect(() =>
    {
        const controller = new AbortController();

        const fetchDetails = async () =>
        {
            try
            {
                const [ appResult, companyResult ] = await Promise.all([
                    AppMasterService.get(controller.signal),
                    CompanyMasterService.get(controller.signal),
                ]);

                if (companyResult.Code === ApiResponseCode.Success)
                {
                    setCompany(companyResult.Data);
                }

                if (appResult.Code === ApiResponseCode.Success)
                {
                    setAppHeader1(appResult.Data.APP_Header1);
                    setAppHeader2(appResult.Data.APP_Header2);
                    setAppLogo(appResult.Data.APP_Logo1 ?? null);
                }
                else if (companyResult.Code === ApiResponseCode.Success)
                {
                    setAppHeader1(companyResult.Data.CMP_Full_Name || "ERP");
                    setAppHeader2(companyResult.Data.CMP_Short_Name || "ERP");
                }
                else
                {
                    setAppHeader1("ERP");
                    setAppHeader2("ERP");
                }
            }
            catch
            {
                setAppHeader1("ERP");
                setAppHeader2("ERP");
            }
        };

        // Defer to next frame so inlined loader paints first
        requestAnimationFrame(() =>
        {
            void fetchDetails();
        });

        return () =>
        {
            controller.abort();
        };
    }, [ setCompany ]);

    // ── Pre-cache IP + clear stale auth on mount ────────────
    useEffect(() =>
    {
        AuthStorage.clear();

        fetch("https://api.ipify.org?format=json")
            .then((res) => res.json() as Promise<{ ip: string }>)
            .then((data) =>
            {
                cachedIpRef.current = data.ip;
            })
            .catch(() =>
            { /* silent */ });
    }, []);

    // ── CAPTCHA: auto-load when username typed ──────────────
    useEffect(() =>
    {
        if (!loginUser || loginUser.length < 3)
        {
            return;
        }

        const timer = setTimeout(() =>
        {
            void (async () =>
            {
                try
                {
                    const result = await AuthService.generateCaptcha({ Login_User: loginUser });
                    if (result.Code === ApiResponseCode.Success)
                    {
                        setCaptchaData(result.Data);
                        if (result.Data.Captcha_Required)
                        {
                            setValue("Captcha_Value", "");
                        }
                    }
                    else
                    {
                        setCaptchaData(null);
                    }
                }
                catch
                {
                    setCaptchaData(null);
                }
            })();
        }, 500);

        return () =>
        {
            clearTimeout(timer);
        };
    }, [ loginUser, setValue ]);

    // ── Refresh CAPTCHA ─────────────────────────────────────
    const handleRefreshCaptcha = useCallback(() =>
    {
        if (!loginUser)
        {
            return;
        }
        setCaptchaLoading(true);

        void (async () =>
        {
            try
            {
                const result = await AuthService.generateCaptcha({ Login_User: loginUser });
                if (result.Code === ApiResponseCode.Success)
                {
                    setCaptchaData(result.Data);
                    setValue("Captcha_Value", "");
                }
            }
            catch
            {
                /* silent */
            }
            finally
            {
                setCaptchaLoading(false);
            }
        })();
    }, [ loginUser, setValue ]);

    // ── Submit ───────────────────────────────────────────────
    const handleFormFinish = useCallback(() =>
    {
        const onSubmit: SubmitHandler<LoginFormValues> = async (data) =>
        {
            setLoading(true);
            GlobalSpinner.show("Authenticating\u2026");
            try
            {
                // Guard: location denied — global modal will prompt the user
                if (locationDenied)
                {
                    setLoading(false);
                    GlobalSpinner.hide();
                    return;
                }

                const position = cachedPosition;

                const request: LoginRequest = {
                    Login_User: data.Login_User,
                    Login_Password: data.Login_Password,
                    Login_Latitude: position?.coords.latitude ?? 0,
                    Login_Longitude: position?.coords.longitude ?? 0,
                    Login_Accuracy: position?.coords.accuracy ?? 0,
                    Login_IP: cachedIpRef.current,
                    Login_Device: navigator.userAgent,
                    ...(captchaData?.Captcha_Required && {
                        Captcha_Id: captchaData.Captcha_Id,
                        Captcha_Value: data.Captcha_Value,
                    }),
                };

                const loginResult = await AuthService.login(request);

                if (loginResult.Code !== ApiResponseCode.Success)
                {
                    Message.error(loginResult.Message || "Login failed.");
                    setValue("Login_Password", "");

                    // Reload CAPTCHA after failure
                    if (data.Login_User)
                    {
                        try
                        {
                            const captchaCheck = await AuthService.generateCaptcha({ Login_User: data.Login_User });
                            if (captchaCheck.Code === ApiResponseCode.Success)
                            {
                                setCaptchaData(captchaCheck.Data);
                                setValue("Captcha_Value", "");
                            }
                            else
                            {
                                setCaptchaData(null);
                            }
                        }
                        catch
                        {
                            setCaptchaData(null);
                        }
                    }
                    return;
                }

                // Persist tokens
                const res = loginResult.Data;
                AuthStorage.setTokens(res.Access_Token, res.Refresh_Token);
                AuthStorage.setLoginUser(res.Login_User);

                // Set auth store
                setUser({
                    loginId: res.Login_Id,
                    loginUser: res.Login_User,
                    loginName: res.Login_Name,
                    roles: res.Roles,
                    branches: res.Branches,
                });

                // Pre-load menus + theme so ProtectedRoute doesn't need to fetch
                try
                {
                    const [ menuResult, themeResult ] = await Promise.all([
                        MenuService.getMenus(res.Login_Id, true),
                        ThemeService.getTheme(),
                    ]);

                    if (menuResult.Code === ApiResponseCode.Success)
                    {
                        setMenus(menuResult.Data);
                    }

                    if (themeResult.Code === ApiResponseCode.Success && themeResult.Data.Theme_Json)
                    {
                        try
                        {
                            const parsed = JSON.parse(themeResult.Data.Theme_Json) as { tokens?: Record<string, unknown>; isCompact?: boolean; isHappyWork?: boolean };
                            if (parsed.tokens)
                            {
                                setCustomTokens(parsed.tokens);
                            }
                            if (parsed.isCompact !== undefined)
                            {
                                setCompact(parsed.isCompact);
                            }
                            if (parsed.isHappyWork !== undefined)
                            {
                                setHappyWork(parsed.isHappyWork);
                            }
                        }
                        catch
                        { /* invalid JSON — keep current theme */ }
                    }
                }
                catch
                { /* ProtectedRoute will retry on failure */ }

                TokenRefreshManager.start();

                if (res.Must_Change_Password)
                {
                    Message.warning("Password reset required. Please change your password.");
                    void navigate("/Admin/ChangePassword", { replace: true });
                }
                else
                {
                    void navigate("/Home");
                }
            }
            catch
            {
                /* handled by interceptor */
            }
            finally
            {
                setLoading(false);
                GlobalSpinner.hide();
            }
        };

        void handleSubmit(onSubmit)();
    }, [ handleSubmit, captchaData, setValue, setUser, setMenus, setCustomTokens, setCompact, setHappyWork, navigate, cachedPosition, locationDenied ]);

    // ── Render ───────────────────────────────────────────────
    return (
        <div className={c("page")}>
            {/* ── Theme toggle (top right — desktop only) ───── */}
            {screens.md && (
                <div style={{ position: "absolute", top: 24, right: 30, zIndex: 999 }}>
                    <ThemeToggleButton
                        size="large"
                        tooltipPlacement="bottomLeft"
                    />
                </div>
            )}

            {/* ── Left panel: background visual ────────────── */}
            <div
                className={[ c("leftPanel"), companyLogo ? c("logoMode") : "" ].filter(Boolean).join(" ")}
                style={{
                    ...(bgImage && bgImageReady ? { backgroundImage: `url(${bgImage})` } : {}),
                    backgroundColor: companyLogo
                        ? (isDarkMode ? (logoBgColor ?? "#1e293b") : "#ffffff")
                        : undefined,
                }}
            >
                {/* Top branding */}
                <div className={c("contentLayer", "animSlideLeft")} style={{ opacity: companyLoaded ? 1 : 0, transition: "opacity 0.5s ease" }}>
                    <div className={c("glassCard")} style={{ padding: "16px 24px", maxWidth: "fit-content" }}>
                        <Space align="center" size={16}>
                            {appLogo && (
                                <Avatar
                                    shape="square"
                                    size={54}
                                    style={{ backgroundColor: "transparent", border: "none" }}
                                    icon={
                                        <img
                                            src={`data:image/png;base64,${appLogo}`}
                                            alt="app logo"
                                            style={{ width: "100%", height: "100%", objectFit: "contain", display: "block" }}
                                        />
                                    }
                                />
                            )}
                            <div>
                                <Typography.Title level={3} style={{ margin: 0, color: "#fff", lineHeight: 1 }}>
                                    {appHeader2}
                                </Typography.Title>
                                {tagline && (
                                    <Typography.Text style={{ color: "rgba(255,255,255,0.7)", letterSpacing: 2, fontSize: 11, fontWeight: 600 }}>
                                        {tagline}
                                    </Typography.Text>
                                )}
                            </div>
                        </Space>
                    </div>
                </div>

                {/* Bottom contact card */}
                <div className={c("contentLayer", "animSlideUp")} style={{ opacity: companyLoaded ? 1 : 0, transition: "opacity 0.5s 0.1s ease" }}>
                    <div className={c("glassCard")}>
                        <Space align="center" style={{ marginBottom: 16 }}>
                            <CustomerServiceOutlined style={{ color: token.colorPrimary, fontSize: 20 }} />
                            <Typography.Text strong style={{ textTransform: "uppercase", color: "rgba(255,255,255,0.6)", letterSpacing: 1, fontSize: 12 }}>
                                Enterprise Support
                            </Typography.Text>
                        </Space>

                        {contactMobile1 && (
                            <Space align="center" style={{ marginBottom: 10, width: "100%", justifyContent: "space-between" }}>
                                <a href={`tel:${contactMobile1}`} className={c("contactLink")}>
                                    IN {contactMobile1}
                                </a>
                                <a
                                    href={`https://wa.me/${contactMobile1.replace(/\+/g, "")}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    aria-label={`WhatsApp chat with ${contactMobile1}`}
                                >
                                    <WhatsAppOutlined style={{ color: "#25D366", fontSize: 18 }} />
                                </a>
                            </Space>
                        )}
                        {contactMobile2 && (
                            <Space align="center" style={{ width: "100%", justifyContent: "space-between" }}>
                                <a href={`tel:${contactMobile2}`} className={c("contactLink")}>
                                    IN {contactMobile2}
                                </a>
                                <a
                                    href={`https://wa.me/${contactMobile2.replace(/\+/g, "")}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    aria-label={`WhatsApp chat with ${contactMobile2}`}
                                >
                                    <WhatsAppOutlined style={{ color: "#25D366", fontSize: 18 }} />
                                </a>
                            </Space>
                        )}

                        <div style={{ marginTop: 14, fontSize: "0.8rem", opacity: 0.6 }}>
                            Available 24/7 for critical incidents.
                        </div>
                    </div>
                </div>
            </div>

            {/* ── Right panel: login form ───────────────────── */}
            <div className={c("rightPanel", themeClass)}>
                <div className={c("formWrapper", "animFadeScale")}>

                    {/* Mobile branding bar + theme toggle */}
                    {!screens.md && (
                        <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 16 }}>
                            <div className={c("mobileBranding", themeClass)}>
                                {appLogo && (
                                    <Avatar
                                        shape="square"
                                        size={42}
                                        style={{ backgroundColor: "transparent", border: "none", flexShrink: 0 }}
                                        icon={
                                            <img
                                                src={`data:image/png;base64,${appLogo}`}
                                                alt="app logo"
                                                style={{ width: "100%", height: "100%", objectFit: "contain", display: "block" }}
                                            />
                                        }
                                    />
                                )}
                                <div style={{ minWidth: 0 }}>
                                    <Typography.Title level={5} style={{ margin: 0, color: token.colorText, lineHeight: 1.2, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                                        {appHeader2}
                                    </Typography.Title>
                                    {tagline && (
                                        <Typography.Text style={{ color: token.colorTextSecondary, fontSize: 10, fontWeight: 600, letterSpacing: 1.5, textTransform: "uppercase" }}>
                                            {tagline}
                                        </Typography.Text>
                                    )}
                                </div>
                            </div>
                            <ThemeToggleButton
                                size="large"
                                tooltipPlacement="bottomLeft"
                            />
                        </div>
                    )}

                    {/* Heading */}
                    <div style={{ marginBottom: 32 }}>
                        <div
                            className={c("secureBadge")}
                            style={{
                                borderRadius: token.borderRadius,
                                background: `${token.colorPrimary}15`,
                                color: token.colorPrimary,
                                border: `1px solid ${token.colorPrimary}30`,
                            }}
                        >
                            SECURE GATEWAY
                        </div>
                        <Typography.Title level={2} style={{ margin: 0, marginBottom: 8, fontWeight: 800, color: token.colorText }}>
                            Welcome Back
                        </Typography.Title>
                        <Typography.Title level={3} style={{ margin: 0, fontWeight: 700, color: token.colorPrimary, textTransform: "uppercase", letterSpacing: 0.5, fontSize: "1.2rem" }}>
                            {appHeader1}
                        </Typography.Title>
                        <Typography.Text style={{ display: "block", marginTop: 8, color: isDarkMode ? "rgba(255,255,255,0.85)" : "rgba(0,0,0,0.65)", fontSize: 14 }}>
                            Please sign in to your dashboard.
                        </Typography.Text>
                    </div>

                    {/* Form */}
                    <Form layout="vertical" onFinish={handleFormFinish}>
                        <TextBox
                            control={control}
                            name="Login_User"
                            label="Username / Email"
                            placeholder="Enter your username"
                            required
                            autoComplete="username"
                            validation={{
                                required: "Username is required",
                                pattern: { value: /^[a-zA-Z0-9._@-]+$/, message: "No spaces or special characters" },
                            }}
                        />

                        <PasswordBox
                            control={control}
                            name="Login_Password"
                            label="Password"
                            placeholder="Enter your password"
                            required
                            autoComplete="current-password"
                            validation={{
                                required: "Password is required",
                            }}
                        />

                        {/* CAPTCHA (shown after failed attempts) */}
                        {captchaData?.Captcha_Required && (
                            <>
                                <div style={{ marginBottom: 16 }}>
                                    <div
                                        className={c("captchaBox")}
                                        style={{
                                            borderRadius: token.borderRadius,
                                            background: isDarkMode ? "rgba(255,255,255,0.05)" : "rgba(0,0,0,0.02)",
                                            border: `2px solid ${token.colorPrimary}30`,
                                        }}
                                    >
                                        <div style={{ flex: 1 }}>
                                            {(isDarkMode ? captchaData.Captcha_Image_Dark : captchaData.Captcha_Image_Light) && (
                                                <img
                                                    src={isDarkMode ? captchaData.Captcha_Image_Dark : captchaData.Captcha_Image_Light}
                                                    alt="CAPTCHA"
                                                    className={c("captchaImage")}
                                                    style={{ borderRadius: token.borderRadius, border: `1px solid ${token.colorBorder}` }}
                                                />
                                            )}
                                        </div>
                                        <Button
                                            type="text"
                                            icon={<ReloadOutlined spin={captchaLoading} />}
                                            onClick={handleRefreshCaptcha}
                                            loading={captchaLoading}
                                            size="large"
                                            title="Refresh CAPTCHA"
                                            style={{ color: token.colorPrimary, fontSize: 20 }}
                                        />
                                    </div>

                                    <div
                                        className={c("captchaWarning")}
                                        style={{
                                            borderRadius: token.borderRadius,
                                            background: `${token.colorWarning}15`,
                                            border: `1px solid ${token.colorWarning}40`,
                                        }}
                                    >
                                        <SafetyCertificateFilled style={{ color: token.colorWarning, fontSize: 16 }} />
                                        <Typography.Text style={{ fontSize: 12, color: token.colorWarning, fontWeight: 500 }}>
                                            CAPTCHA required after {String(captchaData.Failed_Attempts)} failed attempt(s)
                                        </Typography.Text>
                                    </div>
                                </div>

                                <TextBox
                                    control={control}
                                    name="Captcha_Value"
                                    label="Enter CAPTCHA"
                                    placeholder="Enter the characters shown above"
                                    required
                                    autoComplete="off"
                                    validation={{
                                        required: "CAPTCHA is required",
                                        minLength: { value: 6, message: "CAPTCHA must be 6 characters" },
                                        maxLength: { value: 6, message: "CAPTCHA must be 6 characters" },
                                    }}
                                />
                            </>
                        )}

                        {/* Forgot password link */}
                        <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: 24 }}>
                            <a href="#" style={{ color: token.colorPrimary, fontWeight: 600, fontSize: 14, textDecoration: "underline" }}>
                                Forgot password?
                            </a>
                        </div>

                        <Button
                            type="primary"
                            htmlType="submit"
                            block
                            size="large"
                            loading={loading}
                            icon={loading ? <LoadingOutlined /> : undefined}
                        >
                            Sign In
                        </Button>
                    </Form>

                    {/* Footer */}
                    <div style={{ marginTop: screens.md ? 60 : 20, textAlign: "center" }}>
                        <Divider style={{ borderColor: token.colorBorder, fontSize: 11, color: token.colorTextSecondary }}>
                            POWERED BY
                        </Divider>
                        <Space style={{ marginBottom: screens.md ? 16 : 8 }}>
                            <SafetyCertificateFilled style={{ fontSize: 16, color: token.colorTextSecondary }} />
                            <Typography.Text type="secondary" style={{ fontSize: 11, fontWeight: 600, letterSpacing: 0.5 }}>
                                {companyFullName}
                            </Typography.Text>
                        </Space>
                        <Typography.Text type="secondary" style={{ fontSize: 10, marginTop: 8, display: "block" }}>
                            v{__APP_VERSION__}
                        </Typography.Text>
                    </div>

                    {/* Mobile contact (bottom) */}
                    {!screens.md && (contactMobile1 || contactMobile2) && (
                        <div
                            className={c("mobileBranding", themeClass)}
                            style={{ marginTop: 16, flexDirection: "column", alignItems: "flex-start", gap: 6 }}
                        >
                            <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 10 }}>
                                <CustomerServiceOutlined style={{ color: token.colorPrimary, fontSize: 14 }} />
                                <Typography.Text strong style={{ fontSize: 10, textTransform: "uppercase", letterSpacing: 1, color: token.colorTextSecondary }}>
                                    Enterprise Support
                                </Typography.Text>
                            </div>

                            {contactMobile1 && (
                                <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", width: "100%", marginBottom: 6 }}>
                                    <a href={`tel:${contactMobile1}`} style={{ textDecoration: "none", color: token.colorText, fontFamily: "monospace", fontSize: 13, fontWeight: 600 }}>
                                        {contactMobile1}
                                    </a>
                                    <a href={`https://wa.me/${contactMobile1.replace(/\+/g, "")}`} target="_blank" rel="noopener noreferrer" aria-label={`WhatsApp chat with ${contactMobile1}`}>
                                        <WhatsAppOutlined style={{ color: "#25D366", fontSize: 14 }} />
                                    </a>
                                </div>
                            )}
                            {contactMobile2 && (
                                <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", width: "100%" }}>
                                    <a href={`tel:${contactMobile2}`} style={{ textDecoration: "none", color: token.colorText, fontFamily: "monospace", fontSize: 13, fontWeight: 600 }}>
                                        {contactMobile2}
                                    </a>
                                    <a href={`https://wa.me/${contactMobile2.replace(/\+/g, "")}`} target="_blank" rel="noopener noreferrer" aria-label={`WhatsApp chat with ${contactMobile2}`}>
                                        <WhatsAppOutlined style={{ color: "#25D366", fontSize: 14 }} />
                                    </a>
                                </div>
                            )}

                            <div style={{ marginTop: 8, fontSize: 11, color: token.colorTextSecondary, opacity: 0.7 }}>
                                Available 24/7 for critical incidents.
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
