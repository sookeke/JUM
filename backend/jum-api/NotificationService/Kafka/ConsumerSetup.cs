using Confluent.Kafka;
using NotificationService.Extensions;
using NotificationService.Kafka.Interfaces;
using NotificationService.NotificationEvents;
using NotificationService.NotificationEvents.UserProvisioning;
using NotificationService.NotificationEvents.UserProvisioning.Handler;
using NotificationService.NotificationEvents.UserProvisioning.Models;

namespace NotificationService.Kafka;
public static class ConsumerSetup
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, NotificationServiceConfiguration config)
    {
        //Configuration = configuration;
        services.ThrowIfNull(nameof(services));
        config.ThrowIfNull(nameof(config));

        var clientConfig = new ClientConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret,
        };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            Acks = Acks.All,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret,
            EnableIdempotence = true
        };

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "Notification-Consumer-Group",
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            EnableAutoOffsetStore = false,
            AutoCommitIntervalMs = 4000,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret
        };
        //var producerConfig = new ProducerConfig(clientConfig);
        services.AddSingleton(consumerConfig);
        services.AddSingleton(producerConfig);

        services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));

        //services.AddSingleton(consumerConfig);

        services.AddScoped<IKafkaHandler<string, Notification>, UserProvisioningHandler>();
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddHostedService<NotificationServiceConsumer>();

        return services;
    }
}
