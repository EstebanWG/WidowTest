using BestHTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void HandleResponse<T>(Response<T> item);
public class HttpClientCoroutine
{
    private readonly ILogger _logger;
    private readonly ISerializationOption _serializationOption;
    private readonly Uri _baseUrl;
    private readonly string _authorizer;
    public HttpClientCoroutine(ISerializationOption serializationOption, Uri baseUrl,
            string authorizer)
    {
        _serializationOption = serializationOption;
        _logger = Debug.unityLogger;
        _baseUrl = baseUrl;
        _authorizer = authorizer;
    }
    public IEnumerator Get<T>(string resource, HandleResponse<T> callback)
    {
        var uri = new Uri($"{_baseUrl}{resource}");
        var webRequest = new HTTPRequest(uri, HTTPMethods.Get);
        SetHeaders(webRequest);
        yield return webRequest.Send();
        Response<T> response;
        if (!webRequest.Response.IsSuccess)
        {
            _logger.LogError("Request Error",
                $"Failed to process request.Complete Uri:{uri}, Resource:{resource} Error code: {webRequest.Response.StatusCode.ToString()}, Message: {webRequest.Response.Message}, Data: {webRequest.Response.DataAsText}");
            response = new Response<T>(default, webRequest.Response.IsSuccess, webRequest.Response.StatusCode,
                webRequest.Response.Message);
            callback?.Invoke(response);
            yield break;
        }

        var result = DeserializeResponse<T>(webRequest.Response.DataAsText);
        response = new Response<T>(result, webRequest.Response.IsSuccess, webRequest.Response.StatusCode,
            webRequest.Response.Message);
        callback?.Invoke(response);
    }
    private void SetHeaders(HTTPRequest webRequest)
    {
        SetAuthorization(webRequest);
        SetContentType(webRequest);
    }
    private void SetAuthorization(HTTPRequest webRequest)
    {
        if (_authorizer != null)
        {
            webRequest.AddHeader("Authorization", "Bearer "+ _authorizer);
        }
    }
    private void SetContentType(HTTPRequest webRequest)
    {
        webRequest.AddHeader("Content-Type", _serializationOption.ContentType);
    }
    private T DeserializeResponse<T>(string text)
    {
        _logger.Log($"Data: {text}");
        var result = _serializationOption.Deserialize<T>(text);
        return result;
    }
}

public class Response<T>
{
    public Response(T dto, bool isOk, int statusCode, string errorMessage)
    {
        StatusCode = statusCode;
        IsOK = isOk;
        Dto = dto;
        ErrorMessage = errorMessage;
    }

    public int StatusCode { get; private set; }
    public bool IsOK { get; private set; }
    public T Dto { get; private set; }

    public string ErrorMessage { get; private set; }
}

public interface ISerializationOption
{
    string ContentType { get; }
    T Deserialize<T>(string text);
}
public class JsonSerializationOption : ISerializationOption
{
    private readonly ILogger _logger;
    public string ContentType => "application/json";

    public JsonSerializationOption(ILogger logger)
    {
        _logger = logger;
    }

    public JsonSerializationOption() : this(Debug.unityLogger)
    {
    }

    public T Deserialize<T>(string text)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<T>(text);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Json Conversion", $"Could not parse response {text}. {ex.Message}");
            return default;
        }
    }
}