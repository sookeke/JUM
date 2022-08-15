using jumwebapi.Models;

namespace jumwebapi.Data.ef;

[Table(nameof(JustinAgency))]
public class JustinAgency : BaseAuditable
{
    [Key]
    public long AgencyId { get; set; }
    [Required]
    public string Name { get;set; } = string.Empty;
    [Required]
    public string AgencyCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<JustinUser> Users { get; set; } = new List<JustinUser>();    
    public ICollection<JustinAgencyAssignment> AgencyAssignments { get; set; } = new List<JustinAgencyAssignment>();    
  
}