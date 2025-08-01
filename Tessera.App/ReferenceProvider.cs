using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App
{
  internal class ReferenceProvider
  {
    private const string POLYNOM_CLIENT_ID = "A6E64FA3-51DB-42D3-9AD7-B82E828EEC31";
    private static ReferenceProvider _instance;
    private ISession _sesion;

    private ReferenceProvider()
    {
      LoginManager.TryOpenSession(Guid.Parse(POLYNOM_CLIENT_ID), out _sesion);
    }

    public static ReferenceProvider Instance => _instance ??= new ReferenceProvider();
  }
}
