import { useState, useEffect, useCallback, useMemo } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { Form, Card, Row, Col, Space, Typography, Tag, Divider, theme } from "antd";
import { UserOutlined } from "@ant-design/icons";
import { TextBox, DropDown, DatePickerBox, ImageUpload, FormActionButtons, type DropDownItem } from "@/Presentation/Controls/Index";
import { GlobalSpinner, ModalDialog } from "@/Shared/UI/Index";
import { AccountService } from "@/Infrastructure/Index";
import { useAuthStore, useMenuStore } from "@/Application/Index";
import { type ProfileResponse, type UpdateProfileRequest, type Menu } from "@/Domain/Index";
import { ApiResponseCode, V } from "@/Shared/Index";
import { isCancelledRequest } from "@/Shared/Helpers/Index";

const { Title, Text } = Typography;

interface ProfileFormValues
{
    Login_Name: string;
    Login_Mobile_No: string;
    Login_Email_ID: string;
    Login_DOB: string | null;
    Login_Pic: File | string | null;
    Menu_ID: string;
}

const defaultValues: ProfileFormValues = {
    Login_Name: "",
    Login_Mobile_No: "",
    Login_Email_ID: "",
    Login_DOB: null,
    Login_Pic: null,
    Menu_ID: "",
};

/**
 * Flatten a recursive menu tree into a flat list with indented labels.
 * Only leaf menus (pages, not groups) are included — those with a URL.
 */
function flattenMenus(menus: Menu[], depth = 0): DropDownItem<string>[]
{
    const items: DropDownItem<string>[] = [];

    for (const menu of menus)
    {
        if (menu.MENU_URL1)
        {
            items.push({
                Col1: menu.MENU_ID,
                Col2: menu.MENU_Description ?? "",
            });
        }

        if (menu.Children.length > 0)
        {
            items.push(...flattenMenus(menu.Children, depth + 1));
        }
    }

    return items;
}

const readOnlyStyle: React.CSSProperties = {
    padding: "4px 11px",
    borderRadius: 6,
    fontSize: 13,
    minHeight: 32,
    display: "flex",
    alignItems: "center",
};

export default function Profile(): React.JSX.Element
{
    const { token } = theme.useToken();
    const { user, setUser } = useAuthStore();
    const { menus } = useMenuStore();

    const [ profileData, setProfileData ] = useState<ProfileResponse | null>(null);
    const [ submitting, setSubmitting ] = useState(false);

    const { control, handleSubmit, reset, formState: { isDirty } } = useForm<ProfileFormValues>({
        defaultValues,
    });

    const menuOptions = useMemo(() => flattenMenus(menus), [ menus ]);

    const fetchProfile = useCallback(async (signal?: AbortSignal) =>
    {
        GlobalSpinner.show("Loading profile…");
        try
        {
            const result = await AccountService.getProfile(signal);
            if (result.Code === ApiResponseCode.Success)
            {
                const data = result.Data;
                setProfileData(data);
                reset({
                    Login_Name: data.Login_Name,
                    Login_Mobile_No: data.Login_Mobile_No,
                    Login_Email_ID: data.Login_Email_ID,
                    Login_DOB: data.Login_DOB,
                    Login_Pic: data.Login_Pic,
                    Menu_ID: data.Menu_ID ?? "",
                });
            }
        }
        catch (err: unknown)
        {
            if (isCancelledRequest(err))
            {
                return;
            }

            ModalDialog.error({
                title: "Error",
                content: "Failed to load profile. Please try again.",
            });
        }
        finally
        {
            GlobalSpinner.hide();
        }
    }, [ reset ]);

    useEffect(() =>
    {
        const controller = new AbortController();
        void fetchProfile(controller.signal);
        return () =>
        {
            controller.abort();
        };
    }, [ fetchProfile ]);

    const handleFormFinish = useCallback(() =>
    {
        const onSubmit: SubmitHandler<ProfileFormValues> = async (formValues) =>
        {
            setSubmitting(true);
            try
            {
                const request: UpdateProfileRequest = {
                    Login_Name: formValues.Login_Name,
                    Login_Mobile_No: formValues.Login_Mobile_No,
                    Login_Email_ID: formValues.Login_Email_ID,
                    Login_DOB: formValues.Login_DOB,
                    Login_Pic: formValues.Login_Pic as string | null,
                    Menu_ID: formValues.Menu_ID,
                };

                const result = await AccountService.updateProfile(request);

                if (result.Code === ApiResponseCode.Success)
                {
                    ModalDialog.successResult(result.Message);
                    await fetchProfile();

                    if (user && formValues.Login_Name !== user.loginName)
                    {
                        setUser({ ...user, loginName: formValues.Login_Name });
                    }
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
    }, [ handleSubmit, fetchProfile, user, setUser ]);

    const handleReset = useCallback(() =>
    {
        if (profileData)
        {
            reset({
                Login_Name: profileData.Login_Name,
                Login_Mobile_No: profileData.Login_Mobile_No,
                Login_Email_ID: profileData.Login_Email_ID,
                Login_DOB: profileData.Login_DOB,
                Login_Pic: profileData.Login_Pic,
                Menu_ID: profileData.Menu_ID ?? "",
            });
        }
    }, [ profileData, reset ]);

    const tagStyle: React.CSSProperties = {
        ...readOnlyStyle,
        background: token.colorBgLayout,
        border: `1px solid ${token.colorBorderSecondary}`,
    };

    return (
        <Card
            title={
                <Space>
                    <UserOutlined style={{ color: token.colorPrimary }} />
                    <Title level={5} style={{ margin: 0 }}>My Profile</Title>
                    {isDirty && <Tag color="warning">Unsaved Changes</Tag>}
                </Space>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <Form layout="vertical" onFinish={handleFormFinish}>
                <Row gutter={[ 24, 0 ]}>

                    {/* Profile Picture */}
                    <Col xs={24} sm={24} md={8} lg={6} style={{ display: "flex", justifyContent: "center", marginBottom: 24 }}>
                        <ImageUpload
                            control={control}
                            name="Login_Pic"
                            label="Profile Picture"
                            shape="circle"
                            size={140}
                            maxSizeMb={2}
                        />
                    </Col>

                    {/* Editable Fields */}
                    <Col xs={24} sm={24} md={16} lg={18}>
                        <Row gutter={[ 16, 0 ]}>
                            <Col xs={24} sm={12}>
                                <TextBox
                                    control={control}
                                    name="Login_Name"
                                    label="Full Name"
                                    placeholder="Enter Full Name *"
                                    required
                                    maxLength={100}
                                    validation={{
                                        required: V.Required,
                                    }}
                                />
                            </Col>

                            <Col xs={24} sm={12}>
                                <TextBox
                                    control={control}
                                    name="Login_Mobile_No"
                                    label="Mobile Number"
                                    placeholder="Enter Mobile Number *"
                                    required
                                    maxLength={10}
                                    validation={{
                                        required: V.Required,
                                        pattern: {
                                            value: V.PhoneNumberPattern,
                                            message: V.Phone,
                                        },
                                    }}
                                />
                            </Col>

                            <Col xs={24} sm={12}>
                                <TextBox
                                    control={control}
                                    name="Login_Email_ID"
                                    label="Email Address"
                                    placeholder="Enter Email Address *"
                                    required
                                    maxLength={150}
                                    validation={{
                                        required: V.Required,
                                        pattern: {
                                            value: V.EmailPattern,
                                            message: V.Email,
                                        },
                                    }}
                                />
                            </Col>

                            <Col xs={24} sm={12}>
                                <DatePickerBox
                                    control={control}
                                    name="Login_DOB"
                                    label="Date of Birth"
                                    placeholder="Select Date of Birth"
                                />
                            </Col>

                            <Col xs={24} sm={12}>
                                <DropDown
                                    control={control}
                                    name="Menu_ID"
                                    label="Startup Page"
                                    placeholder="Select Startup Page"
                                    dataSource={menuOptions}
                                />
                            </Col>
                        </Row>
                    </Col>
                </Row>

                <Divider />

                {/* Read-Only Account Info */}
                {profileData && (
                    <Row gutter={[ 16, 12 ]} style={{ marginBottom: 24 }}>
                        <Col xs={24}>
                            <Text strong style={{ fontSize: 14 }}>Account Information</Text>
                        </Col>

                        <Col xs={24} sm={12} md={8}>
                            <Form.Item label="User ID" style={{ marginBottom: 8 }}>
                                <div style={tagStyle}>{profileData.Login_User}</div>
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={12} md={8}>
                            <Form.Item label="Designation" style={{ marginBottom: 8 }}>
                                <div style={tagStyle}>{profileData.Login_Designation || "—"}</div>
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={12} md={8}>
                            <Form.Item label="Gender" style={{ marginBottom: 8 }}>
                                <div style={tagStyle}>{profileData.Login_Gender || "—"}</div>
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={12} md={8}>
                            <Form.Item label="Email Verified" style={{ marginBottom: 8 }}>
                                <Tag color={profileData.Login_Email_Verified ? "success" : "error"}>
                                    {profileData.Login_Email_Verified ? "Yes" : "No"}
                                </Tag>
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={12} md={8}>
                            <Form.Item label="Account Status" style={{ marginBottom: 8 }}>
                                <Tag color="success">Active</Tag>
                            </Form.Item>
                        </Col>
                    </Row>
                )}

                <Divider />

                <FormActionButtons
                    submitting={submitting}
                    submitText="Update Profile"
                    onReset={handleReset}
                    disableSubmit={!isDirty}
                />
            </Form>
        </Card>
    );
}
