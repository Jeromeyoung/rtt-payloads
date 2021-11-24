using System;
using System.Diagnostics.Eventing.Reader;
using System.Security;

namespace SharpSniper
{
    class QueryDC
    {
        public static string QueryRemoteComputer(string username, string domain,
            string dc, string dauser = "", string dapass = "")
        {
            bool credentialed = true;
            if (dauser == string.Empty && dapass == string.Empty)
                credentialed = false;

            string queryString = 
                "*[System[(EventID=4624)] and " +
                    "EventData[Data[@Name='TargetUserName']='" + 
                    username + "']]"; // XPATH Query

            EventLogSession session;

            if (credentialed)
            {
                SecureString pw = GetPassword(dapass);

                session = new EventLogSession
                (
                        dc,                               // Remote Computer
                        domain,                                  // Domain
                        dauser,                                // Username
                        pw,
                        SessionAuthentication.Default
                );
                pw.Dispose();
            }
            else
            {
                session = new EventLogSession(dc); // Remote Computer
            }
              
            // Query the Application log on the remote computer.
            EventLogQuery query = new EventLogQuery("Security", PathType.LogName, 
                queryString);
            query.Session = session;
            EventLogReader reader = new EventLogReader(query);
            EventRecord eventRecord;
            string result = String.Empty;
            while ((eventRecord = reader.ReadEvent()) != null)
            {
                result = eventRecord.FormatDescription();
            }
            // Display event info
            return result;
        }
       

        public static SecureString CreateSecureString(string inputString)
        {
            SecureString secureString = new SecureString();

            foreach (Char character in inputString)
            {
                secureString.AppendChar(character);
            }
            return secureString;
        }

        public static SecureString GetPassword(string highpass)
        {
            SecureString pw_bef = CreateSecureString(highpass);
            return pw_bef;
        }
        
    }

}
