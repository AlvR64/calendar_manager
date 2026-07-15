import { z } from 'zod';

const timePattern = /^\d{2}:\d{2}$/;

export const weeklyAvailabilitySchema = z
  .object({
    dayOfWeek: z.number({ error: 'Selecciona un dia.' }).int().min(0).max(6),
    endTime: z.string().regex(timePattern, 'Introduce una hora de fin valida.'),
    startTime: z.string().regex(timePattern, 'Introduce una hora de inicio valida.'),
  })
  .refine((value) => value.startTime < value.endTime, {
    message: 'La hora de inicio debe ser anterior a la hora de fin.',
    path: ['endTime'],
  });

export type WeeklyAvailabilityFormValues = z.infer<typeof weeklyAvailabilitySchema>;

export const availabilityExceptionSchema = z
  .object({
    endTime: z.string().optional(),
    isClosed: z.boolean(),
    localDate: z.string().min(1, 'Selecciona una fecha.'),
    reason: z.string().trim().max(250, 'El motivo es demasiado largo.').optional(),
    startTime: z.string().optional(),
  })
  .superRefine((value, context) => {
    if (value.isClosed) {
      return;
    }

    if (!value.startTime || !timePattern.test(value.startTime)) {
      context.addIssue({ code: 'custom', message: 'Introduce una hora de inicio valida.', path: ['startTime'] });
    }

    if (!value.endTime || !timePattern.test(value.endTime)) {
      context.addIssue({ code: 'custom', message: 'Introduce una hora de fin valida.', path: ['endTime'] });
    }

    if (value.startTime && value.endTime && value.startTime >= value.endTime) {
      context.addIssue({ code: 'custom', message: 'La hora de inicio debe ser anterior a la hora de fin.', path: ['endTime'] });
    }
  });

export type AvailabilityExceptionFormValues = z.infer<typeof availabilityExceptionSchema>;

export function timeToApi(value: string) {
  return value.length === 5 ? `${value}:00` : value;
}

export function timeToInput(value?: string | null) {
  return value?.slice(0, 5) ?? '';
}

export function emptyToNull(value?: string | null) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
