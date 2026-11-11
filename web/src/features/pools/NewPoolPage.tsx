import { zodResolver } from '@hookform/resolvers/zod';
import { format } from 'date-fns';
import { Loader2 } from 'lucide-react';
import { Controller, useForm } from 'react-hook-form';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { toast } from 'sonner';
import { z } from 'zod';
import { ErrorState } from '@/components/ErrorState';
import { Field } from '@/components/Field';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useProducts } from '@/features/products/hooks';
import { positiveInteger } from '@/lib/schemas';
import { useCreatePool } from './hooks';

const schema = z.object({
  productId: z.string().min(1, 'Choose a product'),
  quantity: positiveInteger('Enter how many units you are taking'),
  minimumQuantity: positiveInteger('Enter the minimum total units'),
  minimumMerchants: positiveInteger('Enter the minimum number of merchants'),
  deadline: z
    .string()
    .min(1, 'Choose a deadline')
    .refine((value) => new Date(value).getTime() > Date.now(), 'The deadline must be in the future'),
});

type NewPoolValues = z.infer<typeof schema>;

const DEADLINE_FORMAT = "yyyy-MM-dd'T'HH:mm";
const inSevenDays = () => format(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), DEADLINE_FORMAT);

export function NewPoolPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const createPool = useCreatePool();
  // The pool form only needs a list to choose from, so one large page is enough.
  const products = useProducts({ search: '', category: null, page: 1, pageSize: 50 });

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<NewPoolValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      productId: params.get('productId') ?? '',
      quantity: '50',
      minimumQuantity: '100',
      minimumMerchants: '3',
      deadline: inSevenDays(),
    },
  });

  const onSubmit = handleSubmit((values) => {
    createPool.mutate(
      {
        productId: values.productId,
        quantity: Number(values.quantity),
        minimumQuantity: Number(values.minimumQuantity),
        minimumMerchants: Number(values.minimumMerchants),
        deadline: new Date(values.deadline).toISOString(),
      },
      {
        onSuccess: (result) => {
          toast.success('Pool created');
          navigate(`/pools/${result.poolId}`);
        },
      },
    );
  });

  return (
    <>
      <PageHeader title="Start a pool" description="Set the targets. Other merchants can join until the deadline." />

      {products.isError ? <ErrorState message={products.error.message} onRetry={() => void products.refetch()} /> : null}

      <form onSubmit={onSubmit} className="max-w-xl space-y-5" noValidate>
        <Field label="Product" htmlFor="productId" error={errors.productId?.message}>
          <Controller
            control={control}
            name="productId"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange} disabled={products.isPending}>
                <SelectTrigger id="productId" aria-label="Product">
                  <SelectValue placeholder={products.isPending ? 'Loading products…' : 'Choose a product'} />
                </SelectTrigger>
                <SelectContent>
                  {products.data?.items.map((product) => (
                    <SelectItem key={product.productId} value={product.productId}>
                      {product.name} · {product.sku}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
        </Field>

        <div className="grid gap-5 sm:grid-cols-3">
          <Field label="Your units" htmlFor="quantity" error={errors.quantity?.message} hint="What you take from the pool">
            <Input id="quantity" type="number" inputMode="numeric" min={1} className="font-mono tabular-nums" {...register('quantity')} />
          </Field>
          <Field label="Minimum units" htmlFor="minimumQuantity" error={errors.minimumQuantity?.message} hint="Total for the pool">
            <Input id="minimumQuantity" type="number" inputMode="numeric" min={1} className="font-mono tabular-nums" {...register('minimumQuantity')} />
          </Field>
          <Field label="Minimum merchants" htmlFor="minimumMerchants" error={errors.minimumMerchants?.message}>
            <Input id="minimumMerchants" type="number" inputMode="numeric" min={1} className="font-mono tabular-nums" {...register('minimumMerchants')} />
          </Field>
        </div>

        <Field label="Deadline" htmlFor="deadline" error={errors.deadline?.message} hint="Merchants can join until this time">
          <Input id="deadline" type="datetime-local" className="font-mono" {...register('deadline')} />
        </Field>

        <div className="flex gap-3">
          <Button type="submit" disabled={createPool.isPending}>
            {createPool.isPending ? <Loader2 className="animate-spin" /> : null}
            Create pool
          </Button>
          <Button type="button" variant="ghost" onClick={() => navigate(-1)}>
            Cancel
          </Button>
        </div>
      </form>
    </>
  );
}
