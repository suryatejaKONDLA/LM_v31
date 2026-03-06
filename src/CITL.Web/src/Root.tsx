import { useMemo } from "react";
import { BrowserRouter } from "react-router-dom";
import { ConfigProvider, App as AntdApp, theme } from "antd";
import { HappyProvider } from "@ant-design/happy-work-theme";
import { AppRoutes } from "@/Presentation/Routing/Index";
import { LocationBlockedModal, GlobalSpinnerHolder, ModalDialogHolder, Message, Notify } from "@/Presentation/Controls/Index";
import { AppConfig, ThemeConstants } from "@/Shared/Index";
import { useThemeStore, useLocationPermission } from "@/Application/Index";

/**
 * Root component — reads ThemeStore and feeds ConfigProvider.
 * Memoises the theme config so Ant Design only recalculates when mode/tokens change.
 */
export default function Root(): React.JSX.Element
{
    const { isDarkMode, customTokens, isCompact, isHappyWork } = useThemeStore();

    // Initialise global geolocation watcher — drives LocationStore + LocationBlockedModal
    useLocationPermission();

    const themeConfig = useMemo(() =>
    {
        const algorithms = [ isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm ];
        if (isCompact)
        {
            algorithms.push(theme.compactAlgorithm);
        }

        return {
            algorithm: algorithms,
            token: {
                ...ThemeConstants.FixedTokens,
                ...customTokens,
            },
            hashed: false,
        };
    }, [ isDarkMode, customTokens, isCompact ]);

    return (
        <ConfigProvider
            theme={themeConfig}
            wave={{ disabled: !isHappyWork }}
        >
            <HappyProvider disabled={!isHappyWork}>
                <AntdApp>
                    <BrowserRouter basename={AppConfig.BasePath}>
                        <AppRoutes />
                    </BrowserRouter>
                    <LocationBlockedModal />
                    <GlobalSpinnerHolder />
                    <ModalDialogHolder />
                    <Message.Holder />
                    <Notify.Holder />
                </AntdApp>
            </HappyProvider>
        </ConfigProvider>
    );
}
