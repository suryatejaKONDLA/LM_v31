import { create } from "zustand";
import { type User } from "@/Domain/Index";

/**
 * Authentication state store.
 * Mirrors the auth use-case logic from CITL.Application.Core.Authentication.
 */
interface AuthState
{
    user: User | null;
    isAuthenticated: boolean;
    setUser: (user: User) => void;
    clearUser: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
    user: null,
    isAuthenticated: false,

    setUser: (user: User) =>
    {
        set({ user, isAuthenticated: true });
    },

    clearUser: () =>
    {
        set({ user: null, isAuthenticated: false });
    },
}));
