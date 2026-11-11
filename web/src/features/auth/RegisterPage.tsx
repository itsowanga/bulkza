import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2 } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { z } from 'zod';
import { Field } from '@/components/Field';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useAuthStore } from '@/lib/auth-store';
import { homePathFor, safeNext } from '@/lib/redirect';
import { requiredText } from '@/lib/schemas';
import { AuthLayout } from './AuthLayout';
import { useRegister } from './hooks';

const schema = z.object({
  name: requiredText('Business name', 100),
  phoneNumber: z.string().trim().min(1, 'Enter your phone number'),
  password: z.string().min(8, 'Use at least 8 characters').max(100, 'Use 100 characters or fewer'),
  street: requiredText('Street', 100),
  suburb: z.string().trim().max(60, 'Suburb must be 60 characters or fewer'),
  city: requiredText('City', 60),
  province: requiredText('Province', 60),
  postalCode: requiredText('Postal code', 10),
  country: requiredText('Country', 60),
});

type RegisterValues = z.infer<typeof schema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const registerMerchant = useRegister();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', phoneNumber: '', password: '', street: '', suburb: '', city: '', province: '', postalCode: '', country: 'South Africa' },
  });

  const onSubmit = handleSubmit(({ suburb, ...values }) => {
    registerMerchant.mutate(
      { ...values, ...(suburb ? { suburb } : {}) },
      { onSuccess: () => navigate(safeNext(params.get('next'), homePathFor(useAuthStore.getState().merchant?.role)), { replace: true }) },
    );
  });

  return (
    <AuthLayout title="Create your account" description="Register your business to join bulk pools.">
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <Field label="Business name" htmlFor="name" error={errors.name?.message}>
          <Input id="name" autoComplete="organization" {...register('name')} />
        </Field>
        <Field label="Phone number" htmlFor="phoneNumber" error={errors.phoneNumber?.message} hint="South African number, for example 082 555 0001 or +27 82 555 0001">
          <Input id="phoneNumber" type="tel" autoComplete="tel" inputMode="tel" {...register('phoneNumber')} />
        </Field>
        <Field label="Password" htmlFor="password" error={errors.password?.message} hint="At least 8 characters">
          <Input id="password" type="password" autoComplete="new-password" {...register('password')} />
        </Field>

        <fieldset className="space-y-4 border-t border-border pt-4">
          <legend className="pr-2 text-sm font-medium">Delivery address</legend>
          <Field label="Street" htmlFor="street" error={errors.street?.message}>
            <Input id="street" autoComplete="street-address" {...register('street')} />
          </Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Suburb (optional)" htmlFor="suburb" error={errors.suburb?.message}>
              <Input id="suburb" {...register('suburb')} />
            </Field>
            <Field label="City" htmlFor="city" error={errors.city?.message}>
              <Input id="city" autoComplete="address-level2" {...register('city')} />
            </Field>
            <Field label="Province" htmlFor="province" error={errors.province?.message}>
              <Input id="province" autoComplete="address-level1" {...register('province')} />
            </Field>
            <Field label="Postal code" htmlFor="postalCode" error={errors.postalCode?.message}>
              <Input id="postalCode" autoComplete="postal-code" className="font-mono" {...register('postalCode')} />
            </Field>
          </div>
          <Field label="Country" htmlFor="country" error={errors.country?.message}>
            <Input id="country" autoComplete="country-name" {...register('country')} />
          </Field>
        </fieldset>

        <Button type="submit" className="w-full" disabled={registerMerchant.isPending}>
          {registerMerchant.isPending ? <Loader2 className="animate-spin" /> : null}
          Create account
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-muted-foreground">
        Already registered?{' '}
        <Link to="/login" className="font-medium text-foreground underline-offset-4 hover:underline">
          Sign in
        </Link>
      </p>
    </AuthLayout>
  );
}
