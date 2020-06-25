//using Telegram.Bot.Args;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.InlineQueryResults;
//using Telegram.Bot.Types.InputFiles;
//using Telegram.Bot.Types.ReplyMarkups;
//using Telegram.Bot;
using CryptoTraderScanner;
using System;
using System.Threading.Tasks;
using TLSharp.Core;


// Not: https://github.com/TelegramBots/Telegram.Bot
// Not: https://telegrambots.github.io/book/1/quickstart.html
// https://github.com/Qyperion/TLSharp

namespace CryptoTraderStandard.TelegramAPI
{
    public class TelegramClientAPI
    {
        private static TelegramClient ZignalyTelegramClient = null;
        private static string TelegramClientHash = "";
        private static TeleSharp.TL.TLUser TelegramTLUser = null;

        public TelegramClientAPI()
        {
        }

        public async Task<string> InitZignalyTelegramClient()
        {
            string Message = "";

            try
            {
                Settings Settings = SettingsStore.Load();

                if (Settings.TelegramApiId > 0 && !String.IsNullOrEmpty(Settings.TelegramApiHash))
                {
                    TelegramClientHash = Settings.TelegramClientHash;
                    if (ZignalyTelegramClient == null || !ZignalyTelegramClient.IsConnected)
                    {
                        ZignalyTelegramClient = new TelegramClient(Settings.TelegramApiId, Settings.TelegramApiHash);
                        await ZignalyTelegramClient.ConnectAsync(true); // reconnect is true
                    }
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InitZignalyTelegramClient: {(ZignalyTelegramClient.IsConnected ? "" : "Not ")}Connected and {(ZignalyTelegramClient.IsUserAuthorized() ? "" : "Not ")}Authorized");
                }
                else
                {
                    Message = $"WARNING: NO Telegram Id and/or Hash set!";
                }
            }
            catch (Exception E)
            {
                Message = $"InitZignalyTelegramClient ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Message;
        }

        public bool TelegramIsConnected()
        {
            return (ZignalyTelegramClient != null && ZignalyTelegramClient.IsConnected);
        }

        public async Task<string> AuthenticateTelegramUserRequest(string UserNumber = "")
        {
            try
            {
                Settings Settings = SettingsStore.Load();

                if (String.IsNullOrEmpty(UserNumber)) UserNumber = Settings.TelegramUserNumber;
                if (ZignalyTelegramClient.IsConnected && (!String.IsNullOrEmpty(UserNumber)))
                {
                    TelegramClientHash = await ZignalyTelegramClient.SendCodeRequestAsync(!String.IsNullOrEmpty(UserNumber) ? UserNumber : Settings.TelegramUserNumber);
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} AuthenticateTelegramUserRequest: Done with {UserNumber}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} AuthenticateTelegramUserRequest Not requested: {(ZignalyTelegramClient.IsConnected ? "" : "NOT ")}Connected to telegram, {(String.IsNullOrEmpty(UserNumber) ? "have an" : "with NO")} Usernumber");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} AuthenticateTelegramUserRequest ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }
            return TelegramClientHash;
        }

        public async Task<string> TelegramConnectUser()
        {
            string Message = "";
            Settings Settings = SettingsStore.Load();

            try
            {
                if (ZignalyTelegramClient.IsConnected && !String.IsNullOrEmpty(TelegramClientHash))
                {
                    if (!String.IsNullOrEmpty(Settings.TelegramUserNumber) && !String.IsNullOrEmpty(Settings.TelegramCode))
                    {
                        TelegramTLUser = await ZignalyTelegramClient.MakeAuthAsync(Settings.TelegramUserNumber, TelegramClientHash, Settings.TelegramCode);
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} TelegramConnectUser: User={TelegramTLUser.Username}");
                    }
                    else
                    {
                        Message = $"TelegramConnectUser Request Telegram Code and/or not set (Settings)!";
                    }
                }
                else
                {
                    Message = $"TelegramConnectUser WARNING: {(ZignalyTelegramClient.IsConnected ? "Is" : "Not ")} Connected to Telegram{(String.IsNullOrEmpty(TelegramClientHash) ? ", Set Telegram User Number first and Request Code!" : "")}";
                }
            }
            catch (Exception E)
            {
                if (E.Message == "PHONE_CODE_EXPIRED")
                {
                    Settings.TelegramCode = "";
                    SettingsStore.Save(Settings);
                    Message = $"TelegramConnectUser ERROR: Telegarm Code Expired: Request Code in settings!";
                }
                else
                {
                    Message = $"TelegramConnectUser ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                }
            }

            return Message;
        }

        public bool IsUserAuthenticated()
        {
            return (TelegramTLUser != null);
        }

    }
}
