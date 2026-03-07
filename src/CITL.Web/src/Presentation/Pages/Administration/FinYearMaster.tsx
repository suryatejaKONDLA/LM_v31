import { useEffect } from "react";
import { Form, Card, Row, Col, Tag, Typography, Divider, theme } from "antd";
import { CalendarOutlined } from "@ant-design/icons";
import { CheckBox, FormActionButtons, RecordDetailsBanner, NumberBox, TextBox } from "@/Presentation/Controls/Index";
import { FinYearMasterService } from "@/Infrastructure/Index";
import { useMasterForm } from "@/Application/Index";
import { type FinYearFormValues, type FinYearResponse } from "@/Domain/Index";
import { useWatch } from "react-hook-form";
import { V } from "@/Shared/Index";

const { Title } = Typography;

const Months = [ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" ] as const;

/** Formats yyyy-MM-dd → dd-MMM-yyyy (e.g. 01-Apr-2025). */
function formatDate(iso: string): string
{
    if (!iso)
    {
        return "—";
    }
    const parts = iso.split("-");
    const y = parts[0] ?? "";
    const m = Number(parts[1] ?? "0");
    const d = parts[2] ?? "";
    return `${d}-${Months[m - 1] ?? ""}-${y}`;
}

/**
 * Computes FIN_Year (e.g. 202526), FIN_Date1, FIN_Date2 from a 4-digit start year.
 */
function computeFinFields(year: number): { finYear: number; date1: string; date2: string }
{
    const nextYear = year + 1;
    const nextSuffix = nextYear % 100;
    const finYear = year * 100 + nextSuffix;      // 2025 → 202526
    const date1 = `${String(year)}-04-01`;         // 2025-04-01
    const date2 = `${String(nextYear)}-03-31`;     // 2026-03-31
    return { finYear, date1, date2 };
}

const finYearDefaults: FinYearFormValues = {
    inputYear: null,
    FIN_Year: 0,
    FIN_Date1: "",
    FIN_Date2: "",
    FIN_Active_Flag: true,
    display_FIN_Year: "—",
    display_FIN_Date1: "—",
    display_FIN_Date2: "—",
};

export default function FinYearMaster(): React.JSX.Element
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
    } = useMasterForm<FinYearFormValues, FinYearResponse>({
        defaultValues: finYearDefaults,
        pageTitle: "Financial Year Master",
        fetchById: (id, signal) => FinYearMasterService.getById(id, signal),
        save: (request) => FinYearMasterService.addOrUpdate(request as Parameters<typeof FinYearMasterService.addOrUpdate>[0]),
        mapResponseToForm: (data) =>
        {
            const startYear = Math.floor(data.FIN_Year / 100);
            return {
                inputYear: startYear,
                FIN_Year: data.FIN_Year,
                FIN_Date1: data.FIN_Date1,
                FIN_Date2: data.FIN_Date2,
                FIN_Active_Flag: data.FIN_Active_Flag,
                display_FIN_Year: String(data.FIN_Year),
                display_FIN_Date1: formatDate(data.FIN_Date1),
                display_FIN_Date2: formatDate(data.FIN_Date2),
            };
        },
        buildRequest: (values) => ({
            FIN_Year: values.FIN_Year,
            FIN_Date1: values.FIN_Date1,
            FIN_Date2: values.FIN_Date2,
            FIN_Active_Flag: values.FIN_Active_Flag,
        }),
        buildBannerFields: (data) => [
            `FY ${String(Math.floor(data.FIN_Year / 100))}-${String(data.FIN_Year % 100).padStart(2, "0")}`,
            `${formatDate(data.FIN_Date1)} to ${formatDate(data.FIN_Date2)}`,
            data.FIN_Active_Flag ? "Active" : "Inactive",
        ],
    });

    const { control, setValue } = form;

    const inputYear = useWatch({ control, name: "inputYear" });

    // Derive fields when inputYear changes
    useEffect(() =>
    {
        if (inputYear && inputYear >= 2000 && inputYear <= 2099)
        {
            const { finYear, date1, date2 } = computeFinFields(inputYear);

            // Set real POST payload fields
            setValue("FIN_Year", finYear, { shouldDirty: true });
            setValue("FIN_Date1", date1, { shouldDirty: true });
            setValue("FIN_Date2", date2, { shouldDirty: true });

            // Set read-only display fields
            setValue("display_FIN_Year", String(finYear));
            setValue("display_FIN_Date1", formatDate(date1));
            setValue("display_FIN_Date2", formatDate(date2));
        }
        else
        {
            setValue("FIN_Year", 0);
            setValue("FIN_Date1", "");
            setValue("FIN_Date2", "");

            setValue("display_FIN_Year", "—");
            setValue("display_FIN_Date1", "—");
            setValue("display_FIN_Date2", "—");
        }
    }, [ inputYear, setValue ]);

    return (
        <Card
            title={
                <span>
                    <CalendarOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
                    <Title level={5} style={{ margin: 0, display: "inline" }}>Financial Year Master</Title>
                    {isDirty && <Tag color="orange" style={{ marginLeft: 8 }}>Unsaved Changes</Tag>}
                </span>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <RecordDetailsBanner fields={bannerFields} queryString1={qString1} />

            <Form layout="vertical" onFinish={handleFormSubmit}>
                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12} md={6} lg={4}>
                        <NumberBox
                            control={control}
                            name="inputYear"
                            label="Year"
                            placeholder="2025"
                            required
                            disabled={submitting || isEditMode}
                            min={2000}
                            max={2099}
                            controls={false}
                            validation={{
                                required: V.Required,
                                min: { value: 2000, message: "Year must be \u2265 2000" },
                                max: { value: 2099, message: "Year must be \u2264 2099" },
                            }}
                        />
                    </Col>

                    <Col xs={24} sm={12} md={6} lg={4}>
                        <TextBox
                            control={control}
                            name="display_FIN_Year"
                            label="Financial Year"
                            disabled
                        />
                    </Col>

                    <Col xs={24} sm={12} md={6} lg={4}>
                        <TextBox
                            control={control}
                            name="display_FIN_Date1"
                            label="Start Date"
                            disabled
                        />
                    </Col>

                    <Col xs={24} sm={12} md={6} lg={4}>
                        <TextBox
                            control={control}
                            name="display_FIN_Date2"
                            label="End Date"
                            disabled
                        />
                    </Col>

                    <Col xs={12} sm={6} md={4} lg={4}>
                        <CheckBox
                            control={control}
                            name="FIN_Active_Flag"
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
