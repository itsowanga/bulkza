import { Check, XCircle } from 'lucide-react';
import { ORDER_FLOW } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { OrderStatus } from '@/types/api';

const LABELS: Record<OrderStatus, string> = {
  Pending: 'Awaiting payment',
  Paid: 'Paid',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
};

/** Pending, Paid, Shipped, Delivered. A cancelled order shows a single cancelled state instead. */
export function OrderTimeline({ status }: { status: OrderStatus }) {
  if (status === 'Cancelled') {
    return (
      <div className="flex items-center gap-3 rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-200">
        <XCircle className="size-5 shrink-0 text-red-400" aria-hidden />
        This order was cancelled.
      </div>
    );
  }

  const current = ORDER_FLOW.indexOf(status);

  return (
    <ol className="grid gap-4 sm:grid-cols-4" aria-label="Order progress">
      {ORDER_FLOW.map((step, index) => {
        const done = index < current;
        const active = index === current;
        // A delivered order is complete, so its last step reads as done rather than current.
        const complete = done || (active && status === 'Delivered');

        return (
          <li key={step} aria-current={active ? 'step' : undefined} className="flex items-center gap-3 sm:flex-col sm:items-start">
            <span
              className={cn(
                'flex size-8 items-center justify-center rounded-full border text-xs font-medium',
                complete && 'border-emerald-500 bg-emerald-500 text-slate-950',
                active && !complete && 'border-emerald-500 text-emerald-300',
                !complete && !active && 'border-border text-muted-foreground',
              )}
            >
              {complete ? <Check className="size-4" aria-hidden /> : index + 1}
            </span>
            <span className={cn('text-sm', active || complete ? 'text-foreground' : 'text-muted-foreground')}>{LABELS[step]}</span>
          </li>
        );
      })}
    </ol>
  );
}
