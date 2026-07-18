# Specs De Entrega

Las specs de entrega describen como implementar slices de producto sin convertirse en la fuente de verdad del estado.

`task-groups-roadmap.md` sigue siendo la fuente unica de verdad para saber si backend, frontend y diseno estan hechos. Las specs explican alcance, contratos, plan de implementacion y criterios de aceptacion para una slice.

## Reglas

- Mantener specs a nivel de producto/use-case, no separadas por frontend/backend.
- Declarar el incremento de producto en cada spec.
- Referenciar IDs de capacidad de `task-groups-roadmap.md` en cada spec.
- Una spec puede cubrir IDs de capacidad de varios grupos.
- Usar `TEMPLATE.md` al crear una nueva spec.
- Mantener specs concisas y orientadas a implementacion.
- Actualizar `task-groups-roadmap.md` cuando una spec se completa.
- No marcar `Frontend` como hecho por placeholders; marcarlo solo cuando hay UI/API real implementada y verificada.
- No marcar `Backend` como hecho hasta que API/application behavior y tests relevantes esten completos.
- No marcar `Diseno` como hecho salvo que exista el diseno OpenPencil y cubra el caso.

## Incrementos Actuales

- `first-mvp/`: baseline completada del primer MVP.
- `appointments-mvp/`: creacion y gestion real de appointments completada.
- `marketplace-discovery-mvp/`: siguiente incremento para descubrimiento publico, categorias y busqueda de businesses.
