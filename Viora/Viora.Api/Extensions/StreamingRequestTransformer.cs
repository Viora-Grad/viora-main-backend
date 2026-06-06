namespace Viora.Api.Extensions;

public static class StreamingRequestTransformer
{
    public static void TransformToStream(this HttpContext httpContext)
    {
        httpContext.Response.ContentType = "text/event-stream";
        httpContext.Response.Headers.Append("Cache-Control", "no-cache");
        httpContext.Response.Headers.Append("Connection", "keep-alive");
    }
}
