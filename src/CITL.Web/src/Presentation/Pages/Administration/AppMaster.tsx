import { useState, useEffect, useCallback } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { Form, Card, Row, Col, Space, Typography, theme } from "antd";
import { SettingOutlined } from "@ant-design/icons";
import { TextBox, ImageUpload, FormActionButtons, RecordDetailsBanner, GlobalSpinner, ModalDialog } from "@/Presentation/Controls/Index";
import { AppMasterService } from "@/Infrastructure/Index";
import { type AppMasterResponse, type AppMasterRequest } from "@/Domain/Index";
import { ApiResponseCode, V } from "@/Shared/Index";

const { Title } = Typography;

type AppMasterFormValues = Required<Pick<AppMasterRequest, "APP_Header1" | "APP_Header2" | "APP_Logo1" | "APP_Logo2" | "APP_Logo3">>;

const defaultValues: AppMasterFormValues = {
    APP_Header1: "",
    APP_Header2: "",
    APP_Logo1: null,
    APP_Logo2: null,
    APP_Logo3: null,
};

export default function AppMaster(): React.JSX.Element
{
    const { token } = theme.useToken();

    const [ appData, setAppData ] = useState<AppMasterResponse | null>(null);
    const [ submitting, setSubmitting ] = useState(false);

    const { control, handleSubmit, reset } = useForm<AppMasterFormValues>({
        defaultValues,
    });

    const fetchData = useCallback(async (signal?: AbortSignal) =>
    {
        GlobalSpinner.show("Loading application settings…");
        try
        {
            const result = await AppMasterService.get(signal);
            if (result.Code === ApiResponseCode.Success)
            {
                const data = result.Data;
                setAppData(data);
                reset({
                    APP_Header1: data.APP_Header1,
                    APP_Header2: data.APP_Header2,
                    APP_Logo1: data.APP_Logo1,
                    APP_Logo2: data.APP_Logo2,
                    APP_Logo3: data.APP_Logo3,
                });
            }
        }
        finally
        {
            GlobalSpinner.hide();
        }
    }, [ reset ]);

    useEffect(() =>
    {
        const controller = new AbortController();
        void fetchData(controller.signal);
        return () =>
        {
            controller.abort();
        };
    }, [ fetchData ]);

    const handleFormFinish = useCallback(() =>
    {
        const onSubmit: SubmitHandler<AppMasterFormValues> = async (formValues) =>
        {
            setSubmitting(true);
            try
            {
                const request: AppMasterRequest = {
                    APP_Code: appData?.APP_Code ?? 0,
                    APP_Header1: formValues.APP_Header1,
                    APP_Header2: formValues.APP_Header2,
                    APP_Logo1: formValues.APP_Logo1,
                    APP_Logo2: formValues.APP_Logo2,
                    APP_Logo3: formValues.APP_Logo3,
                    Session_Id: 0,
                    Branch_Code: 0,
                };

                const result = await AppMasterService.addOrUpdate(request);

                if (result.Code === ApiResponseCode.Success)
                {
                    ModalDialog.successResult(result.Message);
                    await fetchData();
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
    }, [ handleSubmit, appData, fetchData ]);

    const handleReset = useCallback(() =>
    {
        if (appData)
        {
            reset({
                APP_Header1: appData.APP_Header1,
                APP_Header2: appData.APP_Header2,
                APP_Logo1: appData.APP_Logo1,
                APP_Logo2: appData.APP_Logo2,
                APP_Logo3: appData.APP_Logo3,
            });
        }
    }, [ appData, reset ]);

    const bannerFields: [string | null, string | null, string | null] = [
        appData ? appData.APP_Code.toString() : null,
        appData?.APP_Created_Name ? `${appData.APP_Created_Name} (${appData.APP_Created_Date})` : null,
        appData?.APP_Modified_Name ? `${appData.APP_Modified_Name} (${appData.APP_Modified_Date ?? ""})` : null,
    ];

    return (
        <Card
            title={
                <Space>
                    <SettingOutlined style={{ color: token.colorPrimary }} />
                    <Title level={5} style={{ margin: 0 }}>Application Settings</Title>
                </Space>
            }
            variant="borderless"
            styles={{ body: { padding: "24px" } }}
        >
            <RecordDetailsBanner
                fields={bannerFields}
                queryString1={appData ? appData.APP_Code.toString() : "0"}
            />

            <Form layout="vertical" onFinish={handleFormFinish}>
                <Row gutter={[ 24, 0 ]}>

                    {/* Logos */}
                    <Col xs={24} sm={8} style={{ display: "flex", justifyContent: "center", marginBottom: 16 }}>
                        <ImageUpload
                            control={control}
                            name="APP_Logo1"
                            label="Logo 1 (Main)"
                            size={120}
                            maxSizeMb={2}
                        />
                    </Col>

                    <Col xs={24} sm={8} style={{ display: "flex", justifyContent: "center", marginBottom: 16 }}>
                        <ImageUpload
                            control={control}
                            name="APP_Logo2"
                            label="Logo 2"
                            size={120}
                            maxSizeMb={2}
                        />
                    </Col>

                    <Col xs={24} sm={8} style={{ display: "flex", justifyContent: "center", marginBottom: 16 }}>
                        <ImageUpload
                            control={control}
                            name="APP_Logo3"
                            label="Logo 3"
                            size={120}
                            maxSizeMb={2}
                        />
                    </Col>
                </Row>

                <Row gutter={[ 16, 0 ]}>
                    <Col xs={24} sm={12}>
                        <TextBox
                            control={control}
                            name="APP_Header1"
                            label="Application Header 1"
                            placeholder="Enter Application Header *"
                            required
                            maxLength={60}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 60, message: V.MaxLength(60) },
                            }}
                        />
                    </Col>

                    <Col xs={24} sm={12}>
                        <TextBox
                            control={control}
                            name="APP_Header2"
                            label="Application Header 2"
                            placeholder="Enter Short Code *"
                            required
                            maxLength={7}
                            validation={{
                                required: V.Required,
                                maxLength: { value: 7, message: V.MaxLength(7) },
                                pattern: {
                                    value: V.NoSpaceAndNoSymbolsPattern,
                                    message: V.NoSpaceAndNoSymbols,
                                },
                            }}
                        />
                    </Col>
                </Row>

                <FormActionButtons
                    submitting={submitting}
                    submitText="Save Settings"
                    onReset={handleReset}
                />
            </Form>
        </Card>
    );
}
