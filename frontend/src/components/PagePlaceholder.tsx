type PagePlaceholderProps = {
  description: string;
  eyebrow: string;
  title: string;
};

export function PagePlaceholder({ description, eyebrow, title }: PagePlaceholderProps) {
  return (
    <section className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-5xl items-center px-6 py-12">
      <div className="rounded-[2rem] border border-slate-200 bg-white p-8 shadow-sm md:p-12">
        <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">{eyebrow}</p>
        <h1 className="mt-4 text-4xl font-black tracking-tight text-slate-950 md:text-6xl">{title}</h1>
        <p className="mt-5 max-w-2xl text-base leading-7 text-slate-600">{description}</p>
      </div>
    </section>
  );
}
