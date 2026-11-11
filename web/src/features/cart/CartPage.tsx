import { ShoppingCart, Trash2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { Money } from '@/components/Money';
import { PageHeader } from '@/components/PageHeader';
import { QuantityStepper } from '@/components/QuantityStepper';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useProductsById } from '@/features/products/hooks';
import { formatUnits, unitLabel } from '@/lib/format';
import { useCart, useRemoveCartItem, useUpdateCartItem } from './hooks';

export function CartPage() {
  const cart = useCart();
  const update = useUpdateCartItem();
  const remove = useRemoveCartItem();
  const lines = cart.data?.cartItems ?? [];
  const { products } = useProductsById(lines.map((line) => line.productId));

  const busy = update.isPending || remove.isPending;
  const subtotal = lines.reduce((sum, line) => sum + (products.get(line.productId)?.basePrice ?? 0) * line.quantity, 0);
  const pricesKnown = lines.every((line) => products.has(line.productId));

  return (
    <>
      <PageHeader title="Your cart" description="Products you plan to buy through pools." />

      {cart.isPending ? (
        <div className="space-y-3" aria-busy>
          {Array.from({ length: 3 }, (_, index) => (
            <Skeleton key={index} className="h-24 rounded-xl" />
          ))}
        </div>
      ) : null}
      {cart.isError ? <ErrorState message={cart.error.message} onRetry={() => void cart.refetch()} /> : null}

      {cart.data && lines.length === 0 ? (
        <EmptyState
          icon={ShoppingCart}
          title="Your cart is empty"
          description="Add products you want, then join or start a pool to unlock volume prices."
          action={
            <Button asChild>
              <Link to="/products">Browse products</Link>
            </Button>
          }
        />
      ) : null}

      {lines.length > 0 ? (
        <div className="grid gap-6 lg:grid-cols-3">
          <ul className="space-y-3 lg:col-span-2">
            {lines.map((line) => {
              const product = products.get(line.productId);
              return (
                <li key={line.productId} className="flex flex-col gap-4 rounded-xl border border-border bg-card p-4 sm:flex-row sm:items-center">
                  <div className="min-w-0 flex-1">
                    {product ? (
                      <Link to={`/products/${product.productId}`} className="block truncate font-medium hover:underline">
                        {product.name}
                      </Link>
                    ) : (
                      <Skeleton className="h-5 w-40" />
                    )}
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      <span className="font-mono">{product?.sku ?? '…'}</span>
                      {product ? (
                        <>
                          {' '}
                          · <Money amount={product.basePrice} /> / {unitLabel(product.unitOfMeasure)}
                        </>
                      ) : null}
                    </p>
                  </div>

                  <QuantityStepper
                    value={line.quantity}
                    label={`Quantity of ${product?.name ?? 'product'}`}
                    disabled={busy}
                    onChange={(quantity) => update.mutate({ productId: line.productId, quantity })}
                  />

                  <div className="flex items-center justify-between gap-4 sm:w-40 sm:justify-end">
                    {product ? <Money amount={product.basePrice * line.quantity} className="font-medium" /> : <Skeleton className="h-5 w-20" />}
                    <Button
                      variant="ghost"
                      size="icon"
                      aria-label={`Remove ${product?.name ?? 'product'} from the cart`}
                      disabled={busy}
                      onClick={() => remove.mutate(line.productId)}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                </li>
              );
            })}
          </ul>

          <aside className="h-fit space-y-4 rounded-xl border border-border bg-card p-5 lg:sticky lg:top-20" aria-label="Order summary">
            <h2 className="text-base font-medium">Summary</h2>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Units</dt>
                <dd className="font-mono tabular-nums">{formatUnits(cart.data?.totalQuantity ?? 0)}</dd>
              </div>
              <div className="flex justify-between text-base">
                <dt>Subtotal</dt>
                <dd>{pricesKnown ? <Money amount={subtotal} className="font-medium" /> : <Skeleton className="h-5 w-20" />}</dd>
              </div>
            </dl>
            <p className="text-xs text-muted-foreground">Shown at base prices. Your final unit price depends on the size of the pool you join.</p>
            <Button asChild className="w-full">
              <Link to="/checkout">Proceed to checkout</Link>
            </Button>
          </aside>
        </div>
      ) : null}
    </>
  );
}
