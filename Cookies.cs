using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Utils.BasicWebCall
{
    public class Cookies
    {
        public CookieContainer Container { get; }

        public Cookies()
        {
            Container = new CookieContainer();
        }

        public void AddFrom(Uri responseUri, CookieCollection cookies)
        {
            foreach (Cookie cookie in cookies)
                Container.Add(responseUri, cookie);

            BugFixCookieDomain();
        }

        private void BugFixCookieDomain()
        {
            var table = (IDictionary)Container.GetType()
                // #if uwp
											.GetRuntimeFields()
											.FirstOrDefault(x => x.Name == "m_domainTable" || x.Name == "_domainTable")
											.GetValue(Container);
                // #else
            //.InvokeMember("m_domainTable", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, Container, new object[] { });


            var keys = table.Keys.OfType<string>().ToList();
            foreach (var key in table.Keys.OfType<string>().ToList())
            {
                if (key[0] != '.')
                {
                    continue;
                }

                var newKey = key.Remove(0, 1);
                if (keys.Contains(newKey))
                {
                    continue;
                }
                table[newKey] = table[key];
                keys.Add(newKey);
            }
        }
    }
}
