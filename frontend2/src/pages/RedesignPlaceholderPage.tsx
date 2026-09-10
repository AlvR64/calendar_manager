type RedesignPlaceholderPageProps = {
  area: 'Admin' | 'Customer' | 'Public';
  title: string;
};

export function RedesignPlaceholderPage({ area, title }: RedesignPlaceholderPageProps) {
  return (
    <section className="redesign-intro" aria-labelledby="page-title">
      <p className="eyebrow">{area} experience</p>
      <h1 id="page-title">{title}</h1>
      <p>
        This route is reserved for the new frontend experience. Its functional and visual scope will be defined in a frontend2 spec before implementation.
      </p>
    </section>
  );
}
