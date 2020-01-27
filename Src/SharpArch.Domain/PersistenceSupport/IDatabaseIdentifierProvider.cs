namespace SharpArch.Domain.PersistenceSupport
{
    using System;


    /// <summary>
    ///     Allows to select database associated with given object.
    /// </summary>
    public interface IDatabaseIdentifierProvider
    {
        /// <summary>
        ///     Gets the database identifier.
        /// </summary>
        /// <param name="anObject">An object that may have an attribute used to determine the database identifier.</param>
        /// <returns>Database identifier.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="anObject"/> is <see langword="null"/></exception>
        string GetFromInstance(object anObject);

        string GetFromType(Type type);
    }
}
