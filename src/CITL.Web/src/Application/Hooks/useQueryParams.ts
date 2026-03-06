import { useMemo } from "react";
import { useLocation } from "react-router-dom";

export interface QueryParams
{
    qString1: string;
    qString2: string;
    qString3: string;
    isEditMode: boolean;
}

/**
 * Reads QString1 / QString2 / QString3 from the current URL search params.
 * Returns isEditMode = true when QString1 is a non-zero, non-empty value.
 */
export function useQueryParams(): QueryParams
{
    const location = useLocation();

    return useMemo(() =>
    {
        const params = new URLSearchParams(location.search);
        const qString1 = params.get("QString1") ?? "0";
        const qString2 = params.get("QString2") ?? "";
        const qString3 = params.get("QString3") ?? "";
        const isEditMode = qString1 !== "0" && qString1 !== "";

        return { qString1, qString2, qString3, isEditMode };
    }, [ location.search ]);
}
