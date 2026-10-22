using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _reservaRepositorio;
    private readonly HabitacionRepositorio _habitacionRepositorio;

    public ReservasController(ReservaRepositorio reservaRepositorio, HabitacionRepositorio habitacionRepositorio)
    {
        _reservaRepositorio = reservaRepositorio;
        _habitacionRepositorio = habitacionRepositorio;
    }

    public async Task<IActionResult> Index()
    {
        var reservas = await _reservaRepositorio.ObtenerTodasAsync();
        return View(reservas);
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;
        ViewBag.Noches = 0;

        if (ingreso is null || salida is null)
        {
            return View(new List<Habitacion>());
        }

        if (salida <= ingreso)
        {
            ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la fecha de ingreso.");
            return View(new List<Habitacion>());
        }

        var noches = salida.Value.DayNumber - ingreso.Value.DayNumber;
        ViewBag.Noches = noches;

        var habitaciones = await _habitacionRepositorio.ObtenerDisponiblesAsync(ingreso.Value, salida.Value);
        return View(habitaciones);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return View(new Reserva
        {
            FechaIngreso = hoy,
            FechaSalida = hoy.AddDays(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.Habitacion));
        ModelState.Remove(nameof(Reserva.Huesped));
        ModelState.Remove(nameof(Reserva.Estado));

        if (reserva.HuespedId <= 0)
        {
            ModelState.AddModelError(nameof(Reserva.HuespedId), "Seleccione un huésped.");
        }

        if (reserva.HabitacionId <= 0)
        {
            ModelState.AddModelError(nameof(Reserva.HabitacionId), "Seleccione una habitación.");
        }

        if (reserva.FechaIngreso < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
        }

        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la fecha de ingreso.");
        }
        else if (reserva.HabitacionId > 0 &&
                 await _reservaRepositorio.HabitacionReservadaAsync(reserva.HabitacionId, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.HabitacionId), "La habitación no está disponible en ese rango de fechas.");
        }

        if (ModelState.IsValid)
        {
            reserva.Estado = "Pendiente";
            await _reservaRepositorio.CrearAsync(reserva);
            TempData["Mensaje"] = "La reserva se registró como pendiente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarListasAsync();
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _reservaRepositorio.CancelarAsync(id);
        TempData["Mensaje"] = cancelada
            ? "La reserva se canceló correctamente."
            : "Solo se pueden cancelar reservas pendientes o confirmadas.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Huespedes = await _reservaRepositorio.ObtenerHuespedesActivosAsync();
        ViewBag.Habitaciones = await _habitacionRepositorio.ObtenerActivasAsync();
    }
}
