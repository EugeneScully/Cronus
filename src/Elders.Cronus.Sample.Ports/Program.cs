﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Mapping.ByCode;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.CommonFiles;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Sample.Ports.Nhibernate;

namespace Elders.Cronus.Sample.Ports
{
    class Program
    {
        public static void Main(string[] args)
        {
            Thread.Sleep(9000);
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();

            const string Collaboration = "Collaboration";
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointPortConsumableSettings>(Collaboration, consumer =>
            {
                consumer.NumberOfWorkers = 2;
                consumer.ScopeFactory.CreateBatchScope = () => new BatchScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var nhHandler = handler as IHaveNhibernateSession;
                        if (nhHandler != null)
                        {
                            nhHandler.Session = nhHandler.Session = context.BatchScopeContext.Get<Lazy<ISession>>().Value;
                        }
                        var port = handler as IPort;
                        if (port != null)
                        {
                            port.CommandPublisher = cfg.GlobalSettings.CommandPublisher;
                        }
                        return handler;
                    });
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            new CronusHost(cfg).Start();

            Console.WriteLine("Ports started");
            Console.ReadLine();
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            Action<ModelMapper> customMappings = modelMapper =>
            {
                modelMapper.Class<User>(mapper =>
                {
                    mapper.Property(pr => pr.Email, prmap => prmap.Unique(true));
                });
            };

            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped, customMappings);
            return cfg.BuildSessionFactory();
        }
    }
}