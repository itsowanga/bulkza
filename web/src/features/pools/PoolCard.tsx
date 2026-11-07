import { Link } from 'react-router-dom';
import { DeadlineCountdown } from '@/components/DeadlineCountdown';
import { PoolProgress } from '@/components/PoolProgress';
import { StatusChip } from '@/components/StatusChip';
import { formatUnits } from '@/lib/format';
import type { BulkPoolDto } from '@/types/api';

interface PoolCardProps {
  pool: BulkPoolDto;
  /** Shown as the title. Falls back to the pool id while the product is loading. */
  productName?: string;
}

export function PoolCard({ pool, productName }: PoolCardProps) {
  return (
    <Link
      to={`/pools/${pool.poolId}`}
      className="block rounded-xl border border-border bg-card p-4 transition duration-150 hover:-translate-y-0.5 hover:border-slate-600 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="truncate font-medium">{productName ?? 'Pool'}</h3>
          <p className="mt-0.5 font-mono text-xs text-muted-foreground">{pool.poolId.slice(0, 8)}</p>
        </div>
        <StatusChip status={pool.status} />
      </div>

      <div className="mt-4">
        <PoolProgress
          compact
          currentQuantity={pool.currentQuantity}
          minimumQuantity={pool.minimumQuantity}
          participantCount={pool.participantCount}
          minimumMerchants={pool.minimumMerchants}
        />
      </div>

      <div className="mt-4 flex items-center justify-between text-sm">
        <DeadlineCountdown deadline={pool.deadline} />
        {pool.myQuantity > 0 ? (
          <span className="text-muted-foreground">
            You: <span className="font-mono tabular-nums text-foreground">{formatUnits(pool.myQuantity)}</span> units
          </span>
        ) : null}
      </div>
    </Link>
  );
}
