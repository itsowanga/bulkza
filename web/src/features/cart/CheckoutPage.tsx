import { Info } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ErrorState } from '@/components/ErrorState';
import { Money } from '@/components/Money';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useProductsById } from '@/features/products/hooks';
import { formatUnits } from '@/lib/format';
import { useCart } from './hooks';

/**
 * Placeholder. The API has no "create order" route: orders are created for every participant when an admin
 * finalises a pool, and are paid from the order page. This page only summarises the cart until a checkout flow is agreed.
 */
export function CheckoutPage() {
  const cart = useCart();
  const lines = cart.data?.cartItems ?? [];
  const { products } = useProductsById(lines.map((line) => line.productId));
  const subtotal = lines.reduce((sum, line) => sum + (products.get(line.productId)?.basePrice ?? 0) * line.quantity, 0);

  return (
    <>
      <PageHeader title="Checkout" description="Review what you plan to buy." />

      <div role="status" className="mb-8 flex gap-3 rounded-xl border border-blue-500/30 bg-blue-500/5 p-4">
        <Info className="mt-0.5 size-5 shrink-0 text-blue-300" aria-hidden />
        <div className="text-sm">
          <p className="font-medium text-blue-100">Checkout opens when your pool finalises</p>
          <p className="mt-1 text-blue-200/80">
            You pay for each order on its own page once the pool is finalised and the volume price is fixed. Until then, join or start a pool for the
            products below.
          </p>
          <div className="mt-3 flex flex-wrap gap-2">
            <Button asChild size="sm">
              <Link to="/pools">My pools</Link>
            </Button>
            <Button asChild size="sm" variant="outline">
              <Link to="/orders">My orders</Link>
            </Button>
          </div>
        </div>
      </div>

      {cart.isPending ? <Skeleton className="h-40 rounded-xl" /> : null}
      {cart.isError ? <ErrorState message={cart.error.message} onRetry={() => void cart.refetch()} /> : null}

      {cart.data ? (
        <section className="rounded-xl border border-border bg-card" aria-label="Cart summary">
          {lines.length === 0 ? (
            <p className="p-5 text-sm text-muted-foreground">Your cart is empty.</p>
          ) : (
            <>
              <ul className="divide-y divide-border">
                {lines.map((line) => {
                  const product = products.get(line.productId);
                  return (
                    <li key={line.productId} className="flex items-center justify-between gap-4 p-4 text-sm">
                      <span className="min-w-0 truncate">
                        {product?.name ?? '…'} <span className="font-mono text-muted-foreground">× {formatUnits(line.quantity)}</span>
                      </span>
                      {product ? <Money amount={product.basePrice * line.quantity} /> : <Skeleton className="h-4 w-16" />}
                    </li>
                  );
                })}
              </ul>
              <div className="flex items-center justify-between border-t border-border p-4">
                <span className="text-sm text-muted-foreground">Subtotal at base prices</span>
                <Money amount={subtotal} className="text-lg font-medium" />
              </div>
            </>
          )}
        </section>
      ) : null}
    </>
  );
}
