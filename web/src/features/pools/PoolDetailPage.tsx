import { ArrowLeft, DoorOpen, Loader2 } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import { ConfirmAction } from '@/components/ConfirmAction';
import { DeadlineCountdown } from '@/components/DeadlineCountdown';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { PoolProgress } from '@/components/PoolProgress';
import { StatusChip } from '@/components/StatusChip';
import { TierLadder } from '@/components/TierLadder';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useProduct } from '@/features/products/hooks';
import { ApiError } from '@/lib/api';
import { useIsAdmin } from '@/lib/auth-store';
import { formatUnits, timeLeft } from '@/lib/format';
import { useNow } from '@/lib/use-now';
import type { BulkPoolDto } from '@/types/api';
import { JoinPoolForm } from './JoinPoolForm';
import { PoolAdminBar } from './PoolAdminBar';
import { useLeavePool, usePool } from './hooks';

function PoolDetailSkeleton() {
  return (
    <div className="space-y-6" aria-busy>
      <Skeleton className="h-9 w-72" />
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Skeleton className="h-56 rounded-xl" />
          <Skeleton className="h-40 rounded-xl" />
        </div>
        <Skeleton className="h-64 rounded-xl" />
      </div>
    </div>
  );
}

function statusMessage(pool: BulkPoolDto): string {
  switch (pool.status) {
    case 'Open':
      return 'Open for new merchants until the deadline. Every unit added can unlock a better price for the whole pool.';
    case 'ThresholdMet':
      return 'The minimum is met. New merchants can no longer join. An admin will lock the pool next.';
    case 'Locked':
      return 'Locked. Quantities are final while the pool waits to be finalised.';
    case 'Finalized':
      return 'Finalised. An order was created for every participant. Find yours under My orders.';
    case 'Expired':
      return 'This pool passed its deadline without being locked.';
    case 'Cancelled':
      return 'This pool was cancelled.';
  }
}

function LeaveButton({ poolId }: { poolId: string }) {
  const leave = useLeavePool(poolId);

  return (
    <ConfirmAction
      title="Leave this pool?"
      description="Your units are removed. If the pool drops below its minimum it goes back to Open."
      confirmLabel="Leave pool"
      destructive
      onConfirm={() => leave.mutate()}
      trigger={
        <Button variant="outline" className="w-full" disabled={leave.isPending}>
          {leave.isPending ? <Loader2 className="animate-spin" /> : <DoorOpen />} Leave pool
        </Button>
      }
    />
  );
}

export function PoolDetailPage() {
  const { id } = useParams<{ id: string }>();
  const poolQuery = usePool(id);
  const productQuery = useProduct(poolQuery.data?.productId);
  const isAdmin = useIsAdmin();
  const now = useNow();
  const deadlineExpired = poolQuery.data ? timeLeft(poolQuery.data.deadline, now).expired : false;

  if (poolQuery.isPending) return <PoolDetailSkeleton />;

  if (poolQuery.isError) {
    return poolQuery.error instanceof ApiError && poolQuery.error.status === 404 ? (
      <EmptyState
        icon={ArrowLeft}
        title="Pool not found"
        description="The link may be wrong, or the pool no longer exists."
        action={
          <Button asChild variant="outline">
            <Link to="/pools">My pools</Link>
          </Button>
        }
      />
    ) : (
      <ErrorState message={poolQuery.error.message} onRetry={() => void poolQuery.refetch()} />
    );
  }

  const pool = poolQuery.data;
  const product = productQuery.data;
  const participating = pool.myQuantity > 0;
  const canJoin = pool.status === 'Open' && !deadlineExpired && !participating;
  const canLeave = participating && !deadlineExpired && (pool.status === 'Open' || pool.status === 'ThresholdMet');

  return (
    <div className="space-y-8">
      <div>
        <Link to="/pools" className="mb-4 inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="size-4" aria-hidden /> My pools
        </Link>
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-2xl font-semibold tracking-tight">{product?.name ?? 'Bulk pool'}</h1>
          <StatusChip status={pool.status} />
        </div>
        <p className="mt-1 text-sm text-muted-foreground">
          {product ? (
            <>
              <Link to={`/products/${product.productId}`} className="font-mono hover:text-foreground">
                {product.sku}
              </Link>{' '}
              ·{' '}
            </>
          ) : null}
          <span className="font-mono">{pool.poolId}</span>
        </p>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <section className="rounded-xl border border-border bg-card p-5 sm:p-6" aria-label="Progress">
            <PoolProgress
              currentQuantity={pool.currentQuantity}
              minimumQuantity={pool.minimumQuantity}
              participantCount={pool.participantCount}
              minimumMerchants={pool.minimumMerchants}
            />
          </section>

          <section className="rounded-xl border border-border bg-card p-5 sm:p-6" aria-labelledby="tiers">
            <h2 id="tiers" className="mb-1 text-base font-medium">
              Volume price tiers
            </h2>
            <p className="mb-4 text-sm text-muted-foreground">Based on the total units in the pool, not on any one merchant's order.</p>
            {product ? (
              <TierLadder tiers={product.discountTiers} quantity={pool.currentQuantity} basePrice={product.basePrice} />
            ) : productQuery.isError ? (
              <p className="text-sm text-muted-foreground">The price tiers could not be loaded.</p>
            ) : (
              <Skeleton className="h-24" />
            )}
          </section>
        </div>

        <div className="space-y-6">
          <section className="space-y-4 rounded-xl border border-border bg-card p-5" aria-labelledby="participation">
            <h2 id="participation" className="text-base font-medium">
              Your participation
            </h2>
            <DeadlineCountdown deadline={pool.deadline} />
            <p className="text-sm text-muted-foreground">{statusMessage(pool)}</p>

            {participating ? (
              <p className="rounded-lg border border-emerald-500/30 bg-emerald-500/5 px-3 py-2 text-sm">
                You are in with <span className="font-mono font-medium tabular-nums text-emerald-300">{formatUnits(pool.myQuantity)}</span> units.
              </p>
            ) : null}

            {canJoin ? <JoinPoolForm pool={pool} product={product} /> : null}
            {canLeave ? <LeaveButton poolId={pool.poolId} /> : null}
            {pool.status === 'Finalized' && participating ? (
              <Button asChild className="w-full">
                <Link to="/orders">View my orders</Link>
              </Button>
            ) : null}
          </section>

          {isAdmin ? <PoolAdminBar pool={pool} /> : null}
        </div>
      </div>
    </div>
  );
}
