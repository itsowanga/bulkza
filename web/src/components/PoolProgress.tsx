import { animate, motion, useReducedMotion } from 'framer-motion';
import { Check } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { formatUnits } from '@/lib/format';
import { cn } from '@/lib/utils';

const EASE = [0.22, 1, 0.36, 1] as const;
const DURATION = 0.9;

/** Counts up from the last shown value to `target`, or jumps straight there when motion is reduced. */
function useCountUp(target: number, animated: boolean): number {
  const [value, setValue] = useState(animated ? 0 : target);
  const shown = useRef(animated ? 0 : target);

  useEffect(() => {
    if (!animated) {
      shown.current = target;
      setValue(target);
      return;
    }
    const controls = animate(shown.current, target, {
      duration: DURATION,
      ease: EASE,
      onUpdate: (latest) => {
        shown.current = latest;
        setValue(Math.round(latest));
      },
    });
    return () => controls.stop();
  }, [target, animated]);

  return value;
}

interface BarProps {
  percent: number;
  label: string;
  thick?: boolean;
  animated: boolean;
}

function Bar({ percent, label, thick, animated }: BarProps) {
  const clamped = Math.max(0, Math.min(100, percent));

  return (
    <div
      role="progressbar"
      aria-label={label}
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={Math.round(clamped)}
      className={cn('w-full overflow-hidden rounded-full bg-muted', thick ? 'h-3' : 'h-1.5')}
    >
      <motion.div
        className="h-full rounded-full bg-emerald-500"
        initial={animated ? { width: 0 } : false}
        animate={{ width: `${clamped}%` }}
        transition={animated ? { duration: DURATION, ease: EASE } : { duration: 0 }}
      />
    </div>
  );
}

interface PoolProgressProps {
  currentQuantity: number;
  minimumQuantity: number;
  participantCount: number;
  minimumMerchants: number;
  /** A smaller version for lists. */
  compact?: boolean;
}

export function PoolProgress({ currentQuantity, minimumQuantity, participantCount, minimumMerchants, compact }: PoolProgressProps) {
  const animated = !useReducedMotion();
  const unitsShown = useCountUp(currentQuantity, animated);

  const unitsMet = currentQuantity >= minimumQuantity;
  const merchantsMet = participantCount >= minimumMerchants;
  const unitsPercent = minimumQuantity > 0 ? (currentQuantity / minimumQuantity) * 100 : 100;
  const merchantsPercent = minimumMerchants > 0 ? (participantCount / minimumMerchants) * 100 : 100;
  const unitsNeeded = Math.max(0, minimumQuantity - currentQuantity);

  if (compact) {
    return (
      <div className="space-y-1.5">
        <div className="flex items-center justify-between text-xs">
          <span className="font-mono tabular-nums">
            {formatUnits(currentQuantity)} / {formatUnits(minimumQuantity)} units
          </span>
          <span className="font-mono tabular-nums text-muted-foreground">
            {participantCount} / {minimumMerchants} merchants
          </span>
        </div>
        <Bar percent={unitsPercent} label="Units towards the minimum" animated={animated} />
      </div>
    );
  }

  return (
    <section aria-label="Pool progress" className="space-y-5">
      <div>
        <div className="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between sm:gap-4">
          <div>
            <p className="text-xs uppercase tracking-wide text-muted-foreground">Units</p>
            <p className="whitespace-nowrap font-mono text-3xl font-medium tabular-nums">
              {formatUnits(unitsShown)} <span className="text-muted-foreground">/ {formatUnits(minimumQuantity)}</span>
            </p>
          </div>
          <p className={cn('flex items-center gap-1.5 text-sm', unitsMet ? 'text-emerald-400' : 'text-muted-foreground')}>
            {unitsMet ? <Check className="size-4" aria-hidden /> : null}
            {unitsMet ? 'Minimum reached' : `${formatUnits(unitsNeeded)} more units needed`}
          </p>
        </div>
        <div className="mt-3">
          <Bar percent={unitsPercent} label="Units towards the minimum" thick animated={animated} />
        </div>
      </div>

      <div>
        <div className="mb-2 flex items-center justify-between text-sm">
          <span className="text-muted-foreground">Merchants</span>
          <span className={cn('flex items-center gap-1.5 font-mono tabular-nums', merchantsMet && 'text-emerald-400')}>
            {merchantsMet ? <Check className="size-4" aria-hidden /> : null}
            {participantCount} / {minimumMerchants}
          </span>
        </div>
        <Bar percent={merchantsPercent} label="Merchants towards the minimum" animated={animated} />
      </div>
    </section>
  );
}
