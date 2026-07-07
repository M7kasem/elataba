using Elattaba.API.Helper;
using Elattba.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

internal static class ControllerResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message, result.Data);

        return result.StatusCode switch
        {
            200 => controller.Ok(response),
            204 => controller.NoContent(),
            400 => controller.BadRequest(response),
            404 => controller.NotFound(response),
            >= 500 => controller.Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => controller.StatusCode(result.StatusCode, response)
        };
    }

    public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message);

        return result.StatusCode switch
        {
            200 => controller.Ok(response),
            400 => controller.BadRequest(response),
            404 => controller.NotFound(response),
            >= 500 => controller.Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => controller.StatusCode(result.StatusCode, response)
        };
    }
}
