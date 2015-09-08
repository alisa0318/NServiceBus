﻿namespace NServiceBus.AcceptanceTests.Mutators
{
    using System.Text;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;

    public class When_using_outgoing_tm_mutator : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_be_able_to_update_message()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<Endpoint>(b => b.Given(bus =>
                    {
                        bus.SendLocal(new MessageToBeMutated());
                        return Task.FromResult(0);
                    }))
                    .Done(c => c.MessageProcessed)
                    .Run();

            Assert.True(context.CanAddHeaders);
        }

        public class Context : ScenarioContext
        {
            public bool MessageProcessed { get; set; }
            public bool CanAddHeaders { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            class MyTransportMessageMutator : IMutateOutgoingTransportMessages, INeedInitialization
            {
                public void MutateOutgoing(MutateOutgoingTransportMessageContext context)
                {
                    context.OutgoingHeaders["HeaderSetByMutator"] = "some value";
                    context.OutgoingHeaders[Headers.EnclosedMessageTypes] = typeof(MessageThatMutatorChangesTo).FullName;
                    context.OutgoingBody = Encoding.UTF8.GetBytes("<MessageThatMutatorChangesTo/>");
                }

                public void Customize(BusConfiguration configuration)
                {
                    configuration.RegisterComponents(c => c.ConfigureComponent<MyTransportMessageMutator>(DependencyLifecycle.InstancePerCall));
                }
            }

            class MessageToBeMutatedHandler : IHandleMessages<MessageThatMutatorChangesTo>
            {
                Context testContext;
                IBus bus;

                public MessageToBeMutatedHandler(Context testContext,IBus bus)
                {
                    this.testContext = testContext;
                    this.bus = bus;
                }

                public void Handle(MessageThatMutatorChangesTo message)
                {
                    testContext.CanAddHeaders = bus.CurrentMessageContext.Headers.ContainsKey("HeaderSetByMutator");
                    testContext.MessageProcessed = true;
                }
            }

        }

        public class MessageToBeMutated : ICommand
        {
        }

        public class MessageThatMutatorChangesTo : ICommand
        {
        }
    }
}