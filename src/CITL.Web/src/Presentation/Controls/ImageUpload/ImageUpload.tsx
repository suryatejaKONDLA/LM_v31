import React, { useCallback, useEffect, useMemo, useRef } from "react";
import { Upload, Form, Button, Space, Popconfirm, theme } from "antd";
import { PictureOutlined, UploadOutlined, DeleteOutlined } from "@ant-design/icons";
import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";
import type { RcFile } from "antd/es/upload";
import { ModalDialog } from "@/Shared/UI/ModalDialog";

// ============================================
// IMAGE HELPERS (inlined to avoid external dependency)
// ============================================

const Base64Regex = /^[A-Za-z0-9+/=]+$/;

function getImageType(value: string): "url" | "base64" | "unknown"
{
    if (value.startsWith("http://") || value.startsWith("https://") || value.startsWith("/"))
    {
        return "url";
    }

    if (value.startsWith("data:image/"))
    {
        return "base64";
    }

    // Raw base64 without prefix
    if (Base64Regex.test(value) && value.length > 20)
    {
        return "base64";
    }

    return "unknown";
}

function addBase64Prefix(value: string): string
{
    if (value.startsWith("data:"))
    {
        return value;
    }

    return `data:image/png;base64,${value}`;
}

// ============================================
// PROPS
// ============================================

/**
 * Props for the ImageUpload control.
 * Wraps Ant Design Upload with react-hook-form Controller and avatar preview.
 */
export interface ImageUploadProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name — value can be File | string | null. */
    name: Path<T>;
    /** Label text. */
    label?: string;
    /** Mark field as required. */
    required?: boolean;
    /** Disable upload interactions. */
    disabled?: boolean;
    /** Preview shape. Defaults to "circle". */
    shape?: "circle" | "square";
    /** Preview size in pixels. Defaults to 100. */
    size?: number;
    /** Maximum file size in megabytes. Defaults to 5. */
    maxSizeMb?: number;
    /** Accepted MIME types. Defaults to common image formats. */
    accept?: string;
    /** Extra react-hook-form validation rules. */
    validation?: RegisterOptions<T>;
    /** Upload button label. Defaults to "Upload Image". */
    uploadButtonText?: string;
    /** Show the clear/remove button. Defaults to true. */
    showClearButton?: boolean;
}

// ============================================
// COMPONENT
// ============================================

const DefaultAccept = "image/png,image/jpeg,image/jpg,image/webp";

/**
 * Image upload control with avatar preview.
 * Stores a raw File for new uploads or keeps existing string URL / base64.
 */
export function ImageUpload<T extends FieldValues>(
    {
        control,
        name,
        label,
        required = false,
        disabled = false,
        shape = "circle",
        size = 100,
        maxSizeMb = 5,
        accept = DefaultAccept,
        validation,
        uploadButtonText = "Upload Image",
        showClearButton = true,
    }: ImageUploadProps<T>,
): React.ReactElement
{
    const { token } = theme.useToken();

    // ----- object URL lifecycle (prevents memory leaks) -----
    const objectUrlRef = useRef<{ file: File; url: string } | null>(null);

    useEffect(() =>
    {
        return () =>
        {
            if (objectUrlRef.current)
            {
                URL.revokeObjectURL(objectUrlRef.current.url);
            }
        };
    }, []);

    // ----- accepted type set (memoised once) -----
    const acceptedTypes = useMemo(
        () => new Set(accept.split(",").map((t) => t.trim())),
        [ accept ],
    );

    // ----- before-upload guard -----
    const beforeUpload = useCallback(
        (file: RcFile): boolean =>
        {
            if (!acceptedTypes.has(file.type))
            {
                ModalDialog.warning({
                    title: "Invalid File Type",
                    content: `Please upload a valid image file (${accept}).`,
                });
                return false;
            }

            if (file.size / 1024 / 1024 >= maxSizeMb)
            {
                ModalDialog.warning({
                    title: "File Too Large",
                    content: `Image must be smaller than ${String(maxSizeMb)} MB.`,
                });
                return false;
            }

            return true;
        },
        [ acceptedTypes, accept, maxSizeMb ],
    );

    // ----- resolve preview src (caches object URLs per File instance) -----
    const getImageSrc = useCallback(
        (value: File | string | null | undefined): string | undefined =>
        {
            if (value instanceof File)
            {
                // Reuse existing URL if same File reference
                if (objectUrlRef.current?.file === value)
                {
                    return objectUrlRef.current.url;
                }

                // Revoke previous URL before creating a new one
                if (objectUrlRef.current)
                {
                    URL.revokeObjectURL(objectUrlRef.current.url);
                }

                const url = URL.createObjectURL(value);
                objectUrlRef.current = { file: value, url };
                return url;
            }

            // Clean up stale object URL when value is no longer a File
            if (objectUrlRef.current)
            {
                URL.revokeObjectURL(objectUrlRef.current.url);
                objectUrlRef.current = null;
            }

            if (typeof value === "string")
            {
                const type = getImageType(value);

                if (type === "url")
                {
                    return value;
                }

                if (type === "base64")
                {
                    return addBase64Prefix(value);
                }
            }

            return undefined;
        },
        [],
    );

    // ----- static styles -----
    const containerStyle = useMemo<React.CSSProperties>(
        () => ({
            width: size,
            height: size,
            borderRadius: shape === "circle" ? "50%" : 8,
            backgroundColor: token.colorBgContainer,
            border: `2px dashed ${token.colorBorder}`,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            overflow: "hidden",
        }),
        [ size, shape, token.colorBgContainer, token.colorBorder ],
    );

    const imgStyle = useMemo<React.CSSProperties>(
        () => ({ width: "100%", height: "100%", objectFit: "contain" }),
        [],
    );

    const placeholderStyle = useMemo<React.CSSProperties>(
        () => ({ fontSize: size / 3, color: token.colorTextDescription }),
        [ size, token.colorTextDescription ],
    );

    const centerStyle = useMemo<React.CSSProperties>(
        () => ({ display: "flex", justifyContent: "center" }),
        [],
    );

    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) =>
            {
                const src = getImageSrc(field.value);

                return (
                    <Form.Item
                        label={label}
                        style={{ textAlign: "left" }}
                        {...(error && { validateStatus: "error" as const, help: error.message })}
                        required={required}
                    >
                        <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
                            {/* Avatar Preview */}
                            <div style={centerStyle}>
                                <div style={containerStyle}>
                                    {src ? (
                                        <img src={src} alt="Preview" style={imgStyle} />
                                    ) : (
                                        <PictureOutlined style={placeholderStyle} />
                                    )}
                                </div>
                            </div>

                            {/* Upload & Clear Buttons */}
                            <Space>
                                <Upload
                                    accept={accept}
                                    showUploadList={false}
                                    beforeUpload={beforeUpload}
                                    customRequest={({ file }) =>
                                    {
                                        field.onChange(file as File);
                                    }}
                                    disabled={disabled}
                                >
                                    <Button icon={<UploadOutlined />} disabled={disabled}>
                                        {uploadButtonText}
                                    </Button>
                                </Upload>

                                {showClearButton && field.value && (
                                    <Popconfirm
                                        title="Remove Image"
                                        description="Are you sure you want to remove this image?"
                                        onConfirm={() =>
                                        {
                                            field.onChange(null);
                                        }}
                                        okText="Yes"
                                        cancelText="No"
                                        disabled={disabled}
                                    >
                                        <Button danger icon={<DeleteOutlined />} disabled={disabled}>
                                            Clear
                                        </Button>
                                    </Popconfirm>
                                )}
                            </Space>
                        </Space>
                    </Form.Item>
                );
            }}
        />
    );
}
