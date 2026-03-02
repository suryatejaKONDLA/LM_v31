import { create } from "zustand";
import { type CompanyMasterResponse } from "@/Domain/Index";

/**
 * Company master state store.
 * Persists company info in memory so Login, MainLayout (header/footer),
 * and any other component can read it without re-fetching.
 */
interface CompanyState
{
    readonly fullName: string;
    readonly shortName: string;
    readonly mobile1: string;
    readonly mobile2: string;
    readonly email: string;
    readonly website: string;
    readonly tagline: string;
    readonly logo1: string | null;
    readonly logo2: string | null;
    readonly logo3: string | null;
    readonly initialized: boolean;

    setCompany: (company: CompanyMasterResponse) => void;
    clearCompany: () => void;
}

export const useCompanyStore = create<CompanyState>((set) => ({
    fullName: "",
    shortName: "",
    mobile1: "",
    mobile2: "",
    email: "",
    website: "",
    tagline: "",
    logo1: null,
    logo2: null,
    logo3: null,
    initialized: false,

    setCompany: (company: CompanyMasterResponse) =>
    {
        set({
            fullName: company.CMP_Full_Name,
            shortName: company.CMP_Short_Name,
            mobile1: company.CMP_Mobile1 ?? "",
            mobile2: company.CMP_Mobile2 ?? "",
            email: company.CMP_Email ?? "",
            website: company.CMP_Website ?? "",
            tagline: company.CMP_Tagline ?? "",
            logo1: company.CMP_Logo1 ?? null,
            logo2: company.CMP_Logo2 ?? null,
            logo3: company.CMP_Logo3 ?? null,
            initialized: true,
        });
    },

    clearCompany: () =>
    {
        set({
            fullName: "",
            shortName: "",
            mobile1: "",
            mobile2: "",
            email: "",
            website: "",
            tagline: "",
            logo1: null,
            logo2: null,
            logo3: null,
            initialized: false,
        });
    },
}));
