import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { api, withQuery } from '@/lib/api';
import type { AdminOrderDto, AdminSummaryDto, BulkPoolDto, BulkPoolStatus, OrderStatus, OrderStatusResult, PagedResult } from '@/types/api';

export const ADMIN_PAGE_SIZE = 20;

// Every admin query key starts with 'admin', so one invalidation refreshes the whole dashboard.

export function useAdminSummary() {
  return useQuery({
    queryKey: ['admin', 'summary'],
    queryFn: ({ signal }) => api.get<AdminSummaryDto>('/api/admin/summary', signal),
  });
}

export function useAdminPools({ status, page }: { status: BulkPoolStatus | undefined; page: number }) {
  return useQuery({
    queryKey: ['admin', 'pools', { status, page }],
    queryFn: ({ signal }) =>
      api.get<PagedResult<BulkPoolDto>>(withQuery('/api/admin/pools', { status, page, pageSize: ADMIN_PAGE_SIZE }), signal),
    placeholderData: keepPreviousData,
  });
}

export function useAdminOrders({ status, page }: { status: OrderStatus | undefined; page: number }) {
  return useQuery({
    queryKey: ['admin', 'orders', { status, page }],
    queryFn: ({ signal }) =>
      api.get<PagedResult<AdminOrderDto>>(withQuery('/api/admin/orders', { status, page, pageSize: ADMIN_PAGE_SIZE }), signal),
    placeholderData: keepPreviousData,
  });
}

type OrderStep = 'ship' | 'deliver';

const ORDER_STEP_MESSAGES: Record<OrderStep, string> = {
  ship: 'Order marked as shipped',
  deliver: 'Order marked as delivered',
};

function useOrderStep(step: OrderStep) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderId: string) => api.post<OrderStatusResult>(`/api/orders/${orderId}/${step}`),
    onSuccess: async () => {
      toast.success(ORDER_STEP_MESSAGES[step]);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['admin'] }),
        queryClient.invalidateQueries({ queryKey: ['orders'] }),
      ]);
    },
  });
}

export const useShipOrder = () => useOrderStep('ship');
export const useDeliverOrder = () => useOrderStep('deliver');
