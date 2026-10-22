using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class TipoHabitacion
{
    public int TipoHabitacionId { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();
}
