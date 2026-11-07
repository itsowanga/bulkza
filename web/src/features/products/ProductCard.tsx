import { Link } from 'react-router-dom';
import { Money } from '@/components/Money';
import { categoryLabel, formatUnits, unitLabel } from '@/lib/format';
import { maxDiscount, sortTiers } from '@/lib/tiers';
import type { ProductDto } from '@/types/api';

/** A row of small segments, one per tier, that get stronger as the discount grows. */
function LadderPreview({ product }: { product: ProductDto }) {
  const tiers = sortTiers(product.discountTiers);
  const top = Math.max(...tiers.map((tier) => tier.discountPercentage), 1);

  return (
    <div className="mt-3 flex gap-1" aria-hidden>
      {tiers.map((tier) => (
        <span
          key={tier.minimumQuantity}
          title={`${formatUnits(tier.minimumQuantity)}+ units: ${tier.discountPercentage}% off`}
          className="h-1 flex-1 rounded-full bg-emerald-500"
          style={{ opacity: 0.15 + (tier.discountPercentage / top) * 0.85 }}
        />
      ))}
    </div>
  );
}

export function ProductCard({ product }: { product: ProductDto }) {
  const best = maxDiscount(product.discountTiers);

  return (
    <Link
      to={`/products/${product.productId}`}
      className="group block rounded-xl border border-border bg-card p-4 transition duration-150 hover:-translate-y-0.5 hover:border-slate-600 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
    >
      <h3 className="line-clamp-2 font-medium">{product.name}</h3>
      <p className="mt-1 flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-muted-foreground">
        <span className="break-all font-mono">{product.sku}</span>
        <span className="rounded-md bg-secondary px-1.5 py-0.5 text-secondary-foreground">{categoryLabel(product.category)}</span>
      </p>

      <div className="mt-4 flex items-baseline gap-1">
        <Money amount={product.basePrice} className="text-xl font-medium" />
        <span className="text-xs text-muted-foreground">/ {unitLabel(product.unitOfMeasure)}</span>
      </div>

      {best && best.discountPercentage > 0 ? (
        <p className="mt-3 text-xs text-emerald-400">
          Up to {best.discountPercentage}% off at {formatUnits(best.minimumQuantity)}+ units
        </p>
      ) : null}
      <LadderPreview product={product} />
    </Link>
  );
}
