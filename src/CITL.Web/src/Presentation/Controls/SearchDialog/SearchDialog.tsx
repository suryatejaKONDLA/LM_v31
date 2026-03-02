import React, { memo, useCallback, useEffect, useDeferredValue, useMemo, useRef, useState } from "react";
import { Input, List, Modal, theme, type InputRef } from "antd";
import { SearchOutlined, EnterOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useMenuStore } from "@/Application/Index";
import { SearchEngineHelper } from "@/Shared/Index";
import type { Menu } from "@/Domain/Index";

// ============================================
// PROPS
// ============================================

/**
 * Props for the SearchDialog control.
 */
export interface SearchDialogProps
{
    /** Whether the dialog is open. */
    open: boolean;
    /** Callback when the dialog is closed. */
    onClose: () => void;
}

// ============================================
// MAX RESULTS
// ============================================

const MaxResults = 10;

// ============================================
// INNER CONTENT (re-mounted on each open via destroyOnHidden)
// ============================================

interface SearchDialogContentProps
{
    onClose: () => void;
    token: ReturnType<typeof theme.useToken>["token"];
}

const SearchDialogContent = memo(({ onClose, token }: SearchDialogContentProps) =>
{
    const inputRef = useRef<InputRef>(null);
    const navigate = useNavigate();
    const { menus } = useMenuStore();

    const [ searchTerm, setSearchTerm ] = useState("");
    const [ selectedIndex, setSelectedIndex ] = useState(0);

    // Defer expensive filtering while the user types rapidly.
    const deferredSearch = useDeferredValue(searchTerm);

    // ----- memoised styles -----
    const inputPrefixStyle = useMemo<React.CSSProperties>(
        () => ({ color: token.colorTextSecondary, fontSize: 18 }),
        [ token.colorTextSecondary ],
    );

    const noResultsStyle = useMemo<React.CSSProperties>(
        () => ({ textAlign: "center", padding: 20, color: token.colorTextSecondary }),
        [ token.colorTextSecondary ],
    );

    const resultsContainerStyle = useMemo<React.CSSProperties>(
        () => ({ maxHeight: 400, overflowY: "auto" }),
        [],
    );

    const listItemTitleStyle = useMemo<React.CSSProperties>(
        () => ({ fontWeight: 500 }),
        [],
    );

    // ----- auto-focus -----
    useEffect(() =>
    {
        const timer = setTimeout(() =>
        {
            inputRef.current?.focus();
        }, 100);

        return () =>
        {
            clearTimeout(timer);
        };
    }, []);

    // ----- flatten menus to searchable list -----
    const flatMenus = useMemo<Menu[]>(() =>
    {
        const flat: Menu[] = [];

        const traverse = (items: Menu[]): void =>
        {
            for (const item of items)
            {
                if (item.MENU_URL1)
                {
                    flat.push(item);
                }

                if (item.Children.length > 0)
                {
                    traverse(item.Children);
                }
            }
        };

        traverse(menus);
        return flat;
    }, [ menus ]);

    // ----- search engine (recreated when menu data changes) -----
    const engine = useMemo(
        () => new SearchEngineHelper<Menu>(
            flatMenus,
            (m) => m.MENU_Name,
            (m) => m.MENU_Description,
            (m) => m.MENU_Parent_ID,
        ),
        [ flatMenus ],
    );

    // ----- filtered results -----
    const filteredMenus = useMemo<Menu[]>(() =>
    {
        if (!deferredSearch.trim())
        {
            return [];
        }

        return engine.search(deferredSearch).slice(0, MaxResults);
    }, [ deferredSearch, engine ]);

    // Clamp selected index to a valid range.
    const clampedIndex = Math.min(
        Math.max(0, selectedIndex),
        Math.max(0, filteredMenus.length - 1),
    );

    // ----- navigation -----
    const handleMenuClick = useCallback(
        (menu: Menu): void =>
        {
            if (!menu.MENU_URL1)
            {
                return;
            }

            void navigate(menu.MENU_URL1);
            onClose();
        },
        [ navigate, onClose ],
    );

    // ----- keyboard -----
    const handleKeyDown = useCallback(
        (e: React.KeyboardEvent): void =>
        {
            if (e.key === "Escape")
            {
                onClose();
                return;
            }

            if (filteredMenus.length === 0)
            {
                return;
            }

            if (e.key === "ArrowDown")
            {
                e.preventDefault();
                setSelectedIndex((prev) => (prev + 1) % filteredMenus.length);
            }
            else if (e.key === "ArrowUp")
            {
                e.preventDefault();
                setSelectedIndex((prev) => (prev - 1 + filteredMenus.length) % filteredMenus.length);
            }
            else if (e.key === "Enter")
            {
                e.preventDefault();
                const selected = filteredMenus[clampedIndex];

                if (selected)
                {
                    handleMenuClick(selected);
                }
            }
        },
        [ onClose, filteredMenus, clampedIndex, handleMenuClick ],
    );

    // ----- per-item style -----
    const getItemStyle = useCallback(
        (index: number): React.CSSProperties => ({
            cursor: "pointer",
            padding: 12,
            borderRadius: 8,
            transition: "background 0.1s",
            background: index === clampedIndex ? token.colorFillTertiary : "transparent",
            scrollMargin: 50,
        }),
        [ clampedIndex, token.colorFillTertiary ],
    );

    const inputStyle = useMemo<React.CSSProperties>(
        () => ({
            fontSize: 16,
            borderRadius: 8,
            marginBottom: filteredMenus.length > 0 ? 16 : 0,
        }),
        [ filteredMenus.length ],
    );

    return (
        <>
            <Input
                ref={inputRef}
                size="large"
                placeholder="Search menus…"
                prefix={<SearchOutlined style={inputPrefixStyle} />}
                allowClear
                value={searchTerm}
                onChange={(e) =>
                {
                    setSearchTerm(e.target.value);
                }}
                onKeyDown={handleKeyDown}
                style={inputStyle}
            />

            {filteredMenus.length > 0 && (
                <div style={resultsContainerStyle}>
                    <List
                        dataSource={filteredMenus}
                        renderItem={(item, index) => (
                            <List.Item
                                onClick={() =>
                                {
                                    handleMenuClick(item);
                                }}
                                onMouseEnter={() =>
                                {
                                    setSelectedIndex(index);
                                }}
                                style={getItemStyle(index)}
                            >
                                <List.Item.Meta
                                    title={<span style={listItemTitleStyle}>{item.MENU_Description ?? item.MENU_Name}</span>}
                                />
                                <EnterOutlined style={{ color: token.colorTextTertiary }} />
                            </List.Item>
                        )}
                        split={false}
                    />
                </div>
            )}

            {searchTerm && filteredMenus.length === 0 && (
                <div style={noResultsStyle}>No results found</div>
            )}
        </>
    );
});

SearchDialogContent.displayName = "SearchDialogContent";

// ============================================
// MAIN COMPONENT
// ============================================

/**
 * Command-palette style search dialog.
 * Opens a modal with a search input over the flattened menu tree.
 * Supports keyboard navigation (↑ ↓ Enter Esc).
 */
export const SearchDialog = memo(({ open, onClose }: SearchDialogProps): React.ReactElement =>
{
    const { token } = theme.useToken();

    return (
        <Modal
            open={open}
            onCancel={onClose}
            footer={null}
            closable={false}
            width={600}
            style={{ top: 100 }}
            styles={{ body: { padding: 16 } }}
            destroyOnHidden
        >
            {open && <SearchDialogContent onClose={onClose} token={token} />}
        </Modal>
    );
});

SearchDialog.displayName = "SearchDialog";
