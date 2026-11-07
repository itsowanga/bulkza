import type { DiscountTierDto } from '@/types/api';

export function sortTiers(tiers: readonly DiscountTierDto[]): DiscountTierDto[] {
  return [...tiers].sort((a, b) => a.minimumQuantity - b.minimumQuantity);
}

/** Index of the highest tier whose minimum is reached, or -1 when none is. Tiers must be sorted ascending. */
export function currentTierIndex(tiers: readonly DiscountTierDto[], quantity: number): number {
  let index = -1;
  tiers.forEach((tier, i) => {
    if (quantity >= tier.minimumQuantity) index = i;
  });
  return index;
}

export function nextTier(tiers: readonly DiscountTierDto[], quantity: number): DiscountTierDto | undefined {
  return tiers[currentTierIndex(tiers, quantity) + 1];
}

export function unitsToNextTier(tiers: readonly DiscountTierDto[], quantity: number): number | null {
  const next = nextTier(tiers, quantity);
  return next ? next.minimumQuantity - quantity : null;
}

/** Unit price after a percentage discount, rounded to cents (half away from zero, like the API). */
export function discountedPrice(basePrice: number, discountPercentage: number): number {
  const cents = (basePrice * (100 - discountPercentage)) / 100 * 100;
  return Math.round(cents + Number.EPSILON * Math.sign(cents)) / 100;
}

/** The best discount a product can reach, for card previews. */
export function maxDiscount(tiers: readonly DiscountTierDto[]): DiscountTierDto | undefined {
  return tiers.reduce<DiscountTierDto | undefined>(
    (best, tier) => (!best || tier.discountPercentage > best.discountPercentage ? tier : best),
    undefined,
  );
}
