import { Plus, Users } from 'lucide-react';
import { Link } from 'react-router-dom';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useProductsById } from '@/features/products/hooks';
import { flattenPages } from '@/lib/paging';
import { PoolCard } from './PoolCard';
import { useMyPools } from './hooks';

export function MyPoolsPage() {
  const query = useMyPools();
  const pools = flattenPages(query.data?.pages);
  const { products } = useProductsById(pools.map((pool) => pool.productId));

  return (
    <>
      <PageHeader
        title="My pools"
        description="Pools you started or joined."
        actions={
          <Button asChild>
            <Link to="/pools/new">
              <Plus /> New pool
            </Link>
          </Button>
        }
      />

      {query.isPending ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3" aria-busy>
          {Array.from({ length: 6 }, (_, index) => (
            <Skeleton key={index} className="h-44 rounded-xl" />
          ))}
        </div>
      ) : null}
      {query.isError ? <ErrorState message={query.error.message} onRetry={() => void query.refetch()} /> : null}

      {query.data && pools.length === 0 ? (
        <EmptyState
          icon={Users}
          title="You are not in any pools yet"
          description="Pick a product, then join an open pool or start your own."
          action={
            <Button asChild>
              <Link to="/products">Browse products</Link>
            </Button>
          }
        />
      ) : null}

      {pools.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {pools.map((pool) => (
              <PoolCard key={pool.poolId} pool={pool} productName={products.get(pool.productId)?.name} />
            ))}
          </div>
          {query.hasNextPage ? (
            <div className="mt-8 flex justify-center">
              <Button variant="outline" disabled={query.isFetchingNextPage} onClick={() => void query.fetchNextPage()}>
                {query.isFetchingNextPage ? 'Loading…' : 'Load more'}
              </Button>
            </div>
          ) : null}
        </>
      ) : null}
    </>
  );
}
