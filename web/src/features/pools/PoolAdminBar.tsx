import { Ban, CalendarX, CheckCheck, Lock, ShieldCheck } from 'lucide-react';
import { ConfirmAction } from '@/components/ConfirmAction';
import { Button } from '@/components/ui/button';
import { timeLeft } from '@/lib/format';
import { useNow } from '@/lib/use-now';
import type { BulkPoolDto } from '@/types/api';
import { type PoolAdminAction, usePoolAdminAction } from './hooks';

interface ActionSpec {
  action: PoolAdminAction;
  label: string;
  icon: typeof Lock;
  enabled: (pool: BulkPoolDto, deadlinePassed: boolean) => boolean;
  title: string;
  description: string;
  destructive?: boolean;
}

const ACTIONS: ActionSpec[] = [
  {
    action: 'lock',
    label: 'Lock',
    icon: Lock,
    enabled: (pool) => pool.status === 'ThresholdMet',
    title: 'Lock this pool?',
    description: 'Quantities become final and nobody can join or leave. This cannot be undone.',
  },
  {
    action: 'finalise',
    label: 'Finalise',
    icon: CheckCheck,
    enabled: (pool) => pool.status === 'Locked',
    title: 'Finalise this pool?',
    description: 'The volume price is fixed and an order is created for every participant. This cannot be undone.',
  },
  {
    action: 'expire',
    label: 'Expire',
    icon: CalendarX,
    enabled: (pool, deadlinePassed) => deadlinePassed && (pool.status === 'Open' || pool.status === 'ThresholdMet'),
    title: 'Expire this pool?',
    description: 'The deadline has passed without the pool being locked. It is closed for good.',
    destructive: true,
  },
  {
    action: 'cancel',
    label: 'Cancel',
    icon: Ban,
    enabled: (pool) => pool.status === 'Open' || pool.status === 'ThresholdMet',
    title: 'Cancel this pool?',
    description: 'The pool is closed for every participant. This cannot be undone.',
    destructive: true,
  },
];

/** Lifecycle controls. Only rendered for admins, and the API enforces the role as well. */
export function PoolAdminBar({ pool }: { pool: BulkPoolDto }) {
  const run = usePoolAdminAction(pool.poolId);
  const now = useNow();
  const deadlinePassed = timeLeft(pool.deadline, now).expired;

  return (
    <section aria-labelledby="admin-actions" className="rounded-xl border border-amber-500/30 bg-amber-500/5 p-4">
      <h2 id="admin-actions" className="mb-3 flex items-center gap-2 text-sm font-medium text-amber-200">
        <ShieldCheck className="size-4" aria-hidden /> Admin actions
      </h2>
      <div className="grid grid-cols-2 gap-2">
        {ACTIONS.map((spec) => (
          <ConfirmAction
            key={spec.action}
            title={spec.title}
            description={spec.description}
            confirmLabel={spec.label}
            destructive={spec.destructive}
            onConfirm={() => run.mutate(spec.action)}
            trigger={
              <Button variant={spec.destructive ? 'outline' : 'secondary'} size="sm" disabled={run.isPending || !spec.enabled(pool, deadlinePassed)}>
                <spec.icon /> {spec.label}
              </Button>
            }
          />
        ))}
      </div>
      <p className="mt-3 text-xs text-muted-foreground">Lock needs the threshold met. Finalise needs a locked pool. Expire needs a passed deadline.</p>
    </section>
  );
}
