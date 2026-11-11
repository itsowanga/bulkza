import { ArrowLeft, Plus, ShoppingCart, Users } from 'lucide-react';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { Money } from '@/components/Money';
import { QuantityStepper } from '@/components/QuantityStepper';
import { TierLadder } from '@/components/TierLadder';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useAddToCart } from '@/features/cart/hooks';
import { PoolCard } from '@/features/pools/PoolCard';
import { useOpenPools } from '@/features/pools/hooks';
import { ApiError } from '@/lib/api';
import { categoryLabel, unitLabel } from '@/lib/format';
import { useProduct } from './hooks';

function OpenPools({ productId, productName }: { productId: string; productName: string }) {
  const pools = useOpenPools(productId);

  return (
    <section aria-labelledby="open-pools">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 id="open-pools" className="text-lg font-medium">
          Open pools
        </h2>
        <Button asChild variant="outline" size="sm">
          <Link to={`/pools/new?productId=${productId}`}>
            <Plus /> Start a pool
          </Link>
        </Button>
      </div>

      {pools.isPending ? <Skeleton className="h-40 rounded-xl" /> : null}
      {pools.isError ? <ErrorState message={pools.error.message} onRetry={() => void pools.refetch()} /> : null}
      {pools.data && pools.data.items.length === 0 ? (
        <EmptyState
          icon={Users}
          title="No open pools yet"
          description="Be the first. Start a pool and other merchants can join it to unlock a better price."
        />
      ) : null}
      {pools.data && pools.data.items.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {pools.data.items.map((pool) => (
            <PoolCard key={pool.poolId} pool={pool} productName={productName} />
          ))}
        </div>
      ) : null}
    </section>
  );
}

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const product = useProduct(id);
  const addToCart = useAddToCart();
  const [quantity, setQuantity] = useState(1);

  if (product.isPending) {
    return (
      <div className="space-y-6" aria-busy>
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 rounded-xl" />
        <Skeleton className="h-40 rounded-xl" />
      </div>
    );
  }

  if (product.isError) {
    return product.error instanceof ApiError && product.error.status === 404 ? (
      <EmptyState
        icon={ArrowLeft}
        title="Product not found"
        description="It may have been removed."
        action={
          <Button asChild variant="outline">
            <Link to="/products">Back to products</Link>
          </Button>
        }
      />
    ) : (
      <ErrorState message={product.error.message} onRetry={() => void product.refetch()} />
    );
  }

  const item = product.data;

  return (
    <div className="space-y-10">
      <div>
        <Link to="/products" className="mb-4 inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="size-4" aria-hidden /> Products
        </Link>
        <div className="flex flex-col gap-6 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">{item.name}</h1>
            <p className="mt-1 text-sm text-muted-foreground">
              <span className="font-mono">{item.sku}</span> · {categoryLabel(item.category)}
            </p>
            <p className="mt-4 flex items-baseline gap-1.5">
              <Money amount={item.basePrice} className="text-3xl font-medium" />
              <span className="text-sm text-muted-foreground">/ {unitLabel(item.unitOfMeasure)}</span>
            </p>
          </div>

          <div className="rounded-xl border border-border bg-card p-4">
            <p className="mb-3 text-sm font-medium">Add to cart</p>
            <div className="flex items-center gap-3">
              <QuantityStepper value={quantity} onChange={setQuantity} label="Quantity to add" disabled={addToCart.isPending} />
              <Button
                disabled={addToCart.isPending}
                onClick={() =>
                  addToCart.mutate(
                    { productId: item.productId, quantity },
                    { onSuccess: () => toast.success(`Added ${quantity} × ${item.name} to your cart`) },
                  )
                }
              >
                <ShoppingCart /> Add
              </Button>
            </div>
          </div>
        </div>
      </div>

      <section aria-labelledby="ladder">
        <h2 id="ladder" className="mb-1 text-lg font-medium">
          Volume prices
        </h2>
        <p className="mb-4 text-sm text-muted-foreground">The more units the whole pool reaches, the lower everyone's unit price.</p>
        <TierLadder tiers={item.discountTiers} basePrice={item.basePrice} />
      </section>

      <OpenPools productId={item.productId} productName={item.name} />
    </div>
  );
}
