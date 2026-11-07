import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ApiError } from '@/lib/api';
import { router } from './router';

function handleSessionExpired(): void {
  // One toast however many requests failed together.
  toast.error('Your session has expired. Please sign in again.', { id: 'session-expired' });
  const { pathname, search } = router.state.location;
  if (pathname === '/login') return;
  void router.navigate(`/login?next=${encodeURIComponent(pathname + search)}`, { replace: true });
}

/**
 * 401 with a token: the session is over, go to /login (api.ts already cleared the token).
 * 403: tell the user plainly. Anything else: show the server's problem detail.
 * A 404 from a query is shown by the page itself as a "not found" state, so it gets no toast.
 */
function reportError(error: unknown, options: { quietNotFound: boolean }): void {
  if (error instanceof ApiError) {
    if (error.status === 401 && error.hadSession) return handleSessionExpired();
    if (error.status === 403) return void toast.error("You don't have permission.");
    if (error.status === 404 && options.quietNotFound) return;
    return void toast.error(error.message);
  }
  toast.error('Something went wrong. Please try again.');
}

const isClientError = (error: unknown) => error instanceof ApiError && error.status >= 400 && error.status < 500;

export const queryClient = new QueryClient({
  queryCache: new QueryCache({ onError: (error) => reportError(error, { quietNotFound: true }) }),
  mutationCache: new MutationCache({ onError: (error) => reportError(error, { quietNotFound: false }) }),
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      // Retrying a 4xx never helps. Network and server errors get two more tries.
      retry: (failureCount, error) => !isClientError(error) && failureCount < 2,
    },
    mutations: { retry: false },
  },
});
