import { differenceInMinutes, format } from 'date-fns';
import type { OrderStatus, ProductCategory, UnitOfMeasure } from '@/types/api';

// One place to change if the display format should differ (the en-ZA locale prints "R 1 234,56").
const zar = new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' });
const integer = new Intl.NumberFormat('en-ZA', { maximumFractionDigits: 0 });

export function formatZAR(amount: number): string {
  return zar.format(amount);
}

export function formatUnits(quantity: number): string {
  return integer.format(quantity);
}

export function formatDateTime(value: string | Date): string {
  return format(new Date(value), 'd MMM yyyy, HH:mm');
}

export interface TimeLeft {
  expired: boolean;
  /** Less than 24 hours left. */
  urgent: boolean;
  label: string;
}

/** "2d 14h 3m left", "3h 12m left", "12m left", or "Deadline passed". Minute precision. */
export function timeLeft(deadline: string | Date, now: Date = new Date()): TimeLeft {
  const totalMinutes = differenceInMinutes(new Date(deadline), now, { roundingMethod: 'floor' });
  if (totalMinutes <= 0) return { expired: true, urgent: true, label: 'Deadline passed' };

  const days = Math.floor(totalMinutes / 1440);
  const hours = Math.floor((totalMinutes % 1440) / 60);
  const minutes = totalMinutes % 60;

  const parts: string[] = [];
  if (days > 0) parts.push(`${days}d`);
  if (days > 0 || hours > 0) parts.push(`${hours}h`);
  parts.push(`${minutes}m`);

  return { expired: false, urgent: totalMinutes < 1440, label: `${parts.join(' ')} left` };
}

const CATEGORY_LABELS: Record<ProductCategory, string> = {
  MaizeMeal: 'Maize meal',
  CookingOil: 'Cooking oil',
  Rice: 'Rice',
  Sugar: 'Sugar',
  SoftDrinks: 'Soft drinks',
  Flour: 'Flour',
};

const UNIT_LABELS: Record<UnitOfMeasure, string> = {
  Each: 'each',
  Kilogram: 'kg',
  Litre: 'litre',
  Box: 'box',
  Bag: 'bag',
};

export const categoryLabel = (category: ProductCategory): string => CATEGORY_LABELS[category];
export const unitLabel = (unit: UnitOfMeasure): string => UNIT_LABELS[unit];

/** The order of the happy path, used by the status timeline. */
export const ORDER_FLOW: readonly OrderStatus[] = ['Pending', 'Paid', 'Shipped', 'Delivered'];
