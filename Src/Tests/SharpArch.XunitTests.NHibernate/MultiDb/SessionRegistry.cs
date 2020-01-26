namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using System.Collections.Concurrent;
    using global::NHibernate;
    using JetBrains.Annotations;


    public delegate void ConfigureSession(string databaseKey, ISessionBuilder sessionBuilder);


    public delegate void ConfigureStatelessSession(string databaseKey, IStatelessSessionBuilder sessionBuilder);


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
        readonly ConcurrentDictionary<string, ISession> _sessions = new ConcurrentDictionary<string, ISession>(4, 4);

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
            foreach (var session in _sessions.Values)
            {
                session.Dispose();
            }
        }

        public ISession GetSession([NotNull] string databaseKey)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));

            return _sessions.GetOrAdd(databaseKey, (key, p) =>
            {
                var builder = p.SessionFactoryRegistry.GetSessionFactory(key).WithOptions();
                p.ConfigureSession?.Invoke(key, builder);
                return builder.OpenSession();
            }, _initializationParams);
        }

        public IStatelessSession CreateStatelessSession([NotNull] string databaseKey)
        {
            if (string.IsNullOrWhiteSpace(databaseKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseKey));
            var builder = _initializationParams.SessionFactoryRegistry.GetSessionFactory(databaseKey).WithStatelessOptions();
            _initializationParams.ConfigureStatelessSession?.Invoke(databaseKey, builder);
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
