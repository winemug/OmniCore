// using OmniCore.Common.Amqp;
// using RabbitMQ.Client;
//
// namespace OmniCore.Framework;
//
// public class AmqpConsumer : AsyncDefaultBasicConsumer
// {
//     private readonly SynchronizedCollection<Func<AmqpMessage, Task<bool>>> _handlers;
//     private readonly string _queue;
//
//     public AmqpConsumer(SynchronizedCollection<Func<AmqpMessage, Task<bool>>> handlers, string queue)
//     {
//         _handlers = handlers;
//         _queue = queue;
//     }
//
//     public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
//         string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
//     {
//         var message = new AmqpMessage
//         {
//             Tag = deliveryTag,
//             Route = routingKey,
//             UserId = properties.UserId,
//             Body = body.ToArray(),
//             Queue = _queue
//         };
//
//         var ack = false;
//         foreach (var messageHandler in _handlers.ToList())
//         {
//             try
//             {
//                 ack = await messageHandler(message);
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine($"Error while handling message: {e}");
//             }
//
//             if (ack)
//                 break;
//         }
//
//         if (ack)
//             Model.BasicAck(message.Tag, false);
//         else
//             Model.BasicNack(message.Tag, false, true);
//     }
// }