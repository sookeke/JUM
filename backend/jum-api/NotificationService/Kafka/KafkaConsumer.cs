using Confluent.Kafka;
using NotificationService.Kafka.Interfaces;

namespace NotificationService.Kafka;
public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
{
    private readonly ConsumerConfig _config;
    private IKafkaHandler<TKey, TValue> _handler;
    private IConsumer<TKey, TValue> _consumer;
    private string _topic;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public KafkaConsumer(ConsumerConfig config, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _config = config;
    }

    public async Task Consume(string topic, CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        _consumer = new ConsumerBuilder<TKey, TValue>(_config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
        _topic = topic;

        await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }
    /// <summary>
    /// This will close the consumer, commit offsets and leave the group cleanly.
    /// </summary>
    public void Close()
    {
        _consumer.Close();
    }
    /// <summary>
    /// Releases all resources used by the current instance of the consumer
    /// </summary>
    public void Dispose()
    {
        _consumer.Dispose();
    }
    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(cancellationToken);
                if (result != null)
                {
                    await _handler.HandleAsync(_consumer.Name, result.Message.Key, result.Message.Value);
                    _consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }
}

