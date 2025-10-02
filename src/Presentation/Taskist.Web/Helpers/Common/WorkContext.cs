using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Localization;
using Taskist.Service.Authentication;
using Taskist.Service.Localization;
using Microsoft.AspNetCore.Localization;

namespace Taskist.Web.Helpers.Common;

	public class WorkContext : IWorkContext
	{
		#region Fields

		protected readonly IAuthenticationService _authenticationService;
		protected readonly IHttpContextAccessor _httpContextAccessor;
		protected readonly ILanguageService _languageService;
		protected User _cachedUser;
		protected Language _cachedLanguage;

		#endregion Fields

		#region Ctor

		public WorkContext(IAuthenticationService authenticationService,
			IHttpContextAccessor httpContextAccessor,
			ILanguageService languageService)
		{
			_authenticationService = authenticationService;
			_httpContextAccessor = httpContextAccessor;
			_languageService = languageService;
		}

		#endregion Ctor

		#region Methods

		public async Task<User> GetCurrentUserAsync()
		{
			if (_cachedUser != null)
				return _cachedUser;

			await SetCurrentUserAsync();

			return _cachedUser;
		}

		public async Task SetCurrentUserAsync(User user = null)
		{
			if (user == null)
				user = await _authenticationService.GetAuthenticatedUserAsync();

			if (user != null && !user.Deleted && user.Status == (int)UserStatusEnum.Active)
			{
				SetUserCookie(user.Code);
				_cachedUser = user;
			}
		}

		public virtual async Task<Language> GetCurrentUserLanguageAsync()
		{
			if (_cachedLanguage != null)
				return _cachedLanguage;

			var user = await GetCurrentUserAsync();

			var currentLanguageId = user.LanguageId;
			var allStoreLanguages = await _languageService.GetAllAsync();

			var detectedLanguage = allStoreLanguages.FirstOrDefault(language => language.Id == currentLanguageId);

			SetLanguageCookie(detectedLanguage);

			_cachedLanguage = detectedLanguage;

			return _cachedLanguage;
		}

		#endregion

		#region  Helper

		protected void SetUserCookie(Guid code)
		{
			if (_httpContextAccessor.HttpContext?.Response?.HasStarted ?? true)
				return;

			//delete current cookie value
			var cookieName = WebConstant.UserCookie;
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

			//get date of cookie expiration
			var cookieExpires = WebConstant.UserCookieExpires;
			var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

			//if passed guid is empty set cookie as expired
			if (code == Guid.Empty)
				cookieExpiresDate = DateTime.Now.AddMonths(-1);

			//set new cookie value
			var options = new CookieOptions
			{
				HttpOnly = true,
				Expires = cookieExpiresDate,
				// Secure = _webHelper.IsCurrentConnectionSecured()
			};

			_httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, code.ToString(), options);
		}

		protected virtual void SetLanguageCookie(Language language)
		{
			if (_httpContextAccessor.HttpContext?.Response?.HasStarted ?? true)
				return;

			//delete current cookie value
			var cookieName = WebConstant.CultureCookie;
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

			if (string.IsNullOrEmpty(language?.LanguageCulture))
				return;

			//set new cookie value
			var value = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language.LanguageCulture));
			var options = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) };
			_httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, value, options);
		}

		#endregion
	}