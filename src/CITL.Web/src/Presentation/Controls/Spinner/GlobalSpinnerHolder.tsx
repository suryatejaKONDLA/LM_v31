import { useState, useEffect, memo } from "react";
import { GlobalSpinner } from "./GlobalSpinner";
import { Spinner } from "./Spinner";

/**
 * GlobalSpinnerHolder — mounts once at app root.
 *
 * Registers itself with the imperative `GlobalSpinner` API so that
 * `GlobalSpinner.show()` / `GlobalSpinner.hide()` work anywhere in the tree
 * without prop drilling or additional context.
 */
function GlobalSpinnerHolderInner(): React.ReactNode
{
    const [ visible, setVisible ] = useState(false);
    const [ tip, setTip ] = useState<string | undefined>(undefined);

    useEffect(() =>
    {
        GlobalSpinner._register((isVisible, spinnerTip) =>
        {
            setVisible(isVisible);
            setTip(spinnerTip);
        });

        return () =>
        {
            GlobalSpinner._unregister();
        };
    }, []);

    return <Spinner loading={visible} tip={tip ?? "Loading\u2026"} />;
}

export const GlobalSpinnerHolder = memo(GlobalSpinnerHolderInner);
