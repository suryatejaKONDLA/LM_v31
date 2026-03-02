import React, { Fragment, memo, useMemo } from "react";
import { Col, Modal, Row, Space, Tag, Typography, theme } from "antd";
import type { ShortcutCategory, ShortcutDefinition } from "@/Application/Index";

const { Text, Title } = Typography;

// ============================================
// PROPS
// ============================================

/**
 * Props for the KeyboardShortcutsHelp component.
 */
export interface KeyboardShortcutsHelpProps
{
    /** Whether the modal is open. */
    open: boolean;
    /** Callback when the modal is closed. */
    onClose: () => void;
    /** Shortcut definitions to display. Pass from a store or context. */
    shortcuts: ReadonlyMap<string, ShortcutDefinition> | ShortcutDefinition[];
}

// ============================================
// CONSTANTS
// ============================================

const CategoryLabels: Record<ShortcutCategory, string> =
{
    global: "Global",
    navigation: "Navigation",
    form: "Form Actions",
    theme: "Theme",
    custom: "Custom",
};

const CategoryOrder: ShortcutCategory[] = [ "global", "navigation", "form", "theme", "custom" ];

// ============================================
// HELPERS
// ============================================

function formatKeys(combo: string): string[]
{
    return combo.split("+").map((k) =>
    {
        const lower = k.trim().toLowerCase();

        switch (lower)
        {
            case "ctrl":
            case "control":
                return "Ctrl";
            case "alt":
                return "Alt";
            case "shift":
                return "Shift";
            case "meta":
            case "cmd":
            case "command":
                return "\u2318";
            case "escape":
                return "Esc";
            case "left":
                return "\u2190";
            case "right":
                return "\u2192";
            case "up":
                return "\u2191";
            case "down":
                return "\u2193";
            case "space":
                return "Space";
            case "enter":
                return "Enter";
            case "tab":
                return "Tab";
            case "backspace":
                return "\u232B";
            case "delete":
                return "Del";
            default:
                return lower.toUpperCase();
        }
    });
}

/** Normalise shortcuts to an array regardless of input shape. */
function toArray(
    input: ReadonlyMap<string, ShortcutDefinition> | ShortcutDefinition[],
): ShortcutDefinition[]
{
    if (Array.isArray(input))
    {
        return input;
    }

    return [ ...input.values() ];
}

// ============================================
// COMPONENT
// ============================================

/**
 * Modal that lists registered keyboard shortcuts, grouped by category.
 * Accepts shortcuts as a prop so it works with any state management approach
 * (Zustand store, React context, or direct prop drilling).
 */
function KeyboardShortcutsHelpInner(
    { open, onClose, shortcuts }: KeyboardShortcutsHelpProps,
): React.ReactElement
{
    const { token } = theme.useToken();

    const grouped = useMemo(() =>
    {
        const map = new Map<ShortcutCategory, ShortcutDefinition[]>();

        for (const cat of CategoryOrder)
        {
            map.set(cat, []);
        }

        for (const shortcut of toArray(shortcuts))
        {
            // Filter out duplicate help toggles if present.
            if (shortcut.id === "helpF1")
            {
                continue;
            }

            const list = map.get(shortcut.category);

            if (list)
            {
                list.push(shortcut);
            }
        }

        // Remove empty categories.
        for (const [ category, items ] of map)
        {
            if (items.length === 0)
            {
                map.delete(category);
            }
        }

        return map;
    }, [ shortcuts ]);

    return (
        <Modal
            title={
                <Space>
                    <span style={{ fontSize: 20 }}>⌨️</span>
                    <Title level={5} style={{ margin: 0 }}>Keyboard Shortcuts</Title>
                </Space>
            }
            open={open}
            onCancel={onClose}
            footer={
                <Text type="secondary" style={{ fontSize: 12 }}>
                    Press <Tag style={{ margin: 0 }}>F1</Tag> to toggle this help
                </Text>
            }
            width={500}
            centered
            styles={{
                body: {
                    maxHeight: "60vh",
                    overflowY: "auto",
                    padding: "16px 0",
                },
            }}
        >
            <Space orientation="vertical" size="large" style={{ width: "100%" }}>
                {CategoryOrder.map((category) =>
                {
                    const items = grouped.get(category);

                    if (!items || items.length === 0)
                    {
                        return null;
                    }

                    return (
                        <div key={category}>
                            <Text
                                strong
                                style={{
                                    display: "block",
                                    marginBottom: 12,
                                    fontSize: 13,
                                    color: token.colorTextSecondary,
                                    textTransform: "uppercase",
                                    letterSpacing: 0.5,
                                }}
                            >
                                {CategoryLabels[category]}
                            </Text>

                            <Space orientation="vertical" size="small" style={{ width: "100%" }}>
                                {items.map((shortcut) => (
                                    <Row
                                        key={shortcut.id}
                                        justify="space-between"
                                        align="middle"
                                        style={{
                                            padding: "8px 12px",
                                            borderRadius: token.borderRadius,
                                            backgroundColor: token.colorFillQuaternary,
                                        }}
                                    >
                                        <Col>
                                            <Text style={{ opacity: shortcut.enabled ? 1 : 0.5 }}>
                                                {shortcut.description}
                                            </Text>
                                        </Col>
                                        <Col>
                                            <Space size={4}>
                                                {formatKeys(shortcut.keys).map((key, idx) => (
                                                    <Fragment key={idx}>
                                                        {idx > 0 && (
                                                            <Text type="secondary" style={{ fontSize: 12 }}>+</Text>
                                                        )}
                                                        <Tag
                                                            style={{
                                                                margin: 0,
                                                                padding: "2px 8px",
                                                                fontSize: 12,
                                                                fontFamily: "monospace",
                                                                backgroundColor: token.colorBgContainer,
                                                                border: `1px solid ${token.colorBorder}`,
                                                                borderRadius: 4,
                                                                boxShadow: `0 2px 0 ${token.colorBorder}`,
                                                            }}
                                                        >
                                                            {key}
                                                        </Tag>
                                                    </Fragment>
                                                ))}
                                            </Space>
                                        </Col>
                                    </Row>
                                ))}
                            </Space>
                        </div>
                    );
                })}
            </Space>
        </Modal>
    );
}

export const KeyboardShortcutsHelp = memo(KeyboardShortcutsHelpInner);
