import { useQuery } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { Link, useParams } from 'react-router-dom';

import type { BusinessProfileResponse, BusinessServiceResponse, BusinessStaffMemberResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { ApiError } from '@/api/httpClient';
import { ApiErrorAlert } from '@/features/auth/authUi';
import { getMarketplaceCategoryLabel } from '@/features/publicBusiness/marketplaceSearch';
import { getPublicBusinessProfileBySlug } from '@/features/publicBusiness/publicBusinessApi';
import { routes } from '@/lib/routes';

const businessProfileQueryKey = (slug: string) => ['public', 'business-profile', slug] as const;

export function BusinessProfilePage() {
  const { slug } = useParams();
  const profileQuery = useQuery({
    enabled: Boolean(slug),
    queryFn: () => getPublicBusinessProfileBySlug(slug!),
    queryKey: businessProfileQueryKey(slug ?? 'missing'),
  });

  if (!slug) {
    return <NotFoundState />;
  }

  if (profileQuery.isPending) {
    return <ProfileSkeleton />;
  }

  if (profileQuery.isError) {
    if (profileQuery.error instanceof ApiError && profileQuery.error.status === 404) {
      return <NotFoundState />;
    }

    return (
      <PublicProfileShell>
        <ApiErrorAlert message={getApiErrorMessage(profileQuery.error)} />
      </PublicProfileShell>
    );
  }

  return <BusinessProfile profile={profileQuery.data} slug={slug} />;
}

function BusinessProfile({ profile, slug }: { profile: BusinessProfileResponse; slug: string }) {
  const { assignments, business, services, staffMembers } = profile;
  const address = formatAddress(business);
  const categoryLabel = getMarketplaceCategoryLabel(business.category);

  return (
    <PublicProfileShell>
      <section className="grid gap-8 lg:grid-cols-[1fr_360px] lg:items-start">
        <div className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-2xl shadow-indigo-100/70 backdrop-blur md:p-8">
          <Link className="text-sm font-black text-indigo-700 hover:text-indigo-900" to={routes.home}>
            Calendar Manager
          </Link>
          <div className="mt-12 flex flex-wrap items-center gap-3">
            <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Public business</p>
            {categoryLabel ? <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs font-black text-indigo-700">{categoryLabel}</span> : null}
          </div>
          <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight text-slate-950 md:text-6xl">{business.name}</h1>
          <p className="mt-5 max-w-3xl text-lg leading-8 text-slate-600">
            {business.description || 'Consulta services activos, staff members disponibles y prepara tu proximo appointment.'}
          </p>
          <div className="mt-7 flex flex-col gap-3 sm:flex-row">
            <Link className="rounded-2xl bg-indigo-600 px-6 py-4 text-center text-base font-black text-white shadow-xl shadow-indigo-200 transition hover:bg-indigo-700" to={routes.appointmentSlotFlow(slug)}>
              Elegir appointment
            </Link>
            {business.websiteUrl ? (
              <a className="rounded-2xl border border-slate-200 bg-white px-6 py-4 text-center text-base font-black text-slate-800 shadow-sm transition hover:bg-slate-50" href={business.websiteUrl} rel="noreferrer" target="_blank">
                Web del business
              </a>
            ) : null}
          </div>
        </div>

        <aside className="rounded-[2rem] border border-slate-200 bg-slate-950 p-6 text-white shadow-2xl shadow-slate-200 md:p-7">
          <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-200">Info</p>
          <dl className="mt-5 grid gap-4 text-sm">
            <InfoRow label="Timezone" value={business.timeZoneId} />
            {categoryLabel ? <InfoRow label="Categoria" value={categoryLabel} /> : null}
            <InfoRow label="Booking window" value={`${business.maxAdvanceBookingDays} dias`} />
            <InfoRow label="Currency" value={business.currencyCode} />
            {address ? <InfoRow label="Direccion" value={address} /> : null}
            {business.contactEmail ? <InfoRow label="Email" value={business.contactEmail} /> : null}
            {business.contactPhoneNumber ? <InfoRow label="Telefono" value={business.contactPhoneNumber} /> : null}
          </dl>
        </aside>
      </section>

      <section className="mt-8 grid gap-8 xl:grid-cols-[1.15fr_0.85fr]">
        <div className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
          <SectionHeader description="Services activos publicados por este business." eyebrow="Services" title="Elige que necesitas" />
          {services.length === 0 ? (
            <EmptyPanel description="Este business todavia no publico services activos." title="Sin services disponibles" />
          ) : (
            <div className="mt-5 grid gap-4">
              {services.map((service) => (
                <ServiceCard assignments={assignments} currencyCode={business.currencyCode} key={service.id} service={service} staffMembers={staffMembers} slug={slug} />
              ))}
            </div>
          )}
        </div>

        <div className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
          <SectionHeader description="Staff members activos visibles para customers." eyebrow="Staff" title="Conoce al equipo" />
          {staffMembers.length === 0 ? (
            <EmptyPanel description="Este business todavia no publico staff members activos." title="Sin staff members" />
          ) : (
            <div className="mt-5 grid gap-4">
              {staffMembers.map((staffMember) => (
                <StaffCard assignments={assignments} key={staffMember.id} services={services} staffMember={staffMember} />
              ))}
            </div>
          )}
        </div>
      </section>
    </PublicProfileShell>
  );
}

function PublicProfileShell({ children }: { children: ReactNode }) {
  return <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#e0e7ff,transparent_32%),linear-gradient(135deg,#ffffff_0%,#f8fafc_55%,#eef2ff_100%)] px-6 py-8 text-slate-950"><div className="mx-auto max-w-6xl">{children}</div></main>;
}

function ProfileSkeleton() {
  return (
    <PublicProfileShell>
      <div className="grid gap-8 lg:grid-cols-[1fr_360px]">
        <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
      <div className="mt-8 grid gap-8 xl:grid-cols-[1.15fr_0.85fr]">
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </PublicProfileShell>
  );
}

function NotFoundState() {
  return (
    <PublicProfileShell>
      <div className="rounded-[2rem] border border-slate-200 bg-white p-10 text-center shadow-sm">
        <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Business no encontrado</p>
        <h1 className="mt-4 text-4xl font-black tracking-tight text-slate-950">No encontramos este business</h1>
        <p className="mx-auto mt-4 max-w-xl text-slate-600">Puede que el slug no exista o que el business no este activo publicamente.</p>
        <Link className="mt-7 inline-flex rounded-2xl bg-slate-950 px-6 py-3.5 text-base font-black text-white hover:bg-indigo-700" to={routes.home}>
          Volver al inicio
        </Link>
      </div>
    </PublicProfileShell>
  );
}

function ServiceCard({ assignments, currencyCode, service, slug, staffMembers }: { assignments: BusinessProfileResponse['assignments']; currencyCode: string; service: BusinessServiceResponse; slug: string; staffMembers: BusinessStaffMemberResponse[] }) {
  const assignedStaffMembers = staffMembers.filter((staffMember) => assignments.some((assignment) => assignment.serviceId === service.id && assignment.staffMemberId === staffMember.id));

  return (
    <article className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h3 className="text-xl font-black">{service.name}</h3>
          {service.description ? <p className="mt-2 text-sm leading-6 text-slate-600">{service.description}</p> : null}
          <div className="mt-4 flex flex-wrap gap-2 text-xs font-black text-slate-700">
            <span className="rounded-full bg-white px-3 py-1 shadow-sm">{service.durationMinutes} min</span>
            <span className="rounded-full bg-white px-3 py-1 shadow-sm">{formatCurrency(service.priceAmount, currencyCode)}</span>
          </div>
        </div>
        <Link className="rounded-2xl bg-slate-950 px-4 py-3 text-center text-sm font-black text-white hover:bg-indigo-700" to={routes.appointmentSlotFlow(slug)}>
          Ver slots
        </Link>
      </div>
      <p className="mt-4 text-sm font-bold text-slate-500">
        Staff: {assignedStaffMembers.length > 0 ? assignedStaffMembers.map((staffMember) => staffMember.displayName).join(', ') : 'Sin staff asignado publicamente'}
      </p>
    </article>
  );
}

function StaffCard({ assignments, services, staffMember }: { assignments: BusinessProfileResponse['assignments']; services: BusinessServiceResponse[]; staffMember: BusinessStaffMemberResponse }) {
  const assignedServices = services.filter((service) => assignments.some((assignment) => assignment.staffMemberId === staffMember.id && assignment.serviceId === service.id));

  return (
    <article className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-5">
      <div className="flex items-start gap-4">
        <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-indigo-100 text-lg font-black text-indigo-700">{initials(staffMember.displayName)}</div>
        <div>
          <h3 className="text-lg font-black">{staffMember.displayName}</h3>
          {staffMember.bio ? <p className="mt-1 text-sm leading-6 text-slate-600">{staffMember.bio}</p> : null}
          <p className="mt-3 text-sm font-bold text-slate-500">
            Services: {assignedServices.length > 0 ? assignedServices.map((service) => service.name).join(', ') : 'Sin services asignados'}
          </p>
        </div>
      </div>
    </article>
  );
}

function SectionHeader({ description, eyebrow, title }: { description: string; eyebrow: string; title: string }) {
  return (
    <div className="border-b border-slate-100 pb-5">
      <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">{eyebrow}</p>
      <h2 className="mt-2 text-2xl font-black tracking-tight">{title}</h2>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function EmptyPanel({ description, title }: { description: string; title: string }) {
  return (
    <div className="mt-5 rounded-[1.5rem] border border-dashed border-slate-300 bg-slate-50 p-6 text-center">
      <h3 className="text-lg font-black">{title}</h3>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="font-black text-indigo-200">{label}</dt>
      <dd className="mt-1 font-semibold text-white/90">{value}</dd>
    </div>
  );
}

function formatAddress(business: BusinessProfileResponse['business']) {
  return [business.addressLine1, business.addressLine2, business.postalCode, business.city, business.countryCode].filter(Boolean).join(', ');
}

function formatCurrency(amount: number, currencyCode: string) {
  return new Intl.NumberFormat(undefined, { currency: currencyCode, style: 'currency' }).format(amount);
}

function initials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase();
}
