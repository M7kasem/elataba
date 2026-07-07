using System.Threading.Channels;
using Elattba.Application.ProductImages;

namespace Elattaba.API.BackgroundServices;

public sealed class ChannelProductImageEmbeddingQueue : IProductImageEmbeddingQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public async ValueTask QueueAsync(int productImageId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(productImageId, cancellationToken);
    }

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
