import { Ban, CalendarX, CheckCheck, Lock } from 'lucide-react';
import { ConfirmAction } from '@/components/ConfirmAction';
import { Button } from '@/components/ui/button';
import { timeLeft } from '@/lib/format';
import { useNow } from '@/lib/use-now';
import { cn } from '@/lib/utils';
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

interface PoolActionButtonsProps {
  pool: BulkPoolDto;
  /** Show only the actions that can be taken now, as small buttons. Used in tables. */
  onlyAvailable?: boolean;
  className?: string;
}

/** Lock, finalise, expire and cancel for one pool. The API enforces the Admin role; the UI only offers them to admins. */
export function PoolActionButtons({ pool, onlyAvailable, className }: PoolActionButtonsProps) {
  const run = usePoolAdminAction(pool.poolId);
  const now = useNow();
  const deadlinePassed = timeLeft(pool.deadline, now).expired;
  const specs = ACTIONS.filter((spec) => !onlyAvailable || spec.enabled(pool, deadlinePassed));

  return (
    <div className={cn(onlyAvailable ? 'flex flex-wrap gap-1.5' : 'grid grid-cols-2 gap-2', className)}>
      {specs.map((spec) => (
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
  );
}
