namespace Scrabby.Networking.ROS
{
    public abstract class RosField
    {
        public const string Args = "args";
        public const string Client = "client";
        public const string Compression = "compression";
        public const string Data = "data";
        public const string Destination = "destination";
        public const string EndTime = "end";
        public const string ID = "id";
        public const string Level = "level";
        public const string Mac = "mac";
        public const string Message = "msg";
        public const string Op = "op";
        public const string Rand = "rand";
        public const string Result = "result";
        public const string Service = "service";
        public const string ThrottleRate = "throttle_rate";
        public const string Time = "t";
        public const string Topic = "topic";
        public const string Type = "type";
        public const string Values = "values";
        public const string Header = "header";
        public const string Format = "format";
    }

    public abstract class RosOpcode
    {
        public const string Advertise = "advertise";
        public const string AdvertiseService = "advertise_service";
        public const string Auth = "auth";
        public const string CallService = "call_service";
        public const string PNG = "png";
        public const string Publish = "publish";
        public const string ServiceResponse = "service_response";
        public const string Subscribe = "subscribe";
        public const string Unsubscribe = "unsubscribe";
        public const string Unadvertise = "unadvertise";
    }

    public abstract class RosCompression
    {
        public const string None = "none";
        public const string PNG = "png";
    }
}