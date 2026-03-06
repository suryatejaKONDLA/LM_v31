import { create } from "zustand";

export interface NavigationGuardOptions
{
    dialogTitle?: string;
    dialogContent?: string;
    stayButtonText?: string;
    leaveButtonText?: string;
    onLeaveConfirmed?: () => void;
}

interface NavigationGuardState
{
    isBlocked: boolean;
    dialogTitle: string;
    dialogContent: string;
    stayButtonText: string;
    leaveButtonText: string;
    onLeaveConfirmed: (() => void) | null;
    blockNavigation: (options?: NavigationGuardOptions) => void;
    unblockNavigation: () => void;
}

const defaults = {
    isBlocked: false,
    dialogTitle: "Unsaved Changes",
    dialogContent: "You have unsaved changes. If you leave, your changes will be lost.",
    stayButtonText: "Continue Editing",
    leaveButtonText: "Exit Without Saving",
    onLeaveConfirmed: null as (() => void) | null,
};

export const useNavigationGuardStore = create<NavigationGuardState>((set) => ({
    ...defaults,

    blockNavigation: (options) =>
    {
        set({
            isBlocked: true,
            dialogTitle: options?.dialogTitle ?? defaults.dialogTitle,
            dialogContent: options?.dialogContent ?? defaults.dialogContent,
            stayButtonText: options?.stayButtonText ?? defaults.stayButtonText,
            leaveButtonText: options?.leaveButtonText ?? defaults.leaveButtonText,
            onLeaveConfirmed: options?.onLeaveConfirmed ?? null,
        });
    },

    unblockNavigation: () =>
    {
        set({ isBlocked: false, onLeaveConfirmed: null });
    },
}));
