using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Controllers
{
    public class ActivityLogger : IActivityLogger
    {
        public async Task LogAsync(IActivity activity)
        {
            //if (!activity.Recipient.Id.Contains("default-user"))
            //{
                //var log = new StreamWriter("content/logs/dialog_" + activity.Recipient.Id + ".log", append: true);
                //log.WriteLine(String.Format("[{0:dd.MM.yy HH:mm:ss}] @{1} - {2}", activity.Timestamp, activity.Recipient.Id, activity.AsMessageActivity()?.Text));
                //log.Flush();
                //log.Close();
            //}
        }
    }
}