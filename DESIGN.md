# Sistema de Diseño – Usaditos Hernando ("Boutique Lúdica")

Documento de referencia visual generado a partir del proyecto de Stitch **Usaditos Hernando Tienda Online** (ID: `15780360776589513001`).

---

## 1. Filosofía y Personalidad de Marca

- **Afectuoso y Cercano:** Calidez hogareña y nostalgia artesanal, combinadas con eficiencia e-commerce.
- **Lúdico y Fresco:** Toques vivos de amarillo sol y naranja mandarina sobre verdes esmeralda profundos y fondos de algodón crudo / marfil.
- **Curaduría y Confianza:** Estados transparentes de las prendas (*Como nueva*, *Muy bueno*, *Nuevo con etiqueta*), detalles claros y cuidado en cada pieza.
- **Tono y Voz (Rioplatense con Voseo):** Cercano, empático y directo (`Elegí`, `Llevate`, `Buscá`). Moneda con formato argentino (`$ 12.800,00`).

---

## 2. Paleta de Colores (Paleta "Selva")

| Nombre del Token | Valor Hex | Rol y Uso Semántico |
|---|---|---|
| **Fondo (Canvas Base)** | `#FFFAEE` | Fondo general de la aplicación (tono crema/marfil suave). |
| **Superficies (Surface)** | `#FFFFFF` | Tarjetas de producto, contenedores modales, barra de navegación, menús. |
| **Principal (Primary)** | `#0B7A5E` | Verde esmeralda para botones de acción principal (CTA), logotipo, insignias activas. |
| **Principal Hover** | `#095F4A` | Estado hover de acciones primarias. |
| **Principal Suave (Tint)** | `#D6F2E7` | Fondos de chips, contenedor del carrito, ícono de marca, selecciones suaves. |
| **Acento Naranja (Accent 1)** | `#FF8A3D` | Badge del carrito, promociones, descuentos, elementos de atención destacada. |
| **Acento Amarillo (Accent 2)** | `#FFD23F` | Destacados lúdicos, estrellas, novedades y beneficios. |
| **Texto Principal (Text Main)** | `#14342B` | Títulos, precios y texto de alto contraste (reemplaza al negro puro). |
| **Texto Secundario (Text Muted)** | `#4F6B62` | Metadatos, talles, categorías, placeholders y etiquetas secundarias. |
| **Bordes Estructurales** | `#E8E2CF` | Delicado trazo tipo avena/lino para tarjetas, inputs y separadores. |
| **Error** | `#C8453A` | Mensajes y estados de error, validaciones y cancelaciones. |

### Reglas de Contraste y Accesibilidad
- Todo botón con fondo `#0B7A5E` debe llevar texto blanco (`#FFFFFF`) (relación de contraste superior a 4.5:1).
- Insignias, botones o fondos con `#FF8A3D` o `#FFD23F` deben usar tipografía en `#14342B` (nunca texto blanco sobre amarillo o naranja claro).
- Elementos interactivos táctiles con dimensión mínima de 44 × 44 px.

---

## 3. Tipografías

Fuente cargada desde Google Fonts:
```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link href="https://fonts.googleapis.com/css2?family=DM+Sans:ital,opsz,wght@0,9..40,100..1000;1,9..40,100..1000&family=Fraunces:ital,opsz,wght@0,9..144,100..900;1,9..144,100..900&display=swap" rel="stylesheet" />
```

- **Fraunces (Serif):**
  - Títulos principales (`h1`, `h2`), títulos de sección y precios destacados.
  - Otorga calidez editorial, personalidad boutique y distinción nostálgica.
- **DM Sans (Sans-serif):**
  - Textos de cuerpo, controles de formulario, botones, menús, talles y metadata.
  - Gran legibilidad en pantallas móviles y grillas densas.

---

## 4. Elevación y Espaciado

- **Estructura en capas (Tonal Layering):** Las tarjetas `#FFFFFF` se elevan de forma natural sobre el fondo crema `#FFFAEE`.
- **Bordes sutiles:** Contenedores delimitados por borde de `1px solid #E8E2CF`.
- **Sombras cálidas:** Sombras teñidas con el neutro `#14342B` en baja opacidad:
  - Reposo / Tarjetas: `0 2px 8px -2px rgba(20, 52, 43, 0.05), 0 1px 2px rgba(20, 52, 43, 0.03)`
  - Hover / Dropdowns: `0 8px 24px -4px rgba(20, 52, 43, 0.08), 0 2px 6px -1px rgba(20, 52, 43, 0.04)`
  - Sticky / Modales: `0 16px 36px -6px rgba(20, 52, 43, 0.12)`
- **Radios de redondeo:**
  - Tarjetas y paneles: `16px` (`rounded-lg` / `border-radius: 1rem`)
  - Botones e inputs: `10px` (`border-radius: 0.625rem`)
  - Chips, badges y píldoras: `9999px` (`rounded-pill`)

---

## 5. Pantallas en Stitch y Mapeo a Componentes

| ID Pantalla | Título en Stitch | Dispositivo | Componente Blazor |
|---|---|---|---|
| `9e1322665a1647888661b7f17886a878` | Catálogo - Usaditos Hernando | Desktop | `Components/Pages/Catalogo.razor` |
| `8861000f3fcb45aa95e5634351576020` | Catálogo - Usaditos Hernando (Mobile) | Mobile | Responsive dentro de `Catalogo.razor` |
| `3d829754591c4128a98fc5b295063d93` | Detalle de producto | Desktop | `DetalleProducto.razor` *(futuro)* |
| `4baf26311ec64117b3d0e43db0b9cdd9` | Tu carrito | Desktop | `Components/Pages/Carrito.razor` |
| `86093a66d7ac43a986853fe35f456061` | Confirmar pedido | Desktop | `ConfirmarPedido.razor` *(futuro)* |
| `6f8ca592fbf643ab9fb393b9a492def7` | Pedido confirmado | Desktop | `PedidoConfirmado.razor` *(futuro)* |
| `3ddd023f03ba4b768d6b2a72eb64bca4` | Mis pedidos | Desktop | `MisPedidos.razor` *(futuro)* |
| `54bcac1ebabb4623a0442ca9d864ce88` | Detalle de pedido | Desktop | `DetallePedido.razor` *(futuro)* |
| `cc2606c50edc44b386967525f713ed7e` | Iniciar sesión | Desktop | `Components/Pages/Login.razor` |
| `3a7350867eaf4045b25c5fe17bd11e62` | Crear cuenta | Desktop | `Components/Pages/Registro.razor` |
| `0f2e0bb6e5644adcac127858bbbcd7af` | Admin - Lista de Pedidos | Desktop | `AdminPedidos.razor` *(futuro)* |
| `a2fa1d37394e49dfbfa6c10576c4445e` | Admin - Detalle de Pedido | Desktop | `AdminDetallePedido.razor` *(futuro)* |
| `cd68a1e1561b4b5fbb959f18cd405dd0` | Admin - Formulario de Producto | Desktop | `AdminProductoForm.razor` *(futuro)* |
| `3318bab1a8ff4f468e0d80564abb0acf` | Admin - Productos | Desktop | `AdminProductos.razor` *(futuro)* |
| `da8d0a39112b410cb4d0a87c3ee1c3a4` | Admin - Categorías | Desktop | `AdminCategorias.razor` *(futuro)* |
