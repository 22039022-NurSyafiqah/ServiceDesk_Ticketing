using System.ComponentModel.DataAnnotations;

public class TicketRequest
{
    public int TicketID { get; set; }

    [Required]
    public int TicketStatus_ID { get; set; }

    public string? TicketStatus_Type { get; set; }

    [Required]
    public int Priority_ID { get; set; }

    public string? Priority_Type { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Ticket_StartDate { get; set; }

    //[DataType(DataType.Date)]
    //public DateTime Ticket_EndDate { get; set; }

    [Required(ErrorMessage = "Enter the End Date")]
    [DataType(DataType.Date)]

    public DateTime TicketLastUpdated { get; set; }

    [Required]
    public int User_ID { get; set; }

    public string? FullName { get; set; }

    [Required]
    public string CreatedBy { get; set; } = null!;

    [Required]
    public int Category_ID { get; set; }

    [Required]
    public string Category_Name { get; set; } = null!;
}
