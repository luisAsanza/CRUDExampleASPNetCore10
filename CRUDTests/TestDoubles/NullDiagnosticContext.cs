using Serilog;

namespace CRUDTests.TestDoubles
{
    public class NullDiagnosticContext : IDiagnosticContext
    {
        public static readonly NullDiagnosticContext Instance = new();

        public void Set(string propertyName, object value, bool destructureObjects = false)
        {
            
        }

        public void SetException(Exception exception)
        {
            
        }
    }
}
