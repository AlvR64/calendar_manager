import { z } from 'zod';

const passwordSchema = z.string().min(8, 'La password debe tener al menos 8 caracteres.').max(200, 'La password es demasiado larga.');

export const loginSchema = z.object({
  email: z.string().trim().email('Introduce un email valido.').max(255, 'El email es demasiado largo.'),
  password: z.string().min(1, 'Introduce tu password.').max(200, 'La password es demasiado larga.'),
});

export const businessRegisterSchema = z.object({
  adminDisplayName: z.string().trim().min(1, 'Introduce el nombre del admin.').max(150, 'El nombre es demasiado largo.'),
  adminEmail: z.string().trim().email('Introduce un email valido.').max(255, 'El email es demasiado largo.'),
  adminPassword: passwordSchema,
  businessName: z.string().trim().min(1, 'Introduce el nombre del business.').max(150, 'El nombre es demasiado largo.'),
  businessSlug: z
    .string()
    .trim()
    .min(1, 'Introduce un slug publico.')
    .max(120, 'El slug es demasiado largo.')
    .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, 'Usa minusculas, numeros y guiones entre palabras.'),
  currencyCode: z
    .string()
    .trim()
    .length(3, 'La moneda debe tener 3 letras.')
    .regex(/^[A-Za-z]{3}$/, 'Usa un codigo ISO 4217, por ejemplo EUR.'),
  timeZoneId: z.string().trim().min(1, 'Introduce un timezone IANA.').max(100, 'El timezone es demasiado largo.'),
});

export const customerRegisterSchema = z.object({
  email: z.string().trim().email('Introduce un email valido.').max(255, 'El email es demasiado largo.'),
  firstName: z.string().trim().min(1, 'Introduce tu nombre.').max(100, 'El nombre es demasiado largo.'),
  lastName: z.string().trim().max(100, 'El apellido es demasiado largo.').optional(),
  password: passwordSchema,
  phoneNumber: z.string().trim().max(30, 'El telefono es demasiado largo.').optional(),
});

export type BusinessRegisterFormValues = z.infer<typeof businessRegisterSchema>;
export type CustomerRegisterFormValues = z.infer<typeof customerRegisterSchema>;
export type LoginFormValues = z.infer<typeof loginSchema>;

export function emptyToNull(value?: string | null) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
