namespace Hatfield.Aquarius.PublicServiceAPI
{
    internal static class Constants
    {
        public static string TokenURL = "/GetAuthToken?user={0}&encPwd={1}";
        public static string DataSetsListURL = "/GetDataSetsList?locId={0}";
        public static string TimeSeriesURL = "/GetTimeSeriesData?dataId={0}&view=Public&queryFrom={1}&queryTo={2}";
        public static string[] Sperators = { "\r\n" };

        //Time Series Headers
        public static string TimeSeriesTime = "Time";

        public static string TimeSeriesValue = "Value";
        public static string TimeSeriesFlag = "Quality";
        public static string TimeSeriesApprovalLevel = "Approval";
    }
}