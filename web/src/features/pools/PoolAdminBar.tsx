import { ShieldCheck } from 'lucide-react';
import type { BulkPoolDto } from '@/types/api';
import { PoolActionButtons } from './PoolActionButtons';

/** Lifecycle controls on the pool page. Only rendered for admins, and the API enforces the role as well. */
export function PoolAdminBar({ pool }: { pool: BulkPoolDto }) {
  return (
    <section aria-labelledby="admin-actions" className="rounded-xl border border-amber-500/30 bg-amber-500/5 p-4">
      <h2 id="admin-actions" className="mb-3 flex items-center gap-2 text-sm font-medium text-amber-200">
        <ShieldCheck className="size-4" aria-hidden /> Admin actions
      </h2>
      <PoolActionButtons pool={pool} />
      <p className="mt-3 text-xs text-muted-foreground">Lock needs the threshold met. Finalise needs a locked pool. Expire needs a passed deadline.</p>
    </section>
  );
}
