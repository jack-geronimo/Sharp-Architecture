namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    public interface ISessionFactoryRegistry
    {
        void Add([NotNull] string databaseKey, [NotNull] INHibernateSessionFactoryBuilder sessionFactoryBuilder);
        ISessionFactory GetSessionFactory([NotNull] string databaseKey);
        Configuration GetConfiguration(string databaseKey);
        bool Contains(string databaseKey);
        bool IsSessionFactoryCreated(string databaseKey);
    }


    /// <summary>
    ///     Contains registered NHibernate Factories.
    ///     <para>
    ///         Must be registered as singleton.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>Requirements:</description>
    ///         </listheader>
    ///         <item>
    ///             <description>Keep track of registered / initialized session factories.</description>
    ///         </item>
    ///         <item>
    ///             <description>Lazy initialization of factory.</description>
    ///         </item>
    ///         <item>
    ///             <description>Ensure factory is create one time only.</description>
    ///         </item>
    ///         <item>
    ///             <description>Dispose all created SessionFactory instances.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class SessionFactoryRegistry : IDisposable, ISessionFactoryRegistry
    {
        public static readonly string DefaultDatabaseKey = "default";

        readonly ConcurrentDictionary<string, Container> _sessionFactoryBuilders =
            new ConcurrentDictionary<string, Container>(4, 16, StringComparer.Ordinal);

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var container in _sessionFactoryBuilders.Values)
            {
                if (container.SessionFactory.IsValueCreated) container.SessionFactory.Value.Dispose();
            }
        }

        public void Add([NotNull] string databaseKey, [NotNull] INHibernateSessionFactoryBuilder sessionFactoryBuilder)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));
            if (sessionFactoryBuilder == null) throw new ArgumentNullException(nameof(sessionFactoryBuilder));

            AddLazyFactory(databaseKey, new Container(sessionFactoryBuilder));
        }

        public ISessionFactory GetSessionFactory([NotNull] string databaseKey)
            => GetLazyFactory(databaseKey).SessionFactory.Value;

        public Configuration GetConfiguration(string databaseKey)
            => GetLazyFactory(databaseKey).Configuration.Value;

        public bool Contains(string databaseKey)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));
            return _sessionFactoryBuilders.ContainsKey(databaseKey);
        }

        public bool IsSessionFactoryCreated(string databaseKey)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));
            return GetLazyFactory(databaseKey).SessionFactory.IsValueCreated;
        }

        void AddLazyFactory(string databaseKey, Container container)
        {
            if (!_sessionFactoryBuilders.TryAdd(databaseKey, container))
                throw new InvalidOperationException($"SessionFactory with databaseKey '{databaseKey}' already registered.")
                {
                    Data = {["SessionFactoryKey"] = databaseKey}
                };
        }

        Container GetLazyFactory(string databaseKey)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));

            if (!_sessionFactoryBuilders.TryGetValue(databaseKey, out var lazyFactory))
                throw new InvalidOperationException($"SessionFactory with databaseKey '{databaseKey}' was not registered.")
                {
                    Data = {["SessionFactoryKey"] = databaseKey}
                };
            return lazyFactory;
        }


        class Container
        {
            public Lazy<Configuration> Configuration { get; }

            public Lazy<ISessionFactory> SessionFactory { get; }

            public Container(INHibernateSessionFactoryBuilder factoryBuilder)
            {
                Configuration = new Lazy<Configuration>(() => factoryBuilder.BuildConfiguration(), LazyThreadSafetyMode.ExecutionAndPublication);
                SessionFactory = new Lazy<ISessionFactory>(
                    () =>
                    {
                        // ensure configuration is created
                        var _ = Configuration.Value;
                        return factoryBuilder.BuildSessionFactory();
                    }
                );
            }
        }
    }
}
