using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetStreamNetPerformanceTest
{
    public static class SlowWay
    {

        public static int RequestsCount = 5;

        public async static Task Fetch()
        {
            for (int i = 0; i < RequestsCount; i++)
            {
                //("98a6bhskrrwj","t3nj7j8m6dtdbbakzbu9p7akjk5da8an5wxwyt6g73nt5hf9yujp8h4jw244r67p")
                var _streamCLient = new Stream.StreamClient("","");
                
                    var userFeed = _streamCLient.Feed("user", "a491ff35-9d5c-4c5c-9a8b-e0e221f121e8");

                    var objActivity = new Stream.Activity("User:a491ff35-9d5c-4c5c-9a8b-e0e221f121e8", "startliveplay", "Liveplay:1855ce18-3173-46b8-92de-18e019ea7e2f")
                    {
                        Time = DateTime.Now,
                        ForeignId = "startliveplay:1855ce18-3173-46b8-92de-18e019ea7e2f"
                    };

                    objActivity.SetData("Username", "Menno");
                    objActivity.SetData("Message", "Menno started playing a live playlist: Alabama Shakes – Boys & Girls");
                    objActivity.SetData("customc", new ActivityStartLivePlay()
                    {
                        UserName = "Menno",
                        UserAvatar = "http://graph.facebook.com/10209476704408015/picture?type=large",
                        LivePlaylistId = "1855ce18-3173-46b8-92de-18e019ea7e2f",
                        LivePlaylistAvatar = "https://i.scdn.co/image/7e28207f9bda5eb36f0ab878f5eef7d24e7e6c53",
                        LivePlaylistDescription = "First album Alabama Shakes.",
                        LivePlaylistTitle = "Alabama Shakes – Boys & Girls"
                    });

                    var responseUser = await userFeed.AddActivity(objActivity);

                    Console.WriteLine($"API responded with: {responseUser}");
            }
            
        }

    }
}
