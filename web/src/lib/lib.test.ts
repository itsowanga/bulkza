import { describe, expect, it } from 'vitest';
import { timeLeft } from '@/lib/format';
import { decodeSession, isExpired } from '@/lib/jwt';
import { currentTierIndex, discountedPrice, maxDiscount, nextTier, sortTiers, unitsToNextTier } from '@/lib/tiers';
import type { DiscountTierDto } from '@/types/api';

function tokenWith(claims: Record<string, unknown>): string {
  const encode = (value: unknown) => Buffer.from(JSON.stringify(value)).toString('base64url');
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(claims)}.signature`;
}

describe('decodeSession', () => {
  it('reads the short claim names the API issues', () => {
    const session = decodeSession(tokenWith({ sub: 'm-1', name: 'Thandi', role: 'Admin', exp: 2_000_000_000 }));

    expect(session).toEqual({ merchantId: 'm-1', name: 'Thandi', role: 'Admin', expiresAt: 2_000_000_000_000 });
  });

  it('reads the long URI claim names too', () => {
    const session = decodeSession(
      tokenWith({
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'm-2',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Wholesaler',
      }),
    );

    expect(session).toMatchObject({ merchantId: 'm-2', role: 'Wholesaler', name: 'Merchant', expiresAt: null });
  });

  it('picks the highest role when several are present, and defaults unknown roles to Merchant', () => {
    expect(decodeSession(tokenWith({ sub: 'x', role: ['Merchant', 'Admin'] }))?.role).toBe('Admin');
    expect(decodeSession(tokenWith({ sub: 'x', role: 'Superuser' }))?.role).toBe('Merchant');
  });

  it('returns null for garbage or a token without an id', () => {
    expect(decodeSession('not-a-token')).toBeNull();
    expect(decodeSession('a.b.c')).toBeNull();
    expect(decodeSession(tokenWith({ name: 'No id' }))).toBeNull();
  });

  it('detects expiry', () => {
    const session = decodeSession(tokenWith({ sub: 'x', exp: 1000 }));

    expect(session && isExpired(session, 1_000_000)).toBe(true);
    expect(session && isExpired(session, 999_999)).toBe(false);
  });
});

describe('tiers', () => {
  const tiers: DiscountTierDto[] = [
    { minimumQuantity: 1, discountPercentage: 0 },
    { minimumQuantity: 50, discountPercentage: 5 },
    { minimumQuantity: 100, discountPercentage: 10 },
    { minimumQuantity: 250, discountPercentage: 15 },
  ];

  it('finds the unlocked tier at and around each boundary', () => {
    expect(currentTierIndex(tiers, 0)).toBe(-1);
    expect(currentTierIndex(tiers, 1)).toBe(0);
    expect(currentTierIndex(tiers, 49)).toBe(0);
    expect(currentTierIndex(tiers, 50)).toBe(1);
    expect(currentTierIndex(tiers, 5000)).toBe(3);
  });

  it('knows the next tier and the units still needed', () => {
    expect(nextTier(tiers, 60)?.minimumQuantity).toBe(100);
    expect(unitsToNextTier(tiers, 60)).toBe(40);
    expect(nextTier(tiers, 250)).toBeUndefined();
    expect(unitsToNextTier(tiers, 250)).toBeNull();
  });

  it('sorts without mutating the input', () => {
    const shuffled = [tiers[2]!, tiers[0]!, tiers[3]!, tiers[1]!];
    expect(sortTiers(shuffled).map((t) => t.minimumQuantity)).toEqual([1, 50, 100, 250]);
    expect(shuffled[0]?.minimumQuantity).toBe(100);
  });

  it('rounds discounted prices to cents like the API', () => {
    expect(discountedPrice(100, 5)).toBe(95);
    expect(discountedPrice(99.99, 5)).toBe(94.99);
    expect(discountedPrice(0.05, 50)).toBe(0.03);
  });

  it('reports the best discount', () => {
    expect(maxDiscount(tiers)?.discountPercentage).toBe(15);
    expect(maxDiscount([])).toBeUndefined();
  });
});

describe('timeLeft', () => {
  const now = new Date('2026-11-07T12:00:00Z');
  const at = (ms: number) => new Date(now.getTime() + ms);
  const minutes = (n: number) => n * 60_000;

  it('formats days, hours and minutes', () => {
    expect(timeLeft(at(minutes(2 * 1440 + 14 * 60 + 3)), now)).toEqual({ expired: false, urgent: false, label: '2d 14h 3m left' });
  });

  it('drops units that do not apply', () => {
    expect(timeLeft(at(minutes(3 * 60 + 12)), now).label).toBe('3h 12m left');
    expect(timeLeft(at(minutes(12)), now).label).toBe('12m left');
  });

  it('is urgent under 24 hours and not at exactly 24 hours', () => {
    expect(timeLeft(at(minutes(1439)), now).urgent).toBe(true);
    expect(timeLeft(at(minutes(1440)), now).urgent).toBe(false);
  });

  it('reports a passed deadline', () => {
    expect(timeLeft(at(-minutes(5)), now)).toEqual({ expired: true, urgent: true, label: 'Deadline passed' });
    expect(timeLeft(now, now).expired).toBe(true);
  });
});
