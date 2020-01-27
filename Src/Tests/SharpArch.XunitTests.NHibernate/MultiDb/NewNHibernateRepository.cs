namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     Prototype.
    /// </summary>
    public class NewNHibernateRepository
    {
        INHibernateTransactionManager _transactionManager;

        public NewNHibernateRepository([NotNull] ISessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider keyProvider)
        {
            if (sessionRegistry == null) throw new ArgumentNullException(nameof(sessionRegistry));
            if (keyProvider == null) throw new ArgumentNullException(nameof(keyProvider));

            _transactionManager = sessionRegistry.GetTransactionManager(keyProvider.GetFromInstance(this));
        }
    }
}
