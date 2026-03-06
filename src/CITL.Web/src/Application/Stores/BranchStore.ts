import { create } from "zustand";
import type { BranchInfo } from "@/Domain/Index";

const StorageKey = "citl_active_branch";

interface BranchState
{
    readonly activeBranch: BranchInfo | null;
    setBranch: (branch: BranchInfo) => void;
    initBranch: (branches: BranchInfo[]) => void;
    clearBranch: () => void;
}

function loadFromSession(): BranchInfo | null
{
    try
    {
        const raw = sessionStorage.getItem(StorageKey);
        if (!raw)
        {
            return null;
        }
        return JSON.parse(raw) as BranchInfo;
    }
    catch
    {
        return null;
    }
}

function saveToSession(branch: BranchInfo): void
{
    sessionStorage.setItem(StorageKey, JSON.stringify(branch));
}

export const useBranchStore = create<BranchState>((set) => ({
    activeBranch: loadFromSession(),

    setBranch: (branch: BranchInfo) =>
    {
        saveToSession(branch);
        set({ activeBranch: branch });
    },

    initBranch: (branches: BranchInfo[]) =>
    {
        const stored = loadFromSession();
        const match = stored
            ? branches.find((b) => b.BRANCH_Code === stored.BRANCH_Code)
            : null;
        const resolved = match ?? branches[0] ?? null;

        if (resolved)
        {
            saveToSession(resolved);
        }
        set({ activeBranch: resolved });
    },

    clearBranch: () =>
    {
        sessionStorage.removeItem(StorageKey);
        set({ activeBranch: null });
    },
}));
