import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { api } from '@/lib/api';
import { useAuthStore } from '@/lib/auth-store';
import type { LoginMerchantRequest, LoginMerchantResult, RegisterMerchantRequest, RegisterMerchantResult } from '@/types/api';

async function loginAndStore(credentials: LoginMerchantRequest): Promise<void> {
  const { token } = await api.post<LoginMerchantResult>('/api/merchants/login', credentials);
  if (!useAuthStore.getState().signIn(token)) {
    throw new Error('The server returned a sign-in token this app cannot use.');
  }
}

// Callbacks given to mutate() do not run once the component has unmounted, and the login page unmounts the moment
// the token is stored (the route guard redirects). Anything that must happen after sign-in therefore lives here.
export function useLogin() {
  return useMutation({
    mutationFn: loginAndStore,
    // An earlier "wrong password" toast should not follow the user into the app.
    onSuccess: () => toast.dismiss(),
  });
}

/** Registers, then signs in with the same credentials. */
export function useRegister() {
  return useMutation({
    mutationFn: async (request: RegisterMerchantRequest) => {
      await api.post<RegisterMerchantResult>('/api/merchants/register', request);
      await loginAndStore({ phoneNumber: request.phoneNumber, password: request.password });
    },
    onSuccess: () => toast.dismiss(),
  });
}
