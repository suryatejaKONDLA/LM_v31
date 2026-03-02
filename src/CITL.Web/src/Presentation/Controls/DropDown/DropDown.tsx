import React, { useState, useMemo, useCallback, useRef } from "react";
import { Select, Form, Button, theme } from "antd";
import { SyncOutlined } from "@ant-design/icons";
import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";

/** Threshold above which Ant Design virtual scrolling is enabled. */
const VirtualizationThreshold = 50;

/**
 * Shape of each item in the DropDown data source.
 */
export interface DropDownItem<V = string | number>
{
    /** Value stored in the form field. */
    Col1: V;
    /** Display text shown in the dropdown. */
    Col2: string;
    /** Optional image URL for the option. */
    Image?: string;
}

/**
 * Props for the DropDown control.
 * Wraps Ant Design Select with react-hook-form Controller.
 */
export interface DropDownProps<T extends FieldValues, V = string | number>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text above the dropdown. */
    label?: string;
    /** Placeholder text. */
    placeholder?: string;
    /** Mark field as required (visual asterisk). */
    required?: boolean;
    /** Disable the dropdown. */
    disabled?: boolean;
    /** Show a clear button. Defaults to true. */
    allowClear?: boolean;
    /** Show the value code alongside the label (e.g. "Label [CODE]"). */
    showValueCode?: boolean;
    /** CSS class name. */
    className?: string;
    /** Array of items to display in the dropdown. */
    dataSource?: DropDownItem<V>[];
    /** Show item count footer. Defaults to true. */
    showFooter?: boolean;
    /** Label for the footer count. Defaults to "Count". */
    footerLabel?: string;
    /** Async callback to refresh the data source. Shows a refresh button in footer. */
    onRefresh?: () => Promise<void>;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
    /** Whether data is currently loading. */
    loading?: boolean;
}

/**
 * DropDown — Searchable select with react-hook-form integration.
 *
 * Features:
 * - AND/OR search with highlighted matching text
 * - Auto virtualization above 50 items
 * - Footer with match count and optional refresh button
 * - Optional value code display and image support
 */
export function DropDown<T extends FieldValues, V = string | number>({
    control,
    name,
    label,
    placeholder,
    required = false,
    disabled = false,
    allowClear = true,
    showValueCode = false,
    className,
    dataSource = [],
    showFooter = true,
    footerLabel = "Count",
    onRefresh,
    validation,
    loading = false,
}: DropDownProps<T, V>): React.JSX.Element
{
    const { token } = theme.useToken();
    const [ searchQuery, setSearchQuery ] = useState("");
    const [ refreshing, setRefreshing ] = useState(false);

    // Cached regex patterns to avoid recompilation
    const regexCache = useRef<Map<string, RegExp>>(new Map());

    // ── Search filtering ──────────────────────────────────────────

    const filteredData = useMemo(() =>
    {
        if (!searchQuery.trim())
        {
            return dataSource;
        }

        const terms = searchQuery
            .trim()
            .split(/\s+/)
            .flatMap((t) => t.split("|"))
            .filter((t) => t.length > 0);

        if (terms.length === 0)
        {
            return dataSource;
        }

        const lowerTerms = terms.map((t) => t.toLowerCase());

        return dataSource.filter((item) =>
        {
            const text = item.Col2.toLowerCase();
            const code = String(item.Col1).toLowerCase();

            // AND logic: every term must appear in either text or code
            return lowerTerms.every((t) => text.includes(t) || code.includes(t));
        });
    }, [ dataSource, searchQuery ]);

    // ── Highlight helper ──────────────────────────────────────────

    const highlightStyle = useMemo(
        () => ({ color: token.colorPrimary, fontWeight: 600 }) as const,
        [ token.colorPrimary ],
    );

    const highlightText = useCallback(
        (text: string, query: string): React.ReactNode =>
        {
            if (!query.trim())
            {
                return text;
            }

            const terms = query
                .trim()
                .split(/\s+/)
                .flatMap((t) => t.split("|"))
                .filter((t) => t.length > 0);

            if (terms.length === 0)
            {
                return text;
            }

            const cacheKey = query;
            let regex = regexCache.current.get(cacheKey);

            if (!regex)
            {
                const escaped = terms.map((t) => t.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"));
                regex = new RegExp(`(${escaped.join("|")})`, "gi");
                regexCache.current.set(cacheKey, regex);

                // Evict oldest entry when cache exceeds limit
                if (regexCache.current.size > 50)
                {
                    const firstKey = regexCache.current.keys().next().value;

                    if (firstKey !== undefined)
                    {
                        regexCache.current.delete(firstKey);
                    }
                }
            }

            const parts = text.split(regex);

            return parts.map((part, i) =>
            {
                const isMatch = terms.some((t) => part.toLowerCase() === t.toLowerCase());

                return isMatch
                    ? <strong key={i} style={highlightStyle}>{part}</strong>
                    : <span key={i}>{part}</span>;
            });
        },
        [ highlightStyle ],
    );

    // ── Option building ───────────────────────────────────────────

    const imageStyle = useMemo(
        () => ({ width: 20, height: 14, borderRadius: 2, objectFit: "cover" as const }),
        [],
    );

    const flexStyle = useMemo(
        () => ({ display: "flex", alignItems: "center", gap: 8 }) as const,
        [],
    );

    const options = useMemo(
        () =>
            filteredData.map((item) => ({
                value: item.Col1,
                label: (
                    <div style={flexStyle}>
                        {item.Image && (
                            <img src={item.Image} alt={item.Col2} style={imageStyle} />
                        )}
                        <span>
                            {showValueCode
                                ? (
                                    <>
                                        {highlightText(item.Col2, searchQuery)}{" "}
                                        [{highlightText(String(item.Col1), searchQuery)}]
                                    </>
                                )
                                : highlightText(item.Col2, searchQuery)}
                        </span>
                    </div>
                ),
            })),
        [ filteredData, showValueCode, searchQuery, highlightText, flexStyle, imageStyle ],
    );

    // ── Virtualization ────────────────────────────────────────────

    const shouldVirtualize = dataSource.length > VirtualizationThreshold;

    // ── Handlers ──────────────────────────────────────────────────

    const handleSearch = useCallback((value: string) =>
    {
        setSearchQuery(value);
    }, []);

    const handleDropdownVisibleChange = useCallback((open: boolean) =>
    {
        if (!open)
        {
            setSearchQuery("");
        }
    }, []);

    const handleRefresh = useCallback(
        async (e: React.MouseEvent) =>
        {
            e.stopPropagation();

            if (!onRefresh || refreshing)
            {
                return;
            }

            setRefreshing(true);

            try
            {
                await onRefresh();
            }
            finally
            {
                setRefreshing(false);
            }
        },
        [ onRefresh, refreshing ],
    );

    // ── Footer styles ─────────────────────────────────────────────

    const footerStyle = useMemo(
        () => ({
            borderTop: `1px solid ${token.colorBorderSecondary}`,
            padding: "8px 12px",
            fontSize: 12,
            color: token.colorTextSecondary,
            display: "flex" as const,
            justifyContent: "space-between" as const,
            alignItems: "center" as const,
            background: token.colorBgLayout,
        }),
        [ token.colorBorderSecondary, token.colorTextSecondary, token.colorBgLayout ],
    );

    const refreshBtnStyle = useMemo(
        () => ({ padding: "2px 6px", height: "auto", fontSize: 12, color: token.colorPrimary }),
        [ token.colorPrimary ],
    );

    // ── Popup renderer ────────────────────────────────────────────

    const renderPopup = useCallback(
        (menu: React.ReactNode) => (
            <>
                {menu}
                {showFooter && dataSource.length > 0 && (
                    <div style={footerStyle}>
                        <div style={{ flex: 1 }}>
                            {footerLabel}: <strong>{filteredData.length}</strong>
                            {searchQuery && filteredData.length !== dataSource.length && (
                                <span style={{ marginLeft: 8, opacity: 0.7 }}>
                                    (of {dataSource.length})
                                </span>
                            )}
                        </div>
                        {onRefresh && (
                            <Button
                                type="link"
                                size="small"
                                icon={<SyncOutlined spin={refreshing} />}
                                onClick={(e) =>
                                {
                                    void handleRefresh(e);
                                }}
                                disabled={refreshing || disabled}
                                style={refreshBtnStyle}
                                title="Refresh data"
                            />
                        )}
                    </div>
                )}
            </>
        ),
        [
            showFooter, dataSource.length, footerStyle, footerLabel,
            filteredData.length, searchQuery, onRefresh, refreshing,
            handleRefresh, disabled, refreshBtnStyle,
        ],
    );

    // ── Render ─────────────────────────────────────────────────────

    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) => (
                <Form.Item
                    label={label}
                    style={{ textAlign: "left" }}
                    {...(error ? { validateStatus: "error" as const, help: error.message } : {})}
                    required={required}
                >
                    <Select
                        value={field.value || undefined}
                        onChange={(v) =>
                        {
                            field.onChange(v);
                        }}
                        onBlur={field.onBlur}
                        ref={field.ref}
                        placeholder={placeholder}
                        disabled={disabled}
                        allowClear={allowClear}
                        loading={loading}
                        showSearch={{ filterOption: false, onSearch: handleSearch }}
                        onOpenChange={handleDropdownVisibleChange}
                        size="large"
                        options={options}
                        style={{ width: "100%" }}
                        virtual={shouldVirtualize}
                        popupRender={renderPopup}
                        {...(className ? { className } : {})}
                    />
                </Form.Item>
            )}
        />
    );
}
