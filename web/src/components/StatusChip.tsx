import { cn } from '@/lib/utils';
import type { BulkPoolStatus, OrderStatus } from '@/types/api';

type Status = BulkPoolStatus | OrderStatus;

interface Style {
  label: string;
  chip: string;
  dot: string;
}

const blue: Omit<Style, 'label'> = { chip: 'border-blue-500/30 bg-blue-500/10 text-blue-300', dot: 'bg-blue-400' };
const green: Omit<Style, 'label'> = { chip: 'border-emerald-500/30 bg-emerald-500/10 text-emerald-300', dot: 'bg-emerald-400' };
const amber: Omit<Style, 'label'> = { chip: 'border-amber-500/30 bg-amber-500/10 text-amber-300', dot: 'bg-amber-400' };
const slate: Omit<Style, 'label'> = { chip: 'border-slate-500/30 bg-slate-500/10 text-slate-300', dot: 'bg-slate-400' };
const red: Omit<Style, 'label'> = { chip: 'border-red-500/30 bg-red-500/10 text-red-300', dot: 'bg-red-400' };

const STYLES: Record<Status, Style> = {
  // Pools
  Open: { label: 'Open', ...blue },
  ThresholdMet: { label: 'Threshold met', ...green },
  Locked: { label: 'Locked', ...amber },
  Finalized: { label: 'Finalised', ...slate },
  Expired: { label: 'Expired', ...red },
  Cancelled: { label: 'Cancelled', ...red },
  // Orders
  Pending: { label: 'Pending', ...amber },
  Paid: { label: 'Paid', ...blue },
  Shipped: { label: 'Shipped', ...blue },
  Delivered: { label: 'Delivered', ...green },
};

export function StatusChip({ status, className }: { status: Status; className?: string }) {
  const style = STYLES[status];

  return (
    <span
      className={cn('inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium', style.chip, className)}
    >
      <span className={cn('size-1.5 rounded-full', style.dot)} aria-hidden />
      {style.label}
    </span>
  );
}
