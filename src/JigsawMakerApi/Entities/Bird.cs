using System.ComponentModel.DataAnnotations;

namespace JigsawMakerApi.Entities;

public class Bird
{
    [Key]
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string Location { get; set; }
    public string Specie { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
}

