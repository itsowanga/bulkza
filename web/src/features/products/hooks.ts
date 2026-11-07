import { keepPreviousData, useQueries, useQuery } from '@tanstack/react-query';
import { api, withQuery } from '@/lib/api';
import type { PagedResult, ProductCategory, ProductDto } from '@/types/api';

export const PRODUCTS_PAGE_SIZE = 12;

interface ProductFilters {
  search: string;
  category: ProductCategory | null;
  page: number;
  pageSize?: number;
}

/**
 * Browses active products, or searches by name when a search term is given.
 * The name search has no category filter, so the category only applies when browsing.
 */
export function useProducts({ search, category, page, pageSize = PRODUCTS_PAGE_SIZE }: ProductFilters) {
  const term = search.trim();

  return useQuery({
    queryKey: ['products', { term, category: term ? null : category, page, pageSize }],
    queryFn: ({ signal }) =>
      term
        ? api.get<PagedResult<ProductDto>>(withQuery('/api/products/search/name', { name: term, page, pageSize }), signal)
        : api.get<PagedResult<ProductDto>>(withQuery('/api/products/browse', { page, pageSize, category }), signal),
    placeholderData: keepPreviousData,
  });
}

export function useProduct(productId: string | undefined) {
  return useQuery({
    queryKey: ['product', productId],
    queryFn: ({ signal }) => api.get<ProductDto>(`/api/products/${productId}`, signal),
    enabled: Boolean(productId),
    staleTime: 5 * 60_000,
  });
}

/** Looks up several products at once (cart lines, order lines, pool lists) and returns them by id. */
export function useProductsById(productIds: readonly string[]) {
  const unique = [...new Set(productIds)];

  return useQueries({
    queries: unique.map((productId) => ({
      queryKey: ['product', productId],
      queryFn: ({ signal }: { signal: AbortSignal }) => api.get<ProductDto>(`/api/products/${productId}`, signal),
      staleTime: 5 * 60_000,
    })),
    combine: (results) => ({
      products: new Map(results.flatMap((result) => (result.data ? [[result.data.productId, result.data] as const] : []))),
      isLoading: results.some((result) => result.isLoading),
    }),
  });
}
