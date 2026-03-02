import { memo } from "react";
import { Divider, Flex, Grid, Space, Tooltip, Typography, theme } from "antd";
import { SafetyCertificateFilled, WhatsAppOutlined } from "@ant-design/icons";
import { useCompanyStore, useThemeStore } from "@/Application/Index";
import c from "./Footer.module.css";

const CurrentYear = new Date().getFullYear();

function FooterInner(): React.ReactNode
{
    const { token } = theme.useToken();
    const { isDarkMode } = useThemeStore();
    const { fullName, shortName, mobile1, mobile2, email, tagline } = useCompanyStore();
    const screens = Grid.useBreakpoint();
    const isCompact = !screens.lg;

    const footerBg = isDarkMode ? token.colorBgContainer : token.colorBgElevated;

    return (
        <footer
            className={c["footer"]}
            style={{
                background: footerBg,
                borderTop: `1px solid ${token.colorBorder}`,
            }}
        >
            <Flex
                align="center"
                justify={isCompact ? "center" : "space-between"}
                vertical={isCompact}
                gap={isCompact ? 2 : 0}
            >
                {/* Company name with tooltip */}
                <Tooltip
                    title={
                        <div style={{ minWidth: 200 }}>
                            <Typography.Text strong style={{ color: "#fff", fontSize: 13, display: "block", marginBottom: 6 }}>
                                {fullName}
                            </Typography.Text>
                            {tagline && (
                                <Typography.Text style={{ color: "rgba(255,255,255,0.7)", fontSize: 11, display: "block", marginBottom: 8 }}>
                                    {tagline}
                                </Typography.Text>
                            )}
                            <Divider style={{ margin: "6px 0", borderColor: "rgba(255,255,255,0.15)" }} />
                            <Space orientation="vertical" size={6} style={{ width: "100%" }}>
                                {mobile1 && (
                                    <Flex align="center" justify="space-between" gap={8}>
                                        <a href={`tel:${mobile1}`} style={{ color: "rgba(255,255,255,0.85)", fontSize: 12, textDecoration: "none" }}>
                                            {mobile1}
                                        </a>
                                        <a
                                            href={`https://wa.me/${mobile1.replace(/\+/g, "")}`}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            style={{ lineHeight: 1 }}
                                        >
                                            <WhatsAppOutlined style={{ color: "#25D366", fontSize: 14 }} />
                                        </a>
                                    </Flex>
                                )}
                                {mobile2 && (
                                    <Flex align="center" justify="space-between" gap={8}>
                                        <a href={`tel:${mobile2}`} style={{ color: "rgba(255,255,255,0.85)", fontSize: 12, textDecoration: "none" }}>
                                            {mobile2}
                                        </a>
                                        <a
                                            href={`https://wa.me/${mobile2.replace(/\+/g, "")}`}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            style={{ lineHeight: 1 }}
                                        >
                                            <WhatsAppOutlined style={{ color: "#25D366", fontSize: 14 }} />
                                        </a>
                                    </Flex>
                                )}
                                {email && (
                                    <a href={`mailto:${email}`} style={{ color: "rgba(255,255,255,0.85)", fontSize: 12, textDecoration: "none" }}>
                                        {email}
                                    </a>
                                )}
                            </Space>
                        </div>
                    }
                    placement="top"
                >
                    <Space size={6} style={{ cursor: "pointer" }}>
                        <SafetyCertificateFilled style={{ fontSize: 13, color: token.colorPrimary }} />
                        <Typography.Text strong style={{ fontSize: 12 }}>
                            {fullName}
                        </Typography.Text>
                    </Space>
                </Tooltip>

                {/* Copyright + Version */}
                <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                    &copy; {CurrentYear} {shortName} | v{__APP_VERSION__}
                </Typography.Text>
            </Flex>
        </footer>
    );
}

const Footer = memo(FooterInner);
export default Footer;
