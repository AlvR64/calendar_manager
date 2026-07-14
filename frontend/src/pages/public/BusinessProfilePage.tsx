import { useParams } from 'react-router-dom';

import { PagePlaceholder } from '@/components/PagePlaceholder';

export function BusinessProfilePage() {
  const { slug } = useParams();

  return (
    <PagePlaceholder
      description={`Perfil publico para /b/${slug ?? ':slug'}. Implementar usando designs/business-public-profile.op.`}
      eyebrow="Public business"
      title="Business profile"
    />
  );
}
