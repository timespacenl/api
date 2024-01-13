using Asp.Versioning;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime.Serialization.SystemTextJson;
using Timespace.Api.Infrastructure;
using Timespace.Api.Infrastructure.Errors;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Persistence.Entities.Users;
using Timespace.Api.Infrastructure.Services;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Timespace.Api;

public static class ConfigureServices
{
	public static void AddServices(this IServiceCollection services, ConfigurationManager configuration)
	{
		_ = services.Configure<ApiBehaviorOptions>(options =>
		{
			options.SuppressModelStateInvalidFilter = true;
		});

		_ = services.AddSingleton<IClock, DateTimeProvider>();

		services.ConfigureIdentity();

		_ = services.AddProblemDetails(ConfigureProblemDetails);
		_ = services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
				opt => opt.UseNodaTime())
		);

		_ = services.AddApiVersioning();
		_ = services.AddValidatorsFromAssembly(typeof(IAssemblyMarker).Assembly);
		_ = services.AddBehaviors();
		_ = services.AddHandlers();

		_ = services.AddHttpClient();

		_ = services.AddDistributedMemoryCache();
	}

	private static void ConfigureIdentity(this IServiceCollection services)
	{
		// Services used by identity
		_ = services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
				options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
			})
			.AddCookie(IdentityConstants.ApplicationScheme, o =>
			{
				o.Cookie.Name = "timespace.session";
				o.Cookie.HttpOnly = true;
				o.Cookie.SameSite = SameSiteMode.Strict;
				o.Cookie.Expiration = TimeSpan.FromDays(7);
				o.Events = new CookieAuthenticationEvents
				{
					OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
				};
			})
			.AddCookie(IdentityConstants.ExternalScheme, o =>
			{
				o.Cookie.Name = IdentityConstants.ExternalScheme;
				o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
			})
			.AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
			{
				o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
				o.Events = new CookieAuthenticationEvents
				{
					OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
				};
			})
			.AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
			{
				o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
				o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
			});

		_ = services.AddIdentityCore<ApplicationUser>(options =>
			{
				options.User.RequireUniqueEmail = true;
			})
			.AddEntityFrameworkStores<AppDbContext>();
	}

	private static void ConfigureProblemDetails(ProblemDetailsOptions options)
	{
		options.IncludeExceptionDetails = (_, _) => false;

		options.Rethrow<NotSupportedException>();

		options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

		options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

		options.Map<Exception>((context, ex) =>
		{
			if (ex is IBaseException baseException)
			{
				var problemDetails = new ProblemDetails
				{
					Type = baseException.Type,
					Title = baseException.Title,
					Status = baseException.StatusCode,
					Instance = context.Request.Path,
					Detail = baseException.Detail,
				};

				foreach (var kvpair in baseException.MapExtensions())
				{
					problemDetails.Extensions.Add(kvpair);
				}

				return problemDetails;
			}

			var internalErrorProblemDetails = new ProblemDetails
			{
				Type = "internal-server-error",
				Title = "Internal server error",
				Status = StatusCodes.Status500InternalServerError,
				Detail = "An internal server error occurred.",
				Instance = context.Request.Path
			};

			return internalErrorProblemDetails;
		});
	}

	private static IServiceCollection AddApiVersioning(this IServiceCollection services)
	{
		_ = services.AddApiVersioning(o =>
		{
			o.AssumeDefaultVersionWhenUnspecified = true;
			o.DefaultApiVersion = new ApiVersion(1, 0);
			o.ApiVersionReader = new UrlSegmentApiVersionReader();
		}).AddApiExplorer(options =>
		{
			// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
			// note: the specified format code will format the version as "'v'major[.minor][-status]"
			options.GroupNameFormat = "'v'VVV";

			// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
			// can also be used to control the format of the API version in route templates
			options.SubstituteApiVersionInUrl = true;
		});

		return services;
	}

	public static void AddAspnetServices(this IServiceCollection services)
	{
		_ = services.AddControllers(options =>
			{
				options.Filters.Add(new ProducesAttribute("application/json"));
			})
			.AddJsonOptions(options =>
			{
				_ = options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
			});

		_ = services.AddHttpContextAccessor();

		_ = services.AddCors(options =>
		{
			options.AddDefaultPolicy(
				policy =>
				{
					_ = policy.WithOrigins("http://localhost:5173", "https://timespace.nl")
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
		});
	}

	public static void AddConfiguration(this IServiceCollection _, ConfigurationManager __)
	{
		// services.Configure<AuthenticationConfiguration>(configuration.GetSection(AuthenticationConfiguration.SectionName));
		// services.Configure<UserSettingsConfiguration>(configuration.GetSection(UserSettingsConfiguration.SectionName));
		// services.Configure<CaptchaConfiguration>(configuration.GetSection(CaptchaConfiguration.SectionName));
	}

	public static void AddApiExplorerServices(this IServiceCollection services)
	{
		services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
		services.TryAddEnumerable(
			ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());
	}
}
