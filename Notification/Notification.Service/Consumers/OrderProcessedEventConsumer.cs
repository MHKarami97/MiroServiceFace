using System;
using System.Drawing;
using System.IO;
using MassTransit;
using EmailService;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Messaging.InterfacesConstants.Events;
using SixLabors.ImageSharp;

namespace Notification.Service.Consumers
{
    public class OrderProcessedEventConsumer : IConsumer<IOrderProcessedEvent>
    {
        private readonly IEmailSender _emailSender;

        public OrderProcessedEventConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<IOrderProcessedEvent> context)
        {
            var rootFolder = AppContext
                .BaseDirectory
                .Substring(0,
                    AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));

            var result = context.Message;
            var facesData = result.Faces;

            if (facesData.Count < 1)
            {
                await Console.Out.WriteLineAsync($"No faces Detected");
            }
            else
            {
                var j = 0;

                foreach (var face in facesData)
                {
                    var ms = new MemoryStream(face);
                    
                    var image = Image.Load(ms.ToArray());
                    
                    await image.SaveAsync(rootFolder + "/Images/face" + j + ".jpg");
                    
                    j++;
                }
            }

            // Here we will add the Email Sending code

            string[] mailAddress = {result.UserEmail};

            await _emailSender.SendEmailAsync(new Message(mailAddress, "your order" + result.OrderId,
                "From FacesAndFaces", facesData));

            await context.Publish<IOrderDispatchedEvent>(new
            {
                context.Message.OrderId,
                DispatchDateTime = DateTime.UtcNow
            });
        }
    }
}