import { z } from 'zod';

export const staffMemberFormSchema = z.object({
  bio: z.string().trim().max(1000, 'La bio es demasiado larga.').optional(),
  displayName: z.string().trim().min(1, 'Introduce el nombre del staff member.').max(150, 'El nombre es demasiado largo.'),
  email: z
    .string()
    .trim()
    .max(255, 'El email es demasiado largo.')
    .refine((value) => value === '' || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value), 'Introduce un email valido.')
    .optional(),
  phoneNumber: z.string().trim().max(30, 'El telefono es demasiado largo.').optional(),
  sortOrder: z.number({ error: 'Introduce el orden.' }).int('El orden debe ser un numero entero.'),
});

export type StaffMemberFormValues = z.infer<typeof staffMemberFormSchema>;

export function emptyToNull(value?: string | null) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
