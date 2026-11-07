import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2 } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { z } from 'zod';
import { Field } from '@/components/Field';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { safeNext } from '@/lib/redirect';
import { AuthLayout } from './AuthLayout';
import { useLogin } from './hooks';

const schema = z.object({
  phoneNumber: z.string().trim().min(1, 'Enter your phone number'),
  password: z.string().min(8, 'Your password is at least 8 characters'),
});

type LoginValues = z.infer<typeof schema>;

export function LoginPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const login = useLogin();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginValues>({ resolver: zodResolver(schema), defaultValues: { phoneNumber: '', password: '' } });

  const onSubmit = handleSubmit((values) => {
    // A failed sign-in is reported by the global error handler as a toast.
    login.mutate(values, { onSuccess: () => navigate(safeNext(params.get('next')), { replace: true }) });
  });

  return (
    <AuthLayout title="Sign in" description="Welcome back. Pool your orders and unlock volume prices.">
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <Field label="Phone number" htmlFor="phoneNumber" error={errors.phoneNumber?.message} hint="For example 082 555 0001 or +27 82 555 0001">
          <Input id="phoneNumber" type="tel" autoComplete="tel" inputMode="tel" {...register('phoneNumber')} />
        </Field>
        <Field label="Password" htmlFor="password" error={errors.password?.message}>
          <Input id="password" type="password" autoComplete="current-password" {...register('password')} />
        </Field>
        <Button type="submit" className="w-full" disabled={login.isPending}>
          {login.isPending ? <Loader2 className="animate-spin" /> : null}
          Sign in
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-muted-foreground">
        New to BulkZA?{' '}
        <Link to="/register" className="font-medium text-foreground underline-offset-4 hover:underline">
          Create an account
        </Link>
      </p>
    </AuthLayout>
  );
}
