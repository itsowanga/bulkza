import { Receipt } from 'lucide-react';
import { Link } from 'react-router-dom';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { Money } from '@/components/Money';
import { PageHeader } from '@/components/PageHeader';
import { StatusChip } from '@/components/StatusChip';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { formatDateTime, formatUnits } from '@/lib/format';
import { flattenPages } from '@/lib/paging';
import { useMyOrders } from './hooks';

export function OrdersPage() {
  const query = useMyOrders();
  const orders = flattenPages(query.data?.pages);

  return (
    <>
      <PageHeader title="My orders" description="Orders are created for you when a pool you are in is finalised." />

      {query.isPending ? (
        <div className="space-y-3" aria-busy>
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} className="h-20 rounded-xl" />
          ))}
        </div>
      ) : null}
      {query.isError ? <ErrorState message={query.error.message} onRetry={() => void query.refetch()} /> : null}

      {query.data && orders.length === 0 ? (
        <EmptyState
          icon={Receipt}
          title="No orders yet"
          description="When a pool you joined is finalised, your order shows up here ready to pay."
          action={
            <Button asChild>
              <Link to="/pools">My pools</Link>
            </Button>
          }
        />
      ) : null}

      {orders.length > 0 ? (
        <>
          <ul className="divide-y divide-border overflow-hidden rounded-xl border border-border bg-card">
            {orders.map((order) => {
              const units = order.orderLines.reduce((sum, line) => sum + line.quantity, 0);
              return (
                <li key={order.orderId}>
                  <Link
                    to={`/orders/${order.orderId}`}
                    className="flex flex-col gap-3 p-4 transition-colors hover:bg-accent/50 sm:flex-row sm:items-center sm:justify-between"
                  >
                    <div>
                      <p className="font-mono text-sm">{order.orderId.slice(0, 8)}</p>
                      <p className="mt-0.5 text-xs text-muted-foreground">
                        {formatDateTime(order.createdAt)} · {formatUnits(units)} units
                      </p>
                    </div>
                    <div className="flex items-center justify-between gap-4 sm:justify-end">
                      <Money amount={order.totalAmount} className="font-medium" />
                      <StatusChip status={order.orderStatus} />
                    </div>
                  </Link>
                </li>
              );
            })}
          </ul>
          {query.hasNextPage ? (
            <div className="mt-8 flex justify-center">
              <Button variant="outline" disabled={query.isFetchingNextPage} onClick={() => void query.fetchNextPage()}>
                {query.isFetchingNextPage ? 'Loading…' : 'Load more'}
              </Button>
            </div>
          ) : null}
        </>
      ) : null}
    </>
  );
}
