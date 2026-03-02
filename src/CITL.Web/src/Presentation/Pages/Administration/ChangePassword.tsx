import { useState, useCallback } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { Form, Card, Row, Col, Space, Typography, Tag, Button, Progress, theme } from "antd";
import { LockOutlined, KeyOutlined, SyncOutlined, CopyOutlined } from "@ant-design/icons";
import { PasswordBox, FormActionButtons, ModalDialog } from "@/Presentation/Controls/Index";
import { AccountService } from "@/Infrastructure/Index";
import { type ChangePasswordRequest } from "@/Domain/Index";
import { ApiResponseCode, V, PasswordMinLength, PasswordPattern, PasswordPatternMessage, generateStrongPassword, calculatePasswordStrength } from "@/Shared/Index";

const { Title, Text } = Typography;

type ChangePasswordFormValues = ChangePasswordRequest & { Login_Password1: string };

const defaultValues: ChangePasswordFormValues = {
    Login_Password_Old: "",
    Login_Password: "",
    Login_Password1: "",
};

const strengthBarWrapperStyle: React.CSSProperties = {
    marginTop: -12,
    marginBottom: 16,
};

const strengthBarRowStyle: React.CSSProperties = {
    display: "flex",
    alignItems: "center",
    gap: 8,
    marginBottom: 4,
};

const strengthProgressStyle: React.CSSProperties = {
    flex: 1,
    margin: 0,
};

export default function ChangePassword(): React.JSX.Element
{
    const { token } = theme.useToken();
    const navigate = useNavigate();

    const [ submitting, setSubmitting ] = useState(false);
    const [ generatedPassword, setGeneratedPassword ] = useState("");

    const { control, handleSubmit, reset, watch, formState: { isDirty } } = useForm<ChangePasswordFormValues>({
        defaultValues,
    });

    const newPassword = watch("Login_Password");

    const handleFormFinish = useCallback(() =>
    {
        const onSubmit: SubmitHandler<ChangePasswordFormValues> = async (formValues) =>
        {
            setSubmitting(true);
            try
            {
                const request: ChangePasswordRequest = {
                    Login_Password_Old: formValues.Login_Password_Old,
                    Login_Password: formValues.Login_Password,
                };

                const result = await AccountService.changePassword(request);

                if (result.Code === ApiResponseCode.Success)
                {
                    ModalDialog.successResult(result.Message);
                    reset(defaultValues);
                    setGeneratedPassword("");
                    void navigate("/Home");
                }
                else
                {
                    ModalDialog.showResult(result.Type, result.Message);
                }
            }
            finally
            {
                setSubmitting(false);
            }
        };

        void handleSubmit(onSubmit)();
    }, [ handleSubmit, reset, navigate ]);

    const handleReset = useCallback(() =>
    {
        reset(defaultValues);
        setGeneratedPassword("");
    }, [ reset ]);

    const handleGenerate = useCallback(() =>
    {
        setGeneratedPassword(generateStrongPassword());
    }, []);

    const handleCopyPassword = useCallback(() =>
    {
        void navigator.clipboard.writeText(generatedPassword);
    }, [ generatedPassword ]);

    const strength = newPassword ? calculatePasswordStrength(newPassword) : null;

    const generatorBoxStyle: React.CSSProperties = {
        border: `1px dashed ${token.colorBorder}`,
        borderRadius: token.borderRadius,
        padding: 12,
        marginBottom: 16,
        background: token.colorBgLayout,
    };

    const generatedDisplayStyle: React.CSSProperties = {
        display: "flex",
        alignItems: "center",
        gap: 8,
        background: token.colorBgContainer,
        border: `1px solid ${token.colorBorder}`,
        borderRadius: token.borderRadius,
        padding: "8px 12px",
    };

    return (
        <Card
            title={
                <Space>
                    <LockOutlined style={{ color: token.colorPrimary }} />
                    <Title level={5} style={{ margin: 0 }}>Change Password</Title>
                    {isDirty && <Tag color="warning">Unsaved Changes</Tag>}
                </Space>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <Form layout="vertical" onFinish={handleFormFinish}>
                <Row gutter={[ 16, 0 ]} justify="center">
                    <Col xs={24} sm={24} md={16} lg={12} xl={10} xxl={8}>
                        <Row gutter={[ 16, 0 ]}>

                            {/* Current Password */}
                            <Col xs={24}>
                                <PasswordBox
                                    control={control}
                                    name="Login_Password_Old"
                                    label="Current Password"
                                    placeholder="Enter Current Password *"
                                    required
                                    autoComplete="current-password"
                                    validation={{
                                        required: V.Required,
                                    }}
                                />
                            </Col>

                            {/* New Password */}
                            <Col xs={24}>
                                <PasswordBox
                                    control={control}
                                    name="Login_Password"
                                    label="New Password"
                                    placeholder="Enter New Password *"
                                    required
                                    autoComplete="new-password"
                                    validation={{
                                        required: V.Required,
                                        minLength: { value: PasswordMinLength, message: V.MinLength(PasswordMinLength) },
                                        pattern: {
                                            value: PasswordPattern,
                                            message: PasswordPatternMessage,
                                        },
                                    }}
                                />

                                {/* Password Strength Bar */}
                                {strength && (
                                    <div style={strengthBarWrapperStyle}>
                                        <div style={strengthBarRowStyle}>
                                            <Progress
                                                percent={strength.score}
                                                size="small"
                                                showInfo={false}
                                                strokeColor={strength.color}
                                                style={strengthProgressStyle}
                                            />
                                            <Text
                                                style={{
                                                    fontSize: 12,
                                                    fontWeight: 500,
                                                    color: strength.color,
                                                    minWidth: 50,
                                                }}
                                            >
                                                {strength.label}
                                            </Text>
                                        </div>
                                    </div>
                                )}
                            </Col>

                            {/* Confirm Password */}
                            <Col xs={24}>
                                <PasswordBox
                                    control={control}
                                    name="Login_Password1"
                                    label="Confirm Password"
                                    placeholder="Confirm New Password *"
                                    required
                                    autoComplete="new-password"
                                    validation={{
                                        required: V.Required,
                                        validate: (value: string) =>
                                            value === newPassword || V.PasswordMatch,
                                    }}
                                />
                            </Col>

                            {/* Password Generator */}
                            <Col xs={24}>
                                <div style={generatorBoxStyle}>
                                    <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 8 }}>
                                        <Text type="secondary" style={{ fontSize: 12 }}>
                                            <KeyOutlined style={{ marginRight: 4 }} />
                                            Generate Strong Password
                                        </Text>
                                        <Button
                                            type="link"
                                            size="small"
                                            icon={<SyncOutlined />}
                                            onClick={handleGenerate}
                                            style={{ padding: 0, height: "auto" }}
                                        >
                                            Generate
                                        </Button>
                                    </div>
                                    {generatedPassword && (
                                        <div style={generatedDisplayStyle}>
                                            <Text
                                                code
                                                style={{ flex: 1, fontFamily: "monospace" }}
                                            >
                                                {generatedPassword}
                                            </Text>
                                            <Button
                                                type="text"
                                                size="small"
                                                icon={<CopyOutlined />}
                                                onClick={handleCopyPassword}
                                            />
                                        </div>
                                    )}
                                </div>
                            </Col>
                        </Row>

                        <FormActionButtons
                            submitting={submitting}
                            submitText="Change Password"
                            onReset={handleReset}
                            disableSubmit={!isDirty}
                        />
                    </Col>
                </Row>
            </Form>
        </Card>
    );
}
