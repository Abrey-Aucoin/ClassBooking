using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Core;

public class Instructor
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public List<FitnessClass> FitnessClasses { get; private set; } = [];
}
