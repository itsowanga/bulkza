import type { PagedResult } from '@/types/api';

/** The next page number for useInfiniteQuery, or undefined when the last page has been loaded. */
export function nextPageParam<T>(last: PagedResult<T>): number | undefined {
  return last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined;
}

export function flattenPages<T>(pages: readonly PagedResult<T>[] | undefined): T[] {
  return pages?.flatMap((page) => page.items) ?? [];
}
