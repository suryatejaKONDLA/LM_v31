import { Form, Card, Row, Col, Tag, Typography, Divider, theme } from "antd";
import { MailOutlined } from "@ant-design/icons";
import { TextBox, CheckBox, NumberBox, PasswordBox, FormActionButtons, RecordDetailsBanner } from "@/Presentation/Controls/Index";
import { MailMasterService } from "@/Infrastructure/Index";
import { useMasterForm } from "@/Application/Index";
import { type MailFormValues, type MailMasterResponse } from "@/Domain/Index";
import { V } from "@/Shared/Index";

const { Title } = Typography;

const mailDefaults: MailFormValues = {
    Mail_From_Address: "",
    Mail_From_Password: "",
    Mail_Display_Name: "",
    Mail_Host: "",
    Mail_Port: 587,
    Mail_SSL_Enabled: true,
    Mail_Max_Recipients: 50,
    Mail_Retry_Attempts: 3,
    Mail_Retry_Interval_Minutes: 5,
    Mail_Is_Active: true,
    Mail_Is_Default: false,
};

export default function MailMaster(): React.JSX.Element
{
    const { token } = theme.useToken();

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
    } = useMasterForm<MailFormValues, MailMasterResponse>({
        defaultValues: mailDefaults,
        pageTitle: "Mail Master",
        fetchById: (id, signal) => MailMasterService.getById(id, signal),
        save: (request) => MailMasterService.addOrUpdate(request as Parameters<typeof MailMasterService.addOrUpdate>[0]),
        mapResponseToForm: (data) => ({
            Mail_From_Address: data.Mail_From_Address,
            Mail_From_Password: "",
            Mail_Display_Name: data.Mail_Display_Name,
            Mail_Host: data.Mail_Host,
            Mail_Port: data.Mail_Port,
            Mail_SSL_Enabled: data.Mail_SSL_Enabled,
            Mail_Max_Recipients: data.Mail_Max_Recipients,
            Mail_Retry_Attempts: data.Mail_Retry_Attempts,
            Mail_Retry_Interval_Minutes: data.Mail_Retry_Interval_Minutes,
            Mail_Is_Active: data.Mail_Is_Active,
            Mail_Is_Default: data.Mail_Is_Default,
        }),
        buildRequest: (values, qs1) => ({
            Mail_SNo: Number(qs1),
            Mail_Branch_Code: 1,
            Mail_From_Address: values.Mail_From_Address,
            Mail_From_Password: values.Mail_From_Password,
            Mail_Display_Name: values.Mail_Display_Name,
            Mail_Host: values.Mail_Host,
            Mail_Port: values.Mail_Port,
            Mail_SSL_Enabled: values.Mail_SSL_Enabled,
            Mail_Max_Recipients: values.Mail_Max_Recipients,
            Mail_Retry_Attempts: values.Mail_Retry_Attempts,
            Mail_Retry_Interval_Minutes: values.Mail_Retry_Interval_Minutes,
            Mail_Is_Active: values.Mail_Is_Active,
            Mail_Is_Default: values.Mail_Is_Default,
        }),
        buildBannerFields: (data) => [
            `${data.Mail_From_Address} — ${data.Mail_Display_Name} (SNo: ${String(data.Mail_SNo)})`,
            data.Mail_Created_Name
                ? `${data.Mail_Created_Name} (${data.Mail_Created_Date ?? ""})`
                : null,
            data.Mail_Modified_Name
                ? `${data.Mail_Modified_Name} (${data.Mail_Modified_Date ?? ""})`
                : null,
        ],
    });

    const { control } = form;

    return (
        <Card
            title={
                <span>
                    <MailOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
                    <Title level={5} style={{ margin: 0, display: "inline" }}>Mail Master</Title>
                    {isDirty && <Tag color="orange" style={{ marginLeft: 8 }}>Unsaved Changes</Tag>}
                </span>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <RecordDetailsBanner fields={bannerFields} queryString1={qString1} />

            <Form layout="vertical" onFinish={handleFormSubmit}>

                {/* Email & Auth */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Mail_From_Address"
                            label="From Address"
                            placeholder="Email Address *"
                            required
                            maxLength={100}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 100, message: V.MaxLength(100) },
                                pattern: {
                                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                                    message: V.Email,
                                },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <PasswordBox
                            control={control}
                            name="Mail_From_Password"
                            label="Password"
                            placeholder={isEditMode ? "Leave blank to keep current" : "Password *"}
                            required={!isEditMode}
                            maxLength={256}
                            validation={isEditMode ? {} : {
                                required: V.Required,
                                maxLength: { value: 256, message: V.MaxLength(256) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Mail_Display_Name"
                            label="Display Name"
                            placeholder="Display Name *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 40, message: V.MaxLength(40) },
                            }}
                        />
                    </Col>
                </Row>

                {/* Server */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="Mail_Host"
                            label="SMTP Host"
                            placeholder="SMTP Host *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 40, message: V.MaxLength(40) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="Mail_Port"
                            label="Port"
                            placeholder="Port *"
                            required
                            validation={{
                                required: V.Required,
                                min: { value: 1, message: "Port must be at least 1" },
                                max: { value: 65535, message: "Port must not exceed 65535" },
                            }}
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="Mail_SSL_Enabled"
                            formLabel=" "
                            label="SSL Enabled"
                        />
                    </Col>
                </Row>

                {/* Limits & Retry */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="Mail_Max_Recipients"
                            label="Max Recipients"
                            placeholder="Max Recipients *"
                            required
                            validation={{
                                required: V.Required,
                                min: { value: 1, message: "Must be at least 1" },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="Mail_Retry_Attempts"
                            label="Retry Attempts"
                            placeholder="Retry Attempts *"
                            required
                            validation={{
                                required: V.Required,
                                min: { value: 0, message: "Must be 0 or greater" },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="Mail_Retry_Interval_Minutes"
                            label="Retry Interval (min)"
                            placeholder="Minutes *"
                            required
                            validation={{
                                required: V.Required,
                                min: { value: 0, message: "Must be 0 or greater" },
                            }}
                        />
                    </Col>
                </Row>

                {/* Flags */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="Mail_Is_Active"
                            formLabel=" "
                            label="Active"
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="Mail_Is_Default"
                            formLabel=" "
                            label="Default"
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
