using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class Huesped
{
    public int HuespedId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
