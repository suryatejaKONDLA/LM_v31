import React, { useCallback, useMemo } from "react";
import { Upload, Form, Button, Space, Tag, Typography, Popconfirm, theme } from "antd";
import { FileOutlined, FilePdfOutlined, FileImageOutlined, DeleteOutlined, InboxOutlined } from "@ant-design/icons";
import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";
import type { RcFile } from "antd/es/upload";
import { ModalDialog } from "@/Shared/UI/ModalDialog";

const { Text } = Typography;
const { Dragger } = Upload;

/**
 * Props for the FileUpload control.
 * Wraps Ant Design Upload.Dragger with react-hook-form Controller.
 */
export interface FileUploadProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text above the upload area. */
    label?: string;
    /** Optional code tag shown above the upload area. */
    code?: string | number;
    /** Mark field as required (visual asterisk). */
    required?: boolean;
    /** Disable the upload. */
    disabled?: boolean;
    /** Maximum file size in MB. Defaults to 5. */
    maxSizeMb?: number;
    /** Accepted MIME types (comma-separated). Defaults to PDF + images. */
    accept?: string;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
    /** Show the clear button when a file is selected. Defaults to true. */
    showClearButton?: boolean;
    /** Callback when a file is uploaded. */
    onFileUpload?: (file: File) => void;
    /** Callback when the file is cleared. */
    onFileClear?: () => void;
}

/** MIME-to-friendly-name mapping. */
const MimeNameMap: Record<string, string> = {
    "application/pdf": "PDF",
    "image/png": "PNG",
    "image/jpeg": "JPEG",
    "image/jpg": "JPG",
    "image/webp": "WEBP",
};

const DefaultAccept = "application/pdf,image/png,image/jpeg,image/jpg";

/**
 * FileUpload — Drag-and-drop file upload with react-hook-form integration.
 *
 * Features:
 * - Supports File objects, URL strings, and base64 strings
 * - File type and size validation with user-friendly modals
 * - Preview strip with type-specific icons
 * - Clear with confirmation popover
 */
export function FileUpload<T extends FieldValues>({
    control,
    name,
    label,
    code,
    required = false,
    disabled = false,
    maxSizeMb = 5,
    accept = DefaultAccept,
    validation,
    showClearButton = true,
    onFileUpload,
    onFileClear,
}: FileUploadProps<T>): React.JSX.Element
{
    const { token } = theme.useToken();

    const acceptedTypes = useMemo(
        () => accept.split(",").map((t) => t.trim()),
        [ accept ],
    );

    const friendlyFormats = useMemo(
        () => acceptedTypes.map((t) => MimeNameMap[t] ?? t.toUpperCase()).join(", "),
        [ acceptedTypes ],
    );

    // ── Validation ────────────────────────────────────────────────

    const beforeUpload = useCallback(
        (file: RcFile): boolean =>
        {
            const isValidType = acceptedTypes.some((type) =>
            {
                if (type.includes("image/"))
                {
                    return file.type.startsWith("image/");
                }

                return file.type === type;
            });

            if (!isValidType)
            {
                ModalDialog.warning({
                    title: "Invalid File Type",
                    content: (
                        <div>
                            <div>Please upload a valid file.</div>
                            <div style={{ marginTop: 8 }}>
                                <strong>Supported formats: </strong>
                                <strong>{friendlyFormats}</strong>
                            </div>
                        </div>
                    ),
                });

                return false;
            }

            if (file.size / 1024 / 1024 >= maxSizeMb)
            {
                ModalDialog.warning({
                    title: "File Too Large",
                    content: `File must be smaller than ${String(maxSizeMb)} MB.`,
                });

                return false;
            }

            return true;
        },
        [ acceptedTypes, friendlyFormats, maxSizeMb ],
    );

    // ── File icon helper ──────────────────────────────────────────

    const getFileIcon = useCallback(
        (value: File | string | null | undefined): React.ReactNode =>
        {
            if (!value)
            {
                return <FileOutlined style={{ fontSize: 24, color: token.colorTextDescription }} />;
            }

            let isPdf = false;
            let isImage = false;

            if (value instanceof File)
            {
                isPdf = value.type === "application/pdf";
                isImage = value.type.startsWith("image/");
            }
            else if (typeof value === "string")
            {
                isPdf = value.toLowerCase().endsWith(".pdf") || value.includes("application/pdf");
                isImage = /\.(jpg|jpeg|png|gif|webp)$/i.test(value) || value.startsWith("data:image/");
            }

            if (isPdf)
            {
                return <FilePdfOutlined style={{ fontSize: 24, color: token.colorError }} />;
            }

            if (isImage)
            {
                return <FileImageOutlined style={{ fontSize: 24, color: token.colorPrimary }} />;
            }

            return <FileOutlined style={{ fontSize: 24, color: token.colorTextDescription }} />;
        },
        [ token.colorTextDescription, token.colorError, token.colorPrimary ],
    );

    // ── File name helper ──────────────────────────────────────────

    const getFileName = useCallback((value: File | string | null | undefined): string =>
    {
        if (!value)
        {
            return "No file selected";
        }

        if (value instanceof File)
        {
            return value.name;
        }

        if (typeof value === "string")
        {
            const parts = value.split("/");
            const filename = parts[parts.length - 1] ?? "Unknown file";

            return filename.length > 30 ? `${filename.substring(0, 27)}...` : filename;
        }

        return "Unknown file";
    }, []);

    // ── Render ─────────────────────────────────────────────────────

    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) => (
                <Form.Item
                    label={label}
                    {...(error ? { validateStatus: "error" as const, help: error.message } : {})}
                    required={required}
                >
                    {code !== undefined && (
                        <Tag color="blue" style={{ marginBottom: 8 }}>Code: {code}</Tag>
                    )}

                    {/* File info strip */}
                    {field.value && (
                        <div
                            style={{
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "space-between",
                                padding: 12,
                                marginBottom: 8,
                                borderRadius: 8,
                                border: `1px solid ${token.colorBorder}`,
                                backgroundColor: token.colorBgContainer,
                            }}
                        >
                            <Space>
                                {getFileIcon(field.value)}
                                <Text ellipsis style={{ maxWidth: 200 }}>
                                    {getFileName(field.value)}
                                </Text>
                            </Space>

                            {showClearButton && (
                                <Popconfirm
                                    title="Remove File"
                                    description="Are you sure you want to remove this file?"
                                    onConfirm={() =>
                                    {
                                        field.onChange(null);
                                        onFileClear?.();
                                    }}
                                    okText="Yes"
                                    cancelText="No"
                                    disabled={disabled}
                                >
                                    <Button
                                        danger
                                        size="small"
                                        icon={<DeleteOutlined />}
                                        disabled={disabled}
                                    >
                                        Clear
                                    </Button>
                                </Popconfirm>
                            )}
                        </div>
                    )}

                    {/* Drag-and-drop upload area */}
                    <Dragger
                        accept={accept}
                        showUploadList={false}
                        beforeUpload={beforeUpload}
                        customRequest={({ file }) =>
                        {
                            field.onChange(file as File);
                            onFileUpload?.(file as File);
                        }}
                        disabled={disabled}
                        style={{
                            backgroundColor: error ? token.colorErrorBg : token.colorBgContainer,
                            borderColor: error ? token.colorError : token.colorBorder,
                        }}
                    >
                        <p className="ant-upload-drag-icon">
                            <InboxOutlined style={{ color: token.colorPrimary }} />
                        </p>
                        <p className="ant-upload-text">
                            {field.value ? "Click or drag file to replace" : "Click or drag file to upload"}
                        </p>
                        <p className="ant-upload-hint">
                            Max size: {maxSizeMb} MB | Supported: {friendlyFormats}
                        </p>
                    </Dragger>
                </Form.Item>
            )}
        />
    );
}
