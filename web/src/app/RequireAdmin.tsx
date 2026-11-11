import { ShieldAlert } from 'lucide-react';
import { Link, Outlet } from 'react-router-dom';
import { EmptyState } from '@/components/EmptyState';
import { Button } from '@/components/ui/button';
import { useIsAdmin } from '@/lib/auth-store';

/** Admin screens. The API refuses everyone else too, so this only saves them a confusing page of errors. */
export function RequireAdmin() {
  if (!useIsAdmin()) {
    return (
      <EmptyState
        icon={ShieldAlert}
        title="You don't have access to this area"
        description="These pages are for BulkZA administrators."
        action={
          <Button asChild variant="outline">
            <Link to="/products">Back to products</Link>
          </Button>
        }
      />
    );
  }
  return <Outlet />;
}
