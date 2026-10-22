using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Reserva>> ObtenerTodasAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Huesped)
            .Include(r => r.Habitacion)
                .ThenInclude(h => h.TipoHabitacion)
            .OrderByDescending(r => r.FechaIngreso)
            .ThenByDescending(r => r.ReservaId)
            .ToListAsync();
    }

    public async Task<List<Huesped>> ObtenerHuespedesActivosAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellido)
            .ThenBy(h => h.Nombre)
            .ToListAsync();
    }

    public async Task<bool> HabitacionReservadaAsync(int habitacionId, DateOnly ingreso, DateOnly salida)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.HabitacionId == habitacionId &&
                           r.Estado != "Cancelada" &&
                           r.FechaIngreso < salida &&
                           r.FechaSalida > ingreso);
    }

    public async Task CrearAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null || (reserva.Estado != "Pendiente" && reserva.Estado != "Confirmada"))
        {
            return false;
        }

        reserva.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}
