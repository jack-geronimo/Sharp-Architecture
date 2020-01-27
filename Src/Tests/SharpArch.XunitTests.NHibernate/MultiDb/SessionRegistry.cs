namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using System.Collections.Concurrent;
    using global::NHibernate;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    public delegate void ConfigureSession(string databaseIdentifier, ISessionBuilder sessionBuilder);


    public delegate void ConfigureStatelessSession(string databaseIdentifier, IStatelessSessionBuilder sessionBuilder);


    /// <summary>
    ///     Manages Sessions in given scope
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>Requirements:</description>
    ///         </listheader>
    ///         <item>
    ///             <description>Keep track of open sessions.</description>
    ///         </item>
    ///         <item>
    ///             <description>Ensure ISession is created one time only.</description>
    ///         </item>
    ///         <item>
    ///             <description>Dispose all created Sessions.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Stateless sessions are not tracked as there is no benefit of using shared instance if
    ///                 <see cref="IStatelessSession" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class SessionRegistry : IDisposable, ISessionRegistry
    {
        readonly InitializationParams _initializationParams;
        readonly ConcurrentDictionary<string, INHibernateTransactionManager> _sessions = new ConcurrentDictionary<string, INHibernateTransactionManager>(4, 4);

        public SessionRegistry(
            [NotNull] ISessionFactoryRegistry sessionFactoryRegistry, ConfigureSession configureSession = null,
            ConfigureStatelessSession configureStatelessSession = null)
        {
            if (sessionFactoryRegistry == null) throw new ArgumentNullException(nameof(sessionFactoryRegistry));
            _initializationParams = new InitializationParams(sessionFactoryRegistry, configureSession, configureStatelessSession);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var transactionManager in _sessions.Values)
            {
                transactionManager.Session.Dispose();
            }
        }

        public INHibernateTransactionManager GetTransactionManager([NotNull] string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));

            return _sessions.GetOrAdd(databaseIdentifier, (key, p) =>
            {
                var builder = p.SessionFactoryRegistry.GetSessionFactory(key).WithOptions();
                p.ConfigureSession?.Invoke(key, builder);
                return new TransactionManager(builder.OpenSession());
            }, _initializationParams);
        }

        public IStatelessSession CreateStatelessSession([NotNull] string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            var builder = _initializationParams.SessionFactoryRegistry.GetSessionFactory(databaseIdentifier).WithStatelessOptions();
            _initializationParams.ConfigureStatelessSession?.Invoke(databaseIdentifier, builder);
            return builder.OpenStatelessSession();
        }


        class InitializationParams
        {
            public ISessionFactoryRegistry SessionFactoryRegistry { get; }
            public ConfigureSession ConfigureSession { get; }
            public ConfigureStatelessSession ConfigureStatelessSession { get; }

            public InitializationParams(
                [NotNull] ISessionFactoryRegistry sessionFactoryRegistry, ConfigureSession configureSession,
                ConfigureStatelessSession configureStatelessSession)
            {
                SessionFactoryRegistry = sessionFactoryRegistry ?? throw new ArgumentNullException(nameof(sessionFactoryRegistry));
                ConfigureSession = configureSession;
                ConfigureStatelessSession = configureStatelessSession;
            }
        }
    }
}
