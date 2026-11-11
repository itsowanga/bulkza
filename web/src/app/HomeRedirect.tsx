import { Navigate } from 'react-router-dom';
import { useAuthStore } from '@/lib/auth-store';
import { homePathFor } from '@/lib/redirect';

/** "/" sends an admin to the dashboard and everyone else to the catalogue. */
export function HomeRedirect() {
  const role = useAuthStore((state) => state.merchant?.role);
  return <Navigate to={homePathFor(role)} replace />;
}
