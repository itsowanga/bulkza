import { Boxes } from 'lucide-react';
import { cn } from '@/lib/utils';

export function Logo({ className }: { className?: string }) {
  return (
    <span className={cn('inline-flex items-center gap-2 text-base font-semibold tracking-tight', className)}>
      <span className="flex size-7 items-center justify-center rounded-md bg-emerald-500 text-slate-950">
        <Boxes className="size-4" aria-hidden />
      </span>
      BulkZA
    </span>
  );
}
