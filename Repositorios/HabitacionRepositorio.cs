using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ObtenerActivasAsync(int? tipoHabitacionId = null)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .Where(h => h.Activo);

        if (tipoHabitacionId.HasValue && tipoHabitacionId.Value > 0)
        {
            consulta = consulta.Where(h => h.TipoHabitacionId == tipoHabitacionId.Value);
        }

        return await consulta.OrderBy(h => h.Numero).ToListAsync();
    }

    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .FirstOrDefaultAsync(h => h.HabitacionId == id);
    }

    public async Task<List<TipoHabitacion>> ObtenerTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int? idExcluir = null)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.Numero == numero && (!idExcluir.HasValue || h.HabitacionId != idExcluir.Value));
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Habitacion habitacion)
    {
        _context.Habitacions.Update(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EliminarLogicoAsync(int id)
    {
        var habitacion = await _context.Habitacions.FindAsync(id);
        if (habitacion is null)
        {
            return false;
        }

        habitacion.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Habitacion>> ObtenerDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .Where(h => h.Activo && !_context.Reservas.Any(r =>
                r.HabitacionId == h.HabitacionId &&
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }
}
