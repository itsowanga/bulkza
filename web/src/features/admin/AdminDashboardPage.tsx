import { ArrowUpRight, CircleCheck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ErrorState } from '@/components/ErrorState';
import { PageHeader } from '@/components/PageHeader';
import { StatusChip } from '@/components/StatusChip';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { BULK_POOL_STATUSES, ORDER_STATUSES, type AdminSummaryDto, type BulkPoolStatus, type OrderStatus } from '@/types/api';
import { useAdminSummary } from './hooks';

interface Attention {
  to: string;
  title: string;
  hint: string;
  count: (summary: AdminSummaryDto) => number;
}

// The work an admin has to do next, in the order a pool and an order move through the system.
const ATTENTION: readonly Attention[] = [
  { to: '/admin/pools?status=ThresholdMet', title: 'Pools ready to lock', hint: 'The minimum is met. Lock them to close the pool to new merchants.', count: (s) => s.pools.ThresholdMet },
  { to: '/admin/pools?status=Locked', title: 'Pools ready to finalise', hint: 'Fix the volume price and create an order for every participant.', count: (s) => s.pools.Locked },
  { to: '/admin/orders?status=Pending', title: 'Orders awaiting payment', hint: 'Created at finalise. Merchants pay these themselves.', count: (s) => s.orders.Pending },
  { to: '/admin/orders?status=Paid', title: 'Orders ready to ship', hint: 'Paid and waiting for you to ship.', count: (s) => s.orders.Paid },
  { to: '/admin/orders?status=Shipped', title: 'Orders awaiting delivery', hint: 'Shipped. Mark them delivered when they arrive.', count: (s) => s.orders.Shipped },
];

function AttentionCard({ item, summary }: { item: Attention; summary: AdminSummaryDto }) {
  const count = item.count(summary);
  const active = count > 0;

  return (
    <Link
      to={item.to}
      className={cn(
        'group flex flex-col justify-between rounded-xl border bg-card p-5 transition duration-150 hover:-translate-y-0.5 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
        active ? 'border-emerald-500/40 hover:border-emerald-500/70' : 'border-border hover:border-slate-600',
      )}
    >
      <div className="flex items-start justify-between">
        <span className={cn('font-mono text-4xl font-medium tabular-nums', active ? 'text-emerald-300' : 'text-muted-foreground')}>{count}</span>
        <ArrowUpRight className="size-4 text-muted-foreground transition-colors group-hover:text-foreground" aria-hidden />
      </div>
      <div className="mt-4">
        <p className="font-medium">{item.title}</p>
        <p className="mt-1 text-sm text-muted-foreground">{item.hint}</p>
      </div>
    </Link>
  );
}

function Breakdown<T extends BulkPoolStatus | OrderStatus>({ title, counts, statuses, basePath }: { title: string; counts: Record<T, number>; statuses: readonly T[]; basePath: string }) {
  const total = statuses.reduce((sum, status) => sum + counts[status], 0);

  return (
    <section className="rounded-xl border border-border bg-card" aria-label={title}>
      <h2 className="flex items-baseline justify-between border-b border-border px-5 py-3 text-base font-medium">
        {title}
        <span className="font-mono text-sm tabular-nums text-muted-foreground">{total} in total</span>
      </h2>
      <ul className="divide-y divide-border">
        {statuses.map((status) => (
          <li key={status}>
            <Link to={`${basePath}?status=${status}`} className="flex items-center justify-between px-5 py-3 transition-colors hover:bg-accent/50">
              <StatusChip status={status} />
              <span className="font-mono text-sm tabular-nums">{counts[status]}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}

export function AdminDashboardPage() {
  const query = useAdminSummary();

  return (
    <>
      <PageHeader title="Admin overview" description="What needs doing across every pool and order on the platform." />

      {query.isPending ? (
        <div className="space-y-6" aria-busy>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 5 }, (_, index) => (
              <Skeleton key={index} className="h-36 rounded-xl" />
            ))}
          </div>
          <Skeleton className="h-64 rounded-xl" />
        </div>
      ) : null}
      {query.isError ? <ErrorState message={query.error.message} onRetry={() => void query.refetch()} /> : null}

      {query.data ? (
        <div className="space-y-10">
          <section aria-labelledby="attention">
            <h2 id="attention" className="mb-4 text-lg font-medium">
              Needs attention
            </h2>
            {ATTENTION.every((item) => item.count(query.data) === 0) ? (
              <p className="mb-4 flex items-center gap-2 text-sm text-emerald-300">
                <CircleCheck className="size-4" aria-hidden /> Nothing is waiting on you right now.
              </p>
            ) : null}
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {ATTENTION.map((item) => (
                <AttentionCard key={item.to} item={item} summary={query.data} />
              ))}
            </div>
          </section>

          <div className="grid gap-6 lg:grid-cols-2">
            <Breakdown title="Pools by status" counts={query.data.pools} statuses={BULK_POOL_STATUSES} basePath="/admin/pools" />
            <Breakdown title="Orders by status" counts={query.data.orders} statuses={ORDER_STATUSES} basePath="/admin/orders" />
          </div>
        </div>
      ) : null}
    </>
  );
}
