namespace Tests.SharpArch.NHibernate.MultiDb
{
    using global::NHibernate;
    using JetBrains.Annotations;


    public interface ISessionRegistry
    {
        ISession GetSession([NotNull] string databaseKey);
        IStatelessSession CreateStatelessSession([NotNull] string databaseKey);
    }
}