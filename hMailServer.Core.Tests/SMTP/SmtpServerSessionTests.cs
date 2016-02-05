﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hMailServer.Core.Protocols.SMTP;
using Moq;
using NUnit.Framework;

namespace hMailServer.Core.Tests.SMTP
{
    [TestFixture]
    public class SmtpServerSessionTests
    {
        [Test]
        public void TestCompleteSmtpServerSession()
        {
            var commandHandlerMock = new Mock<ISmtpServerCommandHandler>();
            commandHandlerMock.Setup(f => f.HandleHelo("example.com.com")).Returns(true);

            var data = "Hello World\r\n.\r\n";
            var memory = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var connectionMock = ConnectionMockFactory.Create(new string[]
                {
                    "HELO example.com",
                    "MAIL FROM: knafve@example.com",
                    "RCPT TO: knafve@example.com",
                    "DATA",
                    "QUIT"
                }, new []
                {
                    memory
                });

            SmtpServerSession session = new SmtpServerSession(commandHandlerMock.Object);

            var task = session.HandleConnection(connectionMock);
            task.Wait();

            commandHandlerMock.Verify(f => f.HandleHelo("example.com"));
            commandHandlerMock.Verify(f => f.HandleData(It.Is<MemoryStream>(f2 => VerifyMemoryStreamContents(f2, "Hello World\r\n"))));
        }



        private bool VerifyMemoryStreamContents(MemoryStream stream, string expectedText)
        {
            var actualBytes = stream.ToArray();
            var actualText = Encoding.UTF8.GetString(actualBytes);

            return expectedText == actualText;
        }
    }
}
