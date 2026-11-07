import { z } from 'zod';

// Form inputs are strings. These helpers validate them and the submit handler converts with Number().

export const positiveInteger = (message = 'Enter a whole number of 1 or more') =>
  z
    .string()
    .trim()
    .min(1, message)
    .refine((value) => /^\d+$/.test(value) && Number(value) >= 1 && Number.isSafeInteger(Number(value)), message);

export const requiredText = (label: string, maxLength: number) =>
  z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .max(maxLength, `${label} must be ${maxLength} characters or fewer`);
