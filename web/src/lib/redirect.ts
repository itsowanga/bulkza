/** Only same-app paths are followed after sign-in, so a crafted ?next= cannot send the user to another site. */
export function safeNext(next: string | null): string {
  if (!next || !next.startsWith('/') || next.startsWith('//') || next.startsWith('/\\')) return '/products';
  if (next === '/login' || next === '/register') return '/products';
  return next;
}
