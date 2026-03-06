import { Modal } from "antd";
import { ModalDialog } from "@/Shared/UI/ModalDialog";

/**
 * ModalDialogHolder — mounts once at the app root.
 *
 * Captures the Ant Design `Modal.useModal()` API and registers it
 * with the imperative `ModalDialog` object in the Shared layer.
 */
export function ModalDialogHolder(): React.ReactNode
{
    const [ api, contextHolder ] = Modal.useModal();
    ModalDialog._register(api);
    return contextHolder;
}
