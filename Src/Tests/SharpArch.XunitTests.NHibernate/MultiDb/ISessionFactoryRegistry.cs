namespace Tests.SharpArch.NHibernate.MultiDb
{
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    public interface ISessionFactoryRegistry
    {
        void Add([NotNull] string databaseIdentifier, [NotNull] INHibernateSessionFactoryBuilder sessionFactoryBuilder);
        ISessionFactory GetSessionFactory([NotNull] string databaseIdentifier);
        Configuration GetConfiguration(string databaseIdentifier);
        bool Contains(string databaseIdentifier);
        bool IsSessionFactoryCreated(string databaseIdentifier);
    }
}
