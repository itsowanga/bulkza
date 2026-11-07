import { Layers } from 'lucide-react';
import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { DeadlineCountdown } from '@/components/DeadlineCountdown';
import { EmptyState } from '@/components/EmptyState';
import { ErrorState } from '@/components/ErrorState';
import { FilterChips, type FilterOption } from '@/components/FilterChips';
import { PageHeader } from '@/components/PageHeader';
import { Pagination } from '@/components/Pagination';
import { PoolProgress } from '@/components/PoolProgress';
import { StatusChip } from '@/components/StatusChip';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { PoolActionButtons } from '@/features/pools/PoolActionButtons';
import { useProductsById } from '@/features/products/hooks';
import { cn } from '@/lib/utils';
import { BULK_POOL_STATUSES, type BulkPoolStatus } from '@/types/api';
import { ADMIN_PAGE_SIZE, useAdminPools, useAdminSummary } from './hooks';

const STATUS_LABELS: Record<BulkPoolStatus, string> = {
  Open: 'Open',
  ThresholdMet: 'Threshold met',
  Locked: 'Locked',
  Finalized: 'Finalised',
  Expired: 'Expired',
  Cancelled: 'Cancelled',
};

export function AdminPoolsPage() {
  const [params, setParams] = useSearchParams();
  const [page, setPage] = useState(1);

  // The filter lives in the URL so the dashboard can link straight to a filtered list.
  const status = BULK_POOL_STATUSES.find((candidate) => candidate === params.get('status'));
  const pools = useAdminPools({ status, page });
  const summary = useAdminSummary();
  const { products } = useProductsById(pools.data?.items.map((pool) => pool.productId) ?? []);

  const options: FilterOption<BulkPoolStatus>[] = [
    { value: undefined, label: 'All' },
    ...BULK_POOL_STATUSES.map((value) => ({ value, label: STATUS_LABELS[value], count: summary.data?.pools[value] })),
  ];

  const select = (value: BulkPoolStatus | undefined) => {
    setPage(1);
    setParams(value ? { status: value } : {});
  };

  return (
    <>
      <PageHeader title="Pools" description="Every pool on the platform. Lock, finalise, expire or cancel from here." />

      <div className="mb-6">
        <FilterChips label="Filter by status" options={options} selected={status} onSelect={select} />
      </div>

      {pools.isPending ? <Skeleton className="h-72 rounded-xl" aria-busy /> : null}
      {pools.isError ? <ErrorState message={pools.error.message} onRetry={() => void pools.refetch()} /> : null}

      {pools.data && pools.data.items.length === 0 ? (
        <EmptyState icon={Layers} title="No pools here" description={status ? `There are no ${STATUS_LABELS[status].toLowerCase()} pools.` : 'No pool has been created yet.'} />
      ) : null}

      {pools.data && pools.data.items.length > 0 ? (
        <>
          <div className={cn('overflow-x-auto rounded-xl border border-border bg-card', pools.isPlaceholderData && 'pointer-events-none opacity-60 transition-opacity')}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Product</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="min-w-52">Progress</TableHead>
                  <TableHead>Deadline</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {pools.data.items.map((pool) => (
                  <TableRow key={pool.poolId}>
                    <TableCell>
                      <Link to={`/pools/${pool.poolId}`} className="font-medium hover:underline">
                        {products.get(pool.productId)?.name ?? <Skeleton className="inline-block h-4 w-32 align-middle" />}
                      </Link>
                      <p className="mt-0.5 font-mono text-xs text-muted-foreground">{pool.poolId.slice(0, 8)}</p>
                    </TableCell>
                    <TableCell>
                      <StatusChip status={pool.status} />
                    </TableCell>
                    <TableCell>
                      <PoolProgress
                        compact
                        currentQuantity={pool.currentQuantity}
                        minimumQuantity={pool.minimumQuantity}
                        participantCount={pool.participantCount}
                        minimumMerchants={pool.minimumMerchants}
                      />
                    </TableCell>
                    <TableCell className="whitespace-nowrap">
                      <DeadlineCountdown deadline={pool.deadline} />
                    </TableCell>
                    <TableCell>
                      <PoolActionButtons pool={pool} onlyAvailable />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Pagination page={page} totalCount={pools.data.totalCount} pageSize={ADMIN_PAGE_SIZE} onPageChange={setPage} />
        </>
      ) : null}
    </>
  );
}
