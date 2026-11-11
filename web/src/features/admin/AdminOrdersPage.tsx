import { PackageCheck, Receipt, Truck } from 'lucide-react';
import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { ConfirmAction } from '@/components/ConfirmAction';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { FilterChips, type FilterOption } from '@/components/FilterChips';
import { Money } from '@/components/Money';
import { PageHeader } from '@/components/PageHeader';
import { Pagination } from '@/components/Pagination';
import { StatusChip } from '@/components/StatusChip';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { formatDateTime, formatUnits } from '@/lib/format';
import { cn } from '@/lib/utils';
import { ORDER_STATUSES, type AdminOrderDto, type OrderStatus } from '@/types/api';
import { ADMIN_PAGE_SIZE, useAdminOrders, useAdminSummary, useDeliverOrder, useShipOrder } from './hooks';

const STATUS_ORDER: readonly OrderStatus[] = ['Paid', 'Shipped', 'Pending', 'Delivered', 'Cancelled'];

/** Ship a paid order, or mark a shipped one delivered. Other states have nothing to do. */
function OrderActions({ order }: { order: AdminOrderDto }) {
  const ship = useShipOrder();
  const deliver = useDeliverOrder();

  if (order.orderStatus === 'Paid') {
    return (
      <ConfirmAction
        title="Mark this order as shipped?"
        description={`${order.merchantName} will see it as shipped. This cannot be undone.`}
        confirmLabel="Mark as shipped"
        onConfirm={() => ship.mutate(order.orderId)}
        trigger={
          <Button size="sm" variant="secondary" disabled={ship.isPending}>
            <Truck /> Ship
          </Button>
        }
      />
    );
  }

  if (order.orderStatus === 'Shipped') {
    return (
      <ConfirmAction
        title="Mark this order as delivered?"
        description={`${order.merchantName} will see it as delivered. This cannot be undone.`}
        confirmLabel="Mark as delivered"
        onConfirm={() => deliver.mutate(order.orderId)}
        trigger={
          <Button size="sm" variant="secondary" disabled={deliver.isPending}>
            <PackageCheck /> Deliver
          </Button>
        }
      />
    );
  }

  return <span className="text-xs text-muted-foreground">No action</span>;
}

export function AdminOrdersPage() {
  const [params, setParams] = useSearchParams();
  const [page, setPage] = useState(1);

  // Paid orders are the work queue, so they are what you see first. ?status=all shows everything.
  const raw = params.get('status');
  const status: OrderStatus | undefined = raw === 'all' ? undefined : (ORDER_STATUSES.find((candidate) => candidate === raw) ?? 'Paid');

  const orders = useAdminOrders({ status, page });
  const summary = useAdminSummary();

  const options: FilterOption<OrderStatus>[] = [
    ...STATUS_ORDER.map((value) => ({ value, label: value, count: summary.data?.orders[value] })),
    { value: undefined, label: 'All' },
  ];

  const select = (value: OrderStatus | undefined) => {
    setPage(1);
    setParams({ status: value ?? 'all' });
  };

  return (
    <>
      <PageHeader title="Orders" description="Orders from every merchant. Ship paid orders and mark shipped ones delivered." />

      <div className="mb-6">
        <FilterChips label="Filter by status" options={options} selected={status} onSelect={select} />
      </div>

      {orders.isPending ? <Skeleton className="h-72 rounded-xl" aria-busy /> : null}
      {orders.isError ? <ErrorState message={orders.error.message} onRetry={() => void orders.refetch()} /> : null}

      {orders.data && orders.data.items.length === 0 ? (
        <EmptyState icon={Receipt} title="No orders here" description={status ? `There are no ${status.toLowerCase()} orders.` : 'No order has been created yet.'} />
      ) : null}

      {orders.data && orders.data.items.length > 0 ? (
        <>
          <div className={cn('overflow-x-auto rounded-xl border border-border bg-card', orders.isPlaceholderData && 'pointer-events-none opacity-60 transition-opacity')}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Order</TableHead>
                  <TableHead>Merchant</TableHead>
                  <TableHead className="text-right">Units</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Placed</TableHead>
                  <TableHead>Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {orders.data.items.map((order) => (
                  <TableRow key={order.orderId}>
                    <TableCell className="font-mono text-xs">{order.orderId.slice(0, 8)}</TableCell>
                    <TableCell className="font-medium">{order.merchantName}</TableCell>
                    <TableCell className="text-right font-mono tabular-nums">{formatUnits(order.orderLines.reduce((sum, line) => sum + line.quantity, 0))}</TableCell>
                    <TableCell className="text-right">
                      <Money amount={order.totalAmount} />
                    </TableCell>
                    <TableCell>
                      <StatusChip status={order.orderStatus} />
                    </TableCell>
                    <TableCell className="whitespace-nowrap text-sm text-muted-foreground">{formatDateTime(order.createdAt)}</TableCell>
                    <TableCell>
                      <OrderActions order={order} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Pagination page={page} totalCount={orders.data.totalCount} pageSize={ADMIN_PAGE_SIZE} onPageChange={setPage} />
        </>
      ) : null}
    </>
  );
}
