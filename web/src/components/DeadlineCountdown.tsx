import { Clock } from 'lucide-react';
import { formatDateTime, timeLeft } from '@/lib/format';
import { useNow } from '@/lib/use-now';
import { cn } from '@/lib/utils';

export function DeadlineCountdown({ deadline, className }: { deadline: string; className?: string }) {
  const now = useNow();
  const left = timeLeft(deadline, now);

  return (
    <span
      className={cn('inline-flex items-center gap-1.5 text-sm', left.urgent ? 'text-red-400' : 'text-muted-foreground', className)}
      title={`Deadline: ${formatDateTime(deadline)}`}
    >
      <Clock className="size-4" aria-hidden />
      <span className="font-mono tabular-nums">{left.label}</span>
    </span>
  );
}
