namespace CyberNexus.Api.Models;

public class UserChallengeResult
{
    public int Id { get; set; }
    public bool IsSolved { get; set; }
    public int Attempts { get; set; }
    public DateTime? SolvedAt { get; set; }

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
}
