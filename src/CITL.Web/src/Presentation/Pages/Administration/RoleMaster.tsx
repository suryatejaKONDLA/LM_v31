import { Form, Card, Row, Col, Tag, Typography, theme } from "antd";
import { SafetyCertificateOutlined } from "@ant-design/icons";
import { TextBox, FormActionButtons, RecordDetailsBanner } from "@/Presentation/Controls/Index";
import { RoleMasterService } from "@/Infrastructure/Index";
import { useMasterForm } from "@/Application/Index";
import { type RoleMasterRequest, type RoleResponse } from "@/Domain/Index";
import { V } from "@/Shared/Index";

const { Title } = Typography;

interface RoleFormValues
{
    ROLE_Name: string;
}

const roleDefaults: RoleFormValues = {
    ROLE_Name: "",
};

export default function RoleMaster(): React.JSX.Element
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
    } = useMasterForm<RoleFormValues, RoleResponse>({
        defaultValues: roleDefaults,
        pageTitle: "Role Master",
        fetchById: (id, signal) => RoleMasterService.getById(id, signal),
        save: (request) => RoleMasterService.addOrUpdate(request as RoleMasterRequest),
        mapResponseToForm: (data) => ({ ROLE_Name: data.ROLE_Name }),
        buildRequest: (values, qs1, branchCode) => ({
            ROLE_ID: Number(qs1),
            ROLE_Name: values.ROLE_Name,
            BRANCH_Code: branchCode,
        } satisfies RoleMasterRequest),
        buildBannerFields: (data) => [
            `${data.ROLE_Name} (${String(data.ROLE_ID)})`,
            data.ROLE_Created_Name
                ? `${data.ROLE_Created_Name} (${data.ROLE_Created_Date})`
                : null,
            data.ROLE_Modified_Name
                ? `${data.ROLE_Modified_Name} (${data.ROLE_Modified_Date ?? ""})`
                : null,
        ],
    });

    const { control } = form;

    return (
        <Card
            title={
                <span>
                    <SafetyCertificateOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
                    <Title level={5} style={{ margin: 0, display: "inline" }}>Role Master</Title>
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
                            name="ROLE_Name"
                            label="Role Name"
                            placeholder="Role Name *"
                            required
                            maxLength={40}
                            validation={{
                                required: V.Required,
                                minLength: { value: 3, message: V.StringLength(3, 40) },
                                maxLength: { value: 40, message: V.MaxLength(40) },
                            }}
                        />
                    </Col>
                </Row>

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
