import { useState, useEffect } from "react";
import { Form, Card, Row, Col, Tag, Typography, Divider, theme } from "antd";
import { UserOutlined } from "@ant-design/icons";
import { TextBox, CheckBox, DatePickerBox, DropDown, FormActionButtons, RecordDetailsBanner, type DropDownItem } from "@/Presentation/Controls/Index";
import { LoginMasterService, GenderMasterService } from "@/Infrastructure/Index";
import { useMasterForm } from "@/Application/Index";
import { type LoginMasterFormValues, type LoginMasterResponse } from "@/Domain/Index";
import { ApiResponseCode, V } from "@/Shared/Index";

const { Title } = Typography;

const today = new Date().toISOString().slice(0, 10);

const loginDefaults: LoginMasterFormValues = {
    Login_User: "",
    Login_Name: "",
    Login_Designation: "",
    Login_Mobile_No: "",
    Login_Email_ID: "",
    Login_DOB: null,
    Login_Gender: "",
    Login_Active_Flag: true,
};

export default function LoginMaster(): React.JSX.Element
{
    const { token } = theme.useToken();

    const [ genderItems, setGenderItems ] = useState<DropDownItem<string>[]>([]);

    useEffect(() =>
    {
        void GenderMasterService.getDropDown().then((result) =>
        {
            if (result.Code === ApiResponseCode.Success)
            {
                setGenderItems(result.Data);
            }
        });
    }, []);

    const {
        form,
        submitting,
        isDirty,
        isEditMode,
        qString1,
        bannerFields,
        handleFormSubmit,
        onReset,
        resetConfirmProps,
    } = useMasterForm<LoginMasterFormValues, LoginMasterResponse>({
        defaultValues: loginDefaults,
        pageTitle: "Login Master",
        fetchById: (id, signal) => LoginMasterService.getById(id, signal),
        save: (request) => LoginMasterService.addOrUpdate(request),
        mapResponseToForm: (data) => ({
            Login_User: data.Login_User,
            Login_Name: data.Login_Name,
            Login_Designation: data.Login_Designation,
            Login_Mobile_No: data.Login_Mobile_No,
            Login_Email_ID: data.Login_Email_ID,
            Login_DOB: data.Login_DOB,
            Login_Gender: data.Login_Gender,
            Login_Active_Flag: data.Login_Active_Flag,
        }),
        buildRequest: (values, qs1, branchCode) => ({
            Login_ID: Number(qs1),
            Login_User: values.Login_User,
            Login_Name: values.Login_Name,
            Login_Designation: values.Login_Designation,
            Login_Mobile_No: values.Login_Mobile_No,
            Login_Email_ID: values.Login_Email_ID,
            Login_DOB: values.Login_DOB ?? null,
            Login_Gender: values.Login_Gender,
            Login_Active_Flag: values.Login_Active_Flag,
            BRANCH_Code: branchCode,
        }),
        buildBannerFields: (data) => [
            `${data.Login_Name} — @${data.Login_User}`,
            data.Login_Created_Name
                ? `${data.Login_Created_Name} (${data.Login_Created_Date ?? ""})`
                : null,
            data.Login_Modified_Name
                ? `${data.Login_Modified_Name} (${data.Login_Modified_Date ?? ""})`
                : null,
        ],
    });

    const { control } = form;

    return (
        <Card
            title={
                <span>
                    <UserOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
                    <Title level={5} style={{ margin: 0, display: "inline" }}>Login Master</Title>
                    {isDirty && <Tag color="orange" style={{ marginLeft: 8 }}>Unsaved Changes</Tag>}
                </span>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <RecordDetailsBanner fields={bannerFields} queryString1={qString1} />

            <Form layout="vertical" onFinish={handleFormSubmit}>

                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Login_User"
                            label="User ID"
                            placeholder="User ID *"
                            required
                            disabled={isEditMode}
                            maxLength={100}
                            validation={{
                                required: V.Required,
                                minLength: { value: 4, message: V.StringLength(4, 100) },
                                maxLength: { value: 100, message: V.StringLength(4, 100) },
                                pattern: { value: /^\S+$/, message: "No spaces allowed" },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Login_Name"
                            label="Full Name"
                            placeholder="Full Name *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                minLength: { value: 4, message: V.StringLength(4, 40) },
                                maxLength: { value: 40, message: V.StringLength(4, 40) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Login_Designation"
                            label="Designation"
                            placeholder="Designation *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 40, message: V.MaxLength(40) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Login_Mobile_No"
                            label="Mobile Number"
                            placeholder="Mobile Number *"
                            required
                            maxLength={10}
                            validation={{
                                required: V.Required,
                                pattern: { value: /^\d{10}$/, message: "Enter a valid 10-digit mobile number" },
                            }}
                        />
                    </Col>
                </Row>

                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={8}>
                        <TextBox
                            control={control}
                            name="Login_Email_ID"
                            label="Email Address"
                            placeholder="Email Address *"
                            required
                            maxLength={100}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 100, message: V.MaxLength(100) },
                                pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: V.Email },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <DatePickerBox
                            control={control}
                            name="Login_DOB"
                            label="Date of Birth"
                            placeholder="Select Date of Birth"
                            allowClear
                            maxDate={today}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <DropDown
                            control={control}
                            name="Login_Gender"
                            label="Gender"
                            placeholder="Select Gender *"
                            required
                            dataSource={genderItems}
                            validation={{ required: V.Required }}
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="Login_Active_Flag"
                            formLabel=" "
                            label="Active"
                        />
                    </Col>
                </Row>

                <Divider style={{ margin: "12px 0" }} />

                <FormActionButtons
                    submitting={submitting}
                    submitText={isEditMode ? "Update" : "Save"}
                    onReset={onReset}
                    resetConfirm={resetConfirmProps}
                    disableSubmit={!isDirty}
                />
            </Form>
        </Card>
    );
}
