using Elattba.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Elattba.InfraStructure.Services;

public sealed class OnnxImageEmbeddingService : IImageEmbeddingService, IDisposable
{
    private const int DefaultInputSize = 224;
    private static readonly float[] ImageNetMean = [0.485f, 0.456f, 0.406f];
    private static readonly float[] ImageNetStd = [0.229f, 0.224f, 0.225f];

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly object _sessionLock = new();
    private InferenceSession? _session;

    public OnnxImageEmbeddingService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<float[]> GenerateEmbeddingAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        var session = GetSession();
        var inputName = session.InputMetadata.Keys.First();
        var inputSize = _configuration.GetValue("ImageSearch:InputSize", DefaultInputSize);

        var inputTensor = await CreateInputTensorAsync(imageStream, inputSize, cancellationToken);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        using var results = session.Run(inputs);
        var embedding = results.First().AsEnumerable<float>().ToArray();
        NormalizeInPlace(embedding);

        return embedding;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }

    private InferenceSession GetSession()
    {
        if (_session != null)
        {
            return _session;
        }

        lock (_sessionLock)
        {
            if (_session != null)
            {
                return _session;
            }

            var modelPath = GetModelPath();
            if (!File.Exists(modelPath))
            {
                throw new InvalidOperationException(
                    $"Image search model was not found at '{modelPath}'. Add an ONNX image embedding model or update ImageSearch:ModelPath.");
            }

            _session = new InferenceSession(modelPath);
            return _session;
        }
    }

    private string GetModelPath()
    {
        var configuredPath = _configuration["ImageSearch:ModelPath"];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            configuredPath = Path.Combine("Models", "image-embedding.onnx");
        }

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
    }

    private static Task<DenseTensor<float>> CreateInputTensorAsync(
        Stream imageStream,
        int inputSize,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            throw new PlatformNotSupportedException("Local image preprocessing currently uses System.Drawing and requires Windows 6.1 or later.");
        }

        using var sourceImage = Image.FromStream(imageStream);
        using var resizedImage = new Bitmap(inputSize, inputSize);
        using (var graphics = Graphics.FromImage(resizedImage))
        {
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(
                sourceImage,
                new Rectangle(0, 0, inputSize, inputSize),
                GetCenterCropRectangle(sourceImage.Width, sourceImage.Height),
                GraphicsUnit.Pixel);
        }

        var tensor = new DenseTensor<float>(new[] { 1, 3, inputSize, inputSize });
        for (var y = 0; y < inputSize; y++)
        {
            for (var x = 0; x < inputSize; x++)
            {
                var pixel = resizedImage.GetPixel(x, y);
                tensor[0, 0, y, x] = ((pixel.R / 255f) - ImageNetMean[0]) / ImageNetStd[0];
                tensor[0, 1, y, x] = ((pixel.G / 255f) - ImageNetMean[1]) / ImageNetStd[1];
                tensor[0, 2, y, x] = ((pixel.B / 255f) - ImageNetMean[2]) / ImageNetStd[2];
            }
        }

        return Task.FromResult(tensor);
    }

    private static Rectangle GetCenterCropRectangle(int width, int height)
    {
        var sideLength = Math.Min(width, height);
        var x = (width - sideLength) / 2;
        var y = (height - sideLength) / 2;

        return new Rectangle(x, y, sideLength, sideLength);
    }

    private static void NormalizeInPlace(float[] values)
    {
        var sum = 0d;
        foreach (var value in values)
        {
            sum += value * value;
        }

        var magnitude = Math.Sqrt(sum);
        if (magnitude == 0)
        {
            return;
        }

        for (var index = 0; index < values.Length; index++)
        {
            values[index] = (float)(values[index] / magnitude);
        }
    }
}
