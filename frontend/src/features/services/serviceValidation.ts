import { z } from 'zod';

export const serviceFormSchema = z.object({
  description: z.string().trim().max(1000, 'La descripcion es demasiado larga.').optional(),
  durationMinutes: z
    .number({ error: 'Introduce la duracion en minutos.' })
    .int('La duracion debe ser un numero entero.')
    .min(1, 'La duracion minima es 1 minuto.')
    .max(1440, 'La duracion maxima es 1440 minutos.'),
  name: z.string().trim().min(1, 'Introduce el nombre del service.').max(150, 'El nombre es demasiado largo.'),
  priceAmount: z
    .number({ error: 'Introduce el precio.' })
    .min(0, 'El precio no puede ser negativo.')
    .max(999999.99, 'El precio es demasiado alto.'),
  sortOrder: z.number({ error: 'Introduce el orden.' }).int('El orden debe ser un numero entero.'),
});

export type ServiceFormValues = z.infer<typeof serviceFormSchema>;

export function emptyToNull(value?: string | null) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
