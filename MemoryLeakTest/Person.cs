using System.ComponentModel.DataAnnotations;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    [ConcurrencyCheck]
    public Guid Version { get; set; }
}
