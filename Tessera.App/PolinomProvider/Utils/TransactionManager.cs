using Ascon.Polynom.Api;

namespace Tessera.App.PolinomProvider.Utils
{
  internal class TransactionManager
  {
    private ISession _session;

    public TransactionManager(ISession session)
    {
      _session = session;
    }

    public void ApplyChanges(Action action)
    {
      var transaction = _session.Objects.StartTransaction();
      try
      {
        action();
        transaction.Commit();
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        //пока так
      }
    }
  }
}