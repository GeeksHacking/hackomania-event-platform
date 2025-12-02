using System.Security.Cryptography;
using SqlSugar;

namespace HackOMania.Api.Entities;

/// <summary>
/// Team and project information
/// </summary>
public class Team
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    /// <summary>
    /// Team / project name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Team / project description
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Team / project location
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Location { get; set; }

    /// <summary>
    /// Project Devpost link
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? DevpostUri { get; set; }

    /// <summary>
    /// Project presentation slides link
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? SlidesUri { get; set; }

    /// <summary>
    /// Project code repository link
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? RepositoryUri { get; set; }

    /// <summary>
    /// Secret code to join team
    /// </summary>
    public string JoinCode { get; set; } =
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public Guid HackathonId { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(Participant.TeamId))]
    public List<Participant> Members { get; set; } = null!;
}
