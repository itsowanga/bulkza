import { cn } from '@/lib/utils';

export interface FilterOption<T extends string> {
  /** Undefined means "no filter" (the All chip). */
  value: T | undefined;
  label: string;
  count?: number;
}

interface FilterChipsProps<T extends string> {
  label: string;
  options: readonly FilterOption<T>[];
  selected: T | undefined;
  onSelect: (value: T | undefined) => void;
}

export function FilterChips<T extends string>({ label, options, selected, onSelect }: FilterChipsProps<T>) {
  return (
    <div className="flex flex-wrap gap-2" role="group" aria-label={label}>
      {options.map((option) => {
        const active = option.value === selected;
        return (
          <button
            key={option.value ?? 'all'}
            type="button"
            aria-pressed={active}
            onClick={() => onSelect(option.value)}
            className={cn(
              'flex items-center gap-2 rounded-full border px-3 py-1 text-sm transition-colors',
              active ? 'border-foreground bg-foreground text-background' : 'border-border text-muted-foreground hover:text-foreground',
            )}
          >
            {option.label}
            {option.count !== undefined ? (
              <span className={cn('font-mono text-xs tabular-nums', active ? 'text-background/70' : 'text-muted-foreground')}>{option.count}</span>
            ) : null}
          </button>
        );
      })}
    </div>
  );
}
