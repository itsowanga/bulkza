import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useAuthStore } from '@/lib/auth-store';
import type { AddToCartRequest, CartDto, UpdateQuantityRequest } from '@/types/api';

const CART_KEY = ['cart'] as const;

/** An empty cart comes back as a normal empty cart, not a 404. */
export function useCart() {
  const signedIn = useAuthStore((state) => state.token !== null);

  return useQuery({
    queryKey: CART_KEY,
    queryFn: ({ signal }) => api.get<CartDto>('/api/carts', signal),
    enabled: signedIn,
  });
}

function useCartMutation<TVariables>(call: (variables: TVariables) => Promise<CartDto>) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: call,
    onSuccess: (cart) => queryClient.setQueryData(CART_KEY, cart),
  });
}

export function useAddToCart() {
  return useCartMutation((request: AddToCartRequest) => api.post<CartDto>('/api/carts/items', request));
}

export function useUpdateCartItem() {
  return useCartMutation(({ productId, quantity }: { productId: string } & UpdateQuantityRequest) =>
    api.put<CartDto>(`/api/carts/items/${productId}`, { quantity }),
  );
}

export function useRemoveCartItem() {
  return useCartMutation((productId: string) => api.delete<CartDto>(`/api/carts/items/${productId}`));
}
