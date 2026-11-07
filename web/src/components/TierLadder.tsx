import { Check } from 'lucide-react';
import { formatUnits, formatZAR } from '@/lib/format';
import { currentTierIndex, discountedPrice, nextTier, sortTiers, unitsToNextTier } from '@/lib/tiers';
import { cn } from '@/lib/utils';
import type { DiscountTierDto } from '@/types/api';

interface TierLadderProps {
  tiers: readonly DiscountTierDto[];
  /** The quantity that decides which tier is unlocked. Leave out to show the ladder without highlighting. */
  quantity?: number;
  /** When given, each tier shows the unit price it gives. */
  basePrice?: number;
}

export function TierLadder({ tiers, quantity, basePrice }: TierLadderProps) {
  const sorted = sortTiers(tiers);
  const activeIndex = quantity === undefined ? -1 : currentTierIndex(sorted, quantity);
  const remaining = quantity === undefined ? null : unitsToNextTier(sorted, quantity);
  const upcoming = quantity === undefined ? undefined : nextTier(sorted, quantity);

  return (
    <div>
      <ol className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
        {sorted.map((tier, index) => {
          const unlocked = index <= activeIndex;
          const active = index === activeIndex;

          return (
            <li
              key={tier.minimumQuantity}
              aria-current={active ? 'step' : undefined}
              className={cn(
                'rounded-lg border p-3 transition-colors',
                unlocked ? 'border-emerald-500/40 bg-emerald-500/5' : 'border-border text-muted-foreground',
                active && 'border-emerald-500 ring-1 ring-emerald-500/60',
              )}
            >
              <div className="flex items-center justify-between">
                <span className="font-mono text-xs tabular-nums">{formatUnits(tier.minimumQuantity)}+</span>
                {unlocked ? <Check className="size-3.5 text-emerald-400" aria-label="Unlocked" /> : null}
              </div>
              <div className={cn('mt-1 text-lg font-semibold', unlocked && 'text-emerald-300')}>
                {tier.discountPercentage}% off
              </div>
              {basePrice !== undefined ? (
                <div className="font-mono text-xs tabular-nums">{formatZAR(discountedPrice(basePrice, tier.discountPercentage))}</div>
              ) : null}
              {active ? <div className="mt-1 text-[11px] font-medium uppercase tracking-wide text-emerald-400">Current</div> : null}
            </li>
          );
        })}
      </ol>

      {quantity !== undefined && sorted.length > 0 ? (
        <p className="mt-3 text-sm text-muted-foreground">
          {upcoming && remaining !== null ? (
            <>
              <span className="font-mono tabular-nums text-foreground">{formatUnits(remaining)}</span> more units to unlock{' '}
              <span className="text-foreground">{upcoming.discountPercentage}%</span>
            </>
          ) : (
            'The top tier is unlocked.'
          )}
        </p>
      ) : null}
    </div>
  );
}
