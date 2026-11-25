using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Servicios
{
    public class AdminPedidoServicio : IAdminPedidoServicio
    {
        private readonly IAdminPedidoRepositorio _adminPedidoRepositorio;
        private readonly IPedidoServicio _pedidoServicio;

        public AdminPedidoServicio(
            IAdminPedidoRepositorio adminPedidoRepositorio,
            IPedidoServicio pedidoServicio)
        {
            _adminPedidoRepositorio = adminPedidoRepositorio;
            _pedidoServicio = pedidoServicio;
        }

        public async Task<PaginaDto<PedidoAdminResumenDto>> ObtenerPedidosAsync(PedidoAdminFiltrosDto filtros)
        {
            var filtrosValidos = NormalizarFiltros(filtros);
            var resultado = await _adminPedidoRepositorio.ListarAsync(filtrosValidos);

            return new PaginaDto<PedidoAdminResumenDto>
            {
                Items = resultado.Pedidos.Select(MapearAResumen).ToList(),
                Total = resultado.Total,
                Page = filtrosValidos.Page,
                PageSize = filtrosValidos.PageSize
            };
        }

        public async Task<PedidoAdminDto> ObtenerPedidoAsync(int pedidoId)
        {
            var pedido = await _adminPedidoRepositorio.ObtenerConDetalleAsync(pedidoId)
                ?? throw new KeyNotFoundException("El pedido no existe.");

            return MapearADetalle(pedido);
        }

        public async Task<PedidoAdminDto> CambiarEstadoAsync(int pedidoId, CambiarEstadoPedidoRequest request)
        {
            ValidarRowVersion(request.RowVersion);

            var nuevoEstado = request.NuevoEstado?.Trim() ?? string.Empty;
            if (nuevoEstado.Length == 0)
                throw new ArgumentException("El nuevo estado es obligatorio.");

            var estadosDestino = new[]
            {
                PedidoConstantes.EstadoListoParaRetirar,
                PedidoConstantes.EstadoEntregado,
                PedidoConstantes.EstadoCancelado
            };
            if (!estadosDestino.Contains(nuevoEstado))
                throw new ArgumentException("El estado destino es inválido.");

            if (nuevoEstado == PedidoConstantes.EstadoCancelado)
            {
                await _pedidoServicio.CancelarPedidoComoAdminAsync(pedidoId, request.RowVersion);
                return await ObtenerPedidoAsync(pedidoId);
            }

            var pedido = await _adminPedidoRepositorio.ObtenerParaEdicionAsync(pedidoId)
                ?? throw new KeyNotFoundException("El pedido no existe.");

            var transicionValida =
                (pedido.Estado == PedidoConstantes.EstadoConfirmado &&
                 nuevoEstado == PedidoConstantes.EstadoListoParaRetirar) ||
                (pedido.Estado == PedidoConstantes.EstadoListoParaRetirar &&
                 nuevoEstado == PedidoConstantes.EstadoEntregado);

            if (!transicionValida)
                throw new EstadoPedidoInvalidoException(
                    $"No se puede pasar de {pedido.Estado} a {nuevoEstado}.");

            if (nuevoEstado == PedidoConstantes.EstadoEntregado && pedido.EstadoPago != EstadoPago.Pagado)
                throw new EstadoPedidoInvalidoException(
                    "No se puede entregar un pedido que no está pagado.");

            pedido.Estado = nuevoEstado;
            await _adminPedidoRepositorio.EstablecerRowVersionOriginalAsync(pedido, request.RowVersion);
            await GuardarConConcurrenciaAsync();

            return MapearADetalle(pedido);
        }

        public async Task<PedidoAdminDto> MarcarPagoAsync(int pedidoId, MarcarPagoRequest request)
        {
            if (!request.EstadoPago.HasValue || !Enum.IsDefined(request.EstadoPago.Value))
                throw new ArgumentException("El estado de pago es inválido.");

            ValidarRowVersion(request.RowVersion);

            var pedido = await _adminPedidoRepositorio.ObtenerParaEdicionAsync(pedidoId)
                ?? throw new KeyNotFoundException("El pedido no existe.");

            if (pedido.Estado == PedidoConstantes.EstadoCancelado ||
                pedido.Estado == PedidoConstantes.EstadoEntregado)
                throw new EstadoPedidoInvalidoException(
                    "No se puede modificar el pago de un pedido cancelado o entregado.");

            pedido.EstadoPago = request.EstadoPago.Value;
            pedido.FechaPago = request.EstadoPago.Value == EstadoPago.Pagado
                ? DateTime.UtcNow
                : null;

            await _adminPedidoRepositorio.EstablecerRowVersionOriginalAsync(pedido, request.RowVersion);
            await GuardarConConcurrenciaAsync();

            return MapearADetalle(pedido);
        }

        private async Task GuardarConConcurrenciaAsync()
        {
            try
            {
                await _adminPedidoRepositorio.GuardarCambiosAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new PedidoModificadoException("El pedido fue modificado, recargá.");
            }
        }

        private static PedidoAdminFiltrosDto NormalizarFiltros(PedidoAdminFiltrosDto filtros)
        {
            if (filtros is null)
                filtros = new PedidoAdminFiltrosDto();

            if (filtros.Page < 1)
                throw new ArgumentException("La página debe ser mayor o igual a 1.");
            if (filtros.PageSize < 1 || filtros.PageSize > 50)
                throw new ArgumentException("El pageSize debe estar entre 1 y 50.");

            if (filtros.FechaDesde.HasValue && filtros.FechaHasta.HasValue &&
                filtros.FechaDesde.Value.Date > filtros.FechaHasta.Value.Date)
                throw new ArgumentException("La fecha desde no puede ser mayor a la fecha hasta.");

            if (filtros.MetodoPago.HasValue && !Enum.IsDefined(filtros.MetodoPago.Value))
                throw new ArgumentException("El método de pago del filtro es inválido.");

            if (!string.IsNullOrWhiteSpace(filtros.Estado))
            {
                var estado = filtros.Estado.Trim();
                var estadosValidos = new[]
                {
                    PedidoConstantes.EstadoConfirmado,
                    PedidoConstantes.EstadoListoParaRetirar,
                    PedidoConstantes.EstadoEntregado,
                    PedidoConstantes.EstadoCancelado
                };
                if (!estadosValidos.Contains(estado))
                    throw new ArgumentException("El estado del filtro es inválido.");

                filtros.Estado = estado;
            }

            return filtros;
        }

        private static void ValidarRowVersion(byte[] rowVersion)
        {
            if (rowVersion is null || rowVersion.Length == 0)
                throw new ArgumentException("El RowVersion es obligatorio. Obtenelo con GET /api/admin/pedidos/{id}.");
        }

        private static PedidoAdminResumenDto MapearAResumen(Pedido pedido)
        {
            return new PedidoAdminResumenDto
            {
                Id = pedido.Id,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                EstadoPago = pedido.EstadoPago,
                MetodoPago = pedido.MetodoPago,
                TipoEntrega = pedido.TipoEntrega,
                Total = pedido.Total,
                ClienteNombre = pedido.Cliente?.Nombre ?? string.Empty,
                TelefonoContacto = pedido.TelefonoContacto,
                CantidadItems = pedido.Items?.Sum(i => i.Cantidad) ?? 0
            };
        }

        private static PedidoAdminDto MapearADetalle(Pedido pedido)
        {
            return new PedidoAdminDto
            {
                Id = pedido.Id,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                EstadoPago = pedido.EstadoPago,
                MetodoPago = pedido.MetodoPago,
                TipoEntrega = pedido.TipoEntrega,
                TelefonoContacto = pedido.TelefonoContacto,
                DireccionEnvio = pedido.DireccionEnvio,
                NotaCliente = pedido.NotaCliente,
                FechaPago = pedido.FechaPago,
                Total = pedido.Total,
                ClienteNombre = pedido.Cliente?.Nombre ?? string.Empty,
                ClienteEmail = pedido.Cliente?.Email ?? string.Empty,
                ClienteTelefono = pedido.TelefonoContacto,
                RowVersion = pedido.RowVersion ?? Array.Empty<byte>(),
                Items = (pedido.Items ?? new List<ItemPedido>()).Select(i => new ItemPedidoDto
                {
                    ProductoId = i.ProductoId,
                    NombreProducto = i.NombreProducto,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            };
        }
    }
}
