using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace CRUDTests.AutoFixtureBuilder
{
    public sealed class EmailForPropertyNamedEmailBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if(request is PropertyInfo p && p.PropertyType == typeof(string) 
                && p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                return new MailAddress($"{Guid.NewGuid():N}@example.com").Address;
            }

            return new NoSpecimen();
        }
    }
}
