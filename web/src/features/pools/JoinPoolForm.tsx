import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2 } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Field } from '@/components/Field';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { formatUnits, formatZAR } from '@/lib/format';
import { positiveInteger } from '@/lib/schemas';
import { currentTierIndex, discountedPrice, sortTiers } from '@/lib/tiers';
import type { BulkPoolDto, ProductDto } from '@/types/api';
import { useJoinPool } from './hooks';

const schema = z.object({ quantity: positiveInteger() });
type JoinValues = z.infer<typeof schema>;

export function JoinPoolForm({ pool, product }: { pool: BulkPoolDto; product?: ProductDto }) {
  const join = useJoinPool(pool.poolId);
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<JoinValues>({ resolver: zodResolver(schema), defaultValues: { quantity: '10' } });

  const parsed = schema.safeParse({ quantity: watch('quantity') });
  const projectedTotal = pool.currentQuantity + (parsed.success ? Number(parsed.data.quantity) : 0);

  // What the unit price would be if the pool ended up at the size it has after this merchant joins.
  const tiers = product ? sortTiers(product.discountTiers) : [];
  const tier = tiers[currentTierIndex(tiers, projectedTotal)];

  const onSubmit = handleSubmit(({ quantity }) => {
    join.mutate({ quantity: Number(quantity) });
  });

  return (
    <form onSubmit={onSubmit} className="space-y-3" noValidate>
      <Field label="Your quantity" htmlFor="quantity" error={errors.quantity?.message}>
        <Input id="quantity" type="number" inputMode="numeric" min={1} className="font-mono tabular-nums" {...register('quantity')} />
      </Field>

      {parsed.success ? (
        <p className="text-xs text-muted-foreground">
          The pool would reach <span className="font-mono tabular-nums text-foreground">{formatUnits(projectedTotal)}</span> units
          {tier && product ? (
            <>
              , a <span className="text-foreground">{tier.discountPercentage}%</span> discount at{' '}
              <span className="font-mono tabular-nums text-foreground">{formatZAR(discountedPrice(product.basePrice, tier.discountPercentage))}</span> a unit.
            </>
          ) : (
            '.'
          )}
        </p>
      ) : null}

      <Button type="submit" className="w-full" disabled={join.isPending}>
        {join.isPending ? <Loader2 className="animate-spin" /> : null}
        Join pool
      </Button>
    </form>
  );
}
