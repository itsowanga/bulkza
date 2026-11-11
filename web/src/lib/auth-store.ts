import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { decodeSession, isExpired } from '@/lib/jwt';
import type { MerchantRole } from '@/types/api';

export interface Merchant {
  id: string;
  name: string;
  role: MerchantRole;
}

interface AuthState {
  token: string | null;
  merchant: Merchant | null;
  /** Stores a freshly issued token. Returns false when the token cannot be read or has already expired. */
  signIn: (token: string) => boolean;
  signOut: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      merchant: null,
      signIn: (token) => {
        const session = decodeSession(token);
        if (!session || isExpired(session)) return false;
        set({ token, merchant: { id: session.merchantId, name: session.name, role: session.role } });
        return true;
      },
      signOut: () => set({ token: null, merchant: null }),
    }),
    {
      name: 'bulkza-auth',
      version: 1,
      partialize: (state) => ({ token: state.token, merchant: state.merchant }),
      // A token that expired while the tab was closed must not survive a reload.
      onRehydrateStorage: () => (state) => {
        if (!state?.token) return;
        const session = decodeSession(state.token);
        if (!session || isExpired(session)) state.signOut();
      },
    },
  ),
);

export function useIsAdmin(): boolean {
  return useAuthStore((state) => state.merchant?.role === 'Admin');
}
