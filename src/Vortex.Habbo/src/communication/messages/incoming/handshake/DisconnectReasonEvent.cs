using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class DisconnectReasonEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(DisconnectReasonEventParser))
{
    public const int const_232 = -1;
    public const int const_64 = -2;
    public const int const_183 = -3;
    public const int const_16 = 0;
    public const int const_189 = 1;
    public const int const_259 = 2;
    public const int const_395 = 3;
    public const int const_27 = 4;
    public const int const_362 = 5;
    public const int const_96 = 10;
    public const int const_291 = 11;
    public const int const_179 = 12;
    public const int const_99 = 13;
    public const int const_55 = 16;
    public const int const_157 = 17;
    public const int const_322 = 18;
    public const int const_400 = 19;
    public const int const_243 = 20;
    public const int const_399 = 22;
    public const int const_213 = 23;
    public const int const_42 = 24;
    public const int const_327 = 25;
    public const int const_169 = 26;
    public const int const_217 = 27;
    public const int const_26 = 28;
    public const int const_212 = 29;
    public const int const_90 = 100;
    public const int const_359 = 101;
    public const int const_24 = 102;
    public const int const_200 = 103;
    public const int const_193 = 104;
    public const int const_77 = 105;
    public const int const_211 = 106;
    public const int const_149 = 107;
    public const int const_125 = 108;
    public const int const_339 = 109;
    public const int const_299 = 110;
    public const int const_133 = 111;
    public const int const_284 = 112;
    public const int const_215 = 113;
    public const int const_44 = 114;
    public const int const_159 = 115;
    public const int const_266 = 116;
    public const int SOCKET_WRITE_EXCEPTION_1 = 117;
    public const int SOCKET_WRITE_EXCEPTION_2 = 118;
    public const int SOCKET_WRITE_EXCEPTION_3 = 119;
    public const int const_121 = 120;
    public const int const_311 = 121;
    public const int const_406 = 122;
    public const int const_84 = 123;
    public const int const_146 = 124;
    public const int const_247 = 125;
    public const int const_168 = 126;

    public int reason => ((DisconnectReasonEventParser)parser!).reason;

    public string reasonString => reason switch
    {
        1 or 10 => "banned",
        2 => "concurrentlogin",
        20 => "incorrectpassword",
        _ => "logout",
    };

    public static string ResolveDisconnectedReasonLocalizationKey(int param1)
    {
        return param1 switch
        {
            -2 => "${disconnected.maintenance}",
            0 => "${disconnected.logged_out}",
            1 => "${disconnected.just_banned}",
            10 => "${disconnected.still_banned}",
            2 or 13 or 11 or 18 => "${disconnected.concurrent_login}",
            12 or 19 => "${disconnected.hotel_closed}",
            20 => "${disconnected.incorrect_password}",
            112 => "${disconnected.idle}",
            122 => "${disconnected.incompatible_client_version}",
            _ => "${disconnected.generic}",
        };
    }

    public string GetReasonName()
    {
        return reason.ToString();
    }
}
