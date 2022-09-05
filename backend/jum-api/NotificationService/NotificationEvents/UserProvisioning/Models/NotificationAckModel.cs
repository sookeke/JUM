using System.ComponentModel.DataAnnotations;

namespace NotificationService.NotificationEvents.UserProvisioning.Models;
public class NotificationAckModel
{
    public string NotificationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long PartId { get; set; }
    public string EmailAddress { get; set; } =string.Empty;
    public string Consumer { get; set; } = string.Empty;
}

