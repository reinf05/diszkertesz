using Android.App;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.Maui.Controls.Platform;

namespace diszkerteszClient.Platforms.Android
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataHost = "auth",
        DataScheme = "msalee879ed8-b52e-4100-9d08-0a530ca01dab")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
