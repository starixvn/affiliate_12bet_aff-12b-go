using Fanex.Affiliate.Link.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fanex.Affiliate.Link.Filters
{
    /// <summary>
    /// Action filter that provides comprehensive affiliate parameter validation.
    /// Performs both pattern validation and business logic validation with database access.
    /// This is part of the Hybrid Approach - basic constraints filter routes, this filter validates business logic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AffiliateValidationAttribute(IAffInfoService affInfoService, ILogger<AffiliateValidationAttribute> logger) : ActionFilterAttribute
    {
        private readonly IAffInfoService _affInfoService = affInfoService ?? throw new ArgumentNullException(nameof(affInfoService));
        private readonly ILogger<AffiliateValidationAttribute> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Constants for error messages
        private const string AffiliateNotFoundMessage = "not found or is inactive. Please contact support if you believe this is an error.";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var validationResult = ValidateAffiliateParameters(context.ActionArguments);

            if (!validationResult.IsValid)
            {
                LogValidationFailure(context, validationResult.ErrorMessage);
                context.Result = CreateErrorResponse(validationResult.ErrorMessage);
                return;
            }

            StoreValidationContext(context, validationResult);
            base.OnActionExecuting(context);
        }

        private void LogValidationFailure(ActionExecutingContext context, string errorMessage)
        {
            _logger.LogWarning("Affiliate validation failed for {Action}: {Error}",
                context.ActionDescriptor.DisplayName, errorMessage);
        }

        private static ViewResult CreateErrorResponse(string errorMessage)
        {
            return new ViewResult
            {
                ViewName = "Link",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                    new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                {
                    ["Result"] = errorMessage
                }
            };
        }

        private static void StoreValidationContext(ActionExecutingContext context, AffiliateValidationResult validationResult)
        {
            context.HttpContext.Items["AffiliateValidationPassed"] = true;
            context.HttpContext.Items["ValidatedAffiliateInfo"] = validationResult.AffiliateInfo;
        }

        private AffiliateValidationResult ValidateAffiliateParameters(IDictionary<string, object> parameters)
        {
            try
            {
                return ValidateBusinessLogic(parameters) ?? AffiliateValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate validation");
                return AffiliateValidationResult.Failure("Unable to validate affiliate information at this time. Please try again later.");
            }
        }

        private AffiliateValidationResult ValidateBusinessLogic(IDictionary<string, object> parameters)
        {
            try
            {
                return ValidateAffiliateById(parameters)
                    ?? ValidateAffiliateByCode(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during business logic validation");
                return AffiliateValidationResult.Failure("Unable to verify affiliate information. Please try again later.");
            }
        }

        private AffiliateValidationResult ValidateAffiliateById(IDictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("affId", out var affIdValue) || affIdValue == null)
            {
                return null;
            }

            if (!int.TryParse(affIdValue.ToString(), out var affId))
            {
                return null;
            }

            var affInfo = _affInfoService.GetAffInfoByAffId(affId);
            if (string.IsNullOrEmpty(affInfo.AffCode))
            {
                return AffiliateValidationResult.Failure($"Affiliate ID '{affId}' {AffiliateNotFoundMessage}");
            }

            return AffiliateValidationResult.Success(affInfo);
        }

        private AffiliateValidationResult ValidateAffiliateByCode(IDictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("affCode", out var affCodeValue) || affCodeValue == null)
            {
                return null;
            }

            var affCode = affCodeValue.ToString();
            if (string.IsNullOrEmpty(affCode))
            {
                return null;
            }

            var affInfo = _affInfoService.GetAffInfoByAffCode(affCode);
            if (affInfo.AffId == 0)
            {
                return AffiliateValidationResult.Failure($"Affiliate code '{affCode}' {AffiliateNotFoundMessage}");
            }

            return AffiliateValidationResult.Success(affInfo);
        }
    }

    /// <summary>
    /// Result of affiliate parameter validation with optional affiliate information.
    /// </summary>
    public class AffiliateValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }
        public object AffiliateInfo { get; private set; }

        private AffiliateValidationResult(bool isValid, string errorMessage = "", object affiliateInfo = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            AffiliateInfo = affiliateInfo;
        }

        public static AffiliateValidationResult Success(object affiliateInfo = null) =>
            new(true, string.Empty, affiliateInfo);

        public static AffiliateValidationResult Failure(string errorMessage) =>
            new(false, errorMessage);
    }
}