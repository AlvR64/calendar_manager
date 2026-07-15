import { z } from 'zod';

const optionalEmail = z.union([z.literal(''), z.string().trim().email('Introduce un email valido.').max(255)]);
const optionalUrl = z.union([z.literal(''), z.string().trim().url('Introduce una URL valida.').max(500)]);

export const businessDetailsSchema = z.object({
  addressLine1: z.string().trim().max(200, 'La direccion es demasiado larga.').optional(),
  addressLine2: z.string().trim().max(200, 'La direccion es demasiado larga.').optional(),
  city: z.string().trim().max(100, 'La ciudad es demasiado larga.').optional(),
  contactEmail: optionalEmail.optional(),
  contactPhoneNumber: z.string().trim().max(30, 'El telefono es demasiado largo.').optional(),
  countryCode: z
    .union([z.literal(''), z.string().trim().length(2, 'El pais debe tener 2 letras.').regex(/^[A-Za-z]{2}$/, 'Usa un codigo ISO 3166-1 alpha-2.')])
    .optional(),
  currencyCode: z
    .string()
    .trim()
    .length(3, 'La moneda debe tener 3 letras.')
    .regex(/^[A-Za-z]{3}$/, 'Usa un codigo ISO 4217, por ejemplo EUR.'),
  description: z.string().trim().max(1000, 'La descripcion es demasiado larga.').optional(),
  name: z.string().trim().min(1, 'Introduce el nombre del business.').max(150, 'El nombre es demasiado largo.'),
  postalCode: z.string().trim().max(20, 'El codigo postal es demasiado largo.').optional(),
  timeZoneId: z.string().trim().min(1, 'Introduce un timezone IANA.').max(100, 'El timezone es demasiado largo.'),
  websiteUrl: optionalUrl.optional(),
});

export const bookingWindowSchema = z.object({
  maxAdvanceBookingDays: z
    .number({ error: 'Introduce un numero de dias.' })
    .int('Introduce un numero entero.')
    .min(1, 'La ventana minima es 1 dia.')
    .max(365, 'La ventana maxima es 365 dias.'),
});

export type BookingWindowFormValues = z.infer<typeof bookingWindowSchema>;
export type BusinessDetailsFormValues = z.infer<typeof businessDetailsSchema>;

export function emptyToNull(value?: string | null) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
