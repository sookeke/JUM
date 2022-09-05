using NotificationService.Kafka.Interfaces;
using NotificationService.NotificationEvents.UserProvisioning.Models;
using System.Net;

namespace NotificationService.NotificationEvents.UserProvisioning;
public class NotificationServiceConsumer : BackgroundService
{
	private readonly IKafkaConsumer<string, UserProvisioningModel> _consumer;
	private readonly NotificationServiceConfiguration _config;
	public NotificationServiceConsumer(IKafkaConsumer<string, UserProvisioningModel> kafkaConsumer, NotificationServiceConfiguration config)
	{
		_consumer = kafkaConsumer;
		_config = config;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			await _consumer.Consume(_config.KafkaCluster.TopicName, stoppingToken);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {_config.KafkaCluster.TopicName}, {ex}");
		}
	}

	public override void Dispose()
	{
		_consumer.Close();
		_consumer.Dispose();

		base.Dispose();
	}
}

