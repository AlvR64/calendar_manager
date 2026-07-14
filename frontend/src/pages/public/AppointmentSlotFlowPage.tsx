import { useParams } from 'react-router-dom';

import { PagePlaceholder } from '@/components/PagePlaceholder';

export function AppointmentSlotFlowPage() {
  const { slug } = useParams();

  return (
    <PagePlaceholder
      description={`Flujo publico de seleccion de appointment para /b/${slug ?? ':slug'}/appointment. Implementar usando designs/public-appointment-slot-flow.op.`}
      eyebrow="Public appointment"
      title="Select appointment slot"
    />
  );
}
