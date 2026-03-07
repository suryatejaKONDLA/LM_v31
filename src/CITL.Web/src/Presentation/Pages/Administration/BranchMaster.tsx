import { Form, Card, Row, Col, Tag, Typography, Divider, theme } from "antd";
import { BankOutlined } from "@ant-design/icons";
import { TextBox, CheckBox, NumberBox, ImageUpload, FormActionButtons, RecordDetailsBanner } from "@/Presentation/Controls/Index";
import { BranchMasterService } from "@/Infrastructure/Index";
import { useMasterForm } from "@/Application/Index";
import { type BranchFormValues, type BranchResponse } from "@/Domain/Index";
import { V } from "@/Shared/Index";

const { Title } = Typography;

const branchDefaults: BranchFormValues = {
    BRANCH_Name: "",
    BRANCH_State: 0,
    BRANCH_Name2: "",
    BRANCH_Address1: "",
    BRANCH_Address2: "",
    BRANCH_Address3: "",
    BRANCH_City: "",
    BRANCH_PIN: "",
    BRANCH_Contact_Person: "",
    BRANCH_Phone_No1: "",
    BRANCH_Phone_No2: "",
    BRANCH_Email_ID: "",
    BRANCH_GSTIN: "",
    BRANCH_PAN_No: "",
    BRANCH_AutoApproval_Enabled: false,
    BRANCH_Discounts_Enabled: false,
    BRANCH_CreditLimits_Enabled: false,
    BRANCH_Currency_Code: "",
    BRANCH_TimeZone_Code: 0,
    BRANCH_Order: 0,
    BRANCH_Active_Flag: true,
    BRANCH_Logo: null,
};

export default function BranchMaster(): React.JSX.Element
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
    } = useMasterForm<BranchFormValues, BranchResponse>({
        defaultValues: branchDefaults,
        pageTitle: "Branch Master",
        fetchById: (id, signal) => BranchMasterService.getById(id, signal),
        save: (request) => BranchMasterService.addOrUpdate(request),
        mapResponseToForm: (data) => ({
            BRANCH_Name: data.BRANCH_Name,
            BRANCH_State: data.BRANCH_State,
            BRANCH_Name2: data.BRANCH_Name2,
            BRANCH_Address1: data.BRANCH_Address1 ?? "",
            BRANCH_Address2: data.BRANCH_Address2 ?? "",
            BRANCH_Address3: data.BRANCH_Address3 ?? "",
            BRANCH_City: data.BRANCH_City,
            BRANCH_PIN: data.BRANCH_PIN,
            BRANCH_Contact_Person: data.BRANCH_Contact_Person,
            BRANCH_Phone_No1: data.BRANCH_Phone_No1,
            BRANCH_Phone_No2: data.BRANCH_Phone_No2 ?? "",
            BRANCH_Email_ID: data.BRANCH_Email_ID,
            BRANCH_GSTIN: data.BRANCH_GSTIN ?? "",
            BRANCH_PAN_No: data.BRANCH_PAN_No ?? "",
            BRANCH_AutoApproval_Enabled: data.BRANCH_AutoApproval_Enabled,
            BRANCH_Discounts_Enabled: data.BRANCH_Discounts_Enabled,
            BRANCH_CreditLimits_Enabled: data.BRANCH_CreditLimits_Enabled,
            BRANCH_Currency_Code: data.BRANCH_Currency_Code,
            BRANCH_TimeZone_Code: data.BRANCH_TimeZone_Code,
            BRANCH_Order: data.BRANCH_Order,
            BRANCH_Active_Flag: data.BRANCH_Active_Flag,
            BRANCH_Logo: data.BRANCH_Logo,
        }),
        buildRequest: (values, qs1) => ({
            BRANCH_Code: Number(qs1),
            BRANCH_Name: values.BRANCH_Name,
            BRANCH_State: values.BRANCH_State,
            BRANCH_Name2: values.BRANCH_Name2,
            BRANCH_Address1: values.BRANCH_Address1 || null,
            BRANCH_Address2: values.BRANCH_Address2 || null,
            BRANCH_Address3: values.BRANCH_Address3 || null,
            BRANCH_City: values.BRANCH_City,
            BRANCH_PIN: values.BRANCH_PIN,
            BRANCH_Contact_Person: values.BRANCH_Contact_Person,
            BRANCH_Phone_No1: values.BRANCH_Phone_No1,
            BRANCH_Phone_No2: values.BRANCH_Phone_No2 || null,
            BRANCH_Email_ID: values.BRANCH_Email_ID,
            BRANCH_GSTIN: values.BRANCH_GSTIN || null,
            BRANCH_PAN_No: values.BRANCH_PAN_No || null,
            BRANCH_AutoApproval_Enabled: values.BRANCH_AutoApproval_Enabled,
            BRANCH_Discounts_Enabled: values.BRANCH_Discounts_Enabled,
            BRANCH_CreditLimits_Enabled: values.BRANCH_CreditLimits_Enabled,
            BRANCH_Currency_Code: values.BRANCH_Currency_Code,
            BRANCH_TimeZone_Code: values.BRANCH_TimeZone_Code,
            BRANCH_Order: values.BRANCH_Order,
            BRANCH_Active_Flag: values.BRANCH_Active_Flag,
            BRANCH_Logo: values.BRANCH_Logo instanceof File ? null : values.BRANCH_Logo,
        }),
        buildBannerFields: (data) => [
            `${data.BRANCH_Name} — ${data.BRANCH_Name2} (Code: ${String(data.BRANCH_Code)})`,
            data.BRANCH_Created_Name
                ? `${data.BRANCH_Created_Name} (${data.BRANCH_Created_Date})`
                : null,
            data.BRANCH_Modified_Name
                ? `${data.BRANCH_Modified_Name} (${data.BRANCH_Modified_Date ?? ""})`
                : null,
        ],
    });

    const { control } = form;

    return (
        <Card
            title={
                <span>
                    <BankOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
                    <Title level={5} style={{ margin: 0, display: "inline" }}>Branch Master</Title>
                    {isDirty && <Tag color="orange" style={{ marginLeft: 8 }}>Unsaved Changes</Tag>}
                </span>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <RecordDetailsBanner fields={bannerFields} queryString1={qString1} />

            <Form layout="vertical" onFinish={handleFormSubmit}>

                {/* Logo */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24}>
                        <ImageUpload
                            control={control}
                            name="BRANCH_Logo"
                            shape="square"
                            size={120}
                            maxSizeMb={2}
                        />
                    </Col>
                </Row>

                {/* Names & Location */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Name"
                            label="Branch Name"
                            placeholder="Branch Name *"
                            required
                            maxLength={4}
                            validation={{
                                required: V.Required,
                                minLength: { value: 1, message: V.StringLength(1, 4) },
                                maxLength: { value: 4, message: V.StringLength(1, 4) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Name2"
                            label="Display Name"
                            placeholder="Display Name *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                minLength: { value: 1, message: V.StringLength(1, 40) },
                                maxLength: { value: 40, message: V.StringLength(1, 40) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="BRANCH_State"
                            label="State"
                            placeholder="State Code *"
                            required
                            validation={{ required: V.Required }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_City"
                            label="City"
                            placeholder="City *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 40, message: V.MaxLength(40) },
                            }}
                        />
                    </Col>
                </Row>

                {/* Address */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Address1"
                            label="Address Line 1"
                            placeholder="Address Line 1"
                            maxLength={40}
                            validation={{ maxLength: { value: 40, message: V.MaxLength(40) } }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Address2"
                            label="Address Line 2"
                            placeholder="Address Line 2"
                            maxLength={40}
                            validation={{ maxLength: { value: 40, message: V.MaxLength(40) } }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Address3"
                            label="Address Line 3"
                            placeholder="Address Line 3"
                            maxLength={40}
                            validation={{ maxLength: { value: 40, message: V.MaxLength(40) } }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_PIN"
                            label="PIN Code"
                            placeholder="PIN Code *"
                            required
                            maxLength={6}
                            validation={{
                                required: V.Required,
                                minLength: { value: 6, message: V.StringLength(6, 6) },
                                maxLength: { value: 6, message: V.StringLength(6, 6) },
                            }}
                        />
                    </Col>
                </Row>

                {/* Contact */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Contact_Person"
                            label="Contact Person"
                            placeholder="Contact Person *"
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
                            name="BRANCH_Phone_No1"
                            label="Phone No 1"
                            placeholder="Phone No 1 *"
                            required
                            maxLength={15}
                            validation={{ required: V.Required }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Phone_No2"
                            label="Phone No 2"
                            placeholder="Phone No 2"
                            maxLength={15}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Email_ID"
                            label="Email"
                            placeholder="Email *"
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
                </Row>

                {/* Tax & Financial */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_GSTIN"
                            label="GSTIN"
                            placeholder="GSTIN"
                            maxLength={15}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_PAN_No"
                            label="PAN No"
                            placeholder="PAN No"
                            maxLength={10}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <TextBox
                            control={control}
                            name="BRANCH_Currency_Code"
                            label="Currency Code"
                            placeholder="Currency Code *"
                            required
                            maxLength={3}
                            validation={{
                                required: V.Required,
                                minLength: { value: 3, message: V.StringLength(3, 3) },
                                maxLength: { value: 3, message: V.StringLength(3, 3) },
                            }}
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <NumberBox
                            control={control}
                            name="BRANCH_TimeZone_Code"
                            label="Time Zone Code"
                            placeholder="Time Zone Code *"
                            required
                            validation={{ required: V.Required }}
                        />
                    </Col>
                </Row>

                {/* Order & Flags */}
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={8} lg={4}>
                        <NumberBox
                            control={control}
                            name="BRANCH_Order"
                            label="Display Order"
                            placeholder="Order *"
                            required
                            validation={{ required: V.Required }}
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="BRANCH_AutoApproval_Enabled"
                            formLabel=" "
                            label="Auto Approval"
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="BRANCH_Discounts_Enabled"
                            formLabel=" "
                            label="Discounts"
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="BRANCH_CreditLimits_Enabled"
                            formLabel=" "
                            label="Credit Limits"
                        />
                    </Col>
                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="BRANCH_Active_Flag"
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
