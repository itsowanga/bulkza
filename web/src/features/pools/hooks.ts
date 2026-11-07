import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { api, withQuery } from '@/lib/api';
import { nextPageParam } from '@/lib/paging';
import type {
  BulkPoolDto,
  CreatePoolRequest,
  CreatePoolResult,
  JoinPoolRequest,
  PagedResult,
  PoolQuantityResult,
  PoolStatusResult,
} from '@/types/api';

export const POOLS_PAGE_SIZE = 20;

export function usePool(poolId: string | undefined) {
  return useQuery({
    queryKey: ['pool', poolId],
    queryFn: ({ signal }) => api.get<BulkPoolDto>(`/api/bulkpools/${poolId}`, signal),
    enabled: Boolean(poolId),
  });
}

/** The pools the signed-in merchant created or joined. */
export function useMyPools() {
  return useInfiniteQuery({
    queryKey: ['pools', 'mine'],
    initialPageParam: 1,
    queryFn: ({ pageParam, signal }) =>
      api.get<PagedResult<BulkPoolDto>>(withQuery('/api/bulkpools/mine', { page: pageParam, pageSize: POOLS_PAGE_SIZE }), signal),
    getNextPageParam: nextPageParam,
  });
}

/** Open pools for a product that any merchant can join. */
export function useOpenPools(productId: string | undefined) {
  return useQuery({
    queryKey: ['pools', 'open', productId],
    queryFn: ({ signal }) =>
      api.get<PagedResult<BulkPoolDto>>(withQuery('/api/bulkpools/open', { productId, page: 1, pageSize: POOLS_PAGE_SIZE }), signal),
    enabled: Boolean(productId),
  });
}

function useRefreshPools() {
  const queryClient = useQueryClient();
  return (poolId?: string) =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: ['pools'] }),
      poolId ? queryClient.invalidateQueries({ queryKey: ['pool', poolId] }) : Promise.resolve(),
    ]);
}

export function useCreatePool() {
  const refresh = useRefreshPools();
  return useMutation({
    mutationFn: (request: CreatePoolRequest) => api.post<CreatePoolResult>('/api/bulkpools', request),
    onSuccess: (result) => refresh(result.poolId),
  });
}

export function useJoinPool(poolId: string) {
  const refresh = useRefreshPools();
  return useMutation({
    mutationFn: (request: JoinPoolRequest) => api.post<PoolQuantityResult>(`/api/bulkpools/${poolId}/join`, request),
    onSuccess: () => {
      toast.success('You joined the pool');
      return refresh(poolId);
    },
  });
}

export function useLeavePool(poolId: string) {
  const refresh = useRefreshPools();
  return useMutation({
    mutationFn: () => api.post<PoolQuantityResult>(`/api/bulkpools/${poolId}/leave`),
    onSuccess: () => {
      toast.success('You left the pool');
      return refresh(poolId);
    },
  });
}

export type PoolAdminAction = 'lock' | 'finalise' | 'expire' | 'cancel';

const ADMIN_ACTION_MESSAGES: Record<PoolAdminAction, string> = {
  lock: 'Pool locked',
  finalise: 'Pool finalised. Orders were created for the participants.',
  expire: 'Pool expired',
  cancel: 'Pool cancelled',
};

/** Admin only. The API answers 403 for anyone else. */
export function usePoolAdminAction(poolId: string) {
  const queryClient = useQueryClient();
  const refresh = useRefreshPools();

  return useMutation({
    mutationFn: (action: PoolAdminAction) => api.post<PoolStatusResult>(`/api/bulkpools/${poolId}/${action}`),
    onSuccess: async (_result, action) => {
      toast.success(ADMIN_ACTION_MESSAGES[action]);
      await refresh(poolId);
      // Finalising a pool creates one order per participant.
      if (action === 'finalise') await queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
}
