import type { ReactNode } from 'react';
import { Navigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/lib/auth-store';
import { homePathFor, safeNext } from '@/lib/redirect';

/** Login and register are for signed-out visitors. Anyone already signed in goes on to the app. */
export function PublicOnly({ children }: { children: ReactNode }) {
  const token = useAuthStore((state) => state.token);
  const role = useAuthStore((state) => state.merchant?.role);
  const [params] = useSearchParams();

  if (token) return <Navigate to={safeNext(params.get('next'), homePathFor(role))} replace />;
  return children;
}
