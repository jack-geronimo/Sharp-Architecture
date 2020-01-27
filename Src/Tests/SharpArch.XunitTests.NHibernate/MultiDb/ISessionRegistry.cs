namespace Tests.SharpArch.NHibernate.MultiDb
{
    using global::NHibernate;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    public interface ISessionRegistry
    {
        INHibernateTransactionManager GetTransactionManager([NotNull] string databaseIdentifier);
        IStatelessSession CreateStatelessSession([NotNull] string databaseIdentifier);
    }
}
