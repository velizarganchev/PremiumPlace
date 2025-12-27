using Microsoft.AspNetCore.Mvc;
using PremiumPlace_API.Services;

namespace PremiumPlace_API.Controllers.Extensions
{
    public static class ServiceResponseExtensions
    {
        public static ActionResult ToActionResult<T>(this ControllerBase controller, ServiceResponse<T> sr)
        {
            if (sr.Success)
            {
                return sr.Data is null ? controller.NoContent() : controller.Ok(sr.Data);
            }

            var payload = new { sr.Message, sr.Error };

            return sr.ErrorType switch
            {
                ServiceErrorType.Validation => controller.BadRequest(payload),
                ServiceErrorType.NotFound => controller.NotFound(payload),
                ServiceErrorType.Conflict => controller.Conflict(payload),
                ServiceErrorType.Unauthorized => controller.Unauthorized(payload),
                ServiceErrorType.Forbidden => controller.Forbid(),
                _ => controller.BadRequest(payload)
            };
        }
    }
}
