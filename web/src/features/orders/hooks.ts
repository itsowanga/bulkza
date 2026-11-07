import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ApiError, api, withQuery } from '@/lib/api';
import { nextPageParam } from '@/lib/paging';
import type { CancelOrderResult, OrderDto, PagedResult, PayOrderRequest, PayOrderResult } from '@/types/api';

export const ORDERS_PAGE_SIZE = 20;

const fetchOrders = (page: number, signal?: AbortSignal) =>
  api.get<PagedResult<OrderDto>>(withQuery('/api/orders/mine', { page, pageSize: ORDERS_PAGE_SIZE }), signal);

export function useMyOrders() {
  return useInfiniteQuery({
    queryKey: ['orders', 'mine'],
    initialPageParam: 1,
    queryFn: ({ pageParam, signal }) => fetchOrders(pageParam, signal),
    getNextPageParam: nextPageParam,
  });
}

/**
 * The API has no "order by id" route, so the order is found by paging through the merchant's own orders.
 * A missing order becomes a 404 so the page can show its not-found state.
 */
export function useOrder(orderId: string | undefined) {
  return useQuery({
    queryKey: ['orders', 'detail', orderId],
    enabled: Boolean(orderId),
    queryFn: async ({ signal }) => {
      for (let page = 1; ; page++) {
        const result = await fetchOrders(page, signal);
        const found = result.items.find((order) => order.orderId === orderId);
        if (found) return found;
        if (nextPageParam(result) === undefined) throw new ApiError(404, null, 'Order not found.');
      }
    },
  });
}

export function usePayOrder(orderId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: PayOrderRequest) => api.post<PayOrderResult>(`/api/orders/${orderId}/pay`, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });
}

export function useCancelOrder(orderId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => api.post<CancelOrderResult>(`/api/orders/${orderId}/cancel`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });
}
