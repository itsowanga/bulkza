import { Minus, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

interface QuantityStepperProps {
  value: number;
  onChange: (value: number) => void;
  min?: number;
  max?: number;
  disabled?: boolean;
  label: string;
}

export function QuantityStepper({ value, onChange, min = 1, max = 1_000_000, disabled, label }: QuantityStepperProps) {
  const set = (next: number) => {
    if (Number.isInteger(next) && next >= min && next <= max) onChange(next);
  };

  return (
    <div className="inline-flex items-center gap-1" role="group" aria-label={label}>
      <Button type="button" variant="outline" size="icon" className="size-8" disabled={disabled || value <= min} onClick={() => set(value - 1)} aria-label="Decrease quantity">
        <Minus />
      </Button>
      <Input
        type="number"
        inputMode="numeric"
        className="h-8 w-16 text-center font-mono tabular-nums [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
        value={value}
        min={min}
        max={max}
        disabled={disabled}
        aria-label={label}
        onChange={(event) => set(Number(event.target.value))}
      />
      <Button type="button" variant="outline" size="icon" className="size-8" disabled={disabled || value >= max} onClick={() => set(value + 1)} aria-label="Increase quantity">
        <Plus />
      </Button>
    </div>
  );
}
