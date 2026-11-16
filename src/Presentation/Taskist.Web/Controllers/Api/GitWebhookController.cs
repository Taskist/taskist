using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Service.Git;
using Taskist.Web.Infrastructure;

namespace Taskist.Web.Controllers.Api
{
	[ApiController]
	[IgnoreAntiforgeryToken]
	[EnableRateLimiting(Taskist.Web.Infrastructure.ServiceCollectionExtensions.GitWebhookRateLimitPolicy)]
	public class GitWebhookController : ControllerBase
	{
		#region Fields

		private const int MaxBodyBytes = 5 * 1024 * 1024;

		protected readonly IGitRepositoryService _gitRepositoryService;
		protected readonly IGitWebhookService _gitWebhookService;
		protected readonly ISecretProtector _secretProtector;
		protected readonly IntegrationSettings _integrationSettings;
		protected readonly ILogger<GitWebhookController> _logger;

		#endregion

		#region Ctor

		public GitWebhookController(IGitRepositoryService gitRepositoryService,
			IGitWebhookService gitWebhookService,
			ISecretProtector secretProtector,
			IntegrationSettings integrationSettings,
			ILogger<GitWebhookController> logger)
		{
			_gitRepositoryService = gitRepositoryService;
			_gitWebhookService = gitWebhookService;
			_secretProtector = secretProtector;
			_integrationSettings = integrationSettings;
			_logger = logger;
		}

		#endregion

		#region Methods

		[HttpPost("api/git/hooks/{key}")]
		[RequestSizeLimit(MaxBodyBytes)]
		public async Task<IActionResult> Receive(string key)
		{
			var repository = await _gitRepositoryService.GetRepositoryByWebhookKeyAsync(key);
			if (repository == null)
				return NotFound();

			return await ReceiveAsync(repository, null);
		}

		[HttpPost("api/gitlab/webhook")]
		[RequestSizeLimit(MaxBodyBytes)]
		public async Task<IActionResult> LegacyGitLab([FromQuery(Name = "project")] int projectId)
		{
			if (projectId <= 0)
				return BadRequest();

			var repository = (await _gitRepositoryService.GetProjectRepositoriesAsync(projectId))
				.FirstOrDefault(r => r.GitConnection.ProviderType == (int)GitProviderType.GitLab);
			if (repository == null)
				return NotFound();

			_logger.LogInformation("Deprecated webhook URL /api/gitlab/webhook used for project {ProjectId}. Switch to {Url}.",
				projectId, $"/api/git/hooks/{repository.WebhookKey}");

			return await ReceiveAsync(repository, _secretProtector.Unprotect(_integrationSettings.WebhookSecret));
		}

		#endregion

		#region Utilities

		private async Task<IActionResult> ReceiveAsync(Core.Domain.Git.ProjectRepository repository, string? fallbackSecret)
		{
			byte[] body;
			using (var buffer = new MemoryStream())
			{
				await Request.Body.CopyToAsync(buffer);
				if (buffer.Length > MaxBodyBytes)
					return StatusCode(StatusCodes.Status413PayloadTooLarge);
				body = buffer.ToArray();
			}

			var result = await _gitWebhookService.ReceiveAsync(repository, name => Request.Headers.TryGetValue(name, out var value) ? value.ToString() : null,
				body, fallbackSecret);

			return StatusCode(result.StatusCode, new { message = result.Message });
		}

		#endregion
	}
}
