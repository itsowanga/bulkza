import { useQueryClient } from '@tanstack/react-query';
import { ChevronDown, LogOut, Menu } from 'lucide-react';
import { useState } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Logo } from '@/components/Logo';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { useCart } from '@/features/cart/hooks';
import { useAuthStore, useIsAdmin } from '@/lib/auth-store';
import { homePathFor } from '@/lib/redirect';
import { cn } from '@/lib/utils';

interface NavEntry {
  to: string;
  label: string;
  /** Match the exact path only, so /admin is not "active" on /admin/pools. */
  end?: boolean;
}

const MERCHANT_NAV: readonly NavEntry[] = [
  { to: '/products', label: 'Products' },
  { to: '/pools', label: 'My pools' },
  { to: '/cart', label: 'Cart' },
  { to: '/orders', label: 'My orders' },
];

// Admins run the platform rather than buy from it, so their navigation is the admin areas plus the catalogue.
const ADMIN_NAV: readonly NavEntry[] = [
  { to: '/admin', label: 'Overview', end: true },
  { to: '/admin/pools', label: 'Pools' },
  { to: '/admin/orders', label: 'Orders' },
  { to: '/products', label: 'Products' },
];

function AdminBadge() {
  return (
    <span className="rounded-full border border-emerald-500/40 bg-emerald-500/10 px-2 py-0.5 text-[11px] font-medium text-emerald-300">Admin</span>
  );
}

function NavItems({ onNavigate, orientation }: { onNavigate?: () => void; orientation: 'row' | 'column' }) {
  const isAdmin = useIsAdmin();
  const cart = useCart();
  const cartCount = !isAdmin ? (cart.data?.cartItems.length ?? 0) : 0;
  const items = isAdmin ? ADMIN_NAV : MERCHANT_NAV;

  return (
    <nav aria-label="Main" className={cn('flex gap-1', orientation === 'column' ? 'flex-col' : 'items-center')}>
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.end}
          onClick={onNavigate}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-2 rounded-md px-3 py-1.5 text-sm transition-colors',
              isActive ? 'bg-accent text-foreground' : 'text-muted-foreground hover:text-foreground',
            )
          }
        >
          {item.label}
          {item.to === '/cart' && cartCount > 0 ? (
            <span className="rounded-full bg-emerald-500 px-1.5 text-[11px] font-medium leading-5 text-slate-950" aria-label={`${cartCount} items`}>
              {cartCount}
            </span>
          ) : null}
        </NavLink>
      ))}
    </nav>
  );
}

function UserMenu() {
  const merchant = useAuthStore((state) => state.merchant);
  const signOut = useAuthStore((state) => state.signOut);
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  if (!merchant) return null;

  const handleSignOut = () => {
    signOut();
    // Cached data belongs to the merchant who just left.
    queryClient.clear();
    navigate('/login', { replace: true });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="sm" className="gap-2" aria-label="Account menu">
          <span className="flex size-6 items-center justify-center rounded-full bg-secondary text-xs font-medium uppercase">{merchant.name.slice(0, 1)}</span>
          <span className="hidden max-w-32 truncate sm:inline">{merchant.name}</span>
          {merchant.role === 'Admin' ? <AdminBadge /> : null}
          <ChevronDown className="size-3.5 text-muted-foreground" aria-hidden />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-52">
        <DropdownMenuLabel className="font-normal">
          <p className="truncate text-sm font-medium">{merchant.name}</p>
          <p className="text-xs text-muted-foreground">{merchant.role}</p>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem onSelect={handleSignOut}>
          <LogOut className="mr-2 size-4" aria-hidden /> Sign out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function AppShell() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const role = useAuthStore((state) => state.merchant?.role);

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-40 border-b border-border bg-background/80 backdrop-blur">
        <div className="mx-auto flex h-14 max-w-6xl items-center gap-4 px-4 sm:px-6">
          <Sheet open={drawerOpen} onOpenChange={setDrawerOpen}>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon" className="md:hidden" aria-label="Open menu">
                <Menu />
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-72">
              <SheetTitle className="sr-only">Menu</SheetTitle>
              <Logo className="mb-6" />
              <NavItems orientation="column" onNavigate={() => setDrawerOpen(false)} />
            </SheetContent>
          </Sheet>

          <Link to={homePathFor(role)} aria-label="BulkZA home">
            <Logo />
          </Link>

          <div className="ml-4 hidden md:block">
            <NavItems orientation="row" />
          </div>

          <div className="ml-auto">
            <UserMenu />
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  );
}
