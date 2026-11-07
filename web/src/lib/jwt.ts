import { MERCHANT_ROLES, type MerchantRole } from '@/types/api';

export interface Session {
  merchantId: string;
  name: string;
  role: MerchantRole;
  /** Expiry in milliseconds since the epoch, or null when the token has no exp claim. */
  expiresAt: number | null;
}

// ASP.NET writes the same claim under a short name or a long URI depending on the handler's claim mapping.
const ID_CLAIMS = ['sub', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', 'nameid'];
const NAME_CLAIMS = ['name', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', 'unique_name'];
const ROLE_CLAIMS = ['role', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

type Claims = Record<string, unknown>;

function isRecord(value: unknown): value is Claims {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function isRole(value: unknown): value is MerchantRole {
  return typeof value === 'string' && (MERCHANT_ROLES as readonly string[]).includes(value);
}

function firstString(claims: Claims, names: string[]): string | null {
  for (const name of names) {
    const value = claims[name];
    if (typeof value === 'string' && value.length > 0) return value;
  }
  return null;
}

function readRole(claims: Claims): MerchantRole {
  for (const name of ROLE_CLAIMS) {
    const value = claims[name];
    // A user with several roles gets an array. The highest role wins.
    const roles = Array.isArray(value) ? value : [value];
    const known = roles.filter(isRole);
    if (known.includes('Admin')) return 'Admin';
    if (known.includes('Wholesaler')) return 'Wholesaler';
    if (known.length > 0) return 'Merchant';
  }
  return 'Merchant';
}

function decodePayload(token: string): Claims | null {
  const payload = token.split('.')[1];
  if (!payload) return null;

  try {
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
    const bytes = Uint8Array.from(atob(padded), (char) => char.charCodeAt(0));
    const parsed: unknown = JSON.parse(new TextDecoder().decode(bytes));
    return isRecord(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

/** Reads the merchant id, name and role out of a JWT. The signature is not checked: the API does that. */
export function decodeSession(token: string): Session | null {
  const claims = decodePayload(token);
  if (!claims) return null;

  const merchantId = firstString(claims, ID_CLAIMS);
  if (!merchantId) return null;

  const exp = claims.exp;
  return {
    merchantId,
    name: firstString(claims, NAME_CLAIMS) ?? 'Merchant',
    role: readRole(claims),
    expiresAt: typeof exp === 'number' ? exp * 1000 : null,
  };
}

export function isExpired(session: Session, now: number = Date.now()): boolean {
  return session.expiresAt !== null && session.expiresAt <= now;
}
