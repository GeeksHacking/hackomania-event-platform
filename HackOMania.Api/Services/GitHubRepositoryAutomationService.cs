using HackOMania.Api.Entities;
using Octokit;
using System.Text;

namespace HackOMania.Api.Services;

public class GitHubRepositoryAutomationService(
    ILogger<GitHubRepositoryAutomationService> logger
) : IGitHubRepositoryAutomationService
{
    public async Task ValidateAndMaybeForkAsync(
        HackathonGitHubRepositorySettings? settings,
        string teamName,
        Uri repositoryUri,
        CancellationToken ct = default
    )
    {
        if (
            settings is null
            || (!settings.IsRepositoryCheckingEnabled && !settings.IsRepositoryForkingEnabled)
        )
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub repository automation is enabled, but no API key is configured."
            );
        }

        var coordinates = ParseRepositoryCoordinates(repositoryUri);
        var client = CreateClient(settings.ApiKey.Trim());

        Repository repository;
        try
        {
            repository = await client.Repository.Get(coordinates.Owner, coordinates.Name);
        }
        catch (NotFoundException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "The submitted repository could not be found on GitHub.",
                ex
            );
        }
        catch (AuthorizationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub rejected the configured API key while validating the repository.",
                ex
            );
        }
        catch (ApiException ex)
        {
            logger.LogWarning(
                ex,
                "GitHub repository validation failed for {Owner}/{Repository}",
                coordinates.Owner,
                coordinates.Name
            );
            throw new GitHubRepositoryAutomationException(
                "GitHub validation failed for the submitted repository.",
                ex
            );
        }

        if (!settings.IsRepositoryForkingEnabled)
        {
            return;
        }

        if (!settings.OrganizationId.HasValue)
        {
            throw new GitHubRepositoryAutomationException(
                "Repository cloning is enabled, but no target GitHub organization is configured."
            );
        }

        Organization organization;
        try
        {
            organization = await client.Organization.Get(settings.OrganizationId.Value.ToString());
        }
        catch (NotFoundException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "The configured GitHub organization could not be found.",
                ex
            );
        }
        catch (AuthorizationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub rejected the configured API key while resolving the target organization.",
                ex
            );
        }
        catch (ApiException ex)
        {
            logger.LogWarning(
                ex,
                "GitHub organization lookup failed for organization id {OrganizationId}",
                settings.OrganizationId.Value
            );
            throw new GitHubRepositoryAutomationException(
                "GitHub validation failed for the configured organization.",
                ex
            );
        }

        Repository fork;
        try
        {
            fork = await client.Repository.Forks.Create(
                repository.Owner.Login,
                repository.Name,
                new NewRepositoryFork { Organization = organization.Login }
            );
        }
        catch (AuthorizationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub rejected the configured API key while forking the repository.",
                ex
            );
        }
        catch (ApiValidationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub could not fork the submitted repository with the configured settings.",
                ex
            );
        }
        catch (ApiException ex)
        {
            logger.LogWarning(
                ex,
                "GitHub fork creation failed for {Owner}/{Repository}",
                repository.Owner.Login,
                repository.Name
            );
            throw new GitHubRepositoryAutomationException(
                "GitHub failed to fork the submitted repository.",
                ex
            );
        }

        var targetName = BuildTargetRepositoryName(
            settings.RepositoryPrefix,
            teamName,
            repository.Name
        );
        if (string.Equals(fork.Name, targetName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            await client.Repository.Edit(
                fork.Id,
                new RepositoryUpdate { Name = targetName }
            );
        }
        catch (AuthorizationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub rejected the configured API key while renaming the forked repository.",
                ex
            );
        }
        catch (ApiValidationException ex)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub could not rename the forked repository with the configured prefix.",
                ex
            );
        }
        catch (ApiException ex)
        {
            logger.LogWarning(
                ex,
                "GitHub fork rename failed for fork id {ForkId} to {TargetName}",
                fork.Id,
                targetName
            );
            throw new GitHubRepositoryAutomationException(
                "GitHub failed to rename the forked repository with the configured prefix.",
                ex
            );
        }
    }

    private static GitHubClient CreateClient(string apiKey)
    {
        var client = new GitHubClient(new ProductHeaderValue("HackOMania"));
        client.Credentials = new Credentials(apiKey);
        return client;
    }

    private static string BuildTargetRepositoryName(
        string? prefix,
        string teamName,
        string repositoryName
    )
    {
        var parts = new[] { prefix, teamName, repositoryName }
            .Select(SanitizeRepositoryNamePart)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length == 0)
        {
            throw new GitHubRepositoryAutomationException(
                "GitHub could not derive a valid name for the forked repository."
            );
        }

        return string.Join("-", parts);
    }

    private static string? SanitizeRepositoryNamePart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSeparator = false;

        foreach (var character in value.Trim())
        {
            if (char.IsLetterOrDigit(character) || character is '-' or '_' or '.')
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (previousWasSeparator)
            {
                continue;
            }

            builder.Append('-');
            previousWasSeparator = true;
        }

        return builder.ToString().Trim('-', '.', '_');
    }

    private static (string Owner, string Name) ParseRepositoryCoordinates(Uri repositoryUri)
    {
        if (
            !string.Equals(repositoryUri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(repositoryUri.Host, "www.github.com", StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new GitHubRepositoryAutomationException(
                "The submitted repository must be a GitHub repository URL."
            );
        }

        var segments = repositoryUri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 2)
        {
            throw new GitHubRepositoryAutomationException(
                "The submitted repository URL must include both the owner and repository name."
            );
        }

        var owner = segments[0];
        var repositoryName = segments[1];
        if (repositoryName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            repositoryName = repositoryName[..^4];
        }

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repositoryName))
        {
            throw new GitHubRepositoryAutomationException(
                "The submitted repository URL must include both the owner and repository name."
            );
        }

        return (owner, repositoryName);
    }
}
