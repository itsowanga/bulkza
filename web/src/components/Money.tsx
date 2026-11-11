import { formatZAR } from '@/lib/format';
import { cn } from '@/lib/utils';

/** A rand amount in the monospaced figure style used across the app. */
export function Money({ amount, className }: { amount: number; className?: string }) {
  return <span className={cn('font-mono tabular-nums', className)}>{formatZAR(amount)}</span>;
}
