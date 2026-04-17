// See https://aka.ms/new-console-template for more information

using Ngaq.Server.Http.Test;

Console.WriteLine("Hello, World!");

await MkDbSchema.InitDb([@"E:\_code\CsNgaq\Ngaq.Server\ExternalRsrc\Ngaq.Server.dev.jsonc"]);
