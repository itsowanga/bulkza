import { PackageSearch, Search } from 'lucide-react';
import { useState } from 'react';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { categoryLabel } from '@/lib/format';
import { useDebouncedValue } from '@/lib/use-now';
import { cn } from '@/lib/utils';
import { PRODUCT_CATEGORIES, type ProductCategory } from '@/types/api';
import { ProductCard } from './ProductCard';
import { PRODUCTS_PAGE_SIZE, useProducts } from './hooks';

function ProductGridSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4" aria-busy>
      {Array.from({ length: 8 }, (_, index) => (
        <Skeleton key={index} className="h-40 rounded-xl" />
      ))}
    </div>
  );
}

export function ProductsPage() {
  const [searchInput, setSearchInput] = useState('');
  const [category, setCategory] = useState<ProductCategory | null>(null);
  const [page, setPage] = useState(1);

  const search = useDebouncedValue(searchInput, 300);
  const searching = search.trim().length > 0;
  const products = useProducts({ search, category, page });

  const totalPages = products.data ? Math.max(1, Math.ceil(products.data.totalCount / PRODUCTS_PAGE_SIZE)) : 1;
  const clearFilters = () => {
    setSearchInput('');
    setCategory(null);
    setPage(1);
  };

  return (
    <>
      <PageHeader title="Products" description="Pick a product, then join or start a pool to unlock volume prices." />

      <div className="mb-6 space-y-4">
        <div className="relative max-w-md">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden />
          <Input
            type="search"
            placeholder="Search by name, e.g. Sunflower"
            aria-label="Search products by name"
            className="pl-9"
            value={searchInput}
            onChange={(event) => {
              setSearchInput(event.target.value);
              setPage(1);
            }}
          />
        </div>

        <div className="flex flex-wrap gap-2" role="group" aria-label="Filter by category">
          {[null, ...PRODUCT_CATEGORIES].map((option) => {
            const selected = category === option;
            return (
              <button
                key={option ?? 'all'}
                type="button"
                aria-pressed={selected}
                disabled={searching}
                onClick={() => {
                  setCategory(option);
                  setPage(1);
                }}
                className={cn(
                  'rounded-full border px-3 py-1 text-sm transition-colors disabled:cursor-not-allowed disabled:opacity-40',
                  selected ? 'border-foreground bg-foreground text-background' : 'border-border text-muted-foreground hover:text-foreground',
                )}
              >
                {option ? categoryLabel(option) : 'All'}
              </button>
            );
          })}
        </div>
        {searching ? <p className="text-xs text-muted-foreground">Categories apply when browsing. Clear the search to use them.</p> : null}
      </div>

      {products.isPending ? <ProductGridSkeleton /> : null}
      {products.isError ? <ErrorState message={products.error.message} onRetry={() => void products.refetch()} /> : null}

      {products.data && products.data.items.length === 0 ? (
        <EmptyState
          icon={PackageSearch}
          title="No products found"
          description={searching || category ? 'Nothing matches your search. Try different words or clear the filters.' : 'There are no active products yet.'}
          action={
            searching || category ? (
              <Button variant="outline" onClick={clearFilters}>
                Clear filters
              </Button>
            ) : undefined
          }
        />
      ) : null}

      {products.data && products.data.items.length > 0 ? (
        <>
          <div className={cn('grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4', products.isPlaceholderData && 'opacity-60 transition-opacity')}>
            {products.data.items.map((product) => (
              <ProductCard key={product.productId} product={product} />
            ))}
          </div>

          {totalPages > 1 ? (
            <nav className="mt-8 flex items-center justify-between" aria-label="Pagination">
              <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>
                Previous
              </Button>
              <span className="font-mono text-sm tabular-nums text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage((current) => current + 1)}>
                Next
              </Button>
            </nav>
          ) : null}
        </>
      ) : null}
    </>
  );
}
