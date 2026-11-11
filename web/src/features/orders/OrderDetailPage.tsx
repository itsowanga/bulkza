import { ArrowLeft, CreditCard, Loader2, XCircle } from 'lucide-react';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { ConfirmAction } from '@/components/ConfirmAction';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { Field } from '@/components/Field';
import { Money } from '@/components/Money';
import { StatusChip } from '@/components/StatusChip';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { useProductsById } from '@/features/products/hooks';
import { ApiError } from '@/lib/api';
import { formatDateTime, formatUnits } from '@/lib/format';
import type { OrderDto } from '@/types/api';
import { OrderTimeline } from './OrderTimeline';
import { useCancelOrder, useOrder, usePayOrder } from './hooks';

const TEST_CARD_TOKEN = 'tok_test_visa_4242';

function PayCard({ order }: { order: OrderDto }) {
  const pay = usePayOrder(order.orderId);
  const [cardToken, setCardToken] = useState(TEST_CARD_TOKEN);

  return (
    <section className="space-y-4 rounded-xl border border-border bg-card p-5" aria-labelledby="pay">
      <h2 id="pay" className="text-base font-medium">
        Pay for this order
      </h2>
      <Field label="Card token (test mode)" htmlFor="cardToken" hint="Payments run against a mock gateway. No real card is charged.">
        <Input id="cardToken" className="font-mono" value={cardToken} onChange={(event) => setCardToken(event.target.value)} />
      </Field>
      <Button
        className="w-full"
        disabled={pay.isPending || cardToken.trim().length === 0}
        onClick={() =>
          pay.mutate(
            { cardToken: cardToken.trim() },
            {
              // The API answers 200 even when the gateway declines, so the new status is checked here.
              onSuccess: (result) =>
                result.orderStatus === 'Paid'
                  ? toast.success('Payment received')
                  : toast.error('The payment was not accepted. Your order is still pending.'),
            },
          )
        }
      >
        {pay.isPending ? <Loader2 className="animate-spin" /> : <CreditCard />}
        Pay <Money amount={order.totalAmount} />
      </Button>
    </section>
  );
}

function CancelButton({ order }: { order: OrderDto }) {
  const cancel = useCancelOrder(order.orderId);
  const refunds = order.orderStatus === 'Paid';

  return (
    <ConfirmAction
      title="Cancel this order?"
      description={refunds ? 'The payment is refunded to your card. This cannot be undone.' : 'The order is closed and nothing is charged. This cannot be undone.'}
      confirmLabel="Cancel order"
      destructive
      onConfirm={() => cancel.mutate(undefined, { onSuccess: () => toast.success(refunds ? 'Order cancelled and refunded' : 'Order cancelled') })}
      trigger={
        <Button variant="outline" className="w-full" disabled={cancel.isPending}>
          {cancel.isPending ? <Loader2 className="animate-spin" /> : <XCircle />} Cancel order
        </Button>
      }
    />
  );
}

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const query = useOrder(id);
  const { products } = useProductsById(query.data?.orderLines.map((line) => line.productId) ?? []);

  if (query.isPending) {
    return (
      <div className="space-y-6" aria-busy>
        <Skeleton className="h-9 w-64" />
        <Skeleton className="h-24 rounded-xl" />
        <Skeleton className="h-48 rounded-xl" />
      </div>
    );
  }

  if (query.isError) {
    return query.error instanceof ApiError && query.error.status === 404 ? (
      <EmptyState
        icon={ArrowLeft}
        title="Order not found"
        description="It may belong to another account, or the link is wrong."
        action={
          <Button asChild variant="outline">
            <Link to="/orders">My orders</Link>
          </Button>
        }
      />
    ) : (
      <ErrorState message={query.error.message} onRetry={() => void query.refetch()} />
    );
  }

  const order = query.data;
  const canPay = order.orderStatus === 'Pending';
  const canCancel = order.orderStatus === 'Pending' || order.orderStatus === 'Paid';

  return (
    <div className="space-y-8">
      <div>
        <Link to="/orders" className="mb-4 inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="size-4" aria-hidden /> My orders
        </Link>
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-2xl font-semibold tracking-tight">Order</h1>
          <StatusChip status={order.orderStatus} />
        </div>
        <p className="mt-1 text-sm text-muted-foreground">
          <span className="font-mono">{order.orderId}</span> · placed {formatDateTime(order.createdAt)}
        </p>
      </div>

      <section className="rounded-xl border border-border bg-card p-5 sm:p-6" aria-label="Status">
        <OrderTimeline status={order.orderStatus} />
      </section>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="overflow-hidden rounded-xl border border-border bg-card lg:col-span-2" aria-labelledby="lines">
          <h2 id="lines" className="border-b border-border px-5 py-3 text-base font-medium">
            Items
          </h2>
          <ul className="divide-y divide-border">
            {order.orderLines.map((line) => {
              const product = products.get(line.productId);
              return (
                <li key={line.productId} className="flex items-center justify-between gap-4 px-5 py-4 text-sm">
                  <div className="min-w-0">
                    <p className="truncate font-medium">{product?.name ?? <Skeleton className="h-4 w-32" />}</p>
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      <span className="font-mono tabular-nums">{formatUnits(line.quantity)}</span> × <Money amount={line.unitPrice} />
                    </p>
                  </div>
                  <Money amount={line.quantity * line.unitPrice} />
                </li>
              );
            })}
          </ul>
          <div className="flex items-center justify-between border-t border-border px-5 py-4">
            <span className="text-sm text-muted-foreground">Total</span>
            <Money amount={order.totalAmount} className="text-lg font-medium" />
          </div>
        </section>

        <div className="space-y-6">
          {canPay ? <PayCard order={order} /> : null}
          {canCancel ? <CancelButton order={order} /> : null}

          {order.paymentId || order.refundId ? (
            <section className="space-y-2 rounded-xl border border-border bg-card p-5 text-sm" aria-label="References">
              {order.paymentId ? (
                <p>
                  <span className="text-muted-foreground">Payment </span>
                  <span className="break-all font-mono text-xs">{order.paymentId}</span>
                </p>
              ) : null}
              {order.refundId ? (
                <p>
                  <span className="text-muted-foreground">Refund </span>
                  <span className="break-all font-mono text-xs">{order.refundId}</span>
                </p>
              ) : null}
            </section>
          ) : null}
        </div>
      </div>
    </div>
  );
}
