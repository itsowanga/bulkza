import type { MerchantRole } from '@/types/api';

/** Where a signed-in user lands when no other page was asked for. Admins start on the dashboard. */
export function homePathFor(role: MerchantRole | undefined): string {
  return role === 'Admin' ? '/admin' : '/products';
}

/** Only same-app paths are followed after sign-in, so a crafted ?next= cannot send the user to another site. */
export function safeNext(next: string | null, fallback = '/products'): string {
  if (!next || !next.startsWith('/') || next.startsWith('//') || next.startsWith('/\\')) return fallback;
  if (next === '/login' || next === '/register' || next === '/') return fallback;
  return next;
}
