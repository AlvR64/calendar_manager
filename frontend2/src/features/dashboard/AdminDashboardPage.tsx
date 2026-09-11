import { useQuery } from '@tanstack/react-query';
import { ArrowUpRight, CalendarDays, CircleAlert, RefreshCw } from 'lucide-react';
import { Link } from 'react-router-dom';

import type { AdminDashboardStatusCountResponse, AppointmentSummaryResponse } from '@/api/contracts';
import { getAuthSession } from '@/auth/authStorage';
import { ApiError } from '@/api/httpClient';
import { getAdminDashboardSummary } from '@/features/dashboard/dashboardApi';

export function AdminDashboardPage() {
  const session = getAuthSession('Admin');
  const summaryQuery = useQuery({
    queryFn: () => getAdminDashboardSummary(session!.accessToken),
    queryKey: ['admin', 'dashboard-summary'],
  });

  if (summaryQuery.isPending) {
    return <DashboardLoading />;
  }

  if (summaryQuery.isError) {
    return <DashboardError error={summaryQuery.error} onRetry={() => void summaryQuery.refetch()} />;
  }

  const summary = summaryQuery.data;

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <p className="dashboard-date">OPERATIONS / LIVE VIEW</p>
          <h1>Today&apos;s agenda</h1>
          <p>Keep an eye on what is next, what is booked, and where the day needs attention.</p>
        </div>
        <Link className="dashboard-primary-action" to="/admin/appointments">
          Open agenda <ArrowUpRight aria-hidden="true" size={17} />
        </Link>
      </header>

      <section aria-label="Agenda signals" className="dashboard-signals">
        <Metric label="Appointments today" value={String(summary.todayAppointmentCount)} />
        <Metric label="Estimated revenue" value={formatMoney(summary.estimatedRevenueAmount, summary.currencyCode)} />
        <Metric label="Window" value={formatDateRange(summary.rangeStartLocalDate, summary.rangeEndLocalDate)} />
      </section>

      <section className="agenda-panel" aria-labelledby="upcoming-heading">
        <div className="panel-heading">
          <div>
            <p className="panel-kicker">NEXT UP</p>
            <h2 id="upcoming-heading">Upcoming Appointments</h2>
          </div>
          <span className="agenda-count">{summary.upcomingAppointments.length} scheduled</span>
        </div>
        {summary.upcomingAppointments.length === 0 ? (
          <EmptyAgenda />
        ) : (
          <div className="agenda-list">
            {summary.upcomingAppointments.map((appointment) => <AgendaRow appointment={appointment} key={appointment.id} />)}
          </div>
        )}
        <Link className="agenda-footer-link" to="/admin/appointments">View all Appointments <ArrowUpRight aria-hidden="true" size={16} /></Link>
      </section>

      <section className="status-panel" aria-labelledby="status-heading">
        <div className="panel-heading">
          <div>
            <p className="panel-kicker">RANGE STATUS</p>
            <h2 id="status-heading">Appointment status</h2>
          </div>
        </div>
        {summary.statusCounts.length === 0 ? (
          <p className="status-empty">No Appointments in this window yet.</p>
        ) : (
          <div className="status-grid">
            {summary.statusCounts.map((status) => <StatusMetric key={status.status} status={status} />)}
          </div>
        )}
      </section>
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return <div className="signal"><span>{label}</span><strong>{value}</strong></div>;
}

function AgendaRow({ appointment }: { appointment: AppointmentSummaryResponse }) {
  const customerName = [appointment.customer.firstName, appointment.customer.lastName].filter(Boolean).join(' ') || appointment.customer.email;

  return (
    <article className="agenda-row">
      <time className="agenda-time" dateTime={appointment.startAtUtc}>{formatTime(appointment.startTime)}<span>{formatTime(appointment.endTime)}</span></time>
      <div className="agenda-main"><h3>{appointment.service.nameSnapshot}</h3><p>{customerName} <span aria-hidden="true">/</span> {appointment.staffMember.displayName}</p></div>
      <span className={`status-badge status-${normalizeStatus(appointment.status)}`}>{formatStatus(appointment.status)}</span>
    </article>
  );
}

function StatusMetric({ status }: { status: AdminDashboardStatusCountResponse }) {
  return <div className="status-metric"><span>{formatStatus(status.status)}</span><strong>{status.count}</strong></div>;
}

function EmptyAgenda() {
  return (
    <div className="agenda-empty">
      <CalendarDays aria-hidden="true" size={22} strokeWidth={1.5} />
      <div><h3>No upcoming Appointments</h3><p>New reservations will appear here when they are scheduled.</p></div>
    </div>
  );
}

function DashboardError({ error, onRetry }: { error: Error; onRetry: () => void }) {
  const message = error instanceof ApiError && error.status === 403
    ? 'Your Admin session does not have access to this business.'
    : 'We could not load the agenda. Check your connection and try again.';

  return (
    <div className="dashboard-page dashboard-error-state" role="alert">
      <CircleAlert aria-hidden="true" size={28} />
      <h1>Agenda unavailable</h1>
      <p>{message}</p>
      <button onClick={onRetry} type="button"><RefreshCw aria-hidden="true" size={16} /> Try again</button>
    </div>
  );
}

function DashboardLoading() {
  return <div aria-label="Loading dashboard" className="dashboard-page dashboard-loading"><div className="loading-header" /><div className="loading-signals" /><div className="loading-agenda" /></div>;
}

function formatTime(value: string) { return value.slice(0, 5); }
function formatMoney(amount: number, currencyCode: string) { return new Intl.NumberFormat('en-US', { currency: currencyCode, style: 'currency' }).format(amount); }
function formatDateRange(start: string, end: string) { return start === end ? start : `${start} - ${end}`; }
function normalizeStatus(status: string) { return status.toLowerCase().replace(/[^a-z]/g, ''); }
function formatStatus(status: string) { return status.replace(/([a-z])([A-Z])/g, '$1 $2'); }
