using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _habitacionRepositorio;

    public HabitacionesController(HabitacionRepositorio habitacionRepositorio)
    {
        _habitacionRepositorio = habitacionRepositorio;
    }

    public async Task<IActionResult> Index(int? tipoId)
    {
        var habitaciones = await _habitacionRepositorio.ObtenerActivasAsync(tipoId);
        ViewBag.Tipos = await _habitacionRepositorio.ObtenerTiposAsync();
        ViewBag.TipoId = tipoId;
        return View(habitaciones);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var habitacion = await _habitacionRepositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion is null)
        {
            return NotFound();
        }

        return View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Tipos = await _habitacionRepositorio.ObtenerTiposAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.TipoHabitacion));
        ModelState.Remove(nameof(Habitacion.Reservas));

        if (habitacion.TipoHabitacionId <= 0)
        {
            ModelState.AddModelError(nameof(Habitacion.TipoHabitacionId), "Seleccione un tipo de habitación.");
        }

        if (await _habitacionRepositorio.ExisteNumeroAsync(habitacion.Numero))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con ese número.");
        }

        if (ModelState.IsValid)
        {
            habitacion.Activo = true;
            await _habitacionRepositorio.CrearAsync(habitacion);
            TempData["Mensaje"] = "La habitación se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Tipos = await _habitacionRepositorio.ObtenerTiposAsync();
        return View(habitacion);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var habitacion = await _habitacionRepositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion is null)
        {
            return NotFound();
        }

        ViewBag.Tipos = await _habitacionRepositorio.ObtenerTiposAsync();
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.HabitacionId)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Habitacion.TipoHabitacion));
        ModelState.Remove(nameof(Habitacion.Reservas));

        if (habitacion.TipoHabitacionId <= 0)
        {
            ModelState.AddModelError(nameof(Habitacion.TipoHabitacionId), "Seleccione un tipo de habitación.");
        }

        var actual = await _habitacionRepositorio.ObtenerPorIdAsync(id);
        if (actual is null)
        {
            return NotFound();
        }

        if (await _habitacionRepositorio.ExisteNumeroAsync(habitacion.Numero, id))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con ese número.");
        }

        if (ModelState.IsValid)
        {
            habitacion.Activo = actual.Activo;
            await _habitacionRepositorio.ActualizarAsync(habitacion);
            TempData["Mensaje"] = "La habitación se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Tipos = await _habitacionRepositorio.ObtenerTiposAsync();
        return View(habitacion);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var habitacion = await _habitacionRepositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion is null)
        {
            return NotFound();
        }

        return View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eliminado = await _habitacionRepositorio.EliminarLogicoAsync(id);
        TempData["Mensaje"] = eliminado
            ? "La habitación se dio de baja correctamente."
            : "No se encontró la habitación.";
        return RedirectToAction(nameof(Index));
    }
}
